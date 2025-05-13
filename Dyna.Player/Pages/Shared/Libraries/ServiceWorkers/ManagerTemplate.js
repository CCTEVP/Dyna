// Service Worker Management Helper
window.swManager = {
    async register(creativeId, debugMode) {
        if ("serviceWorker" in navigator) {
            try {
                const registration = await navigator.serviceWorker.register(
                    `/{{creativeId}}.caching.bundle{{minSuffix}}.js`,
                    {
                        scope: "/{{viewType}}/",
                        updateViaCache: "none",
                    }
                );
                console.log("ServiceWorker registration successful");

                // Force update check
                await registration.update();
                // Update every hour
                setInterval(() => {
                    registration.update();
                }, 3600000);
                return registration;
            } catch (err) {
                console.error("ServiceWorker registration failed:", err);
                throw err;
            }
        }
    },
    async unregister() {
        if ("serviceWorker" in navigator) {
            try {
                const registration = await navigator.serviceWorker.ready;
                if (registration && registration.active) {
                    registration.active.postMessage({ action: "unregister" });
                }
            } catch (error) {
                console.error("Unregister error", error);
            }
        }
    },
    async clearCache() {
        if ("serviceWorker" in navigator) {
            try {
                const registration = await navigator.serviceWorker.ready;
                if (registration && registration.active) {
                    registration.active.postMessage({ action: "clearCache" });
                }
            } catch (error) {
                console.error("Clear cache error", error);
            }
        }
    },
    async skipCache(n) {
        if ("serviceWorker" in navigator) {
            try {
                const registration = await navigator.serviceWorker.ready;
                if (registration && registration.active) {
                    registration.active.postMessage({ action: "skipCache", url: n });
                }
            } catch (error) {
                console.error("Skip cache error", error);
            }
        }
    },
};
