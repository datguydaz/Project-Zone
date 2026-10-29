export function EmptyState({
  title,
  detail,
}: {
  title: string;
  detail: string;
}) {
  return (
    <div className="panel flex flex-col items-start gap-2 px-5 py-8">
      <p className="text-label">Empty</p>
      <h2 className="text-lg font-medium text-bone">{title}</h2>
      <p className="max-w-xl text-sm text-mist">{detail}</p>
    </div>
  );
}
