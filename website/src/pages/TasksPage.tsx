import { useMemo, useState } from "react";
import { EmptyState } from "../components/EmptyState";
import { ErrorState } from "../components/ErrorState";
import { LoadingState } from "../components/LoadingState";
import { StatusBadge } from "../components/StatusBadge";
import { useHub } from "../lib/HubContext";
import { formatDate } from "../lib/progress";
import type { TaskStatus } from "../lib/types";

const statusFilters: Array<{ id: "all" | TaskStatus; label: string }> = [
  { id: "all", label: "All" },
  { id: "open", label: "Open" },
  { id: "completed", label: "Completed" },
  { id: "blocked", label: "Blocked" },
];

export function TasksPage() {
  const { snapshot, loading, error, reload } = useHub();
  const [query, setQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState<"all" | TaskStatus>("all");
  const [labelFilter, setLabelFilter] = useState("all");
  const [milestoneFilter, setMilestoneFilter] = useState("all");

  const labels = useMemo(() => {
    if (!snapshot) return [];
    const set = new Set<string>();
    for (const issue of snapshot.issues) {
      for (const label of issue.labels) set.add(label);
    }
    return [...set].sort((a, b) => a.localeCompare(b));
  }, [snapshot]);

  const filtered = useMemo(() => {
    if (!snapshot) return [];
    const q = query.trim().toLowerCase();
    return snapshot.issues.filter((issue) => {
      if (statusFilter !== "all" && issue.status !== statusFilter) return false;
      if (labelFilter !== "all" && !issue.labels.includes(labelFilter)) return false;
      if (milestoneFilter !== "all" && issue.milestoneId !== milestoneFilter) return false;
      if (!q) return true;
      return (
        issue.title.toLowerCase().includes(q) ||
        String(issue.number).includes(q) ||
        issue.labels.some((label) => label.toLowerCase().includes(q))
      );
    });
  }, [snapshot, query, statusFilter, labelFilter, milestoneFilter]);

  if (loading) return <LoadingState />;
  if (error || !snapshot) {
    return <ErrorState message={error ?? "Snapshot missing."} onRetry={reload} />;
  }

  return (
    <div className="space-y-8 fade-in">
      <header className="space-y-3">
        <p className="text-label">Task management</p>
        <h2 className="text-3xl font-semibold text-bone">Development Tasks</h2>
        <p className="max-w-2xl text-sm text-mist">
          GitHub Issues are the source of truth. Completion follows verified issue state,
          not file changes. Private or excluded labels are omitted from the public snapshot.
        </p>
      </header>

      <div className="panel grid gap-4 p-4 sm:grid-cols-2 lg:grid-cols-4">
        <label className="block sm:col-span-2 lg:col-span-1">
          <span className="text-label">Search</span>
          <input
            value={query}
            onChange={(event) => setQuery(event.target.value)}
            placeholder="Title, number, label…"
            className="mt-2 w-full rounded-md border border-edge bg-charcoal px-3 py-2 text-sm text-bone placeholder:text-mist/60"
          />
        </label>

        <label className="block">
          <span className="text-label">Status</span>
          <select
            value={statusFilter}
            onChange={(event) => setStatusFilter(event.target.value as "all" | TaskStatus)}
            className="mt-2 w-full rounded-md border border-edge bg-charcoal px-3 py-2 text-sm text-bone"
          >
            {statusFilters.map((filter) => (
              <option key={filter.id} value={filter.id}>
                {filter.label}
              </option>
            ))}
          </select>
        </label>

        <label className="block">
          <span className="text-label">Label</span>
          <select
            value={labelFilter}
            onChange={(event) => setLabelFilter(event.target.value)}
            className="mt-2 w-full rounded-md border border-edge bg-charcoal px-3 py-2 text-sm text-bone"
          >
            <option value="all">All labels</option>
            {labels.map((label) => (
              <option key={label} value={label}>
                {label}
              </option>
            ))}
          </select>
        </label>

        <label className="block">
          <span className="text-label">Milestone</span>
          <select
            value={milestoneFilter}
            onChange={(event) => setMilestoneFilter(event.target.value)}
            className="mt-2 w-full rounded-md border border-edge bg-charcoal px-3 py-2 text-sm text-bone"
          >
            <option value="all">All milestones</option>
            {snapshot.milestones.map((milestone) => (
              <option key={milestone.id} value={milestone.id}>
                M{milestone.number}: {milestone.title}
              </option>
            ))}
          </select>
        </label>
      </div>

      {snapshot.issues.length === 0 ? (
        <EmptyState
          title="No synchronized tasks yet"
          detail="Create GitHub Issues with milestone and label associations, then run the hub sync workflow. Until then the dashboard uses curated milestone requirements only."
        />
      ) : filtered.length === 0 ? (
        <EmptyState
          title="No matching tasks"
          detail="Adjust search or filters to see synchronized GitHub issues."
        />
      ) : (
        <ul className="space-y-3">
          {filtered.map((issue) => (
            <li key={issue.number} className="panel-raised p-4 sm:p-5">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div className="min-w-0">
                  <a
                    href={issue.htmlUrl}
                    target="_blank"
                    rel="noreferrer"
                    className="text-lg text-bone hover:text-olive-bright"
                  >
                    <span className="font-mono text-mist">#{issue.number}</span>{" "}
                    {issue.title}
                  </a>
                  <p className="mt-2 text-sm text-mist">
                    {issue.milestoneTitle ?? "No milestone"} · Updated{" "}
                    {formatDate(issue.updatedAt)}
                  </p>
                </div>
                <StatusBadge status={issue.status} />
              </div>
              {issue.labels.length > 0 ? (
                <div className="mt-3 flex flex-wrap gap-2">
                  {issue.labels.map((label) => (
                    <span
                      key={label}
                      className="rounded border border-edge px-2 py-0.5 font-mono text-[0.65rem] tracking-wider text-mist uppercase"
                    >
                      {label}
                    </span>
                  ))}
                </div>
              ) : null}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
