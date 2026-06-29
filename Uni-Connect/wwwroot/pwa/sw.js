const CACHE = "uniconnect-v1";

const FILES = [
    "/",
    "/css/site.css",
    "/js/site.js",
    "/pwa/offline.html"
];

self.addEventListener("install", event => {

    event.waitUntil(

        caches.open(CACHE).then(cache => {

            return cache.addAll(FILES);

        })

    );

    self.skipWaiting();

});

self.addEventListener("activate", event => {

    event.waitUntil(
        self.clients.claim()
    );

});

self.addEventListener("fetch", event => {

    if (event.request.method !== "GET") {
        return;
    }

    event.respondWith(

        fetch(event.request)

            .catch(() => {

                return caches.match(event.request)

                    .then(response => {

                        if (response) {
                            return response;
                        }

                        if (event.request.mode === "navigate") {
                            return caches.match("/pwa/offline.html");
                        }

                    });

            })

    );

});