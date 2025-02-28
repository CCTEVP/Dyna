function renderTextWidgets() {
    const textWidgets = document.querySelectorAll('[id^="text-widget-"]');

    textWidgets.forEach(textWidget => {
        const shadow = textWidget.attachShadow({ mode: 'open' });

        // Add CSS styles within the Shadow DOM
        const style = document.createElement('style');
        style.textContent = `/* CSS_CONTENT_PLACEHOLDER */`; // Placeholder for CSS content (DO NOT DELETE)
        shadow.appendChild(style);
    });
}