/** @type {import('tailwindcss').Config} */
export default {
  content: [
    './index.html',
    './src/**/*.{fs,html,js}'
  ],
  theme: {
    extend: {
      transitionProperty: {
        height: 'height'
      }
    }
  },
  plugins: []
}