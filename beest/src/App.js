import { useState } from 'react';
import './App.css';
import SearchTab from './SearchTab';
import WorksheetTab from './WorksheetTab';
import SyllablesHelpTab from './SyllablesHelpTab';

function App() {
  const [activeTab, setActiveTab] = useState('worksheet');

  return (
    <div className="app-page">
      <header className="search-header">
        <h1 className="search-title">B.E.E.S.T.</h1>
      </header>

      <nav className="tab-bar" aria-label="Main navigation">
        <button
          type="button"
          className={activeTab === 'worksheet' ? 'tab active' : 'tab'}
          onClick={() => setActiveTab('worksheet')}
        >
          Worksheet
        </button>
        <button
          type="button"
          className={activeTab === 'search' ? 'tab active' : 'tab'}
          onClick={() => setActiveTab('search')}
        >
          Word Search
        </button>
            <button
  type="button"
  className={activeTab === 'syllables' ? 'tab active' : 'tab'}
  onClick={() => setActiveTab('syllables')}
>
  Syllables Help
</button>
      </nav>

      <main className="tab-panel">
  {activeTab === 'search' ? (
    <SearchTab />
  ) : activeTab === 'syllables' ? (
    <SyllablesHelpTab />
  ) : (
    <WorksheetTab />
  )}
</main>
    </div>
  );
}

export default App;
