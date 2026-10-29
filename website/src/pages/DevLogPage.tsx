import { ErrorState } from "../components/ErrorState";
import { EmptyState } from "../components/EmptyState";
import { LoadingState } from "../components/LoadingState";
import { useHub } from "../lib/HubContext";
import { formatDate } from "../lib/progress";

export function DevLogPage() {
  const { snapshot, loading, error, reload } = useHub();

  if (loading) return <LoadingState />;
  if (error || !snapshot) {
    return <ErrorState message={error ?? "Snapshot missing."} onRetry={reload} />;
  }

  const entries = snapshot.devlog
    .filter((entry) => entry.public)
    .slice()
    .sort((a, b) => b.date.localeCompare(a.date));

  const upcoming = snapshot.milestones.filter(
    (milestone) =>
      milestone.status === "in_progress" || milestone.status === "not_started",
  );

  return (
    <div className="space-y-8 fade-in">
      <header className="space-y-3">
        <p className="text-label">Development log</p>
        <h2 className="text-3xl font-semibold text-bone">Changelog</h2>
        <p className="max-w-2xl text-sm text-mist">
          Structured notes for completed milestones, features, fixes, and technical
          decisions. Entries are curated — nothing fabricated is published here.
        </p>
      </header>

      {entries.length === 0 ? (
        <EmptyState
          title="No public log entries"
          detail="Add entries under website/public/data/hub-snapshot.json or docs/devlog and include them in the snapshot."
        />
      ) : (
        <ol className="relative space-y-5 border-l border-edge pl-6">
          {entries.map((entry) => (
            <li key={entry.id} className="panel relative p-5">
              <span className="absolute -left-[1.9rem] top-6 h-3 w-3 rounded-full border border-olive-bright bg-olive" />
              <div className="flex flex-wrap items-center gap-3">
                <time className="font-mono text-xs text-mist" dateTime={entry.date}>
                  {formatDate(entry.date)}
                </time>
                <span className="rounded border border-edge px-2 py-0.5 font-mono text-[0.65rem] tracking-wider text-olive-bright uppercase">
                  {entry.kind}
                </span>
              </div>
              <h3 className="mt-3 text-xl text-bone">{entry.title}</h3>
              <p className="mt-2 whitespace-pre-wrap text-sm leading-relaxed text-mist">
                {entry.body}
              </p>
              {entry.screenshots.length > 0 ? (
                <div className="mt-4 grid gap-3 sm:grid-cols-2">
                  {entry.screenshots.map((src) => (
                    <img
                      key={src}
                      src={src}
                      alt=""
                      className="rounded border border-edge object-cover"
                    />
                  ))}
                </div>
              ) : null}
            </li>
          ))}
        </ol>
      )}

      <section className="panel p-5">
        <p className="text-label">Upcoming work</p>
        <ul className="mt-4 space-y-3">
          {upcoming.map((milestone) => (
            <li key={milestone.id} className="flex flex-wrap items-baseline justify-between gap-2">
              <span className="text-bone">
                M{milestone.number}: {milestone.title}
              </span>
              <span className="font-mono text-xs text-mist uppercase tracking-wider">
                {milestone.status.replace("_", " ")}
              </span>
            </li>
          ))}
        </ul>
      </section>

      <section className="panel border-dashed p-5 text-sm text-mist">
        <p className="text-label text-amber">Screenshots / gallery</p>
        <p className="mt-2">
          Optional gallery slot reserved. Add public screenshots under{" "}
          <code className="font-mono text-fog">website/public/gallery/</code> and
          reference them from public devlog entries when ready.
        </p>
      </section>
    </div>
  );
}
