export type MilestoneStatus =
  | "completed"
  | "in_progress"
  | "not_started"
  | "blocked";

export type IssueState = "open" | "closed";

export type TaskStatus = "open" | "completed" | "blocked";

export type SyncStatus = "seed" | "ok" | "failed" | "partial";

export type ProgressMode =
  | "issue_based"
  | "planning_checklist"
  | "milestone_state"
  | "untracked";

export type DataAuthority = "planning_catalog" | "github";

export interface HubRepository {
  owner: string;
  name: string;
  url: string;
  defaultBranch: string;
  visibility: "public" | "private" | "unknown";
}

export interface HubProject {
  title: string;
  tagline: string;
  phase: string;
  description: string;
  engine: string;
}

export interface MilestoneRequirement {
  id: string;
  title: string;
  completed: boolean;
}

export interface HubMilestone {
  id: string;
  number: number;
  title: string;
  description: string;
  status: MilestoneStatus;
  githubMilestoneNumber: number | null;
  githubMilestoneUrl: string | null;
  scope: string[];
  requirements: MilestoneRequirement[];
  blockers: string[];
  lastUpdated: string;
  issueIds: number[];
  progressPercent: number | null;
  progressMode: ProgressMode;
  progressVerified: boolean;
  openCount: number;
  closedCount: number;
  blockedCount: number;
}

export interface HubIssue {
  id: number;
  number: number;
  title: string;
  state: IssueState;
  status: TaskStatus;
  labels: string[];
  milestoneId: string | null;
  milestoneTitle: string | null;
  htmlUrl: string;
  updatedAt: string;
  closedAt: string | null;
  isPublic: boolean;
}

export interface HubActivityItem {
  id: string;
  kind: "milestone" | "feature" | "fix" | "decision" | "note" | "commit";
  title: string;
  summary: string;
  date: string;
  milestoneId: string | null;
  url: string | null;
}

export interface DevLogEntry {
  id: string;
  date: string;
  title: string;
  kind: "milestone" | "feature" | "fix" | "decision" | "note";
  body: string;
  milestoneId: string | null;
  public: boolean;
  screenshots: string[];
}

export interface PublicPolicy {
  mode: "curated" | "public_repo_mirror" | "private_seed_only";
  note: string;
  excludeLabels: string[];
  requirePublicLabelForPrivateRepo?: boolean;
  issueFieldAllowlist?: string[];
  milestoneFieldAllowlist?: string[];
}

export interface HubSnapshot {
  schemaVersion: number;
  generatedAt: string;
  syncedAt: string | null;
  lastSyncAttemptAt?: string | null;
  syncStatus: SyncStatus;
  syncError: string | null;
  syncSource: "catalog" | "seed" | "github_api";
  dataAuthority: DataAuthority;
  repository: HubRepository;
  project: HubProject;
  milestones: HubMilestone[];
  issues: HubIssue[];
  activity: HubActivityItem[];
  devlog: DevLogEntry[];
  publicPolicy: PublicPolicy;
}

export interface HubStats {
  activeMilestone: HubMilestone | null;
  completedMilestones: number;
  totalMilestones: number;
  openTasks: number;
  completedTasks: number;
  blockedTasks: number;
  overallProgressPercent: number | null;
  overallProgressMode: ProgressMode;
  overallProgressVerified: boolean;
}
