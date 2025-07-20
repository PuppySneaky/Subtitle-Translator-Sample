// Global JavaScript functions for Subtitle Translator

$(document).ready(function () {
    // Initialize tooltips if Bootstrap is available
    if (typeof bootstrap !== 'undefined') {
        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    // File drag and drop functionality
    initializeDragAndDrop();

    // Form validation enhancements
    enhanceFormValidation();

    // Progress tracking
    initializeProgressTracking();
});

// Drag and drop file upload functionality
function initializeDragAndDrop() {
    const fileInput = document.querySelector('input[type="file"]');
    if (!fileInput) return;

    const dropZone = fileInput.closest('.card-body') || fileInput.parentElement;

    // Prevent default drag behaviors
    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        dropZone.addEventListener(eventName, preventDefaults, false);
        document.body.addEventListener(eventName, preventDefaults, false);
    });

    // Highlight drop zone when item is dragged over it
    ['dragenter', 'dragover'].forEach(eventName => {
        dropZone.addEventListener(eventName, highlight, false);
    });

    ['dragleave', 'drop'].forEach(eventName => {
        dropZone.addEventListener(eventName, unhighlight, false);
    });

    // Handle dropped files
    dropZone.addEventListener('drop', handleDrop, false);

    function preventDefaults(e) {
        e.preventDefault();
        e.stopPropagation();
    }

    function highlight(e) {
        dropZone.classList.add('drag-over');
    }

    function unhighlight(e) {
        dropZone.classList.remove('drag-over');
    }

    function handleDrop(e) {
        const dt = e.dataTransfer;
        const files = dt.files;

        if (files.length > 0) {
            fileInput.files = files;
            handleFileSelection(files[0]);
        }
    }
}

// Enhanced form validation
function enhanceFormValidation() {
    // Real-time validation for file inputs
    $('input[type="file"]').on('change', function () {
        const file = this.files[0];
        if (file) {
            handleFileSelection(file);
        }
    });

    // Language selection validation
    $('#targetLanguage').on('change', function () {
        validateLanguageSelection();
    });

    // Form submission validation
    $('form').on('submit', function (e) {
        if (!validateForm(this)) {
            e.preventDefault();
            return false;
        }
    });
}

// Handle file selection and validation
function handleFileSelection(file) {
    const maxSize = 20 * 1024 * 1024; // 20MB
    const allowedExtensions = ['.srt', '.vtt', '.ass', '.ssa', '.sub'];

    // Check file size
    if (file.size > maxSize) {
        showAlert('File size must be less than 20MB', 'danger');
        clearFileInput();
        return false;
    }

    // Check file extension
    const fileName = file.name.toLowerCase();
    const isValidExtension = allowedExtensions.some(ext => fileName.endsWith(ext));

    if (!isValidExtension) {
        showAlert('Please select a valid subtitle file (.srt, .vtt, .ass, .ssa, .sub)', 'danger');
        clearFileInput();
        return false;
    }

    // Show file info
    showFileInfo(file);
    return true;
}

// Show file information
function showFileInfo(file) {
    const fileSize = formatFileSize(file.size);
    const fileInfo = `
        <div class="alert alert-info mt-2" id="fileInfo">
            <i class="fas fa-file-alt me-2"></i>
            <strong>${file.name}</strong> (${fileSize})
        </div>
    `;

    $('#fileInfo').remove();
    $('input[type="file"]').after(fileInfo);
}

// Format file size for display
function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';

    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

// Clear file input
function clearFileInput() {
    $('input[type="file"]').val('');
    $('#fileInfo').remove();
}

// Validate language selection
function validateLanguageSelection() {
    const sourceLanguage = $('#sourceLanguage').val();
    const targetLanguage = $('#targetLanguage').val();

    if (sourceLanguage && targetLanguage && sourceLanguage === targetLanguage) {
        showAlert('Source and target languages cannot be the same', 'warning');
        return false;
    }

    return true;
}

// Validate entire form
function validateForm(form) {
    const fileInput = $(form).find('input[type="file"]')[0];
    const targetLanguage = $(form).find('#targetLanguage').val();

    // Check if file is selected
    if (!fileInput || !fileInput.files || fileInput.files.length === 0) {
        showAlert('Please select a subtitle file', 'danger');
        return false;
    }

    // Check if target language is selected
    if (!targetLanguage) {
        showAlert('Please select a target language', 'danger');
        return false;
    }

    // Validate file
    if (!handleFileSelection(fileInput.files[0])) {
        return false;
    }

    // Validate language selection
    if (!validateLanguageSelection()) {
        return false;
    }

    return true;
}

// Show alert messages
function showAlert(message, type = 'info') {
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert" id="dynamicAlert">
            <i class="fas fa-info-circle me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;

    // Remove existing alerts
    $('#dynamicAlert').remove();

    // Add new alert at the top of the form
    $('form').prepend(alertHtml);

    // Auto-hide after 5 seconds
    setTimeout(() => {
        $('#dynamicAlert').fadeOut();
    }, 5000);
}

// Initialize progress tracking
function initializeProgressTracking() {
    // Check if we're on a page with progress tracking
    if ($('#translationProgress').length) {
        startProgressTracking();
    }
}

// Start progress tracking (for future implementation)
function startProgressTracking() {
    // This would be used with SignalR or polling for real-time progress updates
    console.log('Progress tracking initialized');
}

// Copy text to clipboard utility
function copyToClipboard(text, successMessage = 'Copied to clipboard!') {
    if (navigator.clipboard && window.isSecureContext) {
        // Use modern clipboard API
        navigator.clipboard.writeText(text).then(() => {
            showAlert(successMessage, 'success');
        }).catch(() => {
            fallbackCopyToClipboard(text, successMessage);
        });
    } else {
        // Fallback for older browsers
        fallbackCopyToClipboard(text, successMessage);
    }
}

// Fallback clipboard function
function fallbackCopyToClipboard(text, successMessage) {
    const textArea = document.createElement('textarea');
    textArea.value = text;
    textArea.style.position = 'fixed';
    textArea.style.left = '-999999px';
    textArea.style.top = '-999999px';
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();

    try {
        document.execCommand('copy');
        showAlert(successMessage, 'success');
    } catch (err) {
        showAlert('Failed to copy to clipboard', 'danger');
    }

    document.body.removeChild(textArea);
}

// Utility function to debounce events
function debounce(func, wait, immediate) {
    let timeout;
    return function executedFunction() {
        const context = this;
        const args = arguments;
        const later = function () {
            timeout = null;
            if (!immediate) func.apply(context, args);
        };
        const callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow) func.apply(context, args);
    };
}

// Add loading state to buttons
function setButtonLoading(button, isLoading = true) {
    const $btn = $(button);

    if (isLoading) {
        const originalText = $btn.html();
        $btn.data('original-text', originalText);
        $btn.prop('disabled', true);
        $btn.html('<i class="fas fa-spinner fa-spin me-2"></i>Processing...');
    } else {
        const originalText = $btn.data('original-text');
        $btn.prop('disabled', false);
        $btn.html(originalText || $btn.html());
    }
}

// Initialize custom styling for drag and drop
$(document).ready(function () {
    // Add CSS for drag and drop
    if (!$('#dragDropStyles').length) {
        $('<style id="dragDropStyles">')
            .text(`
                .drag-over {
                    border-color: #28a745 !important;
                    background-color: #d4edda !important;
                    transform: scale(1.02);
                    transition: all 0.3s ease;
                }
                
                .file-upload-area {
                    transition: all 0.3s ease;
                }
                
                .file-upload-area:hover {
                    border-color: #007bff !important;
                    background-color: #f8f9fa !important;
                }
            `)
            .appendTo('head');
    }
});