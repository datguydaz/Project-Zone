# Development Hub — Deployment Notes

## Architecture

```
website/data/hub-catalog.json  (planning only)
        │
        ├─ build-planning-snapshot.mjs → planning snapshot (dataAuthority=planning_catalog)
        │
GitHub Issues / Milestones
        │
        ▼
GitHub Action (sync-github.mjs + GITHUB_TOKEN)
        │  allowlisted public fields only; no issue bodies
        ▼
website/public/data/hub-snapshot.json  (sanitized static snapshot)
        │
        ▼
Vite build → website/dist
        │
        ▼
GitHub Pages (only if HUB_DEPLOY_ENABLED=true)
```

Visitors never call the GitHub API. They only load the static snapshot.

Failed sync restores the previous snapshot bytes, then stamps `syncStatus=failed` without recalculating progress.

## Visibility limitations

- **Public repository + Pages:** site can be shared with collaborators/players.
- **Private repository:** issue bodies/titles may be sensitive. The sync script excludes labels `private`, `internal`, and `confidential`, and does not publish private-repo issues unless `HUB_PUBLISH_PRIVATE_ISSUES=1`.
- GitHub Pages for private repos often requires a paid plan for public audience access. Confirm current GitHub plan limits before enabling public sharing.
- Enabling Pages can expose the built site contents (including any public snapshot data). Review the snapshot before the first deploy.

## Enable deploy (manual gate)

1. Repo **Settings → Secrets and variables → Actions → Variables**
2. Create `HUB_DEPLOY_ENABLED` = `true`
3. Repo **Settings → Pages → Build and deployment → Source: GitHub Actions**
4. Run **Hub Sync and Deploy** via `workflow_dispatch` or push to `main`

Until the variable is set, Actions still build/sync but do not deploy.
