/**
 * Theme Helper
 * Manages HTML class changes for dark/light mode
 */
window.themeHelper = (function () {
    'use strict';

    /**
     * Apply dark mode class to HTML element
     * @param {boolean} isDarkMode - Whether dark mode is enabled
     */
    function applyTheme(isDarkMode) {
        const htmlElement = document.documentElement;
        
        if (isDarkMode) {
            htmlElement.classList.add('dark-mode');
            console.log('[ThemeHelper] Dark mode applied');
        } else {
            htmlElement.classList.remove('dark-mode');
            console.log('[ThemeHelper] Light mode applied');
        }
    }

    /**
     * Initialize theme from preferences
     */
    function initializeTheme() {
        // This will be called from Blazor after theme service is loaded
        console.log('[ThemeHelper] Theme helper initialized');
    }

    // Public API
    return {
        applyTheme: applyTheme,
        initializeTheme: initializeTheme
    };
})();

console.log('[ThemeHelper] theme-helper.js loaded successfully');
