function renderVideoWidgets() {
    console.log("video")
    const videoWidgets = document.querySelectorAll('[id^="video-widget-"]');

    videoWidgets.forEach(videoWidget => {
        const shadow = videoWidget.attachShadow({ mode: 'open' });

        // Create the image element within the Shadow DOM
        const vid = document.createElement('video');
        const src = document.createElement('source');
        src.href = imageWidget.getAttribute('data-src');
        vid.appendChild(src);
        shadow.appendChild(vid);

        // Add CSS styles within the Shadow DOM
        const style = document.createElement('style');
        style.textContent = `/* CSS_CONTENT_PLACEHOLDER */`; // Placeholder for CSS content (DO NOT DELETE)
        shadow.appendChild(style);
    });
}