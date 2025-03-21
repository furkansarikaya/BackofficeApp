// Add this script to your page to diagnose rendering issues
// Place it right before the closing </body> tag in _Layout.cshtml

(function() {
    console.log('Diagnostic script running...');

    // Check if Alpine.js is loaded
    if (typeof Alpine === 'undefined') {
        console.error('Alpine.js is not loaded!');
    } else {
        console.log('Alpine.js is loaded successfully');
    }

    // Check if jQuery is loaded
    if (typeof jQuery === 'undefined') {
        console.error('jQuery is not loaded!');
    } else {
        console.log('jQuery ' + jQuery.fn.jquery + ' is loaded successfully');
    }

    // Check if Bootstrap is loaded
    if (typeof bootstrap === 'undefined') {
        console.error('Bootstrap is not loaded!');
    } else {
        console.log('Bootstrap is loaded successfully');
    }

    // Check main content area
    const mainContent = document.querySelector('main > div.pb-12');
    if (mainContent) {
        console.log('Main content container found');
        console.log('Content:', mainContent.innerHTML.trim().substring(0, 100) + '...');

        if (mainContent.innerHTML.trim() === '') {
            console.error('Main content area is empty!');
        }
    } else {
        console.error('Main content container not found!');
    }

    // Log any CSS issues
    const styleSheets = document.styleSheets;
    console.log(`${styleSheets.length} stylesheets loaded`);

    try {
        for (let i = 0; i < styleSheets.length; i++) {
            const sheet = styleSheets[i];
            console.log(`Stylesheet ${i+1}: ${sheet.href || 'inline'}`);

            try {
                // Try to access the rules to check if the stylesheet loaded properly
                const rules = sheet.cssRules || sheet.rules;
                console.log(`- ${rules.length} CSS rules`);
            } catch (e) {
                // This will happen with cross-origin stylesheets
                console.log(`- Unable to read rules (likely cross-origin)`);
            }
        }
    } catch (e) {
        console.error('Error checking stylesheets:', e);
    }

    // Check for console errors
    console.log('Checking for previous console errors...');
    if (window.consoleErrors && window.consoleErrors.length > 0) {
        console.log('Found previous console errors:', window.consoleErrors);
    } else {
        console.log('No previous console errors detected');
    }
})();

// Capture console errors
(function() {
    window.consoleErrors = [];
    const originalConsoleError = console.error;

    console.error = function() {
        window.consoleErrors.push(Array.from(arguments));
        originalConsoleError.apply(console, arguments);
    };
})();