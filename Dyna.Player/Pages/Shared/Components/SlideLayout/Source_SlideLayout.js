function renderSlideLayouts() {
    console.log("Rendering SlideLayouts");
    const slideLayouts = document.querySelectorAll('[id^="slide-layout-"]');

    slideLayouts.forEach(slideWidget => {
        // Get the CSS styles from the data-css attribute
        const cssStyles = slideWidget.getAttribute('data-css');

        // Set the styles directly as inline styles
        //slideWidget.style.cssText = cssStyles;
    });
}