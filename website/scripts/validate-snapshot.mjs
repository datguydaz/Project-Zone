#!/usr/bin/env node
import { readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const snapshotPath = path.join(
  path.dirname(fileURLToPath(import.meta.url)),
  "..",
  "public",
  "data",
  "hub-snapshot.json",
);

const REQUIRED_ROOT = [
  "schemaVersion",
  "generatedAt",
  "syncedAt",
  "syncStatus",
  "syncSource",
  "dataAuthority",
  "repository",
  "project",
  "milestones",
  "issues",
  "activity",
  "devlog",
  "publicPolicy",
];

const FORBIDDEN_ISSUE_KEYS = [
  "body",
  "body_html",
  "body_text",
  "user",
  "assignee",
  "assignees",
  "comments",
  "events",
  "reactions",
];

function assert(condition, message) {
  if (!condition) throw new Error(message);
}

const raw = await readFile(snapshotPath, "utf8");
const snapshot = JSON.parse(raw);

for (const key of REQUIRED_ROOT) {
  assert(key in snapshot, `Missing root field: ${key}`);
}

assert(snapshot.schemaVersion === 1, "schemaVersion must be 1");
assert(
  ["planning_catalog", "github"].includes(snapshot.dataAuthority),
  "dataAuthority must be planning_catalog|github",
);
assert(
  ["seed", "ok", "failed", "partial"].includes(snapshot.syncStatus),
  "invalid syncStatus",
);
assert(Array.isArray(snapshot.milestones), "milestones must be array");
assert(Array.isArray(snapshot.issues), "issues must be array");

for (const milestone of snapshot.milestones) {
  assert(typeof milestone.id === "string", "milestone.id required");
  assert(
    ["issue_based", "planning_checklist", "untracked", "milestone_state"].includes(
      milestone.progressMode,
    ),
    `invalid progressMode on ${milestone.id}`,
  );
  assert(typeof milestone.progressVerified === "boolean", `progressVerified missing on ${milestone.id}`);
  if (snapshot.dataAuthority === "planning_catalog") {
    assert(milestone.progressVerified === false, `planning snapshot marked verified: ${milestone.id}`);
  }
  if (milestone.progressMode === "issue_based") {
    assert(milestone.progressVerified === true, `issue_based must be verified: ${milestone.id}`);
  }
}

for (const issue of snapshot.issues) {
  for (const key of FORBIDDEN_ISSUE_KEYS) {
    assert(!(key in issue), `Forbidden issue field leaked: ${key}`);
  }
  assert(typeof issue.number === "number", "issue.number required");
  assert(typeof issue.title === "string", "issue.title required");
  assert(issue.body === undefined, "issue body must not be published");
}

if (snapshot.dataAuthority === "planning_catalog") {
  assert(snapshot.syncStatus === "seed" || snapshot.syncStatus === "failed", "planning authority unexpected syncStatus");
}

console.log(
  `Snapshot valid: authority=${snapshot.dataAuthority} sync=${snapshot.syncStatus} milestones=${snapshot.milestones.length} issues=${snapshot.issues.length}`,
);
