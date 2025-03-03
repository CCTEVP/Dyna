function renderCountdownWidgets() {
    console.log("Rendering CountdownWidgets");
    const countdownWidgets = document.querySelectorAll('[id^="countdown-widget-"]');

    countdownWidgets.forEach(countdownWidget => {
        // Get the CSS styles from the data-css attribute
        const cssStyles = countdownWidget.getAttribute('data-css');

        // Set the styles directly as inline styles
        countdownWidget.style.cssText = cssStyles;
    });
}