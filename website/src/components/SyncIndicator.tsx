import { formatDateTime } from "../lib/progress";
import type { DataAuthority, SyncStatus } from "../lib/types";

export function SyncIndicator({
  syncedAt,
  status,
  error,
  authority,
  lastAttemptAt,
}: {
  syncedAt: string | null;
  status: SyncStatus;
  error: string | null;
  authority: DataAuthority;
  lastAttemptAt?: string | null;
}) {
  const tone =
    status === "ok"
      ? "text-done"
      : status === "failed"
        ? "text-danger"
        : status === "seed"
          ? "text-amber"
          : "text-mist";

  const label =
    status === "ok"
      ? "Synced from GitHub"
      : status === "failed"
        ? "Sync failed — previous snapshot preserved"
        : status === "seed"
          ? "Planning catalog (not live GitHub verification)"
          : "Partial sync";

  return (
    <div className="panel px-4 py-3">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <p className="text-label">Last successful synchronization</p>
          <p className="mt-1 font-mono text-sm text-bone">{formatDateTime(syncedAt)}</p>
        </div>
        <p className={`font-mono text-xs tracking-wide ${tone}`}>{label}</p>
      </div>
      <p className="mt-2 text-xs text-mist">
        Data authority:{" "}
        <span className="text-fog">
          {authority === "github" ? "GitHub Issues/Milestones" : "Local planning catalog"}
        </span>
        {lastAttemptAt ? ` · Last attempt ${formatDateTime(lastAttemptAt)}` : null}
      </p>
      {error ? <p className="mt-2 text-sm text-danger/90">{error}</p> : null}
      <p className="mt-2 text-xs text-mist">
        Updates are periodic via GitHub Actions, not real-time. Planning percentages are
        never shown as live verified progress.
      </p>
    </div>
  );
}
