import type { ProgressMode } from "../lib/types";
import { progressDisplay, progressSourceLabel } from "../lib/progress";

export function ProgressBar({
  percent,
  mode,
  verified = false,
  label,
}: {
  percent: number | null;
  mode: ProgressMode;
  verified?: boolean;
  label?: string;
}) {
  const tracked = percent !== null && mode !== "untracked";
  const width = tracked ? Math.max(0, Math.min(100, percent)) : 0;

  return (
    <div className="space-y-2">
      <div className="flex items-baseline justify-between gap-3">
        <span className="text-label">{label ?? "Progress"}</span>
        <span className="font-mono text-sm text-bone">
          {progressDisplay(percent, mode)}
          <span className="ml-2 text-mist">{progressSourceLabel(mode, verified)}</span>
        </span>
      </div>
      <div
        className="h-1.5 overflow-hidden rounded-full bg-edge"
        role="progressbar"
        aria-valuemin={0}
        aria-valuemax={100}
        aria-valuenow={tracked ? width : undefined}
        aria-valuetext={`${progressDisplay(percent, mode)} — ${progressSourceLabel(mode, verified)}`}
      >
        <div
          className={`h-full rounded-full transition-[width] duration-500 ${
            verified
              ? "bg-gradient-to-r from-olive to-olive-bright"
              : tracked
                ? "bg-gradient-to-r from-amber/70 to-amber"
                : "bg-edge-strong"
          }`}
          style={{ width: tracked ? `${width}%` : "100%", opacity: tracked ? 1 : 0.35 }}
        />
      </div>
    </div>
  );
}
