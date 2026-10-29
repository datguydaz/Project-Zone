#!/usr/bin/env node
import { readFile, writeFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const catalogPath = path.join(path.dirname(fileURLToPath(import.meta.url)), "..", "data", "hub-catalog.json");

const NEW = [
  {
    id: "m5_5-technical-foundation",
    number: 5.5,
    title: "Technical Foundation & Asset Readiness",
    description:
      "Performance-first technical baseline, asset registry, world architecture proposal, AI and persistence readiness. Approval gate before M6.",
    status: "in_progress",
    scope: [
      "Scene/package/custom systems audit",
      "Region/scene-loading architecture",
      "Asset import conventions",
      "Lighting/terrain/weather strategy",
      "Performance targets and profiling procedure",
      "Emerald multi-region readiness",
      "Git LFS / save versioning / licensing",
      "Technical decisions and roadmap sync",
    ],
  },
  {
    id: "m6-world-architecture",
    number: 6,
    title: "World Architecture & Environment Pipeline",
    description: "Scalable Outskirts environment foundation using approved assets.",
    status: "not_started",
    scope: [
      "Region/scene organization",
      "Settlement / checkpoint / industrial / underground",
      "Modular standards, LOD/culling",
      "NavMesh and transitions",
      "Persistence across regions",
      "Traversal benchmarks",
    ],
  },
  {
    id: "m7-atmosphere-lighting-audio",
    number: 7,
    title: "Atmosphere, Lighting & Audio",
    description: "Exclusion-zone atmosphere with performance-disciplined fog/foliage/audio.",
    status: "not_started",
    scope: [
      "Lighting presets",
      "HAZE vs simple fog",
      "Foliage workflow",
      "Ambient audio",
      "Atmosphere checkpoint",
    ],
  },
  {
    id: "m8-living-world-encounters",
    number: 8,
    title: "Living World & Dynamic Encounters",
    description: "Region-based encounters with AI budgets and activation distances.",
    status: "not_started",
    scope: ["Encounter management", "Human/mutant spawns", "AI budgets", "Persistence", "Load profiling"],
  },
  {
    id: "m9-factions-npcs-settlements",
    number: 9,
    title: "Factions, NPCs & Settlements",
    description: "Factions, traders, light dialogue, reputation, settlement performance.",
    status: "not_started",
    scope: ["Factions", "Traders", "Dialogue", "Reputation", "Settlement NPC budgets"],
  },
  {
    id: "m10-loot-economy-equipment",
    number: 10,
    title: "Loot, Economy & Equipment Progression",
    description: "Meaningful scavenging and equipment progression on HQ FPS systems.",
    status: "not_started",
    scope: ["Loot", "Traders", "Repair", "Attachments/armor", "Economy", "Anti-dupe"],
  },
  {
    id: "m11-contracts-exploration-anomalies",
    number: 11,
    title: "Contracts, Exploration & Anomalies",
    description: "Expanded contracts, POIs, anomaly prototypes.",
    status: "not_started",
    scope: ["Contract types", "POIs", "Anomalies", "Hazards", "Objective persistence"],
  },
  {
    id: "m12-combat-mutants-survival",
    number: 12,
    title: "Combat, Mutants & Survival",
    description: "Varied combat and carefully scoped survival systems.",
    status: "not_started",
    scope: ["Human/mutant combat", "Feedback", "Survival evaluation", "Stress tests"],
  },
  {
    id: "m13-world-expansion-optimization",
    number: 13,
    title: "World Expansion & Optimization",
    description: "Expand proven content while preserving high FPS.",
    status: "not_started",
    scope: ["New locations", "Optimization passes", "Worst-case profiling"],
  },
  {
    id: "m14-uiux-accessibility-polish",
    number: 14,
    title: "UI/UX, Accessibility & Polish",
    description: "Readable UI, settings, accessibility, usability.",
    status: "not_started",
    scope: ["Inventory/loot/contracts/map/HUD", "Settings", "Accessibility", "Usability"],
  },
  {
    id: "m15-vertical-slice-alpha",
    number: 15,
    title: "Vertical Slice & Alpha Readiness",
    description: "Stable internal alpha with full Outskirts loop and performance acceptance.",
    status: "not_started",
    scope: ["Feature freeze", "Critical bugs", "Full loop tests", "Perf/memory stress", "Alpha build"],
  },
];

function reqsFromScope(idPrefix, scope) {
  return scope.map((title, index) => ({
    id: `${idPrefix}-r${index + 1}`,
    title,
    completed: false,
    verification: "planned",
  }));
}

const catalog = JSON.parse(await readFile(catalogPath, "utf8"));
catalog.updatedAt = new Date().toISOString();
catalog.project.phase = "Technical Foundation & Asset Readiness (M5.5)";
catalog.project.description =
  "PROJECT ZONE is an independent Unity 6 URP open-zone FPS RPG. Explore a dangerous exclusion zone, fight hostiles, scavenge gear, complete contracts, and return to safety. Performance-first: 120–150+ FPS typical, 100 FPS floor on reference hardware. Built with HQ FPS Weapons 2.0 and Emerald AI 2026, bridged by Project Zone adapters.";

// Mark M5 completed in catalog
const m5 = catalog.milestones.find((m) => m.id === "m5-death-contracts");
if (m5) {
  m5.status = "completed";
  m5.blockers = [];
  m5.requirements = (m5.requirements || []).map((r) => ({
    ...r,
    completed: true,
    verification: "playtest_confirmed",
  }));
}

const existingIds = new Set(catalog.milestones.map((m) => m.id));
for (const milestone of NEW) {
  if (existingIds.has(milestone.id)) continue;
  catalog.milestones.push({
    ...milestone,
    requirements: reqsFromScope(milestone.id, milestone.scope),
    blockers: milestone.id === "m5_5-technical-foundation" ? [] : ["Waiting for previous milestone approval"],
  });
}

catalog.activity = [
  {
    id: "act-m55-roadmap",
    kind: "milestone",
    title: "Roadmap M5.5–M15 established",
    summary:
      "GitHub and Development Hub tracking extended through Milestone 15. M5.5 technical foundation audit and asset registry seeded. Performance doctrine documented. M6+ gated on approval.",
    date: new Date().toISOString(),
    milestoneId: "m5_5-technical-foundation",
    url: "https://github.com/datguydaz/Project-Zone/milestones",
    public: true,
  },
  {
    id: "act-m5-complete",
    kind: "milestone",
    title: "Milestone 5 completed — Death & Contracts",
    summary: "Play Mode confirmed hybrid death, caches, wallet, contracts, and expedition loop. Issues #27–#32 closed.",
    date: "2026-10-29T16:20:00.000Z",
    milestoneId: "m5-death-contracts",
    url: "https://github.com/datguydaz/Project-Zone/milestone/5",
    public: true,
  },
  ...(catalog.activity || []).filter((a) => a.id !== "act-m55-roadmap" && a.id !== "act-m5-complete"),
];

await writeFile(catalogPath, `${JSON.stringify(catalog, null, 2)}\n`, "utf8");
console.log(`Patched ${catalogPath} — milestones: ${catalog.milestones.length}`);
