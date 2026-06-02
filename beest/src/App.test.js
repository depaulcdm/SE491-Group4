import { fireEvent, render, screen } from '@testing-library/react';
import App from './App';

test('renders tabs and worksheet UI by default', () => {
  render(<App />);
  expect(screen.getByRole('heading', { name: /B\.E\.E\.S\.T\./i })).toBeInTheDocument();
  expect(screen.getByRole('button', { name: /word search/i })).toBeInTheDocument();
  expect(screen.getByRole('button', { name: /^worksheet$/i })).toBeInTheDocument();
  expect(screen.getByRole('button', { name: /generate/i })).toBeInTheDocument();
  expect(screen.getByLabelText(/random seed/i)).toBeInTheDocument();
});

test('word search tab shows search form', () => {
  render(<App />);
  fireEvent.click(screen.getByRole('button', { name: /word search/i }));
  expect(screen.getByRole('button', { name: /^search$/i })).toBeInTheDocument();
  expect(screen.getByRole('group', { name: /language/i })).toBeInTheDocument();
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

  // Get input field and search button
  const input = screen.getByLabelText(/search word/i); 
  const searchButton = screen.getByRole('button', { name: /search/i });

  // Simulate user typing 'hello' and search button
  fireEvent.change(input, { target: { value: 'hello' } });
  fireEvent.click(searchButton);

  // Wait for elements to appear
  const resultWord = await screen.findByText('HELLO');
  expect(resultWord).toBeInTheDocument();

  // Verify that the phonemes are split up and rendering 'HH AH L OW' & 'HH EH L OW'
  expect(screen.getAllByText('HH')).toHaveLength(2); // Appears in both pronunciations
  expect(screen.getByText('AH')).toBeInTheDocument();
  expect(screen.getByText('EH')).toBeInTheDocument();
  expect(screen.getAllByText('L')).toHaveLength(2);
  expect(screen.getAllByText('OW')).toHaveLength(2);

  // Verify metadata status string updates dynamically
  expect(screen.getByText(/1 result for “hello”/i)).toBeInTheDocument();
});


test('returns no matches without crashing', async () => {
  // Mock an empty result dataset from the database
  const mockEmptyResponse = { 
    query: 'xyz', 
    mode: 'prefix', 
    results: [] 
  };
  searchCmudict.mockResolvedValueOnce(mockEmptyResponse);
  render(<App />);

  // Get input field and search button
  const input = screen.getByLabelText(/search word/i);
  const searchButton = screen.getByRole('button', { name: /search/i });

  // Simulate user typing 'xyz' and search button
  fireEvent.change(input, { target: { value: 'xyz' } });
  fireEvent.click(searchButton);

  // Assert status field catches this and displays "No matches"
  const noMatchesMessage = await screen.findByText('No matches.');
  expect(noMatchesMessage).toBeInTheDocument();
});

//test('display error message when API request crashes', async () => {
  // Mock API request fail 
  //searchCmudict.mockResolvedValueOnce(new Error('Internal Server Error'));
  //render(<App />);

  //// Get search box
  //const input = screen.getByLabelText(/search word/i);

  // Simulate errormessage typed in, click search
  //fireEvent.change(input, { target: { value: 'errorword'} });
  //fireEvent.click(screen.getByRole('button', { name: /search/i }));
 
  //const errorMsg = await screen.findByText('Internal Server Error');
  //expect (errorMsg).toBeInTheDocument();
//});
