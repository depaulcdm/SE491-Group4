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
