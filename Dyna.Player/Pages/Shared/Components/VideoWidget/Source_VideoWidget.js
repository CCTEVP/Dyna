function renderVideoWidgets() {
    console.log("Rendering VideoWidgets");
    const videoWidgets = document.querySelectorAll('[id^="image-widget-"]');

    videoWidgets.forEach(videoWidget => {
        // Get the CSS styles from the data-css attribute
        const cssStyles = videoWidget.getAttribute('data-css');

        // Set the styles directly as inline styles
        videoWidget.style.cssText = cssStyles;

        // Create the image element within the imageWidget
        const vid = document.createElement('video');
        const src = document.createElement('source');
        src.href = videoWidget.getAttribute('data-src');
        vid.appendChild(src);
        videoWidget.appendChild(vid);
    });
}