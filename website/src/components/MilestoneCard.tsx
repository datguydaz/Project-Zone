import { Link } from "react-router-dom";
import type { HubMilestone } from "../lib/types";
import { formatDate } from "../lib/progress";
import { ProgressBar } from "./ProgressBar";
import { StatusBadge } from "./StatusBadge";

export function MilestoneCard({
  milestone,
  compact = false,
}: {
  milestone: HubMilestone;
  compact?: boolean;
}) {
  return (
    <article className="panel-raised flex h-full flex-col gap-4 p-5 transition-colors hover:border-olive/50">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-label">Milestone {milestone.number}</p>
          <h3 className="mt-1 text-lg font-medium text-bone">
            <Link to={`/roadmap#${milestone.id}`} className="hover:text-olive-bright">
              {milestone.title}
            </Link>
          </h3>
        </div>
        <StatusBadge status={milestone.status} />
      </div>

      {!compact ? (
        <p className="text-sm leading-relaxed text-mist">{milestone.description}</p>
      ) : null}

      <ProgressBar
        percent={milestone.progressPercent}
        mode={milestone.progressMode}
        verified={milestone.progressVerified}
      />

      <div className="mt-auto flex flex-wrap gap-x-4 gap-y-1 font-mono text-xs text-mist">
        <span>{milestone.closedCount} done</span>
        <span>{milestone.openCount} remaining</span>
        {milestone.blockedCount > 0 ? <span>{milestone.blockedCount} blocked</span> : null}
        <span>Updated {formatDate(milestone.lastUpdated)}</span>
      </div>
    </article>
  );
}
