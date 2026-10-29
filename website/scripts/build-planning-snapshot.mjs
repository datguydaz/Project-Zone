#!/usr/bin/env node
/**
 * Builds public/data/hub-snapshot.json from the local planning catalog.
 * Output is explicitly labeled planning/unverified — not live GitHub progress.
 */

import { readFile, writeFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const websiteRoot = path.resolve(__dirname, "..");
const catalogPath = path.join(websiteRoot, "data", "hub-catalog.json");
const snapshotPath = path.join(websiteRoot, "public", "data", "hub-snapshot.json");

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
  const closed = requirements.filter((item) => item.completed).length;
  return {
    progressPercent: Math.round((closed / requirements.length) * 100),
    progressMode: "planning_checklist",
    progressVerified: false,
    openCount: requirements.length - closed,
    closedCount: closed,
    blockedCount: 0,
  };
}

const catalog = JSON.parse(await readFile(catalogPath, "utf8"));
const now = new Date().toISOString();

const snapshot = {
  schemaVersion: 1,
  generatedAt: now,
  syncedAt: null,
  lastSyncAttemptAt: null,
  syncStatus: "seed",
  syncError: null,
  syncSource: "catalog",
  dataAuthority: "planning_catalog",
  repository: {
    ...catalog.repository,
    visibility: "unknown",
  },
  project: catalog.project,
  milestones: catalog.milestones.map((milestone) => ({
    id: milestone.id,
    number: milestone.number,
    title: milestone.title,
    description: milestone.description,
    status: milestone.status,
    githubMilestoneNumber: null,
    githubMilestoneUrl: null,
    scope: milestone.scope,
    requirements: milestone.requirements.map(({ id, title, completed }) => ({
      id,
      title,
      completed,
    })),
    blockers: milestone.blockers,
    lastUpdated: catalog.updatedAt,
    issueIds: [],
    ...planningProgress(milestone.requirements),
  })),
  issues: [],
  activity: (catalog.activity || []).filter((item) => item.public !== false).map((item) => ({
    id: item.id,
    kind: item.kind,
    title: item.title,
    summary: item.summary,
    date: item.date,
    milestoneId: item.milestoneId,
    url: item.url,
  })),
  devlog: (catalog.devlog || []).filter((entry) => entry.public).map((entry) => ({
    id: entry.id,
    date: entry.date,
    title: entry.title,
    kind: entry.kind,
    body: entry.body,
    milestoneId: entry.milestoneId,
    public: true,
    screenshots: entry.screenshots || [],
  })),
  publicPolicy: catalog.publicPolicy,
};

await writeFile(snapshotPath, `${JSON.stringify(snapshot, null, 2)}\n`, "utf8");
console.log(`Wrote planning snapshot → ${snapshotPath}`);
