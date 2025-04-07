module.exports = {
    extends: ['./.eslintrc.js'],
    plugins: ['jest', 'testing-library'],
    rules: {
      'testing-library/await-async-queries': 'error',
      'testing-library/no-await-sync-queries': 'error',
      'testing-library/no-unnecessary-act': 'warn',
      'jest/no-disabled-tests': 'warn',
      'jest/no-focused-tests': 'error',
      'jest/valid-expect': 'error',
    },
  };