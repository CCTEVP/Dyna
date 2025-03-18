/**
 * CountdownWidget/Default.js
 * Handles the rendering and functionality of countdown widgets
 */

// Function to render all countdown widgets
function renderCountdownWidgets() {
  const countdownWidgets = document.querySelectorAll(
    'div[id^="countdown-widget-"]'
  );

  if (countdownWidgets.length === 0) return;

  //console.log(`Found ${countdownWidgets.length} countdown widgets to render`);

  countdownWidgets.forEach((widget) => {
    const targetDateStr = widget.getAttribute("data-target-date-time");

    if (!targetDateStr) {
      //console.warn(`Countdown widget ${widget.id} has no target date`);
      return;
    }

    const targetDate = new Date(targetDateStr);

    if (isNaN(targetDate.getTime())) {
      //console.warn(`Invalid target date: ${targetDateStr}`);
      return;
    }

    // Set up the countdown
    setupCountdown(widget, targetDate);
  });
}

// Function to set up a countdown for a specific widget
function setupCountdown(widget, targetDate) {
  // Find all card widgets inside this countdown by their box layout containers
  const cardWidgets = {
    days: widget.querySelector("#box-layout-days .number"),
    hours: widget.querySelector("#box-layout-hours .number"),
    minutes: widget.querySelector("#box-layout-minutes .number"),
    seconds: widget.querySelector("#box-layout-seconds .number"),
  };

  // Calculate initial values but don't set them yet
  const timeRemaining = calculateTimeRemaining(new Date(), targetDate);

  // Format values with leading zeros
  const values = {
    days: timeRemaining.days.toString().padStart(2, "0"),
    hours: timeRemaining.hours.toString().padStart(2, "0"),
    minutes: timeRemaining.minutes.toString().padStart(2, "0"),
    seconds: timeRemaining.seconds.toString().padStart(2, "0"),
  };

  //console.log(`Initial values calculated: ${JSON.stringify(values)}`);

  // Delay setting the values to ensure widgets are fully initialized
  setTimeout(() => {
    // Set days and hours first
    if (cardWidgets.days) {
      cardWidgets.days.setAttribute("data-value", values.days);
      //console.log(`Set days widget to ${values.days}`);
    }

    if (cardWidgets.hours) {
      cardWidgets.hours.setAttribute("data-value", values.hours);
      //console.log(`Set hours widget to ${values.hours}`);
    }
    if (cardWidgets.minutes) {
      cardWidgets.minutes.setAttribute("data-value", values.minutes);
      //console.log(`Set minutes widget to ${values.minutes}`);
    }

    if (cardWidgets.seconds) {
      cardWidgets.seconds.setAttribute("data-value", values.seconds);
      //console.log(`Set seconds widget to ${values.seconds}`);
    }

    // Update widget status
    widget.setAttribute(
      "data-status",
      timeRemaining.total <= 0 ? "expired" : "active"
    );
  }, 500);

  // Subscribe to the ticker for updates
  if (window.creativeTicker) {
    window.creativeTicker.subscribe(
      `countdown-${widget.id}`,
      () => updateCountdown(widget, cardWidgets, targetDate),
      1000 // Update every second
    );
  } else {
    //console.warn("CreativeTicker not found. Countdown will not update automatically.");
    // Fallback to setInterval if creativeTicker is not available
    setInterval(() => updateCountdown(widget, cardWidgets, targetDate), 1000);
  }
}

// Function to update the countdown display
function updateCountdown(widget, cardWidgets, targetDate) {
  const now = new Date();
  const timeRemaining = calculateTimeRemaining(now, targetDate);

  // Update widget status based on time remaining
  if (timeRemaining.total <= 0) {
    widget.setAttribute("data-status", "expired");
  } else {
    widget.setAttribute("data-status", "active");
  }

  // Format values with leading zeros
  const daysStr = timeRemaining.days.toString().padStart(2, "0");
  const hoursStr = timeRemaining.hours.toString().padStart(2, "0");
  const minutesStr = timeRemaining.minutes.toString().padStart(2, "0");
  const secondsStr = timeRemaining.seconds.toString().padStart(2, "0");

  // Update each card widget with the new values only if they've changed
  if (cardWidgets.days) {
    const currentValue = cardWidgets.days.getAttribute("data-value");
    if (currentValue !== daysStr) {
      cardWidgets.days.setAttribute("data-value", daysStr);
    }
  }

  if (cardWidgets.hours) {
    const currentValue = cardWidgets.hours.getAttribute("data-value");
    if (currentValue !== hoursStr) {
      cardWidgets.hours.setAttribute("data-value", hoursStr);
    }
  }

  if (cardWidgets.minutes) {
    const currentValue = cardWidgets.minutes.getAttribute("data-value");
    if (currentValue !== minutesStr) {
      cardWidgets.minutes.setAttribute("data-value", minutesStr);
    }
  }

  if (cardWidgets.seconds) {
    const currentValue = cardWidgets.seconds.getAttribute("data-value");
    if (currentValue !== secondsStr) {
      cardWidgets.seconds.setAttribute("data-value", secondsStr);
    }
  }
}

// Function to calculate time remaining
function calculateTimeRemaining(now, targetDate) {
  let diff = Math.max(0, targetDate - now) / 1000; // Convert to seconds
  const total = diff;

  const days = Math.floor(diff / 86400);
  diff -= days * 86400;

  const hours = Math.floor(diff / 3600) % 24;
  diff -= hours * 3600;

  const minutes = Math.floor(diff / 60) % 60;
  diff -= minutes * 60;

  const seconds = Math.floor(diff % 60);

  return { days, hours, minutes, seconds, total };
}

// Initialize when the DOM is ready
document.addEventListener("DOMContentLoaded", renderCountdownWidgets);
