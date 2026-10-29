import type { HubIssue, HubSnapshot } from "./types";
import { enrichMilestones, issueTaskStatus } from "./progress";

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

function sanitizeIssue(raw: unknown): HubIssue {
  if (!isRecord(raw)) {
    throw new Error("Invalid issue entry in hub snapshot.");
  }

  const labels = Array.isArray(raw.labels)
    ? raw.labels.filter((label): label is string => typeof label === "string")
    : [];

  const issue: HubIssue = {
    id: Number(raw.id),
    number: Number(raw.number),
    title: String(raw.title ?? ""),
    state: raw.state === "closed" ? "closed" : "open",
    status: "open",
    labels,
    milestoneId: typeof raw.milestoneId === "string" ? raw.milestoneId : null,
    milestoneTitle: typeof raw.milestoneTitle === "string" ? raw.milestoneTitle : null,
    htmlUrl: String(raw.htmlUrl ?? ""),
    updatedAt: String(raw.updatedAt ?? ""),
    closedAt: typeof raw.closedAt === "string" ? raw.closedAt : null,
    isPublic: raw.isPublic !== false,
  };

  issue.status = issueTaskStatus(issue);
  return issue;
}

function validateSnapshot(data: unknown): HubSnapshot {
  if (!isRecord(data)) {
    throw new Error("Hub snapshot is not an object.");
  }

  if (typeof data.schemaVersion !== "number") {
    throw new Error("Hub snapshot missing schemaVersion.");
  }

  if (!Array.isArray(data.milestones) || !Array.isArray(data.issues)) {
    throw new Error("Hub snapshot missing milestones or issues.");
  }

  const snapshot = data as unknown as HubSnapshot;
  const issues = snapshot.issues.map((issue) => sanitizeIssue(issue));

  return {
    ...snapshot,
    dataAuthority: snapshot.dataAuthority ?? "planning_catalog",
    syncSource: snapshot.syncSource ?? "catalog",
    issues,
    milestones: enrichMilestones(
      snapshot.milestones.map((milestone) => ({
        ...milestone,
        progressVerified: milestone.progressVerified ?? false,
      })),
      issues,
    ),
  };
}

export async function loadSnapshot(signal?: AbortSignal): Promise<HubSnapshot> {
  const url = `${import.meta.env.BASE_URL}data/hub-snapshot.json`;
  const response = await fetch(url, { signal, cache: "no-cache" });

  if (!response.ok) {
    throw new Error(`Failed to load hub snapshot (${response.status}).`);
  }

  const json: unknown = await response.json();
  return validateSnapshot(json);
}
