// Execute when loaded in Broadsign Player
function BroadSignPlay() {
    const creative = document.getElementById("outcome");
    creative.play();
    return true;
}
// Execute when DOM is ready.
$(() => {
    if (typeof BroadsignObject == 'undefined') {
        //BroadSignPlay();
    }
    setTimeout(() => {
        const widgetsUsed = { imageWidgets: (typeof renderImageWidgets === 'function'), videoWidgets: (typeof renderVideoWidgets === 'function'), countdownWidgets: (typeof renderCountdownWidgets === 'function') }
        const widgetsPresent = Object.values(widgetsUsed).some(value => value === true);
        if (widgetsPresent) {
            console.log("Rendering widgets:")
            if (typeof renderCardWidgets === 'function') { renderCardWidgets(); }
            if (typeof renderTextWidgets === 'function') { renderTextWidgets(); }
            if (typeof renderImageWidgets === 'function') { renderImageWidgets(); }
            if (typeof renderVideoWidgets === 'function') { renderVideoWidgets(); }
            if (typeof renderCountdownWidgets === 'function') { renderCountdownWidgets(); }
            // Add other widget rendering functions here
            if (typeof renderBoxLayouts === 'function') { renderBoxLayouts(); }
            if (typeof renderSlideLayouts === 'function') { renderSlideLayouts(); }
            // Add other layouts rendering functions here
        }
    }, 500);
})