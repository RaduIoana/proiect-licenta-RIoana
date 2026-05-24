/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: 'class',
  content: [
    "./src/**/*.{html,ts}"
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          50: 'var(--p-primary-50)',    // sky-50
          100: 'var(--p-primary-100)',
          200: 'var(--p-primary-200)',
          300: 'var(--p-primary-300)',
          400: 'var(--p-primary-400)',
          500: 'var(--p-primary-500)',
          600: 'var(--p-primary-600)',
          700: 'var(--p-primary-700)',
          800: 'var(--p-primary-800)',
          900: 'var(--p-primary-900)',
          950: 'var(--p-primary-950)',   // sky-950
        },
        background: 'var(--p-surface-ground)',
        surface: 'var(--p-surface-card)',
        accent: 'var(--p-accent-color)',
        text: 'var(--p-text-color)',
        border: 'var(--p-border-color)',
      },
    },
  },
  plugins: [
    require('tailwindcss-primeui')
  ],
}

