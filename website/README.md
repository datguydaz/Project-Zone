# PROJECT ZONE — Development Hub

Static React + Vite + Tailwind site that tracks PROJECT ZONE milestones, tasks, and development history.

## Stack

- React + TypeScript + Vite
- Tailwind CSS 4
- Static JSON snapshot (`public/data/hub-snapshot.json`)
- GitHub Actions sync + optional GitHub Pages deploy

## Local development

```bash
cd website
npm install
npm run dev
```

Production build:

```bash
npm run build
npm run preview
```

Default Vite `base` is `/Project-Zone/` for GitHub Pages. For local root hosting:

```bash
VITE_BASE_PATH=/ npm run build
```

## Data model

- `data/hub-catalog.json` — local planning catalog (not live verification)
- `public/data/hub-snapshot.json` — site snapshot (`dataAuthority`: `planning_catalog` | `github`)
- Planning progress is labeled unverified. Live progress uses GitHub issue state only.

```bash
npm run catalog:build      # regenerate planning snapshot from catalog
npm run validate:snapshot  # schema + safety checks
npm run tracking:plan      # dry-run remote milestone/issue plan
# GITHUB_TOKEN required for private-repo sync:
npm run sync
```

- Sync writes allowlisted public fields only (never issue bodies)
- On failure, restores prior snapshot bytes, then stamps failure metadata
- Private repos publish only issues labeled `public` by default
- Never embeds tokens in client code or the snapshot

## Publishing (requires your approval)

1. Create GitHub Milestones matching M1–M5 titles and Issues with labels.
2. Push the `website/` + `.github/` files when ready.
3. In the repo: **Settings → Pages → Source = GitHub Actions**.
4. Set repository variable `HUB_DEPLOY_ENABLED=true` only when you want deploys.
5. Understand Pages visibility: a private repository’s GitHub Pages site is **not** publicly shareable on free private repos the same way a public repo is. To share publicly, the repository (or Pages access plan) must allow public Pages.

Do not claim real-time sync. Updates run on push/issue/milestone events and every 6 hours.
