export function PronunciationRow({ pronunciation }) {
  if (!pronunciation) return null;

  return (
    <div className="pronunciation-row">
      {pronunciation.split(/\s+/).map((phone) => (
        <span key={phone} className="phone-chip">
          {phone}
        </span>
      ))}
    </div>
  );
}
