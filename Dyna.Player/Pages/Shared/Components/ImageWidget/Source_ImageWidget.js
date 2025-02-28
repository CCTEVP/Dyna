function renderImageWidgets() {

    const imageWidgets = document.querySelectorAll('[id^="image-widget-"]');

    imageWidgets.forEach(imageWidget => {
        const shadow = imageWidget.attachShadow({ mode: 'open' });

        // Create the image element within the Shadow DOM
        const img = document.createElement('img');
        img.src = imageWidget.getAttribute('data-src');
        shadow.appendChild(img);

        // Add CSS styles within the Shadow DOM
        const style = document.createElement('style');
        style.textContent = `/* CSS_CONTENT_PLACEHOLDER */`; // Placeholder for CSS content (DO NOT DELETE)
        shadow.appendChild(style);
    });
}