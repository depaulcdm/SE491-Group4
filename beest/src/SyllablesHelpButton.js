export default function SyllablesHelpButton({ onOpen }) {
  return (
    <button
      type="button"
      className="syllables-help-trigger secondary-button"
      onClick={onOpen}
      aria-label="Sound help"
    >
      Sounds
    </button>
  );
}
