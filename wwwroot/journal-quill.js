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
            console.log(`[Quill] Initializing editor: ${editorId}`);
            
            // Check if Quill library is loaded
            if (typeof Quill === 'undefined') {
                console.error('[Quill] Quill library is not loaded!');
                return false;
            }

            // Check if editor already exists - reuse it instead of creating new one
            if (editors.has(editorId)) {
                console.warn(`[Quill] Editor "${editorId}" already initialized, reusing existing instance`);
                const quill = editors.get(editorId);
                if (initialContent && initialContent.trim() !== '' && initialContent !== '<p><br></p>') {
                    quill.root.innerHTML = initialContent;
                }
                return true;
            }

            // Verify container exists
            const container = document.getElementById(editorId);
            if (!container) {
                console.error(`[Quill] Editor container not found: ${editorId}`);
                return false;
            }

            // Clear any existing content in the container
            container.innerHTML = '';

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
            if (initialContent && initialContent.trim() !== '' && initialContent !== '<p><br></p>') {
                console.log(`[Quill] Setting initial content for ${editorId}, length: ${initialContent.length}`);
                quill.root.innerHTML = initialContent;
            } else {
                console.log(`[Quill] No initial content for ${editorId}`);
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

            console.log(`[Quill] Editor "${editorId}" initialized successfully`);
            return true;

        } catch (error) {
            console.error(`[Quill] Failed to initialize editor "${editorId}":`, error);
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
                console.error(`[Quill] Editor not found: ${editorId}`);
                return '';
            }
            const html = quill.root.innerHTML;
            console.log(`[Quill] Retrieved HTML from ${editorId}, length: ${html.length}`);
            return html;
        } catch (error) {
            console.error(`[Quill] Failed to get HTML from "${editorId}":`, error);
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
                console.error(`[Quill] Editor not found: ${editorId}`);
                return '';
            }
            return quill.getText();
        } catch (error) {
            console.error(`[Quill] Failed to get text from "${editorId}":`, error);
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
                console.error(`[Quill] Editor not found: ${editorId}`);
                return false;
            }
            
            const content = html || '';
            console.log(`[Quill] Setting HTML in ${editorId}, length: ${content.length}`);
            
            // Clear first then set content to ensure proper rendering
            if (content === '' || content === '<p><br></p>') {
                quill.setText('');
            } else {
                quill.root.innerHTML = content;
            }
            
            return true;
        } catch (error) {
            console.error(`[Quill] Failed to set HTML in "${editorId}":`, error);
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
                console.error(`[Quill] Editor not found: ${editorId}`);
                return false;
            }
            quill.setText('');
            console.log(`[Quill] Cleared ${editorId}`);
            return true;
        } catch (error) {
            console.error(`[Quill] Failed to clear "${editorId}":`, error);
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
                console.error(`[Quill] Editor not found: ${editorId}`);
                return 0;
            }
            return quill.getLength() - 1; // Subtract 1 for the trailing newline
        } catch (error) {
            console.error(`[Quill] Failed to get length from "${editorId}":`, error);
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
                console.error(`[Quill] Editor not found: ${editorId}`);
                return false;
            }
            quill.enable(enabled);
            console.log(`[Quill] Set ${editorId} enabled: ${enabled}`);
            return true;
        } catch (error) {
            console.error(`[Quill] Failed to set enabled state for "${editorId}":`, error);
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
                console.error(`[Quill] Editor not found: ${editorId}`);
                return false;
            }
            quill.focus();
            return true;
        } catch (error) {
            console.error(`[Quill] Failed to focus "${editorId}":`, error);
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
                console.error(`[Quill] Editor not found: ${editorId}`);
                return true;
            }
            const text = quill.getText().trim();
            return text.length === 0;
        } catch (error) {
            console.error(`[Quill] Failed to check if "${editorId}" is empty:`, error);
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
                console.warn(`[Quill] Editor not found: ${editorId}`);
                return false;
            }

            // Remove event listeners
            quill.off('text-change');

            // Clear the container but don't remove it
            const container = document.getElementById(editorId);
            if (container) {
                container.innerHTML = '';
            }

            // Remove from map
            editors.delete(editorId);

            console.log(`[Quill] Editor "${editorId}" disposed successfully`);
            return true;

        } catch (error) {
            console.error(`[Quill] Failed to dispose editor "${editorId}":`, error);
            return false;
        }
    }

    /**
     * Get all active editor IDs
     * @returns {Array<string>} - Array of editor IDs
     */
    function getActiveEditors() {
        const editorIds = Array.from(editors.keys());
        console.log(`[Quill] Active editors: ${editorIds.join(', ')}`);
        return editorIds;
    }

    /**
     * Dispose all editors
     * @returns {boolean} - Success status
     */
    function disposeAll() {
        try {
            const editorIds = Array.from(editors.keys());
            editorIds.forEach(id => dispose(id));
            console.log('[Quill] All editors disposed');
            return true;
        } catch (error) {
            console.error('[Quill] Failed to dispose all editors:', error);
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

// Log when script is loaded
console.log('[Quill] journal-quill.js loaded successfully');
