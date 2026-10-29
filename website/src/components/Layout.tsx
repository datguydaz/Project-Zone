import { NavLink, Outlet } from "react-router-dom";
import { useHub } from "../lib/HubContext";
import { formatDateTime } from "../lib/progress";

const links = [
  { to: "/", label: "Dashboard", end: true },
  { to: "/roadmap", label: "Roadmap" },
  { to: "/tasks", label: "Tasks" },
  { to: "/devlog", label: "Dev Log" },
];

export function Layout() {
  const { snapshot } = useHub();
  const repoUrl = snapshot?.repository.url;

  return (
    <div className="min-h-screen">
      <a
        href="#main"
        className="sr-only focus:not-sr-only focus:absolute focus:left-4 focus:top-4 focus:z-50 focus:rounded focus:bg-panel focus:px-3 focus:py-2"
      >
        Skip to content
      </a>

      <header className="sticky top-0 z-40 border-b border-edge/80 bg-charcoal/85 backdrop-blur-md">
        <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-4 px-4 py-4 sm:px-6">
          <div className="min-w-0">
            <p className="text-label">Development Hub</p>
            <h1 className="truncate text-xl font-semibold tracking-[0.08em] text-bone sm:text-2xl">
              PROJECT ZONE
            </h1>
          </div>

          <nav aria-label="Primary" className="flex flex-wrap gap-1">
            {links.map((link) => (
              <NavLink
                key={link.to}
                to={link.to}
                end={link.end}
                className={({ isActive }) =>
                  `rounded-md px-3 py-2 font-mono text-xs tracking-[0.12em] uppercase transition-colors ${
                    isActive
                      ? "bg-olive/25 text-olive-bright"
                      : "text-mist hover:bg-panel hover:text-bone"
                  }`
                }
              >
                {link.label}
              </NavLink>
            ))}
          </nav>
        </div>
      </header>

      <main id="main" className="mx-auto max-w-6xl px-4 py-8 sm:px-6 sm:py-10">
        <Outlet />
      </main>

      <footer className="border-t border-edge/80">
        <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-3 px-4 py-5 text-xs text-mist sm:px-6">
          <p>PROJECT ZONE · Unity 6 open-zone FPS RPG</p>
          <div className="flex flex-wrap items-center gap-4">
            <span className="font-mono">
              Sync {formatDateTime(snapshot?.syncedAt ?? null)}
            </span>
            {repoUrl ? (
              <a
                href={repoUrl}
                className="text-olive-bright hover:text-amber-bright"
                target="_blank"
                rel="noreferrer"
              >
                GitHub repository
              </a>
            ) : null}
          </div>
        </div>
      </footer>
    </div>
  );
}
