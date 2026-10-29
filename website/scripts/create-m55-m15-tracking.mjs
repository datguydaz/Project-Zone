#!/usr/bin/env node
/**
 * Creates GitHub labels, milestones M5.5–M15, and tracking issues.
 * Idempotent: skips existing milestones/issues by exact title.
 *
 * Dry-run (default):
 *   node scripts/create-m55-m15-tracking.mjs
 * Execute:
 *   CONFIRM_GITHUB_TRACKING=1 node scripts/create-m55-m15-tracking.mjs --execute
 */

import { execFileSync } from "node:child_process";

const EXECUTE = process.argv.includes("--execute") && process.env.CONFIRM_GITHUB_TRACKING === "1";

const NEW_LABELS = [
  { name: "architecture", color: "4A667F", description: "Architecture / systems design" },
  { name: "performance", color: "B85C4A", description: "Performance / profiling" },
  { name: "environment", color: "3D4634", description: "Environment / world building" },
  { name: "assets", color: "C4A35A", description: "Asset pipeline / vendor packs" },
  { name: "persistence", color: "6B7F4A", description: "Save / load / progression" },
  { name: "audio", color: "5F8F6A", description: "Audio / atmosphere sound" },
  { name: "testing", color: "9AA392", description: "Verification / benchmarks" },
];

const MILESTONES = [
  {
    title: "5.5 — Technical Foundation & Asset Readiness",
    description:
      "Performance-first technical baseline, asset registry, world architecture proposal, AI/persistence readiness. Stop for approval before M6.",
  },
  {
    title: "6 — World Architecture & Environment Pipeline",
    description:
      "Scalable Outskirts environment foundation: settlement, checkpoint, industrial, underground, LOD/culling, NavMesh, persistence.",
  },
  {
    title: "7 — Atmosphere, Lighting & Audio",
    description:
      "Exclusion-zone atmosphere: lighting presets, fog/foliage choice, ambient audio, weather evaluation, playable atmosphere checkpoint.",
  },
  {
    title: "8 — Living World & Dynamic Encounters",
    description:
      "Region-based encounters, human/mutant spawns, AI budgets, activation distances, encounter persistence.",
  },
  {
    title: "9 — Factions, NPCs & Settlements",
    description:
      "Factions, traders, dialogue, reputation, settlement NPC populations, contract/encounter links.",
  },
  {
    title: "10 — Loot, Economy & Equipment Progression",
    description:
      "Loot categories, traders, repair, attachments/armor, economy rules, save-safe inventory progression on HQ FPS.",
  },
  {
    title: "11 — Contracts, Exploration & Anomalies",
    description:
      "Expanded contracts, POIs, anomaly prototypes, hazard counterplay, objective persistence.",
  },
  {
    title: "12 — Combat, Mutants & Survival",
    description:
      "Human/mutant combat variety, feedback, survival systems evaluation, combat stress tests within FPS budgets.",
  },
  {
    title: "13 — World Expansion & Optimization",
    description:
      "Additional locations, connectivity, vegetation/lighting/AI optimization, worst-case profiling.",
  },
  {
    title: "14 — UI/UX, Accessibility & Polish",
    description:
      "Inventory/loot/contracts/map/HUD polish, settings, rebinding, accessibility, usability testing.",
  },
  {
    title: "15 — Vertical Slice & Alpha Readiness",
    description:
      "Feature freeze, critical bugs, full loop validation, performance/memory stress, internal alpha build.",
  },
];

/** @type {Record<string, Array<{ title: string, labels: string[], body: string }>>} */
const ISSUES_BY_MILESTONE = {
  "5.5 — Technical Foundation & Asset Readiness": [
    issue(
      "Audit Unity scenes, packages, and custom systems",
      ["architecture", "documentation", "public"],
      "Inventory PZ scenes, Packages/manifest, Assets/_ProjectZone systems, and vendor frameworks. Deliverable: technical foundation audit. Acceptance: risks documented; existing gameplay preserved.",
    ),
    issue(
      "Define region and scene-loading architecture",
      ["architecture", "world", "public"],
      "Propose additive/region loading for Outskirts and future areas without networking. Acceptance: written architecture proposal approved before M6 implementation.",
    ),
    issue(
      "Establish asset import and folder conventions",
      ["assets", "documentation", "public"],
      "Document import standards, folder conventions, and production vs vendor demo isolation. Acceptance: standards doc approved; no major new imports.",
    ),
    issue(
      "Define lighting, terrain, and weather integration strategy",
      ["environment", "architecture", "public"],
      "Choose single primary approach for lighting/terrain/weather; avoid stacking frameworks. Acceptance: strategy documented with performance considerations.",
    ),
    issue(
      "Set performance targets and profiling procedure",
      ["performance", "testing", "public"],
      "Document 120–150+ FPS target / 100 FPS floor, reference PC, Profile Analyzer / standalone build procedure, optional validation scene. Acceptance: baseline procedure ready; measurements recorded where possible (do not invent FPS).",
    ),
    issue(
      "Verify Emerald AI navigation and multi-region readiness",
      ["ai", "architecture", "public"],
      "Assess NavMesh, Emerald persistence, and cross-region activation risks. Acceptance: AI readiness assessment documented.",
    ),
    issue(
      "Audit Git LFS, save-data versioning, and asset licensing",
      ["persistence", "assets", "documentation", "public"],
      "Confirm LFS coverage for large binaries, save schema versioning for HQ/PZ data, and licensing notes for production use. Acceptance: findings + remediation plan documented.",
    ),
    issue(
      "Document technical decisions and update GitHub roadmap",
      ["documentation", "milestone", "public"],
      "Publish decision log, asset registry seed, and ensure M5.5–M15 tracking is live on GitHub + hub. Acceptance: docs + tracking synchronized; stop for M6 approval.",
    ),
  ],
  "6 — World Architecture & Environment Pipeline": [
    issue("Establish region and scene organization", ["world", "architecture", "public"], "Organize Outskirts production scenes vs vendor demos. Acceptance: clear scene ownership documented and applied."),
    issue("Create or refine the safe settlement", ["world", "environment", "public"], "Production settlement greybox/art using approved rural assets. Acceptance: explorable hub with stash/contracts access preserved."),
    issue("Build an abandoned checkpoint", ["world", "environment", "public"], "Checkpoint location using approved assets. Acceptance: explorable with combat/loot space."),
    issue("Develop an industrial exploration location", ["world", "environment", "public"], "Industrial site from approved industrial packs. Acceptance: explorable; performance evaluated."),
    issue("Integrate an underground exploration area", ["world", "environment", "public"], "Connect underground using Abandoned World / Sewers selectively. Acceptance: enter/exit works; loading strategy documented."),
    issue("Establish modular environment construction standards", ["environment", "assets", "public"], "Modular kit rules, collision, and prefab conventions. Acceptance: standards published and used on core locations."),
    issue("Implement LOD, culling, and collision conventions", ["performance", "environment", "public"], "LOD/culling/collision standards for production props. Acceptance: conventions applied on representative location."),
    issue("Validate NavMesh and region transitions", ["ai", "world", "testing", "public"], "Bake/validate NavMesh; transitions keep AI pathing. Acceptance: Play Mode verified."),
    issue("Verify persistence across regions", ["persistence", "testing", "public"], "Inventory/stash/contracts/wallet survive region loads. Acceptance: Play Mode verified; no data loss."),
    issue("Benchmark representative traversal scenarios", ["performance", "testing", "public"], "Standalone/dev benchmarks for settlement→field traversal. Acceptance: metrics recorded vs targets."),
  ],
  "7 — Atmosphere, Lighting & Audio": [
    issue("Create unified environmental lighting strategy", ["environment", "architecture", "public"], "Single lighting approach for Outskirts. Acceptance: strategy doc + applied preset."),
    issue("Establish overcast daytime preset", ["environment", "public"], "Daytime overcast look. Acceptance: Play Mode visual check; fog does not ruin combat readability."),
    issue("Establish nighttime preset", ["environment", "public"], "Night preset with readable combat. Acceptance: Play Mode check + performance note."),
    issue("Benchmark HAZE against simpler fog alternatives", ["performance", "assets", "testing", "public"], "A/B HAZE vs lightweight fog. Acceptance: measured cost/benefit; choose primary."),
    issue("Select primary foliage workflow", ["assets", "performance", "public"], "One foliage path (e.g. Toby) with density rules. Acceptance: workflow approved; no dual stacks."),
    issue("Implement ambient audio zones", ["audio", "world", "public"], "Indoor/outdoor ambient zones. Acceptance: audible transitions in Play Mode."),
    issue("Establish indoor/outdoor atmosphere transitions", ["environment", "audio", "public"], "Lighting/audio transitions between spaces. Acceptance: Play Mode verified."),
    issue("Evaluate lightweight weather effects", ["environment", "performance", "public"], "Optional light rain/wind only if budget allows. Acceptance: accept/defer/reject with metrics."),
    issue("Create environmental storytelling standards", ["environment", "documentation", "public"], "Props/layout storytelling rules. Acceptance: short standards doc."),
    issue("Atmosphere playable checkpoint + profile", ["testing", "performance", "gameplay", "public"], "Small slice with explore/combat/loot/audio/atmosphere; profile it. Acceptance: Play Mode + metrics recorded."),
  ],
  "8 — Living World & Dynamic Encounters": [
    issue("Design region-based encounter management", ["architecture", "ai", "public"], "Encounter manager design with budgets. Acceptance: design approved."),
    issue("Implement controlled enemy spawning", ["ai", "gameplay", "public"], "Spawn points with caps. Acceptance: Play Mode; no spawn spam."),
    issue("Add human patrol encounters", ["ai", "combat", "public"], "Human patrols using approved assets. Acceptance: Play Mode."),
    issue("Add mutant roaming encounters", ["ai", "combat", "public"], "Mutant roamers with activation distances. Acceptance: Play Mode."),
    issue("Introduce limited dynamic events", ["gameplay", "ai", "public"], "Few scripted/dynamic events. Acceptance: non-disruptive; documented."),
    issue("Establish enemy respawn rules", ["gameplay", "ai", "public"], "Respawn timers/rules. Acceptance: documented + verified."),
    issue("Implement AI activation and deactivation distances", ["performance", "ai", "public"], "Distance-based AI enable. Acceptance: distant AI inactive; profiled."),
    issue("Establish active AI budgets", ["performance", "ai", "public"], "Max concurrent AI budget. Acceptance: budget enforced in tests."),
    issue("Verify encounter persistence", ["persistence", "ai", "public"], "Encounter state across sessions/regions. Acceptance: Play Mode."),
    issue("Profile Emerald AI under representative load", ["performance", "ai", "testing", "public"], "Load test with budgeted AI. Acceptance: CPU frame time recorded."),
  ],
  "9 — Factions, NPCs & Settlements": [
    issue("Define initial factions", ["gameplay", "documentation", "public"], "Faction list and roles. Acceptance: design doc."),
    issue("Establish faction hostility and neutrality", ["gameplay", "ai", "public"], "Relation rules for Emerald/HQ. Acceptance: verified in Play Mode."),
    issue("Implement friendly NPC interactions", ["gameplay", "ui", "public"], "Basic interact prompts. Acceptance: Play Mode."),
    issue("Introduce traders", ["gameplay", "inventory", "public"], "Buy/sell via HQ inventory patterns. Acceptance: Play Mode."),
    issue("Create lightweight dialogue interactions", ["gameplay", "ui", "public"], "Minimal dialogue, not a full RPG dialogue system. Acceptance: Play Mode."),
    issue("Establish reputation rules", ["gameplay", "persistence", "public"], "Rep gain/loss rules. Acceptance: documented + persisted."),
    issue("Integrate NPC schedules where beneficial", ["ai", "performance", "public"], "Only if cheap and useful. Acceptance: accept/defer with rationale."),
    issue("Connect factions to contracts and encounters", ["gameplay", "public"], "Faction-linked contracts/spawns. Acceptance: Play Mode."),
    issue("Persist relationships and reputation", ["persistence", "public"], "Save/load reputation. Acceptance: Play Mode."),
    issue("Profile settlement NPC populations", ["performance", "testing", "public"], "Settlement NPC count budget. Acceptance: metrics recorded."),
  ],
  "10 — Loot, Economy & Equipment Progression": [
    issue("Expand loot categories", ["inventory", "gameplay", "public"], "More loot types on HQ items. Acceptance: Play Mode."),
    issue("Introduce item condition where appropriate", ["inventory", "gameplay", "public"], "Condition only if it strengthens loop. Acceptance: accept/defer."),
    issue("Add trader buying and selling", ["inventory", "gameplay", "public"], "Economy with traders. Acceptance: Play Mode."),
    issue("Establish repair costs", ["inventory", "gameplay", "public"], "Repair pricing rules. Acceptance: documented + verified."),
    issue("Expand weapon attachment progression", ["combat", "inventory", "public"], "Attachments via HQ systems. Acceptance: Play Mode."),
    issue("Introduce armor and protection values", ["combat", "inventory", "public"], "Armor stats integrated with HQ damage. Acceptance: Play Mode."),
    issue("Balance ammunition and consumable availability", ["gameplay", "public"], "Scarcity without busywork. Acceptance: design + spot checks."),
    issue("Establish economy rules", ["gameplay", "documentation", "public"], "Credits sinks/sources. Acceptance: economy doc."),
    issue("Prevent duplication exploits", ["persistence", "bug", "public"], "Save/transfer dupe audit. Acceptance: tests pass."),
    issue("Validate inventory and save compatibility", ["persistence", "testing", "public"], "M4/M5 saves remain valid. Acceptance: Play Mode."),
  ],
  "11 — Contracts, Exploration & Anomalies": [
    issue("Expand contract types", ["gameplay", "public"], "More contract templates on existing board. Acceptance: Play Mode."),
    issue("Add retrieval objectives", ["gameplay", "public"], "Retrieve item contracts. Acceptance: Play Mode."),
    issue("Add elimination objectives", ["gameplay", "combat", "public"], "Elimination contracts. Acceptance: Play Mode."),
    issue("Add investigation objectives", ["gameplay", "public"], "Investigate POI contracts. Acceptance: Play Mode."),
    issue("Add exploration objectives", ["gameplay", "world", "public"], "Reach/discover objectives. Acceptance: Play Mode."),
    issue("Establish discoverable points of interest", ["world", "gameplay", "public"], "POI markers/discoveries. Acceptance: Play Mode."),
    issue("Prototype environmental anomalies", ["gameplay", "environment", "public"], "Readable anomaly prototype. Acceptance: Play Mode; performance noted."),
    issue("Create hazard detection and counterplay", ["gameplay", "public"], "Detect/avoid/mitigate hazards. Acceptance: Play Mode."),
    issue("Establish contract reward balancing", ["gameplay", "public"], "Reward tables. Acceptance: documented."),
    issue("Verify objective persistence", ["persistence", "testing", "public"], "Objectives survive save/death. Acceptance: Play Mode."),
  ],
  "12 — Combat, Mutants & Survival": [
    issue("Refine human enemy combat", ["combat", "ai", "public"], "Human AI tuning. Acceptance: Play Mode."),
    issue("Improve enemy reactions and awareness", ["combat", "ai", "public"], "Perception/reaction polish. Acceptance: Play Mode."),
    issue("Develop distinct mutant behaviors", ["combat", "ai", "public"], "Mutant vs human differentiation. Acceptance: Play Mode."),
    issue("Improve hit reactions and combat feedback", ["combat", "public"], "Feedback without heavy cost. Acceptance: Play Mode + perf note."),
    issue("Balance damage and survivability", ["combat", "gameplay", "public"], "TTK/survivability pass. Acceptance: documented values."),
    issue("Evaluate bleeding and medical treatment", ["gameplay", "public"], "Accept/defer survival mechanic. Acceptance: decision recorded."),
    issue("Evaluate radiation and environmental hazards", ["gameplay", "environment", "public"], "Accept/defer. Acceptance: decision recorded."),
    issue("Evaluate equipment degradation", ["inventory", "gameplay", "public"], "Accept/defer. Acceptance: decision recorded."),
    issue("Optimize animation, physics, and gore effects", ["performance", "assets", "public"], "Budget gore/physics. Acceptance: metrics before/after."),
    issue("Stress-test combat with multiple enemies", ["testing", "combat", "performance", "public"], "Multi-enemy stress. Acceptance: FPS/CPU recorded."),
  ],
  "13 — World Expansion & Optimization": [
    issue("Add additional approved exploration locations", ["world", "environment", "public"], "New locations from approved assets only. Acceptance: Play Mode."),
    issue("Improve region connectivity", ["world", "architecture", "public"], "Traversal links between regions. Acceptance: Play Mode."),
    issue("Expand environmental variety", ["environment", "public"], "Biome/prop variety within style. Acceptance: visual review."),
    issue("Optimize vegetation and terrain", ["performance", "environment", "public"], "Density/LOD pass. Acceptance: metrics."),
    issue("Optimize lighting and shadows", ["performance", "environment", "public"], "Shadow caster budget. Acceptance: metrics."),
    issue("Optimize materials and shaders", ["performance", "assets", "public"], "Material cost pass. Acceptance: metrics."),
    issue("Optimize AI and physics workloads", ["performance", "ai", "public"], "AI/physics budgets. Acceptance: metrics."),
    issue("Reduce loading and memory spikes", ["performance", "persistence", "public"], "Load/memory profiling. Acceptance: spikes documented/reduced."),
    issue("Profile worst-case gameplay scenarios", ["performance", "testing", "public"], "Worst-case suite. Acceptance: report vs targets."),
    issue("Validate the complete playable world", ["testing", "world", "public"], "Full Outskirts traversal. Acceptance: Play Mode sign-off."),
  ],
  "14 — UI/UX, Accessibility & Polish": [
    issue("Refine inventory UX", ["ui", "inventory", "public"], "Inventory flow polish. Acceptance: Play Mode."),
    issue("Improve loot feedback", ["ui", "inventory", "public"], "Clear loot feedback. Acceptance: Play Mode."),
    issue("Improve contract journal", ["ui", "gameplay", "public"], "Contract tracking UI. Acceptance: Play Mode."),
    issue("Develop map and navigation tools", ["ui", "world", "public"], "Map/compass/nav aids. Acceptance: Play Mode."),
    issue("Improve HUD readability", ["ui", "public"], "HUD contrast/clarity. Acceptance: Play Mode."),
    issue("Add or refine graphics settings", ["ui", "performance", "public"], "Quality presets tied to perf targets. Acceptance: settings work."),
    issue("Add input rebinding where needed", ["ui", "accessibility", "public"], "Rebind critical actions. Acceptance: Play Mode."),
    issue("Improve audio settings and mixing", ["audio", "ui", "public"], "Volumes/mix buses. Acceptance: Play Mode."),
    issue("Improve accessibility options", ["accessibility", "ui", "public"], "Subtitles/contrast/etc. as appropriate. Acceptance: documented options."),
    issue("Conduct usability and regression testing", ["testing", "public"], "Usability + regression pass. Acceptance: issues filed; blockers listed."),
  ],
  "15 — Vertical Slice & Alpha Readiness": [
    issue("Freeze major feature development", ["milestone", "documentation", "public"], "Feature freeze declared. Acceptance: freeze note published."),
    issue("Resolve critical bugs", ["bug", "testing", "public"], "P0/P1 bug sweep. Acceptance: no open criticals."),
    issue("Test fresh-game progression", ["testing", "gameplay", "public"], "New game through Outskirts loop. Acceptance: Play Mode."),
    issue("Test inventory and stash persistence", ["testing", "persistence", "public"], "Inventory/stash round-trip. Acceptance: Play Mode."),
    issue("Test death-cache recovery", ["testing", "persistence", "public"], "Death cache claim path. Acceptance: Play Mode."),
    issue("Test contracts and faction progression", ["testing", "gameplay", "public"], "Contracts + rep progression. Acceptance: Play Mode."),
    issue("Test human and mutant combat", ["testing", "combat", "public"], "Both combat types. Acceptance: Play Mode."),
    issue("Test environmental hazards", ["testing", "gameplay", "public"], "Hazards/anomalies if shipped. Acceptance: Play Mode or N/A documented."),
    issue("Run extended gameplay sessions", ["testing", "public"], "Long-session stability. Acceptance: notes; no critical crashes."),
    issue("Run performance and memory stress tests", ["testing", "performance", "public"], "Standalone stress vs 100 FPS floor. Acceptance: report; blockers escalated."),
    issue("Validate save compatibility", ["testing", "persistence", "public"], "Forward-compatible saves. Acceptance: Play Mode."),
    issue("Produce internal alpha build", ["milestone", "testing", "public"], "Internal alpha package. Acceptance: build produced; alpha criteria checklist signed."),
  ],
};

function issue(title, labels, body) {
  const full = [
    body,
    "",
    "## Dependencies",
    "- Preserve M1–M5 gameplay and HQ FPS / Emerald frameworks.",
    "- No networking packages. No major new asset imports without approval.",
    "",
    "## Verification",
    "- Do not close until acceptance criteria are verified (Play Mode / benchmarks as required).",
    "- Never mark complete solely because code was generated.",
    "",
    "## Performance",
    "- Target 120–150+ FPS typical; 100 FPS floor on documented reference config.",
    "- Measure before accepting expensive systems; Editor FPS is not final.",
  ].join("\n");
  return { title, labels: [...new Set(["milestone", "public", ...labels])], body: full };
}

function ghJson(args) {
  const out = execFileSync("gh", args, {
    encoding: "utf8",
    env: { ...process.env, PATH: process.env.PATH },
    maxBuffer: 20 * 1024 * 1024,
  });
  return out ? JSON.parse(out) : null;
}

function gh(args) {
  execFileSync("gh", args, {
    encoding: "utf8",
    stdio: ["ignore", "pipe", "pipe"],
    env: { ...process.env, PATH: process.env.PATH },
  });
}

function ensureGhOnPath() {
  try {
    execFileSync("gh", ["--version"], { encoding: "utf8" });
    return "gh";
  } catch {
    const fallback = "C:\\Users\\Pontu\\AppData\\Local\\gh-cli\\bin\\gh.exe";
    process.env.PATH = `C:\\Users\\Pontu\\AppData\\Local\\gh-cli\\bin;${process.env.PATH || ""}`;
    execFileSync(fallback, ["--version"], { encoding: "utf8" });
    return fallback;
  }
}

async function main() {
  ensureGhOnPath();
  console.log(EXECUTE ? "EXECUTE mode" : "DRY-RUN (set CONFIRM_GITHUB_TRACKING=1 and --execute to apply)");

  const existingLabels = new Set(ghJson(["label", "list", "--limit", "200", "--json", "name"]).map((l) => l.name));
  for (const label of NEW_LABELS) {
    if (existingLabels.has(label.name)) {
      console.log(`label exists: ${label.name}`);
      continue;
    }
    console.log(`label create: ${label.name}`);
    if (EXECUTE) {
      gh(["label", "create", label.name, "--color", label.color, "--description", label.description]);
    }
  }

  const existingMilestones = ghJson(["api", "repos/{owner}/{repo}/milestones?state=all&per_page=100"]);
  const milestoneByTitle = new Map(existingMilestones.map((m) => [m.title, m]));

  for (const ms of MILESTONES) {
    if (milestoneByTitle.has(ms.title)) {
      console.log(`milestone exists: ${ms.title}`);
      continue;
    }
    console.log(`milestone create: ${ms.title}`);
    if (EXECUTE) {
      const created = ghJson([
        "api",
        "-X",
        "POST",
        "repos/{owner}/{repo}/milestones",
        "-f",
        `title=${ms.title}`,
        "-f",
        `description=${ms.description}`,
        "-f",
        "state=open",
      ]);
      milestoneByTitle.set(ms.title, created);
    }
  }

  // Refresh milestones after creates
  const milestones = EXECUTE
    ? ghJson(["api", "repos/{owner}/{repo}/milestones?state=all&per_page=100"])
    : [...existingMilestones, ...MILESTONES.map((m, i) => ({ title: m.title, number: 1000 + i }))];
  const msMap = new Map(milestones.map((m) => [m.title, m]));

  const existingIssues = ghJson([
    "issue",
    "list",
    "--state",
    "all",
    "--limit",
    "500",
    "--json",
    "number,title,milestone",
  ]);
  const issueTitles = new Set(existingIssues.map((i) => i.title));

  let createCount = 0;
  for (const [msTitle, issues] of Object.entries(ISSUES_BY_MILESTONE)) {
    const milestone = msMap.get(msTitle);
    if (!milestone) {
      console.warn(`missing milestone for issues: ${msTitle}`);
      continue;
    }
    for (const item of issues) {
      if (issueTitles.has(item.title)) {
        console.log(`issue exists: ${item.title}`);
        continue;
      }
      createCount += 1;
      console.log(`issue create: [${msTitle}] ${item.title}`);
      if (EXECUTE) {
        const labelArgs = item.labels.flatMap((l) => ["--label", l]);
        gh([
          "issue",
          "create",
          "--title",
          item.title,
          "--body",
          item.body,
          "--milestone",
          msTitle,
          ...labelArgs,
        ]);
        // gentle pacing for secondary rate limits
        await new Promise((r) => setTimeout(r, 350));
      }
    }
  }

  console.log(`Done. New issues planned/created: ${createCount}`);
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
