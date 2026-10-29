# Known Issues & Technical Debt

| ID | Severity | Area | Issue | Mitigation |
| --- | --- | --- | --- | --- |
| TD-01 | High | Repo | Very large imported asset tree; LFS coverage incomplete for some binaries | LFS audit (M5.5); no new major imports |
| TD-02 | High | Perf | No measured FPS baseline yet | Follow `docs/performance/BASELINE.md` |
| TD-03 | Medium | AI | Female Zombie not on Emerald Animation Profile | Greybox OK; profile later |
| TD-04 | Medium | Namespaces | ALP `LevelManager` vs Polymind | Fully qualify Polymind |
| TD-05 | Medium | World | Underground is stub only | M6 additive/real load |
| TD-06 | Medium | Assets | Many fantasy/gothic packs imported | Marked Excluded in registry; do not use |
| TD-07 | Low | Vendor | Toby MirrorReflection Unity 6 API | Patched `GetEntityId` |
| TD-08 | Low | Tracking | GitHub M4 milestone counters may look stale while issues are closed | Trust issue state + matrix; do not reopen M4 |
| TD-11 | Medium | Verification | Agents must not re-complete M1–M5 from code presence | Use `docs/verification/M1-M5_VERIFICATION_MATRIX.md` |
| TD-12 | Medium | Combat | Melee checklist after Female Zombie greybox | Optional Outskirts Play Mode re-check; keep M2 closed unless regression |
| TD-09 | Medium | Content | Vendor demo lighting settings litter Assets root | Keep out of production scenes |
| TD-10 | Medium | Persistence | PZ saves lack schemaVersion | Add before economy expansion |
