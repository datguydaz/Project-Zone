# PROJECT ZONE — Backup Policy

**Goal:** Recover scripts, scenes, docs, and project config — not duplicate vendor Asset Store packs.

## Primary backup (required)

1. **Commit** meaningful `_ProjectZone`, `docs`, `website`, and config changes.
2. **Push** to GitHub (`origin`) so history exists off-machine.
3. **Tag** milestone gates (example: `m5.5-foundation-docs`) before starting the next milestone.
4. **Branch** for risky experiments; merge only after Play Mode confirmation.

Do **not** keep `*.bak` copies or `Scene_Backup.unity` clones inside `Assets/`.

## Secondary backup (optional local zip)

For a dated snapshot of production code/scenes/docs only:

```powershell
powershell -File tools/Export-ProjectZoneBackup.ps1
```

Writes to gitignored `Backups/` (or `-OutputRoot` elsewhere). Excludes vendor packs, `Library/`, and LFS bulk assets.

## What is not backed up this way

- Full imported Asset Store / Fab packs (use re-download + registry).
- Unity `Library/`, player save files on disk, MemoryCaptures.

## Restore

| Need | How |
| --- | --- |
| Undo recent work | `git checkout` / revert commit (ask before destructive ops) |
| Milestone restore point | `git checkout <tag>` or new branch from tag |
| Offline zip | Extract zip over a clean clone, or copy `_ProjectZone`/docs selectively |
