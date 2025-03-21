/* Bundle generated in debug mode on 20/03/2025 09:59:05 */
/* Contains 10 js assets */

/* ===== BEGIN Libraries/Creative/CreativeTicker.js ===== */
window.creativeTicker = window.creativeTicker || {
  interval: null,
  tickRate: 10, // 10ms tick rate
  lastTick: Date.now(),
  subscribers: new Map(),

  start() {
    if (this.interval) return;
    this.lastTick = Date.now();
    this.interval = setInterval(() => {
      const now = Date.now();
      const elapsed = now - this.lastTick;
      this.lastTick = now;

      // Call all subscribers with elapsed time
      this.subscribers.forEach((sub) => {
        sub.elapsed += elapsed;
        if (sub.elapsed >= sub.updateRate) {
          sub.callback();
          sub.elapsed = 0;
        }
      });
    }, this.tickRate);
  },

  subscribe(id, callback, updateRate) {
    this.subscribers.set(id, {
      callback,
      updateRate,
      elapsed: 0,
    });

    // Start ticker if it's not running
    this.start();
    return () => this.subscribers.delete(id);
  },
};

/* ===== END Libraries/Creative/CreativeTicker.js ===== */

/* ===== BEGIN Libraries/Creative/WidgetInitializer.js ===== */
/**
 * WidgetInitializer.js
 * Centralized system for initializing all widgets after the document has loaded
 */
window.widgetInitializer = {
  // Store widget render functions
  renderFunctions: {},

  // Register a widget render function
  register: function (widgetName, renderFunction) {
    this.renderFunctions[widgetName] = renderFunction;
  },

  // Initialize all registered widgets in sequence
  initializeAll: function () {
    console.log("Rendering widgets:");

    // Define the initialization order
    const initOrder = [
      // Base layouts first
      "SlideLayouts",
      "BoxLayouts",
      // Then widgets
      "CardWidgets",
      "TextWidgets",
      "CountdownWidgets",
      // Add other widgets as needed
    ];

    // Initialize in order
    initOrder.forEach((widgetName) => {
      if (this.renderFunctions[widgetName]) {
        console.log(`Rendering ${widgetName}`);
        try {
          this.renderFunctions[widgetName]();
        } catch (error) {
          console.error(`Error initializing ${widgetName}:`, error);
        }
      }
    });

    // Initialize any remaining widgets not in the predefined order
    Object.keys(this.renderFunctions).forEach((widgetName) => {
      if (!initOrder.includes(widgetName)) {
        console.log(`Rendering ${widgetName}`);
        try {
          this.renderFunctions[widgetName]();
        } catch (error) {
          console.error(`Error initializing ${widgetName}:`, error);
        }
      }
    });
  },
};

// Initialize all widgets when the document is ready
document.addEventListener("DOMContentLoaded", function () {
  window.widgetInitializer.initializeAll();
});

/* ===== END Libraries/Creative/WidgetInitializer.js ===== */

/* ===== BEGIN layout/SlideLayout.js ===== */
function renderSlideLayouts() {
    console.log("Rendering SlideLayouts");
    const slideLayouts = document.querySelectorAll('[id^="slide-layout-"]');

    slideLayouts.forEach(slideWidget => {

    });
}
/* ===== END layout/SlideLayout.js ===== */

/* ===== BEGIN layout/BoxLayout.js ===== */
function renderBoxLayouts() {
    console.log("Rendering BoxLayouts");
    const boxLayouts = document.querySelectorAll('[id^="box-layout-"]');

    boxLayouts.forEach(boxLayout => {

    });
}
/* ===== END layout/BoxLayout.js ===== */

/* ===== BEGIN widget/CardWidget.js ===== */
/**
 * CardWidget/Default.js
 * Handles the rendering and functionality of card widgets
 */

// Function to render all card widgets
function renderCardWidgets() {
  const cardWidgets = document.querySelectorAll('div[id^="card-widget-"]');

  if (cardWidgets.length === 0) {
    return;
  }

  //console.log(`Found ${cardWidgets.length} card widgets to render`);

  cardWidgets.forEach((widget) => {
    // Ensure the widget has an ID
    if (!widget.id) {
      widget.id = `card-widget-${Math.random().toString(36).substr(2, 9)}`;
    }

    // Ensure data-current-value is set
    if (!widget.hasAttribute("data-current-value")) {
      const value = widget.getAttribute("data-value") || "0";
      widget.setAttribute("data-current-value", value);
    }
  });

  //console.log("Card widgets rendering complete");
}

// Register the render function with the widget initializer if available
if (window.widgetInitializer) {
  window.widgetInitializer.register("CardWidgets", renderCardWidgets);
} else {
  // Fallback to direct initialization if needed
  document.addEventListener("DOMContentLoaded", renderCardWidgets);
}

function handleAttributeChange(cardWidget, attributeName, newValue) {
  if (attributeName === "data-value") {
    const currentValue = cardWidget.getAttribute("data-current-value") || "";

    if (newValue !== currentValue) {
      // Store the current value as an attribute for future reference
      cardWidget.setAttribute("data-current-value", newValue);

      // Check the animation type
      const animationType = cardWidget.getAttribute("data-animation");

      // Apply the appropriate animation based on the data-animation attribute
      switch (animationType) {
        case "roll":
          // Check if the animation function exists (loaded from the library)
          if (typeof animateCardWidgetRoll === "function") {
            animateCardWidgetRoll(cardWidget, currentValue, newValue);
          } else {
            //console.warn("Roll animation function not found. Make sure CardWidgetAnimations library is loaded.");
            updateCardWidgetContent(cardWidget, newValue);
          }
          break;
        case "flip":
          // Check if the animation function exists (loaded from the library)
          if (typeof animateCardWidgetFlip === "function") {
            animateCardWidgetFlip(cardWidget, currentValue, newValue);
          } else {
            //console.warn("Flip animation function not found. Make sure CardWidgetAnimations library is loaded.");
            updateCardWidgetContent(cardWidget, newValue);
          }
          break;
        default:
          // Default behavior for other animation types or no animation
          updateCardWidgetContent(cardWidget, newValue);
      }
    }
  }
}

// ===== ROLL ANIMATION =====
function animateCardWidgetRoll(cardWidget, oldValue, newValue) {
  // Clear existing content
  cardWidget.textContent = "";

  // Create a container for the digits
  const digitsContainer = document.createElement("div");
  digitsContainer.className = "card-widget-digits-container roll-animation";
  cardWidget.appendChild(digitsContainer);

  // Create the current value container (will move up)
  const currentContainer = document.createElement("div");
  currentContainer.className = "card-widget-digit-container current";
  currentContainer.style.transform = "translateY(0)";
  digitsContainer.appendChild(currentContainer);

  // Create the digit for the current value
  const currentDigit = document.createElement("div");
  currentDigit.className = "card-widget-digit";
  currentDigit.textContent = oldValue || "0";
  currentContainer.appendChild(currentDigit);

  // Create the new value container (will move up from below)
  const newContainer = document.createElement("div");
  newContainer.className = "card-widget-digit-container new";
  newContainer.style.transform = "translateY(100%)";
  digitsContainer.appendChild(newContainer);

  // Create the digit for the new value
  const newDigit = document.createElement("div");
  newDigit.className = "card-widget-digit";
  newDigit.textContent = newValue;
  newContainer.appendChild(newDigit);

  // Trigger the animation after a small delay
  setTimeout(() => {
    // Move current value up and out of view
    currentContainer.style.transform = "translateY(-100%)";
    // Move new value up to the center
    newContainer.style.transform = "translateY(0)";
  }, 50);
}

// ===== FLIP ANIMATION =====
function animateCardWidgetFlip(cardWidget, oldValue, newValue) {
  // Clear existing content
  cardWidget.textContent = "";

  // Get font size and dimensions
  const style = window.getComputedStyle(cardWidget);
  const fontSize = style.fontSize;

  // Create a container for the animation
  const digitsContainer = document.createElement("div");
  digitsContainer.className = "card-widget-digits-container flip-animation";
  digitsContainer.style.fontSize = fontSize;
  cardWidget.appendChild(digitsContainer);

  // Create the panels container
  const flipPanels = document.createElement("div");
  flipPanels.className = "flip-panels";
  digitsContainer.appendChild(flipPanels);

  // Create the static bottom half (showing bottom half of current value)
  const staticBottom = document.createElement("div");
  staticBottom.className = "static-bottom";
  flipPanels.appendChild(staticBottom);

  const bottomDigit = document.createElement("div");
  bottomDigit.className = "digit";
  bottomDigit.textContent = oldValue || "0";
  staticBottom.appendChild(bottomDigit);

  // Create the static top half (showing top half of next value, initially hidden)
  const staticTop = document.createElement("div");
  staticTop.className = "static-top";
  flipPanels.appendChild(staticTop);

  const topDigit = document.createElement("div");
  topDigit.className = "digit";
  topDigit.textContent = newValue;
  staticTop.appendChild(topDigit);

  // Create the flipping card
  const flipCard = document.createElement("div");
  flipCard.className = "flip-card";
  flipPanels.appendChild(flipCard);

  // Front of flip card (showing top half of current value)
  const flipCardFront = document.createElement("div");
  flipCardFront.className = "flip-card-front";
  flipCard.appendChild(flipCardFront);

  const frontDigit = document.createElement("div");
  frontDigit.className = "digit";
  frontDigit.textContent = oldValue || "0";
  flipCardFront.appendChild(frontDigit);

  // Back of flip card (showing bottom half of next value, upside down)
  const flipCardBack = document.createElement("div");
  flipCardBack.className = "flip-card-back";
  flipCard.appendChild(flipCardBack);

  const backDigit = document.createElement("div");
  backDigit.className = "digit";
  backDigit.textContent = newValue;
  // Add a data attribute to help with debugging
  backDigit.setAttribute("data-debug", "back-digit");
  flipCardBack.appendChild(backDigit);

  // Add debug info
  //console.log(`Animating flip from ${oldValue} to ${newValue}`);

  // Trigger the animation after a small delay
  setTimeout(() => {
    // Add the flipping class to start the animation
    digitsContainer.classList.add("flipping");

    // After animation completes, clean up and show only the new value
    setTimeout(() => {
      // Only if this is still the current animation (hasn't been replaced)
      if (cardWidget.contains(digitsContainer)) {
        updateCardWidgetContent(cardWidget, newValue);
      }
    }, 650); // Slightly longer than the CSS transition
  }, 50);
}

function updateCardWidgetContent(cardWidget, value) {
  // Store the current value as an attribute
  cardWidget.setAttribute("data-current-value", value);

  // Check if we need animation structure
  const animationType = cardWidget.getAttribute("data-animation");

  if (animationType) {
    // For widgets with animation attribute but not currently animating
    // Clear any existing content
    cardWidget.textContent = "";

    // Get font size
    const style = window.getComputedStyle(cardWidget);
    const fontSize = style.fontSize;

    // Create a container for the digits
    const digitsContainer = document.createElement("div");
    digitsContainer.className = "card-widget-digits-container";
    digitsContainer.style.fontSize = fontSize;

    // Add animation-specific class if needed
    if (animationType === "roll") {
      digitsContainer.classList.add("roll-animation");
    } else if (animationType === "flip") {
      digitsContainer.classList.add("flip-animation");
    }

    cardWidget.appendChild(digitsContainer);

    if (animationType === "flip") {
      // For flip animation, create the static display
      const flipPanels = document.createElement("div");
      flipPanels.className = "flip-panels";
      digitsContainer.appendChild(flipPanels);

      // Top half
      const staticTop = document.createElement("div");
      staticTop.className = "static-top";
      staticTop.style.opacity = "1"; // Make visible for static display
      flipPanels.appendChild(staticTop);

      const topDigit = document.createElement("div");
      topDigit.className = "digit";
      topDigit.textContent = value;
      staticTop.appendChild(topDigit);

      // Bottom half
      const staticBottom = document.createElement("div");
      staticBottom.className = "static-bottom";
      flipPanels.appendChild(staticBottom);

      const bottomDigit = document.createElement("div");
      bottomDigit.className = "digit";
      bottomDigit.textContent = value;
      staticBottom.appendChild(bottomDigit);
    } else {
      // For other animations, use the standard container
      const digitContainer = document.createElement("div");
      digitContainer.className = "card-widget-digit-container";

      // Set appropriate styles based on animation type
      if (animationType === "roll") {
        digitContainer.style.transform = "translateY(0)";
      }

      digitsContainer.appendChild(digitContainer);

      // Create the digit
      const digit = document.createElement("div");
      digit.className = "card-widget-digit";
      digit.textContent = value;
      digitContainer.appendChild(digit);
    }
  } else {
    // For widgets with no animation, just set the text directly
    cardWidget.textContent = value;
  }
}

/* ===== END widget/CardWidget.js ===== */

/* ===== BEGIN widget/CountdownWidget.js ===== */
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

/* ===== END widget/CountdownWidget.js ===== */

/* ===== BEGIN widget/TextWidget.js ===== */
function renderTextWidgets() {
    console.log("Rendering TextWidgets");
    const textWidgets = document.querySelectorAll('[id^="text-widget-"]');

    textWidgets.forEach(textWidget => {

    });
}
/* ===== END widget/TextWidget.js ===== */

/* ===== BEGIN Libraries/WidgetAnimations/CardWidgetAnimation.js ===== */
// CardWidgetAnimation.js - Central animation manager for card widgets
//console.log("LOADING: CardWidgetAnimation.js");

// Create a namespace for our animation system to avoid global conflicts
window.CardWidgetAnimationSystem = (function () {
  //console.log("INITIALIZING: CardWidgetAnimation System");

  // Track observed widgets to prevent duplicate observers
  const observedWidgets = new Set();

  // Track widgets that have been initialized with real values
  const initializedWidgets = new Set();

  // Store animation handlers by type
  const animationHandlers = {
    // Default handler for widgets without specific animation
    default: function (cardWidget, oldValue, newValue) {
      //console.log(`Default animation from ${oldValue} to ${newValue}`);
      updateCardWidgetContent(cardWidget, newValue);
    },
  };

  // Register an animation handler
  function registerAnimationHandler(type, handler) {
    //console.log(`Registering animation handler for type: ${type}`);
    animationHandlers[type] = handler;
  }

  // Helper function to update card widget content without animation
  function updateCardWidgetContent(cardWidget, value) {
    //console.log(`Updating content for widget ${cardWidget.id} to ${value}`);

    // Update the data-value and data-current-value attributes
    cardWidget.setAttribute("data-value", value);
    cardWidget.setAttribute("data-current-value", value);

    // Find the main content div and update its text
    const contentDivs = cardWidget.querySelectorAll(
      "div[style*='display: flex']"
    );
    contentDivs.forEach((div) => {
      div.textContent = value;
    });

    // Mark this widget as initialized with a real value
    initializedWidgets.add(cardWidget.id);
  }

  // Force an animation update on a widget, even if the value hasn't changed
  function forceAnimationUpdate(cardWidget) {
    const currentValue = cardWidget.getAttribute("data-current-value");
    const animationType =
      cardWidget.getAttribute("data-animation") || "default";

    // Skip if the widget doesn't have a real value yet
    if (currentValue === "00" && !initializedWidgets.has(cardWidget.id)) {
      //console.log(`Skipping animation for ${cardWidget.id} - not yet initialized with real value`);
      return;
    }

    //console.log(`Forcing animation update for ${cardWidget.id} with value ${currentValue} and animation ${animationType}`);

    // Temporarily set a different value to force a change
    const tempValue = currentValue === "00" ? "01" : "00";
    cardWidget.setAttribute("data-current-value", tempValue);

    // Then set it back to trigger the animation
    setTimeout(() => {
      if (animationHandlers[animationType]) {
        animationHandlers[animationType](cardWidget, tempValue, currentValue);
      } else {
        //console.warn(`No handler for animation type: ${animationType}, using default`);
        animationHandlers["default"](cardWidget, tempValue, currentValue);
      }
    }, 50);
  }

  // Central function to handle widget value changes
  function handleWidgetValueChange(cardWidget, newValue) {
    const currentValue = cardWidget.getAttribute("data-current-value");
    const animationType =
      cardWidget.getAttribute("data-animation") || "default";

    // Mark this widget as initialized with a real value
    initializedWidgets.add(cardWidget.id);

    if (newValue !== currentValue) {
      //console.log(`Value change detected for ${cardWidget.id}: ${currentValue} -> ${newValue} (animation: ${animationType})`);

      // Call the appropriate animation handler
      if (animationHandlers[animationType]) {
        animationHandlers[animationType](cardWidget, currentValue, newValue);
      } else {
        //console.warn(`No handler for animation type: ${animationType}, using default`);
        animationHandlers["default"](cardWidget, currentValue, newValue);
      }
    }
  }

  // Set up a single mutation observer for all card widgets
  function setupObserver() {
    //console.log("Setting up central mutation observer for all card widgets");

    const observer = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        if (
          mutation.type === "attributes" &&
          mutation.attributeName === "data-value"
        ) {
          const cardWidget = mutation.target;
          const newValue = cardWidget.getAttribute("data-value");
          handleWidgetValueChange(cardWidget, newValue);
        }
      });
    });

    // Function to observe a widget
    function observeWidget(widget) {
      if (observedWidgets.has(widget.id)) {
        //console.log(`Widget ${widget.id} already observed, skipping`);
        return;
      }

      //console.log(`Setting up observer for widget ${widget.id}`);
      observer.observe(widget, { attributes: true });
      observedWidgets.add(widget.id);
      widget.setAttribute("data-has-observer", "true");

      // Don't force animation on initial setup - wait for real values
    }

    // Observe all existing card widgets
    const cardWidgets = document.querySelectorAll('div[id^="card-widget-"]');
    //console.log(`Found ${cardWidgets.length} card widgets to observe`);
    cardWidgets.forEach(observeWidget);

    // Set up an observer to catch dynamically added widgets
    const bodyObserver = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        if (mutation.type === "childList" && mutation.addedNodes.length > 0) {
          mutation.addedNodes.forEach((node) => {
            if (node.nodeType === 1) {
              // Element node
              // Check if this is a card widget
              if (
                node.id &&
                node.id.startsWith("card-widget-") &&
                !observedWidgets.has(node.id)
              ) {
                observeWidget(node);
              }

              // Check if it contains card widgets
              const widgets = node.querySelectorAll
                ? node.querySelectorAll(
                    'div[id^="card-widget-"]:not([data-has-observer="true"])'
                  )
                : [];

              if (widgets.length > 0) {
                //console.log(`Found ${widgets.length} new card widgets to observe`);
                widgets.forEach(observeWidget);
              }
            }
          });
        }
      });
    });
    bodyObserver.observe(document.body, { childList: true, subtree: true });

    // Periodically check for unobserved widgets
    setInterval(() => {
      const unobservedWidgets = document.querySelectorAll(
        'div[id^="card-widget-"]:not([data-has-observer="true"])'
      );
      if (unobservedWidgets.length > 0) {
        //console.log(`Found ${unobservedWidgets.length} unobserved card widgets`);
        unobservedWidgets.forEach(observeWidget);
      }
    }, 2000);
  }

  // Initialize the animation system
  function initialize() {
    //console.log("Initializing card widget animation system");
    setupObserver();

    // Add a click handler for testing (can be removed in production)
    document.addEventListener("click", (event) => {
      const widget = event.target.closest('div[id^="card-widget-"]');
      if (widget) {
        const currentValue = parseInt(widget.getAttribute("data-value")) || 0;
        widget.setAttribute("data-value", (currentValue + 1).toString());
      }
    });
  }

  // Initialize when the DOM is ready
  document.addEventListener("DOMContentLoaded", initialize);

  // Public API
  return {
    registerAnimationHandler: registerAnimationHandler,
    updateCardWidgetContent: updateCardWidgetContent,
    forceAnimationUpdate: forceAnimationUpdate,
    initialize: initialize,
  };
})();

/* ===== END Libraries/WidgetAnimations/CardWidgetAnimation.js ===== */

/* ===== BEGIN Libraries/WidgetAnimations/CardWidgetAnimationRoll.js ===== */
/**
 * CardWidgetAnimationRoll.js
 * Handles the roll animation for card widgets
 */
//console.log("LOADING: CardWidgetAnimationRoll.js");

(function () {
  //console.log("INITIALIZING: Roll Animation Module");

  // Function to animate a card widget with roll animation
  function animateCardWidgetRoll(cardWidget, oldValue, newValue) {
    if (oldValue === newValue) return;

    //console.log(`Animating roll from ${oldValue} to ${newValue} for widget ${cardWidget.id}`);

    // For roll animation, we need to create a temporary animation structure
    // that overlays the existing content

    // Create a container for the animation if it doesn't exist
    let animContainer = cardWidget.querySelector(".roll-animation-container");
    if (!animContainer) {
      // Hide all direct children of the card widget (the original content)
      Array.from(cardWidget.children).forEach((child) => {
        if (!child.classList.contains("roll-animation-container")) {
          child.style.visibility = "hidden";
        }
      });

      // Create animation container
      animContainer = document.createElement("div");
      animContainer.className = "roll-animation-container";
      animContainer.style.position = "absolute";
      animContainer.style.top = "0";
      animContainer.style.left = "0";
      animContainer.style.width = "100%";
      animContainer.style.height = "100%";
      animContainer.style.overflow = "hidden";
      animContainer.style.zIndex = "10";
      animContainer.style.fontSize =
        window.getComputedStyle(cardWidget).fontSize;
      animContainer.style.color = window.getComputedStyle(cardWidget).color;
      // Ensure the background is completely transparent
      animContainer.style.backgroundColor = "transparent";

      // Create current digit container (showing the old value)
      const currentDigit = document.createElement("div");
      currentDigit.className = "current-digit";
      currentDigit.style.position = "absolute";
      currentDigit.style.width = "100%";
      currentDigit.style.height = "100%";
      currentDigit.style.display = "flex";
      currentDigit.style.justifyContent = "center";
      currentDigit.style.alignItems = "center";
      currentDigit.style.transition = "transform 0.5s ease-in-out";
      currentDigit.textContent = oldValue;
      animContainer.appendChild(currentDigit);

      // Create new digit container (showing the new value, initially below)
      const newDigit = document.createElement("div");
      newDigit.className = "new-digit";
      newDigit.style.position = "absolute";
      newDigit.style.width = "100%";
      newDigit.style.height = "100%";
      newDigit.style.display = "flex";
      newDigit.style.justifyContent = "center";
      newDigit.style.alignItems = "center";
      newDigit.style.transition = "transform 0.5s ease-in-out";
      newDigit.style.transform = "translateY(100%)";
      newDigit.textContent = newValue;
      animContainer.appendChild(newDigit);

      // Add the animation container to the widget
      cardWidget.appendChild(animContainer);
    } else {
      // Update existing animation elements
      const currentDigit = animContainer.querySelector(".current-digit");
      const newDigit = animContainer.querySelector(".new-digit");

      if (currentDigit && newDigit) {
        currentDigit.textContent = oldValue;
        newDigit.textContent = newValue;

        // Reset positions
        currentDigit.style.transform = "translateY(0)";
        newDigit.style.transform = "translateY(100%)";
      }
    }

    // Get the animation elements
    const currentDigit = animContainer.querySelector(".current-digit");
    const newDigit = animContainer.querySelector(".new-digit");

    // Apply the animation
    setTimeout(() => {
      // Move current value up and out of view
      currentDigit.style.transform = "translateY(-100%)";
      // Move new value up to the center
      newDigit.style.transform = "translateY(0)";

      // Update the data-current-value attribute after animation completes
      setTimeout(() => {
        cardWidget.setAttribute("data-current-value", newValue);

        // Remove the animation container and update the actual content
        if (animContainer.parentNode === cardWidget) {
          // Find the main content div and update its text
          const contentDivs = cardWidget.querySelectorAll(
            "div:not(.roll-animation-container)"
          );
          contentDivs.forEach((div) => {
            if (div.style.visibility === "hidden") {
              div.textContent = newValue;
              div.style.visibility = "visible";
            }
          });

          // Remove the animation container
          cardWidget.removeChild(animContainer);
        }
      }, 500); // Match the CSS transition duration
    }, 20);
  }

  // Register the roll animation handler with the central animation system
  if (window.CardWidgetAnimationSystem) {
    window.CardWidgetAnimationSystem.registerAnimationHandler(
      "roll",
      animateCardWidgetRoll
    );
    //console.log("Roll animation handler registered with animation system");
  } else {
    //console.error("CardWidgetAnimationSystem not found. Make sure CardWidgetAnimation.js is loaded first.");
  }
})();

/* ===== END Libraries/WidgetAnimations/CardWidgetAnimationRoll.js ===== */

/* ===== BEGIN Libraries/WidgetAnimations/CardWidgetAnimationFlip.js ===== */
// CardWidgetAnimationFlip.js - Handles flip animation for card widgets
//console.log("LOADING: CardWidgetAnimationFlip.js");

(function () {
  //console.log("INITIALIZING: Flip Animation Module");

  // ===== FLIP ANIMATION =====
  function animateCardWidgetFlip(cardWidget, oldValue, newValue) {
    //console.log(`Animating flip from ${oldValue} to ${newValue} for widget ${cardWidget.id}`);

    // Find the elements
    const flipCard = cardWidget.querySelector('[data-role="flip-card"]');
    const staticTop = cardWidget.querySelector('[data-role="static-top"]');
    const topDigit = cardWidget.querySelector('[data-role="top-digit"]');
    const backDigit = cardWidget.querySelector('[data-role="back-digit"]');

    if (!flipCard || !staticTop || !topDigit || !backDigit) {
      //console.error(`Required elements for flip animation not found in widget ${cardWidget.id}`);
      // Update the value without animation
      cardWidget.setAttribute("data-current-value", newValue);
      const contentDiv = cardWidget.querySelector("div");
      if (contentDiv) {
        contentDiv.textContent = newValue;
      }
      return;
    }

    // Update the new value elements
    topDigit.textContent = newValue;
    backDigit.textContent = newValue;

    // Animation phases
    const phases = {
      DELAY: "delay",
      FLIP: "flip",
      SHOW_TOP: "showTop",
      COMPLETE: "complete",
    };

    // Animation state
    const state = {
      phase: phases.DELAY,
      elapsed: 0,
      totalDelay: 50, // 50ms delay before starting
      flipDuration: 600, // 600ms for the flip animation
      showTopAt: 300, // Show top half at 300ms into the animation
    };

    // Start the animation
    setTimeout(() => {
      // Start the flip animation
      flipCard.style.transform = "rotateX(-180deg)";
      flipCard.style.transition = "transform 0.6s ease-in-out";

      // After half the animation, show the static top
      setTimeout(() => {
        staticTop.style.opacity = "1";
        staticTop.style.transition = "opacity 0.2s ease-in-out";
      }, 300);

      // After animation completes, update the value
      setTimeout(() => {
        cardWidget.setAttribute("data-current-value", newValue);

        // Reset the flip card for the next animation
        flipCard.style.transform = "rotateX(0deg)";
        flipCard.style.transition = "none";
        staticTop.style.opacity = "0";

        // Update all digit elements to show the new value
        const allDigits = cardWidget.querySelectorAll(
          "div[style*='display: flex']"
        );
        allDigits.forEach((digit) => {
          digit.textContent = newValue;
        });
      }, 650); // Slightly longer than the CSS transition
    }, 50);
  }

  // Register the flip animation handler with the central animation system
  if (window.CardWidgetAnimationSystem) {
    window.CardWidgetAnimationSystem.registerAnimationHandler(
      "flip",
      animateCardWidgetFlip
    );
    //console.log("Flip animation handler registered with animation system");
  } else {
    //console.error("CardWidgetAnimationSystem not found. Make sure CardWidgetAnimation.js is loaded first.");
  }
})();

/* ===== END Libraries/WidgetAnimations/CardWidgetAnimationFlip.js ===== */

