import type { DataAuthority } from "../lib/types";

export function DataAuthorityBanner({ authority }: { authority: DataAuthority }) {
  if (authority === "github") {
    return (
      <div className="rounded-md border border-done/30 bg-done/10 px-4 py-3 text-sm text-fog">
        Showing <span className="text-done">GitHub-verified</span> milestone and issue
        progress. Completion follows issue state only — never commits alone.
      </div>
    );
  }

  return (
    <div className="rounded-md border border-amber/40 bg-amber/10 px-4 py-3 text-sm text-fog">
      Showing <span className="text-amber-bright">planning catalog</span> data. Percentages
      are checklist estimates for roadmap drafting and are{" "}
      <strong className="font-medium text-bone">not</strong> live GitHub-verified progress.
      Create remote milestones/issues and run sync to switch authority to GitHub.
    </div>
  );
}
