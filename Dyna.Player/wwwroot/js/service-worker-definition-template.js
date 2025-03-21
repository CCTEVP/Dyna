const cacheName = "creative-cache-{{creativeId}}";
const filesToCache = {{filesToCache}};
const excludedUrls = {{excludedUrls}};

// Helper function to check if URL should be cached
function shouldCache(url) {
  return !excludedUrls.some((pattern) => url.includes(pattern));
}

// Clean old caches
async function cleanOldCaches() {
  const cacheNames = await caches.keys();
  return Promise.all(
    cacheNames
      .filter((name) => name !== cacheName)
      .map((name) => caches.delete(name))
  );
}

// Install event - cache initial resources
self.addEventListener("install", (event) => {
  event.waitUntil(
    caches
      .open(cacheName)
      .then((cache) => cache.addAll(filesToCache))
      .then(() => cleanOldCaches())
  );
});

// Activate event - clean up old caches
self.addEventListener("activate", (event) => {
  event.waitUntil(cleanOldCaches());
});

// Message event - handle control messages
self.addEventListener("message", (event) => {
  switch (event.data.action) {
    case "skipCache":
      excludedUrls.push(event.data.url);
      break;
    case "clearCache":
      event.waitUntil(caches.delete(cacheName));
      break;
    case "unregister":
      self.registration.unregister().then(() => caches.delete(cacheName));
      break;
  }
});

// Fetch event - handle resource requests
self.addEventListener("fetch", (event) => {
  if (!shouldCache(event.request.url)) {
    return fetch(event.request);
  }

  event.respondWith(
    caches.match(event.request).then((cachedResponse) => {
      if (cachedResponse) {
        // Return cached response
        return cachedResponse;
      }

      return fetch(event.request).then((networkResponse) => {
        if (!networkResponse || networkResponse.status !== 200) {
          return networkResponse;
        }

        // Cache new successful responses
        const responseToCache = networkResponse.clone();
        caches.open(cacheName).then((cache) => {
          if (shouldCache(event.request.url)) {
            cache.put(event.request, responseToCache);
          }
        });

        return networkResponse;
      });
    })
  );
});
