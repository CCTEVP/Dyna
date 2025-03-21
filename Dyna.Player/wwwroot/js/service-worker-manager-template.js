// Service Worker Management Helper
window.swManager = {
  async register(creativeId, debugMode) {
    if ("serviceWorker" in navigator) {
      try {
        const minSuffix = debugMode ? "" : ".min";
        const registration = await navigator.serviceWorker.register(
          "/{{viewType}}/{{creativeId}}/caching.bundle{{minSuffix}}.js",
          { scope: "/{{viewType}}/{{creativeId}}/" }
        );
        console.log("ServiceWorker registration successful");
        return registration;
      } catch (err) {
        console.error("ServiceWorker registration failed:", err);
        throw err;
      }
    }
  },

  async unregister() {
    if ("serviceWorker" in navigator) {
      const registration = await navigator.serviceWorker.ready;
      await registration.active.postMessage({ action: "unregister" });
    }
  },

  async clearCache() {
    if ("serviceWorker" in navigator) {
      const registration = await navigator.serviceWorker.ready;
      await registration.active.postMessage({ action: "clearCache" });
    }
  },

  async skipCache(url) {
    if ("serviceWorker" in navigator) {
      const registration = await navigator.serviceWorker.ready;
      await registration.active.postMessage({ action: "skipCache", url });
    }
  },
};
