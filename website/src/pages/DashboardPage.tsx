import { Link } from "react-router-dom";
import { DataAuthorityBanner } from "../components/DataAuthorityBanner";
import { ErrorState } from "../components/ErrorState";
import { LoadingState } from "../components/LoadingState";
import { MilestoneCard } from "../components/MilestoneCard";
import { ProgressBar } from "../components/ProgressBar";
import { StatusBadge } from "../components/StatusBadge";
import { SyncIndicator } from "../components/SyncIndicator";
import { useHub } from "../lib/HubContext";
import { formatDate } from "../lib/progress";

export function DashboardPage() {
  const { snapshot, stats, loading, error, reload } = useHub();

  if (loading) return <LoadingState />;
  if (error || !snapshot || !stats) {
    return <ErrorState message={error ?? "Snapshot missing."} onRetry={reload} />;
  }

  const completedMilestones = snapshot.milestones.filter(
    (milestone) => milestone.status === "completed",
  );
  const upcoming = snapshot.milestones.filter(
    (milestone) =>
      milestone.status === "not_started" || milestone.status === "in_progress",
  );
  const recentFeatures = snapshot.activity.filter(
    (item) => item.kind === "feature" || item.kind === "milestone",
  );
  const openIssues = snapshot.issues.filter((issue) => issue.status === "open");
  const completedIssues = snapshot.issues.filter((issue) => issue.status === "completed");
  const blockedIssues = snapshot.issues.filter((issue) => issue.status === "blocked");

  return (
    <div className="space-y-8 fade-in">
      <DataAuthorityBanner authority={snapshot.dataAuthority} />

      <section className="grid gap-6 lg:grid-cols-[1.4fr_1fr]">
        <div className="panel overflow-hidden p-6 sm:p-8">
          <p className="text-label">Project overview</p>
          <h2 className="mt-3 text-3xl font-semibold tracking-wide text-bone sm:text-4xl">
            {snapshot.project.title}
          </h2>
          <p className="mt-2 text-olive-bright">{snapshot.project.tagline}</p>
          <p className="mt-4 max-w-2xl text-sm leading-relaxed text-mist sm:text-base">
            {snapshot.project.description}
          </p>
          <div className="mt-6 flex flex-wrap gap-3 text-xs text-mist">
            <span className="rounded border border-edge px-2 py-1 font-mono">
              {snapshot.project.engine}
            </span>
            <span className="rounded border border-edge px-2 py-1 font-mono">
              Phase: {snapshot.project.phase}
            </span>
            <a
              href={snapshot.repository.url}
              target="_blank"
              rel="noreferrer"
              className="rounded border border-olive/40 px-2 py-1 font-mono text-olive-bright hover:bg-olive/15"
            >
              {snapshot.repository.owner}/{snapshot.repository.name}
            </a>
          </div>
        </div>

        <div className="space-y-4">
          <div className="panel p-5">
            <p className="text-label">Current phase</p>
            <p className="mt-2 text-xl text-bone">{snapshot.project.phase}</p>
            {stats.activeMilestone ? (
              <div className="mt-4 space-y-3">
                <div className="flex items-center justify-between gap-2">
                  <p className="text-sm text-mist">Active milestone</p>
                  <StatusBadge status={stats.activeMilestone.status} />
                </div>
                <Link
                  to={`/roadmap#${stats.activeMilestone.id}`}
                  className="block text-lg text-bone hover:text-olive-bright"
                >
                  {stats.activeMilestone.title}
                </Link>
                <ProgressBar
                  percent={stats.activeMilestone.progressPercent}
                  mode={stats.activeMilestone.progressMode}
                  verified={stats.activeMilestone.progressVerified}
                />
              </div>
            ) : (
              <p className="mt-3 text-sm text-mist">No active milestone.</p>
            )}
          </div>

          <div className="panel p-5">
            <ProgressBar
              label="Overall development progress"
              percent={stats.overallProgressPercent}
              mode={stats.overallProgressMode}
              verified={stats.overallProgressVerified}
            />
            <p className="mt-3 font-mono text-xs text-mist">
              {stats.completedMilestones}/{stats.totalMilestones} milestones completed
            </p>
          </div>
        </div>
      </section>

      <SyncIndicator
        syncedAt={snapshot.syncedAt}
        status={snapshot.syncStatus}
        error={snapshot.syncError}
        authority={snapshot.dataAuthority}
        lastAttemptAt={snapshot.lastSyncAttemptAt}
      />

      <section className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4 fade-in-delay">
        {[
          { label: "Open tasks", value: stats.openTasks, detail: `${openIssues.length} from GitHub` },
          {
            label: "Completed tasks",
            value: stats.completedTasks,
            detail: `${completedIssues.length} closed issues`,
          },
          {
            label: "Blocked tasks",
            value: stats.blockedTasks,
            detail: `${blockedIssues.length} labeled blocked`,
          },
          {
            label: "Completed milestones",
            value: stats.completedMilestones,
            detail: `of ${stats.totalMilestones} planned`,
          },
        ].map((card) => (
          <div key={card.label} className="panel p-4">
            <p className="text-label">{card.label}</p>
            <p className="mt-2 font-mono text-3xl text-bone">{card.value}</p>
            <p className="mt-1 text-xs text-mist">{card.detail}</p>
          </div>
        ))}
      </section>

      {snapshot.issues.length === 0 ? (
        <div className="panel border-amber/30 px-5 py-4 text-sm text-mist">
          No GitHub issues are in this snapshot yet. Task counts stay at zero until
          Issues/Milestones are created and the sync workflow runs. Milestone progress
          currently uses verified requirement checklists from seed data.
        </div>
      ) : null}

      <section className="space-y-4 fade-in-late">
        <div className="flex items-end justify-between gap-3">
          <div>
            <p className="text-label">Roadmap focus</p>
            <h3 className="mt-1 text-xl text-bone">Active & upcoming priorities</h3>
          </div>
          <Link to="/roadmap" className="font-mono text-xs text-olive-bright hover:text-amber-bright">
            Full roadmap
          </Link>
        </div>
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
          {upcoming.slice(0, 3).map((milestone) => (
            <MilestoneCard key={milestone.id} milestone={milestone} compact />
          ))}
        </div>
      </section>

      <section className="grid gap-6 lg:grid-cols-2">
        <div className="panel p-5">
          <p className="text-label">Recently completed features</p>
          <ul className="mt-4 space-y-4">
            {recentFeatures.length === 0 ? (
              <li className="text-sm text-mist">No recorded features yet.</li>
            ) : (
              recentFeatures.slice(0, 5).map((item) => (
                <li key={item.id} className="border-b border-edge/70 pb-3 last:border-0">
                  <p className="text-bone">{item.title}</p>
                  <p className="mt-1 text-sm text-mist">{item.summary}</p>
                  <p className="mt-2 font-mono text-xs text-mist">{formatDate(item.date)}</p>
                </li>
              ))
            )}
          </ul>
        </div>

        <div className="panel p-5">
          <p className="text-label">Latest development activity</p>
          <ul className="mt-4 space-y-4">
            {snapshot.activity.length === 0 ? (
              <li className="text-sm text-mist">No activity entries.</li>
            ) : (
              snapshot.activity.slice(0, 6).map((item) => (
                <li key={item.id} className="flex gap-3">
                  <span className="mt-1 h-2 w-2 shrink-0 rounded-full bg-olive" />
                  <div>
                    <p className="text-bone">{item.title}</p>
                    <p className="mt-1 text-sm text-mist">{item.summary}</p>
                    <p className="mt-1 font-mono text-xs uppercase tracking-wider text-mist">
                      {item.kind} · {formatDate(item.date)}
                    </p>
                  </div>
                </li>
              ))
            )}
          </ul>
        </div>
      </section>

      {completedMilestones.length > 0 ? (
        <section className="space-y-3">
          <p className="text-label">Completed milestones</p>
          <div className="flex flex-wrap gap-2">
            {completedMilestones.map((milestone) => (
              <Link
                key={milestone.id}
                to={`/roadmap#${milestone.id}`}
                className="rounded border border-done/30 bg-done/10 px-3 py-2 text-sm text-done hover:bg-done/20"
              >
                M{milestone.number}: {milestone.title}
              </Link>
            ))}
          </div>
        </section>
      ) : null}
    </div>
  );
}
