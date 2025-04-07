module.exports = {
    extends: [
      'react-app',
      'react-app/jest',
      'plugin:react-hooks/recommended'
    ],
    rules: {
      // Custom rules for your team
      'react/jsx-uses-react': 'error',
      'react/jsx-uses-vars': 'error',
      'no-console': ['warn', { allow: ['warn', 'error'] }],
      'no-unused-vars': 'warn'
    },
    settings: {
      react: {
        version: 'detect'
      }
    }
  };
  