#!/usr/bin/env node
/**
 * Creates GitHub labels, milestones, and issues from the approved tracking plan.
 *
 * Default: dry-run (prints planned operations only).
 * Execute only after explicit approval:
 *   CONFIRM_GITHUB_TRACKING=1 node scripts/create-github-tracking.mjs --execute
 */

import { readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const websiteRoot = path.resolve(__dirname, "..");
const planPath = path.join(websiteRoot, "..", "docs", "hub", "github-tracking-plan.md");

const DEFAULT_OWNER = "datguydaz";
const DEFAULT_REPO = "Project-Zone";

const LABELS = [
  { name: "milestone", color: "6B7F4A", description: "Milestone-scoped work" },
  { name: "gameplay", color: "8FA862", description: "Gameplay systems" },
  { name: "ai", color: "5F8F6A", description: "NPC / Emerald AI" },
  { name: "combat", color: "B85C4A", description: "Combat systems" },
  { name: "inventory", color: "C4A35A", description: "Inventory and items" },
  { name: "world", color: "3D4634", description: "World / region content" },
  { name: "multiplayer", color: "4A667F", description: "Future co-op" },
  { name: "ui", color: "9AA392", description: "User interface" },
  { name: "bug", color: "B85C4A", description: "Bug" },
  { name: "enhancement", color: "6B7F4A", description: "Enhancement" },
  { name: "blocked", color: "A67C52", description: "Blocked work" },
  { name: "documentation", color: "5A6B7F", description: "Docs / process" },
  { name: "public", color: "E0BE6A", description: "Safe to mirror on public hub" },
];

const MILESTONES = [
  {
    title: "Unity & HQ FPS Foundation",
    state: "closed",
    description:
      "Unity 6 URP setup, HQ FPS Weapons 2.0 integration, Firing Range validation, Git init, Cursor isolation rule.",
  },
  {
    title: "Emerald AI Integration",
    state: "open",
    description:
      "HQ↔Emerald damage adapters, detection/pursuit, combat validation, death/hit reactions, package compatibility.",
  },
  {
    title: "The Outskirts",
    state: "open",
    description:
      "First playable region: settlement, checkpoint, industrial building, underground entrance, NPCs, atmosphere.",
  },
  {
    title: "Inventory & Persistence",
    state: "open",
    description:
      "Loot, backpack UI, stash, transfers, save/load, persistent equipment.",
  },
  {
    title: "Death & Contracts",
    state: "open",
    description:
      "Hybrid death, recoverable caches, currency, contracts, rewards, expedition loop.",
  },
];

/** @type {Array<{ milestone: string, title: string, labels: string[], state: "open"|"closed", body: string }>} */
const ISSUES = [
  // M1 closed
  {
    milestone: "Unity & HQ FPS Foundation",
    title: "Unity 6 project setup",
    labels: ["milestone", "documentation", "public"],
    state: "closed",
    body: "Verified complete by developer. Acceptance: Unity 6 project opens and builds.",
  },
  {
    milestone: "Unity & HQ FPS Foundation",
    title: "URP configuration",
    labels: ["milestone", "documentation", "public"],
    state: "closed",
    body: "Verified complete by developer. Acceptance: URP pipeline active for HQ FPS content.",
  },
  {
    milestone: "Unity & HQ FPS Foundation",
    title: "HQ FPS Weapons 2.0 integration",
    labels: ["milestone", "gameplay", "combat", "public"],
    state: "closed",
    body: "Verified complete by developer. Acceptance: HQ FPS Weapons available in project.",
  },
  {
    milestone: "Unity & HQ FPS Foundation",
    title: "Weapon and movement validation",
    labels: ["milestone", "gameplay", "combat", "public"],
    state: "closed",
    body: "Verified complete by developer. Acceptance: weapons and movement validated in play mode.",
  },
  {
    milestone: "Unity & HQ FPS Foundation",
    title: "Firing Range testing",
    labels: ["milestone", "gameplay", "public"],
    state: "closed",
    body: "Verified complete by developer. Acceptance: Firing Range scene playable for foundation tests.",
  },
  {
    milestone: "Unity & HQ FPS Foundation",
    title: "Git repository initialization",
    labels: ["milestone", "documentation", "public"],
    state: "closed",
    body: "Verified complete by developer. Acceptance: project tracked in GitHub repository.",
  },
  {
    milestone: "Unity & HQ FPS Foundation",
    title: "Permanent Cursor project rule",
    labels: ["milestone", "documentation", "public"],
    state: "closed",
    body: "Verified complete by developer. Acceptance: `.cursor/rules/project-zone.mdc` always applied.",
  },
  // M2 verified closed
  {
    milestone: "Emerald AI Integration",
    title: "HQ FPS to Emerald damage adapter",
    labels: ["milestone", "ai", "combat", "public"],
    state: "closed",
    body: "Playtest confirmed. HQ shots damage Emerald AI via Hitbox-layer volumes and Flesh surface mapping.",
  },
  {
    milestone: "Emerald AI Integration",
    title: "Emerald to HQ FPS damage adapter",
    labels: ["milestone", "ai", "combat", "public"],
    state: "closed",
    body: "Playtest confirmed. Emerald damage reaches the HQ FPS player damage bridge.",
  },
  // M2 remaining open
  {
    milestone: "Emerald AI Integration",
    title: "Enemy detection and pursuit",
    labels: ["milestone", "ai", "gameplay", "public"],
    state: "open",
    body: "Validate Emerald detection and pursuit against the HQ FPS player in test scenes.",
  },
  {
    milestone: "Emerald AI Integration",
    title: "Melee and ranged combat validation",
    labels: ["milestone", "ai", "combat", "public"],
    state: "open",
    body: "Validate melee and ranged AI combat loops with the damage adapters.",
  },
  {
    milestone: "Emerald AI Integration",
    title: "Death and hit reaction testing",
    labels: ["milestone", "ai", "combat", "public"],
    state: "open",
    body: "Validate death and hit reactions for Emerald AI under HQ FPS damage.",
  },
  {
    milestone: "Emerald AI Integration",
    title: "Package compatibility validation",
    labels: ["milestone", "ai", "documentation", "public"],
    state: "open",
    body: "Confirm HQ FPS Weapons 2.0 and Emerald AI 2026 package compatibility for Milestone 2 sign-off.",
  },
  // M3
  {
    milestone: "The Outskirts",
    title: "First playable region layout",
    labels: ["milestone", "world", "public"],
    state: "open",
    body: "Block out the first playable Outskirts region.",
  },
  {
    milestone: "The Outskirts",
    title: "Safe settlement",
    labels: ["milestone", "world", "public"],
    state: "open",
    body: "Create the safe settlement space in The Outskirts.",
  },
  {
    milestone: "The Outskirts",
    title: "Abandoned checkpoint",
    labels: ["milestone", "world", "public"],
    state: "open",
    body: "Create the abandoned checkpoint location.",
  },
  {
    milestone: "The Outskirts",
    title: "Industrial building",
    labels: ["milestone", "world", "public"],
    state: "open",
    body: "Create the industrial building location.",
  },
  {
    milestone: "The Outskirts",
    title: "Underground entrance",
    labels: ["milestone", "world", "public"],
    state: "open",
    body: "Create the underground entrance point.",
  },
  {
    milestone: "The Outskirts",
    title: "NPC placement and navigation",
    labels: ["milestone", "ai", "world", "public"],
    state: "open",
    body: "Place NPCs and validate navigation in The Outskirts.",
  },
  {
    milestone: "The Outskirts",
    title: "Environmental atmosphere",
    labels: ["milestone", "world", "public"],
    state: "open",
    body: "Establish lighting, audio, and environmental atmosphere for The Outskirts.",
  },
  // M4
  {
    milestone: "Inventory & Persistence",
    title: "Loot containers",
    labels: ["milestone", "inventory", "gameplay", "public"],
    state: "open",
    body: "Implement loot containers and pickup flow.",
  },
  {
    milestone: "Inventory & Persistence",
    title: "Backpack UI",
    labels: ["milestone", "inventory", "ui", "public"],
    state: "open",
    body: "Implement backpack inventory UI.",
  },
  {
    milestone: "Inventory & Persistence",
    title: "Permanent stash",
    labels: ["milestone", "inventory", "public"],
    state: "open",
    body: "Implement permanent stash storage.",
  },
  {
    milestone: "Inventory & Persistence",
    title: "Inventory transfers",
    labels: ["milestone", "inventory", "public"],
    state: "open",
    body: "Implement transfers between containers, backpack, and stash.",
  },
  {
    milestone: "Inventory & Persistence",
    title: "Save/load validation",
    labels: ["milestone", "inventory", "public"],
    state: "open",
    body: "Validate save and load for inventory state.",
  },
  {
    milestone: "Inventory & Persistence",
    title: "Persistent equipment and items",
    labels: ["milestone", "inventory", "gameplay", "public"],
    state: "open",
    body: "Ensure equipment and items persist across sessions.",
  },
  // M5
  {
    milestone: "Death & Contracts",
    title: "Hybrid death rules",
    labels: ["milestone", "gameplay", "public"],
    state: "open",
    body: "Implement hybrid death rules for expeditions.",
  },
  {
    milestone: "Death & Contracts",
    title: "Recoverable death caches",
    labels: ["milestone", "gameplay", "inventory", "public"],
    state: "open",
    body: "Implement recoverable death caches.",
  },
  {
    milestone: "Death & Contracts",
    title: "Currency",
    labels: ["milestone", "gameplay", "public"],
    state: "open",
    body: "Implement expedition currency.",
  },
  {
    milestone: "Death & Contracts",
    title: "Repeatable contracts",
    labels: ["milestone", "gameplay", "public"],
    state: "open",
    body: "Implement repeatable contracts.",
  },
  {
    milestone: "Death & Contracts",
    title: "Rewards and progression",
    labels: ["milestone", "gameplay", "public"],
    state: "open",
    body: "Implement contract rewards and progression hooks.",
  },
  {
    milestone: "Death & Contracts",
    title: "Complete expedition gameplay loop",
    labels: ["milestone", "gameplay", "public"],
    state: "open",
    body: "Validate the full expedition loop end-to-end.",
  },
];

function parseRepo() {
  const fromEnv = process.env.GITHUB_REPOSITORY || process.env.HUB_GITHUB_REPOSITORY;
  if (fromEnv?.includes("/")) {
    const [owner, name] = fromEnv.split("/");
    return { owner, name };
  }
  return { owner: DEFAULT_OWNER, name: DEFAULT_REPO };
}

function getToken() {
  return process.env.GITHUB_TOKEN || process.env.GH_TOKEN || "";
}

async function github(method, urlPath, token, body) {
  const response = await fetch(`https://api.github.com${urlPath}`, {
    method,
    headers: {
      Accept: "application/vnd.github+json",
      Authorization: `Bearer ${token}`,
      "X-GitHub-Api-Version": "2022-11-28",
      "User-Agent": "project-zone-hub-tracking",
      ...(body ? { "Content-Type": "application/json" } : {}),
    },
    body: body ? JSON.stringify(body) : undefined,
  });
  const text = await response.text();
  let json = null;
  try {
    json = text ? JSON.parse(text) : null;
  } catch {
    json = { raw: text };
  }
  if (!response.ok) {
    throw new Error(`${method} ${urlPath} → ${response.status}: ${text.slice(0, 300)}`);
  }
  return json;
}

function printPlan() {
  console.log("PROJECT ZONE — GitHub tracking dry-run\n");
  console.log(`Plan doc: ${planPath}`);
  console.log(`Labels: ${LABELS.length}`);
  console.log(`Milestones: ${MILESTONES.length}`);
  console.log(`Issues: ${ISSUES.length} (${ISSUES.filter((i) => i.state === "closed").length} closed, ${ISSUES.filter((i) => i.state === "open").length} open)`);
  console.log("\nMilestones:");
  for (const milestone of MILESTONES) {
    console.log(`  - [${milestone.state}] ${milestone.title}`);
  }
  console.log("\nIssues by milestone:");
  for (const milestone of MILESTONES) {
    console.log(`\n  ${milestone.title}`);
    for (const issue of ISSUES.filter((item) => item.milestone === milestone.title)) {
      console.log(`    - [${issue.state}] ${issue.title} (${issue.labels.join(", ")})`);
    }
  }
  console.log("\nNo remote changes were made.");
  console.log("To execute after approval:");
  console.log("  CONFIRM_GITHUB_TRACKING=1 node scripts/create-github-tracking.mjs --execute");
}

async function execute() {
  if (process.env.CONFIRM_GITHUB_TRACKING !== "1") {
    throw new Error(
      "Refusing execute: set CONFIRM_GITHUB_TRACKING=1 after explicit user approval.",
    );
  }

  const token = getToken();
  if (!token) throw new Error("Missing GITHUB_TOKEN / GH_TOKEN");

  const { owner, name } = parseRepo();
  const repoPath = `/repos/${owner}/${name}`;

  console.log(`Creating tracking on ${owner}/${name}…`);

  for (const label of LABELS) {
    try {
      await github("POST", `${repoPath}/labels`, token, label);
      console.log(`Label created: ${label.name}`);
    } catch (error) {
      const message = error instanceof Error ? error.message : String(error);
      if (message.includes("422")) {
        console.log(`Label exists: ${label.name}`);
      } else {
        throw error;
      }
    }
  }

  /** @type {Map<string, number>} */
  const milestoneNumbers = new Map();
  const existing = await github("GET", `${repoPath}/milestones?state=all&per_page=100`, token);
  for (const remote of existing) {
    milestoneNumbers.set(remote.title, remote.number);
  }

  for (const milestone of MILESTONES) {
    if (milestoneNumbers.has(milestone.title)) {
      const number = milestoneNumbers.get(milestone.title);
      await github("PATCH", `${repoPath}/milestones/${number}`, token, {
        title: milestone.title,
        state: milestone.state,
        description: milestone.description,
      });
      console.log(`Milestone updated: ${milestone.title} (#${number})`);
    } else {
      const created = await github("POST", `${repoPath}/milestones`, token, {
        title: milestone.title,
        state: "open",
        description: milestone.description,
      });
      milestoneNumbers.set(milestone.title, created.number);
      if (milestone.state === "closed") {
        await github("PATCH", `${repoPath}/milestones/${created.number}`, token, {
          state: "closed",
        });
      }
      console.log(`Milestone created: ${milestone.title} (#${created.number})`);
    }
  }

  const existingIssues = await github(
    "GET",
    `${repoPath}/issues?state=all&per_page=100`,
    token,
  );
  const existingTitles = new Set(
    existingIssues.filter((issue) => !issue.pull_request).map((issue) => issue.title),
  );

  for (const issue of ISSUES) {
    if (existingTitles.has(issue.title)) {
      console.log(`Issue exists, skipped: ${issue.title}`);
      continue;
    }
    const milestone = milestoneNumbers.get(issue.milestone);
    const created = await github("POST", `${repoPath}/issues`, token, {
      title: issue.title,
      body: issue.body,
      labels: issue.labels,
      milestone,
    });
    if (issue.state === "closed") {
      await github("PATCH", `${repoPath}/issues/${created.number}`, token, {
        state: "closed",
      });
    }
    console.log(`Issue created: #${created.number} ${issue.title} [${issue.state}]`);
  }

  console.log("Done.");
}

const executeRequested = process.argv.includes("--execute");
if (!executeRequested) {
  printPlan();
} else {
  execute().catch((error) => {
    console.error(error instanceof Error ? error.message : error);
    process.exitCode = 1;
  });
}

// Keep plan file referenced so packaging tools notice the coupling.
void readFile(planPath, "utf8").catch(() => undefined);
