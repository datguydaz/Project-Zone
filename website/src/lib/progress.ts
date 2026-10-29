import type {
  HubIssue,
  HubMilestone,
  HubSnapshot,
  HubStats,
  MilestoneStatus,
  ProgressMode,
  TaskStatus,
} from "./types";

export function issueTaskStatus(issue: Pick<HubIssue, "state" | "labels">): TaskStatus {
  if (issue.state === "closed") return "completed";
  if (issue.labels.some((label) => label.toLowerCase() === "blocked")) return "blocked";
  return "open";
}

export function computeMilestoneProgress(
  milestone: Pick<HubMilestone, "status" | "requirements" | "issueIds" | "progressVerified" | "progressMode" | "progressPercent" | "openCount" | "closedCount" | "blockedCount">,
  issues: HubIssue[],
): {
  percent: number | null;
  mode: ProgressMode;
  verified: boolean;
  openCount: number;
  closedCount: number;
  blockedCount: number;
} {
  const linked = issues.filter((issue) => milestone.issueIds.includes(issue.number));
  if (linked.length > 0) {
    const openCount = linked.filter((issue) => issue.status === "open").length;
    const closedCount = linked.filter((issue) => issue.status === "completed").length;
    const blockedCount = linked.filter((issue) => issue.status === "blocked").length;
    return {
      percent: Math.round((closedCount / linked.length) * 100),
      mode: "issue_based",
      verified: true,
      openCount,
      closedCount,
      blockedCount,
    };
  }

  // Preserve snapshot-authored planning values; never invent live verification.
  if (milestone.progressMode === "planning_checklist" || milestone.progressMode === "milestone_state") {
    return {
      percent: milestone.progressPercent,
      mode: "planning_checklist",
      verified: false,
      openCount: milestone.openCount,
      closedCount: milestone.closedCount,
      blockedCount: milestone.blockedCount,
    };
  }

  if (milestone.status === "completed" && milestone.progressPercent === 100) {
    return {
      percent: 100,
      mode: "planning_checklist",
      verified: false,
      openCount: 0,
      closedCount: milestone.closedCount,
      blockedCount: 0,
    };
  }

  return {
    percent: null,
    mode: "untracked",
    verified: false,
    openCount: 0,
    closedCount: 0,
    blockedCount: 0,
  };
}

export function enrichMilestones(
  milestones: HubMilestone[],
  issues: HubIssue[],
): HubMilestone[] {
  return milestones.map((milestone) => {
    const progress = computeMilestoneProgress(milestone, issues);
    return {
      ...milestone,
      progressPercent: progress.percent,
      progressMode: progress.mode,
      progressVerified: progress.verified,
      openCount: progress.openCount,
      closedCount: progress.closedCount,
      blockedCount: progress.blockedCount,
    };
  });
}

export function deriveStats(snapshot: HubSnapshot): HubStats {
  const milestones = enrichMilestones(snapshot.milestones, snapshot.issues);
  const activeMilestone =
    milestones.find((milestone) => milestone.status === "in_progress") ??
    milestones.find((milestone) => milestone.status === "blocked") ??
    null;

  const openTasks = snapshot.issues.filter((issue) => issue.status === "open").length;
  const completedTasks = snapshot.issues.filter((issue) => issue.status === "completed").length;
  const blockedTasks = snapshot.issues.filter((issue) => issue.status === "blocked").length;

  const verified = milestones.filter((milestone) => milestone.progressVerified);
  let overallProgressPercent: number | null = null;
  let overallProgressMode: ProgressMode = "untracked";
  let overallProgressVerified = false;

  if (verified.length > 0) {
    const sum = verified.reduce((acc, milestone) => acc + (milestone.progressPercent ?? 0), 0);
    overallProgressPercent = Math.round(sum / milestones.length);
    overallProgressMode = "issue_based";
    overallProgressVerified = true;
  } else if (snapshot.dataAuthority === "planning_catalog") {
    const tracked = milestones.filter((milestone) => milestone.progressPercent !== null);
    if (tracked.length > 0) {
      const sum = tracked.reduce((acc, milestone) => acc + (milestone.progressPercent ?? 0), 0);
      overallProgressPercent = Math.round(sum / milestones.length);
      overallProgressMode = "planning_checklist";
      overallProgressVerified = false;
    }
  }

  return {
    activeMilestone,
    completedMilestones: milestones.filter((milestone) => milestone.status === "completed").length,
    totalMilestones: milestones.length,
    openTasks,
    completedTasks,
    blockedTasks,
    overallProgressPercent,
    overallProgressMode,
    overallProgressVerified,
  };
}

export function statusLabel(status: MilestoneStatus | TaskStatus): string {
  switch (status) {
    case "completed":
      return "Completed";
    case "in_progress":
      return "In Progress";
    case "not_started":
      return "Planned";
    case "blocked":
      return "Blocked";
    case "open":
      return "Open";
    default: {
      const _exhaustive: never = status;
      return _exhaustive;
    }
  }
}

export function formatDate(value: string | null | undefined): string {
  if (!value) return "—";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "—";
  return new Intl.DateTimeFormat("en-GB", {
    year: "numeric",
    month: "short",
    day: "2-digit",
  }).format(date);
}

export function formatDateTime(value: string | null | undefined): string {
  if (!value) return "Never";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "Never";
  return new Intl.DateTimeFormat("en-GB", {
    year: "numeric",
    month: "short",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    timeZoneName: "short",
  }).format(date);
}

export function progressDisplay(percent: number | null, mode: ProgressMode): string {
  if (percent === null || mode === "untracked") return "Untracked";
  return `${percent}%`;
}

export function progressSourceLabel(mode: ProgressMode, verified: boolean): string {
  if (mode === "issue_based" && verified) return "GitHub issues";
  if (mode === "planning_checklist" || mode === "milestone_state") {
    return "Planning checklist (not GitHub-verified)";
  }
  return "Untracked";
}
