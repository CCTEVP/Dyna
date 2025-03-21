if ("serviceWorker" in navigator) {
  navigator.serviceWorker
    .register("{placeholder}")
    .then(() => console.log("Service worker registered!"))
    .catch((err) => console.error("Service worker registration failed:", err));
}
