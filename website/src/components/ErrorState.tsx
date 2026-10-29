export function ErrorState({
  message,
  onRetry,
}: {
  message: string;
  onRetry?: () => void;
}) {
  return (
    <div className="panel border-danger/40 px-5 py-8" role="alert">
      <p className="text-label text-danger">Error</p>
      <h2 className="mt-2 text-lg font-medium text-bone">Hub data unavailable</h2>
      <p className="mt-2 max-w-xl text-sm text-mist">{message}</p>
      {onRetry ? (
        <button
          type="button"
          onClick={onRetry}
          className="mt-4 rounded-md border border-edge-strong bg-panel-raised px-3 py-2 font-mono text-xs tracking-wider text-bone uppercase hover:border-olive"
        >
          Retry
        </button>
      ) : null}
    </div>
  );
}
