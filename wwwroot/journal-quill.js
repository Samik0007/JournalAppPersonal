/**
 * Journal Quill Editor Manager
 * Manages multiple Quill editor instances for the journal application
 */
window.journalQuill = (function () {
    'use strict';

    // Store all editor instances
    const editors = new Map();

    /**
     * Initialize a new Quill editor instance
     * @param {string} editorId - The HTML element ID for the editor container
     * @param {string} initialContent - Initial HTML content to load
     * @returns {boolean} - Success status
     */
    function init(editorId, initialContent) {
        try {
            // Check if editor already exists
            if (editors.has(editorId)) {
                console.warn(`Editor "${editorId}" already initialized`);
                return true;
            }

            // Verify container exists
            const container = document.getElementById(editorId);
            if (!container) {
                console.error(`Editor container not found: ${editorId}`);
                return false;
            }

            // Create Quill editor instance
            const quill = new Quill(`#${editorId}`, {
                theme: 'snow',
                placeholder: 'Write your journal entry here...',
                modules: {
                    toolbar: [
                        // Text formatting
                        [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
                        [{ 'font': [] }],
                        [{ 'size': ['small', false, 'large', 'huge'] }],
                        
                        // Text style
                        ['bold', 'italic', 'underline', 'strike'],
                        
                        // Text color and background
                        [{ 'color': [] }, { 'background': [] }],
                        
                        // Lists
                        [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                        [{ 'indent': '-1' }, { 'indent': '+1' }],
                        
                        // Blocks
                        ['blockquote', 'code-block'],
                        
                        // Alignment
                        [{ 'align': [] }],
                        
                        // Links and media
                        ['link', 'image'],
                        
                        // Clear formatting
                        ['clean']
                    ],
                    clipboard: {
                        matchVisual: false // Prevent pasting with formatting
                    }
                }
            });

            // Set initial content if provided
            if (initialContent && initialContent.trim() !== '') {
                quill.root.innerHTML = initialContent;
            }

            // Add text change listener for auto-save functionality (optional)
            quill.on('text-change', function (delta, oldDelta, source) {
                if (source === 'user') {
                    // Trigger custom event for potential auto-save
                    const event = new CustomEvent('quill-content-changed', {
                        detail: {
                            editorId: editorId,
                            content: quill.root.innerHTML,
                            textLength: quill.getLength()
                        }
                    });
                    document.dispatchEvent(event);
                }
            });

            // Store the editor instance
            editors.set(editorId, quill);

            console.log(`Quill editor "${editorId}" initialized successfully`);
            return true;

        } catch (error) {
            console.error(`Failed to initialize editor "${editorId}":`, error);
            return false;
        }
    }

    /**
     * Get HTML content from editor
     * @param {string} editorId - The editor instance ID
     * @returns {string} - HTML content
     */
    function getHtml(editorId) {
        try {
            const quill = editors.get(editorId);
            if (!quill) {
                console.error(`Editor not found: ${editorId}`);
                return '';
            }
            return quill.root.innerHTML;
        } catch (error) {
            console.error(`Failed to get HTML from "${editorId}":`, error);
            return '';
        }
    }

    /**
     * Get plain text content from editor
     * @param {string} editorId - The editor instance ID
     * @returns {string} - Plain text content
     */
    function getText(editorId) {
        try {
            const quill = editors.get(editorId);
            if (!quill) {
                console.error(`Editor not found: ${editorId}`);
                return '';
            }
            return quill.getText();
        } catch (error) {
            console.error(`Failed to get text from "${editorId}":`, error);
            return '';
        }
    }

    /**
     * Set HTML content in editor
     * @param {string} editorId - The editor instance ID
     * @param {string} html - HTML content to set
     * @returns {boolean} - Success status
     */
    function setHtml(editorId, html) {
        try {
            const quill = editors.get(editorId);
            if (!quill) {
                console.error(`Editor not found: ${editorId}`);
                return false;
            }
            quill.root.innerHTML = html || '';
            return true;
        } catch (error) {
            console.error(`Failed to set HTML in "${editorId}":`, error);
            return false;
        }
    }

    /**
     * Clear editor content
     * @param {string} editorId - The editor instance ID
     * @returns {boolean} - Success status
     */
    function clear(editorId) {
        try {
            const quill = editors.get(editorId);
            if (!quill) {
                console.error(`Editor not found: ${editorId}`);
                return false;
            }
            quill.setText('');
            return true;
        } catch (error) {
            console.error(`Failed to clear "${editorId}":`, error);
            return false;
        }
    }

    /**
     * Get character count
     * @param {string} editorId - The editor instance ID
     * @returns {number} - Character count
     */
    function getLength(editorId) {
        try {
            const quill = editors.get(editorId);
            if (!quill) {
                console.error(`Editor not found: ${editorId}`);
                return 0;
            }
            return quill.getLength() - 1; // Subtract 1 for the trailing newline
        } catch (error) {
            console.error(`Failed to get length from "${editorId}":`, error);
            return 0;
        }
    }

    /**
     * Enable or disable editor
     * @param {string} editorId - The editor instance ID
     * @param {boolean} enabled - Enable/disable state
     * @returns {boolean} - Success status
     */
    function setEnabled(editorId, enabled) {
        try {
            const quill = editors.get(editorId);
            if (!quill) {
                console.error(`Editor not found: ${editorId}`);
                return false;
            }
            quill.enable(enabled);
            return true;
        } catch (error) {
            console.error(`Failed to set enabled state for "${editorId}":`, error);
            return false;
        }
    }

    /**
     * Focus the editor
     * @param {string} editorId - The editor instance ID
     * @returns {boolean} - Success status
     */
    function focus(editorId) {
        try {
            const quill = editors.get(editorId);
            if (!quill) {
                console.error(`Editor not found: ${editorId}`);
                return false;
            }
            quill.focus();
            return true;
        } catch (error) {
            console.error(`Failed to focus "${editorId}":`, error);
            return false;
        }
    }

    /**
     * Check if editor is empty
     * @param {string} editorId - The editor instance ID
     * @returns {boolean} - True if empty
     */
    function isEmpty(editorId) {
        try {
            const quill = editors.get(editorId);
            if (!quill) {
                console.error(`Editor not found: ${editorId}`);
                return true;
            }
            const text = quill.getText().trim();
            return text.length === 0;
        } catch (error) {
            console.error(`Failed to check if "${editorId}" is empty:`, error);
            return true;
        }
    }

    /**
     * Dispose/destroy an editor instance
     * @param {string} editorId - The editor instance ID
     * @returns {boolean} - Success status
     */
    function dispose(editorId) {
        try {
            const quill = editors.get(editorId);
            if (!quill) {
                console.warn(`Editor not found: ${editorId}`);
                return false;
            }

            // Remove event listeners
            quill.off('text-change');

            // Remove from map
            editors.delete(editorId);

            console.log(`Editor "${editorId}" disposed successfully`);
            return true;

        } catch (error) {
            console.error(`Failed to dispose editor "${editorId}":`, error);
            return false;
        }
    }

    /**
     * Get all active editor IDs
     * @returns {Array<string>} - Array of editor IDs
     */
    function getActiveEditors() {
        return Array.from(editors.keys());
    }

    /**
     * Dispose all editors
     * @returns {boolean} - Success status
     */
    function disposeAll() {
        try {
            const editorIds = Array.from(editors.keys());
            editorIds.forEach(id => dispose(id));
            console.log('All editors disposed');
            return true;
        } catch (error) {
            console.error('Failed to dispose all editors:', error);
            return false;
        }
    }

    // Public API
    return {
        init: init,
        getHtml: getHtml,
        getText: getText,
        setHtml: setHtml,
        clear: clear,
        getLength: getLength,
        setEnabled: setEnabled,
        focus: focus,
        isEmpty: isEmpty,
        dispose: dispose,
        getActiveEditors: getActiveEditors,
        disposeAll: disposeAll
    };
})();

// Cleanup on page unload
window.addEventListener('beforeunload', function () {
    if (window.journalQuill) {
        window.journalQuill.disposeAll();
    }
});
