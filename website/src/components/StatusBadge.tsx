import type { MilestoneStatus, TaskStatus } from "../lib/types";
import { statusLabel } from "../lib/progress";

const styles: Record<MilestoneStatus | TaskStatus, string> = {
  completed: "border-done/40 bg-done/15 text-done",
  in_progress: "border-olive-bright/40 bg-olive/20 text-olive-bright",
  open: "border-olive-bright/40 bg-olive/20 text-olive-bright",
  not_started: "border-edge-strong bg-panel text-mist",
  blocked: "border-blocked/50 bg-blocked/15 text-amber-bright",
};

export function StatusBadge({ status }: { status: MilestoneStatus | TaskStatus }) {
  return (
    <span
      className={`inline-flex items-center rounded-sm border px-2 py-0.5 font-mono text-[0.65rem] tracking-[0.12em] uppercase ${styles[status]}`}
    >
      {statusLabel(status)}
    </span>
  );
}
