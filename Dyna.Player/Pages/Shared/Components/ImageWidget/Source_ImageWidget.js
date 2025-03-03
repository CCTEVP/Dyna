function renderImageWidgets() {
    console.log("Rendering ImageWidgets");
    const imageWidgets = document.querySelectorAll('[id^="image-widget-"]');

    imageWidgets.forEach(imageWidget => {
        // Get the CSS styles from the data-css attribute
        const cssStyles = imageWidget.getAttribute('data-css');

        // Set the styles directly as inline styles
        imageWidget.style.cssText = cssStyles;

        // Create the image element within the imageWidget
        const img = document.createElement('img');
        img.src = imageWidget.getAttribute('data-src');
        img.alt = imageWidget.getAttribute('data-alt');
        imageWidget.appendChild(img);
    });
}