import { fireEvent, render, screen, within } from '@testing-library/react';
import App from './App';
import { searchCmudict } from './cmudictApi';

jest.mock('./cmudictApi', () => ({
  searchCmudict: jest.fn(),
  generateWorksheet: jest.fn(),
}));

test('renders tabs and worksheet UI by default', () => {
  render(<App />);
  expect(screen.getByRole('heading', { name: /B\.E\.E\.S\.T\./i })).toBeInTheDocument();
  expect(screen.getByRole('button', { name: /word search/i })).toBeInTheDocument();
  expect(screen.getByRole('button', { name: /^worksheet$/i })).toBeInTheDocument();
  expect(screen.getByRole('button', { name: /generate/i })).toBeInTheDocument();
  expect(screen.getByRole('spinbutton', { name: /^random seed$/i })).toBeInTheDocument();
});

test('word search tab shows search form', () => {
  render(<App />);
  fireEvent.click(screen.getByRole('button', { name: /word search/i }));
  expect(screen.getByRole('button', { name: /^search$/i })).toBeInTheDocument();
  expect(screen.getByRole('group', { name: /language/i })).toBeInTheDocument();
});

test('sound help opens and closes without leaving dialog open', () => {
  render(<App />);
  expect(screen.getByRole('button', { name: /sound help/i })).toBeInTheDocument();

  fireEvent.click(screen.getByRole('button', { name: /sound help/i }));
  const dialog = screen.getByRole('dialog', { name: /^sounds$/i });
  expect(within(dialog).getByRole('cell', { name: 'AE' })).toBeInTheDocument();
  expect(within(dialog).getByRole('cell', { name: 'cat' })).toBeInTheDocument();

  fireEvent.click(within(dialog).getByRole('radio', { name: /spanish/i }));
  expect(within(dialog).getByRole('cell', { name: 'A' })).toBeInTheDocument();
  expect(within(dialog).getAllByRole('cell', { name: 'casa' }).length).toBeGreaterThan(0);

  fireEvent.keyDown(document, { key: 'Escape' });
  expect(screen.queryByRole('dialog', { name: /^sounds$/i })).not.toBeInTheDocument();
  expect(screen.getByRole('button', { name: /generate/i })).toBeInTheDocument();
});

test('random seed help toggles explanatory text', () => {
  render(<App />);
  fireEvent.click(screen.getByRole('button', { name: /random seed help/i }));
  expect(screen.getByText(/same seed number with the same filters/i)).toBeInTheDocument();
});

test('simulate user search, display phonetic results successfully', async () => {
  const mockApiResponse = {
    query: 'hello',
    mode: 'prefix',
    results: [
      {
        word: 'HELLO',
        pronunciations: ['HH AH L OW', 'HH EH L OW']
      }
    ]
  };
  searchCmudict.mockResolvedValueOnce(mockApiResponse);
  render(<App />);
  fireEvent.click(screen.getByRole('button', { name: /word search/i }));

  const input = screen.getByLabelText(/search word/i);
  const searchButton = screen.getByRole('button', { name: /^search$/i });

  fireEvent.change(input, { target: { value: 'hello' } });
  fireEvent.click(searchButton);

  const resultWord = await screen.findByText('HELLO');
  expect(resultWord).toBeInTheDocument();

  expect(screen.getAllByText('HH')).toHaveLength(2);
  expect(screen.getByText('AH')).toBeInTheDocument();
  expect(screen.getByText('EH')).toBeInTheDocument();
  expect(screen.getAllByText('L')).toHaveLength(2);
  expect(screen.getAllByText('OW')).toHaveLength(2);

  expect(screen.getByText(/1 result for “hello”/i)).toBeInTheDocument();
});

test('returns no matches without crashing', async () => {
  const mockEmptyResponse = {
    query: 'xyz',
    mode: 'prefix',
    results: []
  };
  searchCmudict.mockResolvedValueOnce(mockEmptyResponse);
  render(<App />);
  fireEvent.click(screen.getByRole('button', { name: /word search/i }));

  const input = screen.getByLabelText(/search word/i);
  const searchButton = screen.getByRole('button', { name: /^search$/i });

  fireEvent.change(input, { target: { value: 'xyz' } });
  fireEvent.click(searchButton);

  const noMatchesMessage = await screen.findByText('No matches.');
  expect(noMatchesMessage).toBeInTheDocument();
});
