/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        primary:   { DEFAULT: '#4361EE', hover: '#3451D1', light: '#EEF1FD' },
        surface:   { DEFAULT: '#FFFFFF', alt: '#F4F6FA', border: '#E8EBF3' },
        text:      { DEFAULT: '#1A1D2E', muted: '#6B7280', light: '#9CA3AF' },
        success:   '#22C55E',
        warning:   '#F59E0B',
        danger:    '#EF4444',
      },
      fontFamily: {
        sans:    ['"Plus Jakarta Sans"', 'sans-serif'],
        display: ['"DM Serif Display"', 'serif'],
      },
      borderRadius: { DEFAULT: '10px', lg: '14px', xl: '20px' },
      boxShadow: {
        card:  '0 1px 3px rgba(0,0,0,.06), 0 4px 16px rgba(0,0,0,.04)',
        hover: '0 4px 20px rgba(67,97,238,.12)',
        input: '0 0 0 3px rgba(67,97,238,.15)',
      },
    },
  },
  plugins: [],
}
