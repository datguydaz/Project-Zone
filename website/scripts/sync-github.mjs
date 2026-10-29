#!/usr/bin/env node
/**
 * Fetches GitHub milestones/issues and writes a sanitized hub snapshot.
 *
 * Tokens: GITHUB_TOKEN / GH_TOKEN only (never written to disk or snapshot).
 * On failure: restores the previous snapshot bytes, then stamps failure metadata
 * without recalculating progress.
 *
 * Public issue payloads are allowlisted — bodies/comments/assignees never published.
 */

import { readFile, writeFile, copyFile, access } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const websiteRoot = path.resolve(__dirname, "..");
const catalogPath = path.join(websiteRoot, "data", "hub-catalog.json");
const snapshotPath = path.join(websiteRoot, "public", "data", "hub-snapshot.json");
const backupPath = path.join(websiteRoot, "public", "data", "hub-snapshot.prev.json");

const DEFAULT_OWNER = "datguydaz";
const DEFAULT_REPO = "Project-Zone";

const ISSUE_PUBLIC_FIELDS = [
  "id",
  "number",
  "title",
  "state",
  "status",
  "labels",
  "milestoneId",
  "milestoneTitle",
  "htmlUrl",
  "updatedAt",
  "closedAt",
  "isPublic",
];

const MILESTONE_TITLE_MAP = [
  { match: /unity.*hq.*fps|foundation/i, id: "m1-unity-hqfps-foundation" },
  { match: /emerald/i, id: "m2-emerald-ai-integration" },
  { match: /outskirts/i, id: "m3-the-outskirts" },
  { match: /inventory|persistence/i, id: "m4-inventory-persistence" },
  { match: /death|contract/i, id: "m5-death-contracts" },
];

function parseRepo() {
  const fromEnv = process.env.GITHUB_REPOSITORY || process.env.HUB_GITHUB_REPOSITORY;
  if (fromEnv && fromEnv.includes("/")) {
    const [owner, name] = fromEnv.split("/");
    return { owner, name };
  }
  return { owner: DEFAULT_OWNER, name: DEFAULT_REPO };
}

function getToken() {
  return process.env.GITHUB_TOKEN || process.env.GH_TOKEN || "";
}

async function exists(filePath) {
  try {
    await access(filePath);
    return true;
  } catch {
    return false;
  }
}

async function loadJson(filePath) {
  return JSON.parse(await readFile(filePath, "utf8"));
}

function pick(object, keys) {
  const out = {};
  for (const key of keys) {
    if (Object.prototype.hasOwnProperty.call(object, key)) out[key] = object[key];
  }
  return out;
}

function sanitizeText(value) {
  if (typeof value !== "string") return "";
  return value
    .replace(/ghp_[A-Za-z0-9]{20,}/g, "[redacted]")
    .replace(/github_pat_[A-Za-z0-9_]{20,}/g, "[redacted]")
    .replace(/gho_[A-Za-z0-9]{20,}/g, "[redacted]")
    .replace(/ghu_[A-Za-z0-9]{20,}/g, "[redacted]")
    .replace(/ghs_[A-Za-z0-9]{20,}/g, "[redacted]")
    .replace(/[A-Za-z]:\\Users\\[^\\\s]+/gi, "[path]")
    .replace(/\/Users\/[^/\s]+/g, "[path]")
    .replace(/C:\\Users\\[^\\\s]+/gi, "[path]")
    .trim();
}

function mapMilestoneId(title, catalogMilestones) {
  for (const entry of MILESTONE_TITLE_MAP) {
    if (entry.match.test(title)) return entry.id;
  }
  const seed = catalogMilestones.find(
    (milestone) => milestone.title.toLowerCase() === title.toLowerCase(),
  );
  return seed?.id ?? null;
}

function issueStatus(state, labels) {
  if (state === "closed") return "completed";
  if (labels.some((label) => label.toLowerCase() === "blocked")) return "blocked";
  return "open";
}

function labelsAllowed(labels, excludeLabels) {
  return !labels.some((label) =>
    excludeLabels.map((item) => item.toLowerCase()).includes(label.toLowerCase()),
  );
}

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

async function githubFetch(url, token, attempt = 1) {
  const headers = {
    Accept: "application/vnd.github+json",
    "X-GitHub-Api-Version": "2022-11-28",
    "User-Agent": "project-zone-hub-sync",
  };
  if (token) headers.Authorization = `Bearer ${token}`;

  const response = await fetch(url, { headers });
  const remaining = response.headers.get("x-ratelimit-remaining");
  const reset = response.headers.get("x-ratelimit-reset");

  if (response.status === 403 || response.status === 429) {
    const body = await response.text();
    const retryAfter = Number(response.headers.get("retry-after") || 0);
    const resetMs = reset ? Math.max(0, Number(reset) * 1000 - Date.now()) : 0;
    const waitMs = Math.min(60_000, Math.max(retryAfter * 1000, resetMs, 2000));
    if (attempt < 4 && (response.status === 429 || /rate limit/i.test(body))) {
      console.warn(`Rate limited (${response.status}). Waiting ${waitMs}ms…`);
      await sleep(waitMs);
      return githubFetch(url, token, attempt + 1);
    }
    throw new Error(
      `GitHub API ${response.status} rate/auth for ${url} (remaining=${remaining}): ${body.slice(0, 240)}`,
    );
  }

  if (!response.ok) {
    const body = await response.text();
    throw new Error(`GitHub API ${response.status} for ${url}: ${body.slice(0, 240)}`);
  }

  return response.json();
}

async function fetchAll(pathname, token, { owner, name }) {
  const items = [];
  let page = 1;
  while (page <= 20) {
    const url = `https://api.github.com/repos/${owner}/${name}/${pathname}${
      pathname.includes("?") ? "&" : "?"
    }per_page=100&page=${page}`;
    const batch = await githubFetch(url, token);
    if (!Array.isArray(batch) || batch.length === 0) break;
    items.push(...batch);
    if (batch.length < 100) break;
    page += 1;
  }
  return items;
}

function computeIssueProgress(linkedIssues) {
  if (linkedIssues.length === 0) {
    return {
      progressPercent: null,
      progressMode: "untracked",
      progressVerified: false,
      openCount: 0,
      closedCount: 0,
      blockedCount: 0,
    };
  }
  const closedCount = linkedIssues.filter((issue) => issue.status === "completed").length;
  return {
    progressPercent: Math.round((closedCount / linkedIssues.length) * 100),
    progressMode: "issue_based",
    progressVerified: true,
    openCount: linkedIssues.filter((issue) => issue.status === "open").length,
    closedCount,
    blockedCount: linkedIssues.filter((issue) => issue.status === "blocked").length,
  };
}

function planningProgress(requirements) {
  if (!requirements.length) {
    return {
      progressPercent: null,
      progressMode: "untracked",
      progressVerified: false,
      openCount: 0,
      closedCount: 0,
      blockedCount: 0,
    };
  }
  const closedCount = requirements.filter((item) => item.completed).length;
  return {
    progressPercent: Math.round((closedCount / requirements.length) * 100),
    progressMode: "planning_checklist",
    progressVerified: false,
    openCount: requirements.length - closedCount,
    closedCount,
    blockedCount: 0,
  };
}

/**
 * Hub status must NOT treat "has open issues" as active development.
 * Planned milestones keep open tracking issues before work starts.
 *
 * - completed: GitHub milestone closed, or every linked public issue closed
 * - blocked: only blocked issues remain (no open work items)
 * - in_progress: at least one linked issue is completed AND work remains
 * - not_started (Planned): zero completed linked issues yet
 */
function remoteMilestoneStatus(remote, linkedIssues) {
  if (remote.state === "closed") return "completed";

  const open = linkedIssues.filter((issue) => issue.status === "open").length;
  const closed = linkedIssues.filter((issue) => issue.status === "completed").length;
  const blocked = linkedIssues.filter((issue) => issue.status === "blocked").length;

  if (linkedIssues.length > 0 && closed === linkedIssues.length) return "completed";
  if (blocked > 0 && open === 0 && closed === 0) return "blocked";
  if (closed > 0 && (open > 0 || blocked > 0)) return "in_progress";
  return "not_started";
}

async function writeFailureMetadata(message, now) {
  // Restore prior bytes first so a partial/corrupt write cannot stick.
  if (await exists(backupPath)) {
    await copyFile(backupPath, snapshotPath);
  }

  if (!(await exists(snapshotPath))) {
    console.error("No snapshot available to stamp failure metadata.");
    return;
  }

  const preserved = await loadJson(snapshotPath);
  const stamped = {
    ...preserved,
    generatedAt: now,
    lastSyncAttemptAt: now,
    syncStatus: "failed",
    syncError: sanitizeText(message).slice(0, 400),
    // Keep prior syncedAt / dataAuthority / progress untouched.
  };
  await writeFile(snapshotPath, `${JSON.stringify(stamped, null, 2)}\n`, "utf8");
}

async function main() {
  const { owner, name } = parseRepo();
  const token = getToken();
  const now = new Date().toISOString();

  if (!(await exists(catalogPath))) {
    throw new Error(`Missing planning catalog at ${catalogPath}`);
  }
  if (!(await exists(snapshotPath))) {
    throw new Error(`Missing snapshot at ${snapshotPath}. Run: node scripts/build-planning-snapshot.mjs`);
  }

  const catalog = await loadJson(catalogPath);
  await copyFile(snapshotPath, backupPath);

  try {
    if (!token && process.env.CI === "true") {
      // Actions always provides GITHUB_TOKEN; missing means misconfiguration.
      throw new Error("Missing GITHUB_TOKEN in CI. Cannot sync authoritative GitHub data.");
    }

    if (!token) {
      throw new Error(
        "Missing credentials: set GITHUB_TOKEN or GH_TOKEN for private-repo sync. Snapshot left unchanged except failure metadata.",
      );
    }

    const repo = await githubFetch(`https://api.github.com/repos/${owner}/${name}`, token);
    const milestonesRaw = await fetchAll("milestones?state=all", token, { owner, name });
    const issuesRaw = await fetchAll("issues?state=all&filter=all", token, { owner, name });

    const excludeLabels =
      catalog.publicPolicy?.excludeLabels ?? ["private", "internal", "confidential"];
    const requirePublicLabel = Boolean(catalog.publicPolicy?.requirePublicLabelForPrivateRepo);
    const visibility = repo.private ? "private" : "public";
    const publishPrivateRepoIssues = process.env.HUB_PUBLISH_PRIVATE_ISSUES === "1";

    const issues = [];
    for (const issue of issuesRaw) {
      if (issue.pull_request) continue;

      const labels = (issue.labels || []).map((label) =>
        typeof label === "string" ? label : label.name,
      );

      if (!labelsAllowed(labels, excludeLabels)) continue;

      // Private repositories: only publish issues explicitly labeled "public"
      // unless HUB_PUBLISH_PRIVATE_ISSUES=1 (still excludes private/internal/confidential).
      if (visibility === "private" && requirePublicLabel && !publishPrivateRepoIssues) {
        if (!labels.some((label) => label.toLowerCase() === "public")) continue;
      }

      const publicIssue = {
        id: issue.id,
        number: issue.number,
        title: sanitizeText(issue.title),
        state: issue.state,
        status: issueStatus(issue.state, labels),
        labels: labels.filter((label) => !excludeLabels.map((x) => x.toLowerCase()).includes(label.toLowerCase())),
        milestoneId: null,
        milestoneTitle: issue.milestone ? sanitizeText(issue.milestone.title) : null,
        htmlUrl: issue.html_url,
        updatedAt: issue.updated_at,
        closedAt: issue.closed_at,
        isPublic: true,
      };

      issues.push(pick(publicIssue, ISSUE_PUBLIC_FIELDS));
    }

    const catalogById = new Map(catalog.milestones.map((milestone) => [milestone.id, milestone]));
    const mergedMilestones = catalog.milestones.map((milestone) => {
      const requirements = milestone.requirements.map(({ id, title, completed }) => ({
        id,
        title,
        completed,
      }));
      return {
        id: milestone.id,
        number: milestone.number,
        title: milestone.title,
        description: milestone.description,
        status: milestone.status,
        githubMilestoneNumber: null,
        githubMilestoneUrl: null,
        scope: milestone.scope,
        requirements,
        blockers: milestone.blockers,
        lastUpdated: catalog.updatedAt,
        issueIds: [],
        ...planningProgress(requirements),
      };
    });

    let linkedAnyIssues = false;

    for (const remote of milestonesRaw) {
      const mappedId = mapMilestoneId(remote.title, catalog.milestones);
      if (!mappedId) continue;
      const local = catalogById.get(mappedId);
      if (!local) continue;

      const index = mergedMilestones.findIndex((milestone) => milestone.id === mappedId);
      const linkedIssues = issues.filter(
        (issue) => issue.milestoneTitle?.toLowerCase() === remote.title.toLowerCase(),
      );
      for (const issue of linkedIssues) issue.milestoneId = mappedId;
      if (linkedIssues.length > 0) linkedAnyIssues = true;

      const progress = computeIssueProgress(linkedIssues);
      // Prefer GitHub issue state for live progress. Do not treat commits as completion.
      // Catalog requirements remain visible as checklist context only.
      const status = remoteMilestoneStatus(remote, linkedIssues);

      mergedMilestones[index] = {
        ...mergedMilestones[index],
        title: sanitizeText(remote.title) || local.title,
        description: sanitizeText(remote.description || "") || local.description,
        status,
        githubMilestoneNumber: remote.number,
        githubMilestoneUrl: remote.html_url,
        lastUpdated: remote.updated_at || catalog.updatedAt,
        issueIds: linkedIssues.map((issue) => issue.number),
        ...progress,
      };
    }

    // Milestones without a GitHub counterpart stay planning_checklist / unverified.
    const hasRemoteMilestones = milestonesRaw.length > 0;
    const dataAuthority =
      hasRemoteMilestones || linkedAnyIssues ? "github" : "planning_catalog";

    const snapshot = {
      schemaVersion: 1,
      generatedAt: now,
      syncedAt: now,
      lastSyncAttemptAt: now,
      syncStatus: "ok",
      syncError: null,
      syncSource: "github_api",
      dataAuthority,
      repository: {
        owner,
        name,
        url: repo.html_url,
        defaultBranch: repo.default_branch || "main",
        visibility,
      },
      project: catalog.project,
      milestones: mergedMilestones,
      issues,
      activity: (catalog.activity || [])
        .filter((item) => item.public !== false)
        .map((item) => ({
          id: item.id,
          kind: item.kind,
          title: item.title,
          summary: item.summary,
          date: item.date,
          milestoneId: item.milestoneId,
          url: item.url,
        })),
      devlog: (catalog.devlog || [])
        .filter((entry) => entry.public)
        .map((entry) => ({
          id: entry.id,
          date: entry.date,
          title: entry.title,
          kind: entry.kind,
          body: entry.body,
          milestoneId: entry.milestoneId,
          public: true,
          screenshots: entry.screenshots || [],
        })),
      publicPolicy: {
        ...catalog.publicPolicy,
        mode:
          visibility === "public"
            ? "public_repo_mirror"
            : requirePublicLabel
              ? "curated"
              : "private_seed_only",
        note:
          visibility === "public"
            ? "Repository is public. Snapshot publishes allowlisted issue fields only (no bodies). Tokens never ship to the client."
            : "Repository is private. Only issues labeled `public` are published (unless HUB_PUBLISH_PRIVATE_ISSUES=1). Issue bodies and excluded labels never ship.",
      },
    };

    await writeFile(snapshotPath, `${JSON.stringify(snapshot, null, 2)}\n`, "utf8");
    console.log(
      `Sync ok (${dataAuthority}): ${issues.length} public issues, ${mergedMilestones.length} milestones → ${snapshotPath}`,
    );
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error);
    console.error(`Sync failed: ${message}`);
    await writeFailureMetadata(message, now);
    process.exitCode = 1;
  }
}

main();
