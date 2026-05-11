import { render, screen } from '@testing-library/react';
import App from './App';

test('renders word search UI', () => {
  render(<App />);
  expect(screen.getByRole('heading', { name: /B\.E\.E\.S\.T\./i })).toBeInTheDocument();
  expect(screen.getByRole('button', { name: /search/i })).toBeInTheDocument();
  expect(screen.getByRole('group', { name: /language/i })).toBeInTheDocument();
});
