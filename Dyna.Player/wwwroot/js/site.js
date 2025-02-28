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
    // widgets.js
    setTimeout(() => {
        console.log("Rendering widgets")
        renderImageWidgets();
        renderVideoWidgets();
        renderCountdownWidgets();
        // Add other widget rendering functions here
    }, 1000);
    // Widget-specific rendering functions and prototype definitions will be added below
});