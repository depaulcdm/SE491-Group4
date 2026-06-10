import { useState } from 'react';
import './App.css';
import SearchTab from './SearchTab';
import SyllablesHelpButton from './SyllablesHelpButton';
import SyllablesHelpOverlay from './SyllablesHelpOverlay';
import WorksheetTab from './WorksheetTab';
import { readStoredLang } from './langStorage';

function App() {
  const [activeTab, setActiveTab] = useState('worksheet');
  const [helpOpen, setHelpOpen] = useState(false);

  return (
    <div className="app-page">
      <SyllablesHelpButton onOpen={() => setHelpOpen(true)} />

      <header className="search-header app-header">
        <h1 className="search-title">B.E.E.S.T.</h1>
      </header>

      {helpOpen && (
        <SyllablesHelpOverlay
          initialLang={readStoredLang()}
          onClose={() => setHelpOpen(false)}
        />
      )}

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
      </nav>

      <main className="tab-panel">
        {activeTab === 'search' ? <SearchTab /> : <WorksheetTab />}
      </main>
    </div>
  );
}

export default App;
