const cacheName = "creative-cache-{{creativeId}}";
const filesToCache = {{filesToCache}};
const excludedUrls = {{excludedUrls}};

self.addEventListener("install", (event) => {
    event.waitUntil(
        caches.open(cacheName).then((cache) => {
            return cache.addAll(filesToCache);
        }).catch(error => {
            console.error('Service Worker Install Error:', error);
        })
    );
});

self.addEventListener("fetch", (event) => {
    event.respondWith(
        caches.match(event.request).then((cachedResponse) => {
            if (cachedResponse) {
                return cachedResponse;
            } else {
                return fetch(event.request).then((networkResponse) => {
                    if (networkResponse.ok) {
                        const responseClone = networkResponse.clone();
                        caches.open(cacheName).then((cache) => {
                            cache.put(event.request, responseClone);
                        }).catch(error => {
                            console.error('Cache Put Error:', error);
                        });
                    }
                    return networkResponse;
                }).catch(error => {
                    console.error('Fetch Error:', error);
                    return fetch(event.request); // Attempt network fetch regardless of errors
                });
            }
        }).catch(error => {
            console.error('Cache Match Error:', error);
            return fetch(event.request); // Attempt network fetch regardless of errors
        })
    );
});

self.addEventListener("message", (event) => {
    if (event.data.action === "unregister") {
        self.registration.unregister().then(() => {
            console.log("Service Worker unregistered.");
        });
    } else if (event.data.action === "clearCache") {
        caches.delete(cacheName).then(() => {
            console.log("Cache cleared.");
        });
    } else if (event.data.action === "skipCache") {
        // Logic to skip cache for a specific URL (if needed)
        console.log(`Skipping cache for: ${event.data.url}`);
    }
});