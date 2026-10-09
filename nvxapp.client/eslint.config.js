// @ts-check
// Configurazione ESLint in formato "flat" (ESLint 9 / angular-eslint 22).
// Sostituisce .eslintrc.json, non piu' supportato: stesse regole della configurazione precedente.
const tseslint = require('typescript-eslint');
const angular = require('angular-eslint');

module.exports = tseslint.config(
  {
    ignores: ['projects/**/*', 'www/**/*', 'schema-generator/**/*'],
  },
  {
    files: ['**/*.ts'],
    extends: [
      ...angular.configs.tsRecommended,
    ],
    processor: angular.processInlineTemplates,
    rules: {
      '@angular-eslint/prefer-standalone': 'off',
      // Regole introdotte da angular-eslint 21/22 in contrasto con scelte del progetto:
      // - OnPush: i componenti usano ChangeDetectionStrategy.Eager per mantenere il comportamento pre-v22
      // - inject(): il progetto usa l'iniezione nel costruttore
      '@angular-eslint/prefer-on-push-component-change-detection': 'off',
      '@angular-eslint/prefer-inject': 'off',
      // Da sistemare gradualmente, segnalate come warning:
      // - classi base astratte in pages/_BASE: ngOnInit e' invocato dai componenti che le estendono
      // - page-toolbar: l'alias 'ev_Filter' e' il nome dell'evento usato dalle pagine
      '@angular-eslint/contextual-lifecycle': 'warn',
      '@angular-eslint/no-output-rename': 'warn',
      '@angular-eslint/no-empty-lifecycle-method': 'warn',
      '@angular-eslint/component-class-suffix': [
        'error',
        {
          suffixes: ['Page', 'Component'],
        },
      ],
      '@angular-eslint/component-selector': [
        'error',
        {
          type: 'element',
          prefix: 'app',
          style: 'kebab-case',
        },
      ],
      '@angular-eslint/directive-selector': [
        'error',
        {
          type: 'attribute',
          prefix: 'app',
          style: 'camelCase',
        },
      ],
    },
  },
  {
    files: ['**/*.html'],
    extends: [
      ...angular.configs.templateRecommended,
    ],
    rules: {
      // == / != nei template: il passaggio a === / !== puo' cambiare il comportamento, da valutare caso per caso
      '@angular-eslint/template/eqeqeq': 'warn',
    },
  },
);
