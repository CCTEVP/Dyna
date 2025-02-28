function renderSlideLayouts() {

    const slideLayouts = document.querySelectorAll('[id^="image-widget-"]');

    slideLayouts.forEach(slideLayout => {
        const shadow = slideLayout.attachShadow({ mode: 'open' });

        // Add CSS styles within the Shadow DOM
        const style = document.createElement('style');
        style.textContent = `/* CSS_CONTENT_PLACEHOLDER */`; // Placeholder for CSS content (DO NOT DELETE)
        shadow.appendChild(style);
    });
}