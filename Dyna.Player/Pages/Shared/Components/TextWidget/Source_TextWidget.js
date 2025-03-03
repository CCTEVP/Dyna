function renderTextWidgets() {
    console.log("Rendering TextWidgets");
    const textWidgets = document.querySelectorAll('[id^="text-widget-"]');

    textWidgets.forEach(textWidget => {
        // Get the CSS styles from the data-css attribute
        const cssStyles = textWidget.getAttribute('data-css');

        // Set the styles directly as inline styles
        textWidget.style.cssText = cssStyles;
    });
}