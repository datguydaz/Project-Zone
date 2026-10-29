import {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { loadSnapshot } from "./loadSnapshot";
import { deriveStats } from "./progress";
import type { HubSnapshot, HubStats } from "./types";

interface HubContextValue {
  snapshot: HubSnapshot | null;
  stats: HubStats | null;
  loading: boolean;
  error: string | null;
  reload: () => void;
}

const HubContext = createContext<HubContextValue | null>(null);

export function HubProvider({ children }: { children: ReactNode }) {
  const [snapshot, setSnapshot] = useState<HubSnapshot | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [reloadToken, setReloadToken] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    setLoading(true);
    setError(null);

    loadSnapshot(controller.signal)
      .then((data) => {
        setSnapshot(data);
        setLoading(false);
      })
      .catch((err: unknown) => {
        if (controller.signal.aborted) return;
        setSnapshot(null);
        setError(err instanceof Error ? err.message : "Failed to load hub data.");
        setLoading(false);
      });

    return () => controller.abort();
  }, [reloadToken]);

  const value = useMemo<HubContextValue>(
    () => ({
      snapshot,
      stats: snapshot ? deriveStats(snapshot) : null,
      loading,
      error,
      reload: () => setReloadToken((token) => token + 1),
    }),
    [snapshot, loading, error],
  );

  return <HubContext.Provider value={value}>{children}</HubContext.Provider>;
}

export function useHub(): HubContextValue {
  const context = useContext(HubContext);
  if (!context) {
    throw new Error("useHub must be used within HubProvider");
  }
  return context;
}
