function renderCountdownWidgets() {
    console.log("Rendering CountdownWidgets");
    const countdownWidgets = document.querySelectorAll(
        '[id^="countdown-widget-"]'
    );

    countdownWidgets.forEach((countdownWidget) => {
        const countdownId = countdownWidget.id;
        const targetDateTime = countdownWidget.getAttribute(
            "data-target-date-time"
        );

        if (!targetDateTime) {
            console.warn(`No target date specified for ${countdownId}`);
            return;
        }

        const elements = {
            days: countdownWidget.querySelector("[id$='-days'] .number"),
            hours: countdownWidget.querySelector("[id$='-hours'] .number"),
            minutes: countdownWidget.querySelector("[id$='-minutes'] .number"),
            seconds: countdownWidget.querySelector("[id$='-seconds'] .number"),
        };

        function calculateTimeRemaining() {
            const now = new Date().getTime();
            const target = new Date(targetDateTime).getTime();
            if (isNaN(target)) {
                console.error(`Invalid target date specified for ${countdownId}`);
                return {
                    days: 0,
                    hours: 0,
                    minutes: 0,
                    seconds: 0,
                    completed: true,
                };
            }
            const difference = target - now;

            if (difference <= 0) {
                return {
                    days: 0,
                    hours: 0,
                    minutes: 0,
                    seconds: 0,
                    completed: true,
                };
            }

            return {
                days: Math.floor(difference / (1000 * 60 * 60 * 24)),
                hours: Math.floor(
                    (difference % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60)
                ),
                minutes: Math.floor((difference % (1000 * 60 * 60)) / (1000 * 60)),
                seconds: Math.floor((difference % (1000 * 60)) / 1000),
                completed: false,
            };
        }

        function updateDisplay(timeRemaining) {
            const formatNumber = (num) => num.toString().padStart(2, "0");

            if (elements.days) {
                elements.days.setAttribute('data-value', formatNumber(timeRemaining.days));
            }
            if (elements.hours) {
                elements.hours.setAttribute('data-value', formatNumber(timeRemaining.hours));
            }
            if (elements.minutes) {
                elements.minutes.setAttribute('data-value', formatNumber(timeRemaining.minutes));
            }
            if (elements.seconds) {
                elements.seconds.setAttribute('data-value', formatNumber(timeRemaining.seconds));
            }

            if (timeRemaining.completed) {
                countdownWidget.dispatchEvent(new CustomEvent("countdownComplete"));
                if (unsubscribe) unsubscribe();
            }
        }

        // Subscribe to the universal ticker
        const unsubscribe = window.creativeTicker.subscribe(
            countdownId,
            () => updateDisplay(calculateTimeRemaining()),
            1000 // update every second
        );

        // Store unsubscribe function on widget for cleanup
        countdownWidget.unsubscribe = unsubscribe;

        // Use MutationObserver to detect when the widget is removed
        const observer = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
                mutation.removedNodes.forEach((node) => {
                    if (node === countdownWidget && countdownWidget.unsubscribe) {
                        countdownWidget.unsubscribe();
                        observer.disconnect();
                    }
                });
            });
        });

        observer.observe(countdownWidget.parentNode, { childList: true });

        // Initial update
        updateDisplay(calculateTimeRemaining());
    });
}

