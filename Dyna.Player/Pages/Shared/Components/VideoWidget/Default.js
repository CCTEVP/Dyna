function renderVideoWidgets() {
    console.log("Rendering VideoWidgets");
    const videoWidgets = document.querySelectorAll('[id^="video-widget-"]');

    videoWidgets.forEach(videoWidget => {

        // Create the image element within the imageWidget
        const vid = document.createElement('video');
        const src = document.createElement('source');
        src.href = videoWidget.getAttribute('data-src');
        vid.appendChild(src);
        videoWidget.appendChild(vid);
    });
}