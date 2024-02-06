/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: 'class',
  content: [
    "./ApplicationCore/**/*.{cs,razor,html,cshtml}",
    "./Companies/**/*.{cs,razor,html,cshtml}",
    "./Domain/**/*.{cs,razor,html,cshtml}",
    "./DesktopHost/**/*.{cs,razor,html,cshtml}"
  ],
  theme: {
    extend: {
      colors: {
        primary: '#0ea5e9',
        'primary-hover': '#0284c7',
        'primary-disabled': '#bae6fd',
        
        secondary: '#94a3b8',
        'secondary-hover': '#64748b',
        'secondary-disabled': '#cbd5e1',

        'dark-secondary': '#475569',
        'dark-secondary-hover': '#334155',
        'dark-secondary-disabled': '#94a3b8'
      }
    },
  },
  plugins: [],
}
