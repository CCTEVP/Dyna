function renderBoxLayouts() {
    console.log("Rendering BoxLayouts");
    const boxLayouts = document.querySelectorAll('[id^="box-layout-"]');

    boxLayouts.forEach(boxLayout => {
        // Get the CSS styles from the data-css attribute
        const cssStyles = boxLayout.getAttribute('data-css');

        // Set the styles directly as inline styles
        boxLayout.style.cssText = cssStyles;
    });
}