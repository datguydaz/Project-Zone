export function LoadingState() {
  return (
    <div className="panel px-5 py-10 text-center" role="status" aria-live="polite">
      <p className="text-label">Loading</p>
      <p className="mt-2 text-bone">Fetching development snapshot…</p>
    </div>
  );
}
