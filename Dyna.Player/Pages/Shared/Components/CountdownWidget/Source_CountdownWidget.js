function renderCountdownWidgets() {
    const countdownWidgets = document.querySelectorAll('[id^="countdown-widget-"]');

    countdownWidgets.forEach(countdownWidget => {
        const shadow = countdownWidget.attachShadow({ mode: 'open' });

        // Add CSS styles within the Shadow DOM
        const style = document.createElement('style');
        style.textContent = `/* CSS_CONTENT_PLACEHOLDER */`; // Placeholder for CSS content (DO NOT DELETE)
        shadow.appendChild(style);
    });
}