import { useMemo, useState } from "react";
import { DataAuthorityBanner } from "../components/DataAuthorityBanner";
import { ErrorState } from "../components/ErrorState";
import { LoadingState } from "../components/LoadingState";
import { ProgressBar } from "../components/ProgressBar";
import { StatusBadge } from "../components/StatusBadge";
import { useHub } from "../lib/HubContext";
import { formatDate } from "../lib/progress";
import type { MilestoneStatus } from "../lib/types";

const filters: Array<{ id: "all" | MilestoneStatus; label: string }> = [
  { id: "all", label: "All" },
  { id: "in_progress", label: "In Progress" },
  { id: "completed", label: "Completed" },
  { id: "not_started", label: "Planned" },
  { id: "blocked", label: "Blocked" },
];

export function RoadmapPage() {
  const { snapshot, loading, error, reload } = useHub();
  const [statusFilter, setStatusFilter] = useState<"all" | MilestoneStatus>("all");

  const milestones = useMemo(() => {
    if (!snapshot) return [];
    if (statusFilter === "all") return snapshot.milestones;
    return snapshot.milestones.filter((milestone) => milestone.status === statusFilter);
  }, [snapshot, statusFilter]);

  if (loading) return <LoadingState />;
  if (error || !snapshot) {
    return <ErrorState message={error ?? "Snapshot missing."} onRetry={reload} />;
  }

  return (
    <div className="space-y-8 fade-in">
      <header className="space-y-3">
        <p className="text-label">Milestone roadmap</p>
        <h2 className="text-3xl font-semibold text-bone">Development Roadmap</h2>
        <p className="max-w-2xl text-sm text-mist">
          Live progress comes only from linked GitHub issue state after sync. Planning
          checklist percentages are labeled unverified and are never treated as live
          completion. Untracked milestones show as Untracked.
        </p>
      </header>

      <DataAuthorityBanner authority={snapshot.dataAuthority} />

      <div className="flex flex-wrap gap-2" role="group" aria-label="Milestone status filter">
        {filters.map((filter) => (
          <button
            key={filter.id}
            type="button"
            onClick={() => setStatusFilter(filter.id)}
            className={`rounded-md border px-3 py-2 font-mono text-xs tracking-wider uppercase transition-colors ${
              statusFilter === filter.id
                ? "border-olive bg-olive/20 text-olive-bright"
                : "border-edge text-mist hover:border-edge-strong hover:text-bone"
            }`}
          >
            {filter.label}
          </button>
        ))}
      </div>

      <div className="space-y-5">
        {milestones.map((milestone) => {
          const linkedIssues = snapshot.issues.filter((issue) =>
            milestone.issueIds.includes(issue.number),
          );
          const completedRequirements = milestone.requirements.filter(
            (requirement) => requirement.completed,
          );
          const remainingRequirements = milestone.requirements.filter(
            (requirement) => !requirement.completed,
          );

          return (
            <article
              key={milestone.id}
              id={milestone.id}
              className="panel scroll-mt-28 overflow-hidden"
            >
              <div className="border-b border-edge px-5 py-5 sm:px-6">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <p className="text-label">Milestone {milestone.number}</p>
                    <h3 className="mt-1 text-2xl text-bone">{milestone.title}</h3>
                  </div>
                  <StatusBadge status={milestone.status} />
                </div>
                <p className="mt-3 max-w-3xl text-sm leading-relaxed text-mist">
                  {milestone.description}
                </p>
                <div className="mt-5 max-w-xl">
                  <ProgressBar
                    percent={milestone.progressPercent}
                    mode={milestone.progressMode}
                    verified={milestone.progressVerified}
                    label="Completion"
                  />
                </div>
              </div>

              <div className="grid gap-6 px-5 py-5 sm:px-6 lg:grid-cols-2">
                <div>
                  <p className="text-label">Scope</p>
                  <ul className="mt-3 space-y-2">
                    {milestone.scope.map((item) => (
                      <li key={item} className="flex gap-2 text-sm text-fog">
                        <span className="text-olive">▸</span>
                        <span>{item}</span>
                      </li>
                    ))}
                  </ul>
                </div>

                <div className="space-y-5">
                  <div>
                    <p className="text-label">Completed requirements</p>
                    <ul className="mt-3 space-y-2">
                      {completedRequirements.length === 0 ? (
                        <li className="text-sm text-mist">None yet.</li>
                      ) : (
                        completedRequirements.map((requirement) => (
                          <li key={requirement.id} className="text-sm text-done">
                            ✓ {requirement.title}
                          </li>
                        ))
                      )}
                    </ul>
                  </div>
                  <div>
                    <p className="text-label">Remaining work</p>
                    <ul className="mt-3 space-y-2">
                      {remainingRequirements.length === 0 ? (
                        <li className="text-sm text-mist">No remaining checklist items.</li>
                      ) : (
                        remainingRequirements.map((requirement) => (
                          <li key={requirement.id} className="text-sm text-fog">
                            ○ {requirement.title}
                          </li>
                        ))
                      )}
                    </ul>
                  </div>
                </div>
              </div>

              <div className="grid gap-6 border-t border-edge px-5 py-5 sm:px-6 lg:grid-cols-3">
                <div>
                  <p className="text-label">Blockers</p>
                  {milestone.blockers.length === 0 ? (
                    <p className="mt-2 text-sm text-mist">None recorded.</p>
                  ) : (
                    <ul className="mt-2 space-y-1">
                      {milestone.blockers.map((blocker) => (
                        <li key={blocker} className="text-sm text-amber-bright">
                          {blocker}
                        </li>
                      ))}
                    </ul>
                  )}
                </div>

                <div>
                  <p className="text-label">Related GitHub issues</p>
                  {linkedIssues.length === 0 ? (
                    <p className="mt-2 text-sm text-mist">
                      No linked public issues in snapshot.
                    </p>
                  ) : (
                    <ul className="mt-2 space-y-2">
                      {linkedIssues.map((issue) => (
                        <li key={issue.number}>
                          <a
                            href={issue.htmlUrl}
                            target="_blank"
                            rel="noreferrer"
                            className="text-sm text-olive-bright hover:text-amber-bright"
                          >
                            #{issue.number} {issue.title}
                          </a>
                        </li>
                      ))}
                    </ul>
                  )}
                  {milestone.githubMilestoneUrl ? (
                    <a
                      href={milestone.githubMilestoneUrl}
                      target="_blank"
                      rel="noreferrer"
                      className="mt-3 inline-block font-mono text-xs text-mist hover:text-bone"
                    >
                      GitHub milestone #{milestone.githubMilestoneNumber}
                    </a>
                  ) : (
                    <p className="mt-3 font-mono text-xs text-mist">
                      GitHub milestone not linked yet
                    </p>
                  )}
                </div>

                <div>
                  <p className="text-label">Last updated</p>
                  <p className="mt-2 font-mono text-sm text-bone">
                    {formatDate(milestone.lastUpdated)}
                  </p>
                  <p className="mt-4 text-label">Associated tasks</p>
                  <p className="mt-2 font-mono text-sm text-mist">
                    {milestone.closedCount} completed · {milestone.openCount} open
                    {milestone.blockedCount > 0
                      ? ` · ${milestone.blockedCount} blocked`
                      : ""}
                  </p>
                </div>
              </div>
            </article>
          );
        })}
      </div>
    </div>
  );
}
