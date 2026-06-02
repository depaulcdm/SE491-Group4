export default function SyllablesHelpTab() {
  const examples = [
    { pattern: 'CV', meaning: 'Consonant + vowel', english: 'go', spanish: 'si' },
    { pattern: 'CVC', meaning: 'Consonant + vowel + consonant', english: 'cat', spanish: 'sol' },
    { pattern: 'CVV', meaning: 'Consonant + vowel sound + vowel sound', english: 'tie', spanish: 'pie' },
    { pattern: 'CVCV', meaning: 'Two simple syllables', english: 'baby', spanish: 'casa' },
    { pattern: 'CVCC', meaning: 'Consonant + vowel + two consonants', english: 'milk', spanish: '—' }
  ];

  return (
    <section className="panel">
      <h2>Syllables Help</h2>
      <p>
        Use this reference to understand common syllable patterns while creating
        speech therapy worksheets.
      </p>

      <table>
        <thead>
          <tr>
            <th>Pattern</th>
            <th>Meaning</th>
            <th>English Example</th>
            <th>Spanish Example</th>
          </tr>
        </thead>
        <tbody>
          {examples.map((item) => (
            <tr key={item.pattern}>
              <td>{item.pattern}</td>
              <td>{item.meaning}</td>
              <td>{item.english}</td>
              <td>{item.spanish}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </section>
  );
}
