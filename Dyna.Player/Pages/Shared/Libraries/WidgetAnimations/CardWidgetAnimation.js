// CardWidgetAnimation.js - Central animation manager for card widgets
console.log("LOADING: CardWidgetAnimation.js");

// Create a namespace for our animation system to avoid global conflicts
window.CardWidgetAnimationSystem = (function () {
  console.log("INITIALIZING: CardWidgetAnimation System");

  // Track observed widgets to prevent duplicate observers
  const observedWidgets = new Set();

  // Track widgets that have been initialized with real values
  const initializedWidgets = new Set();

  // Store animation handlers by type
  const animationHandlers = {
    // Default handler for widgets without specific animation
    default: function (cardWidget, oldValue, newValue) {
      console.log(`Default animation from ${oldValue} to ${newValue}`);
      updateCardWidgetContent(cardWidget, newValue);
    },
  };

  // Register an animation handler
  function registerAnimationHandler(type, handler) {
    console.log(`Registering animation handler for type: ${type}`);
    animationHandlers[type] = handler;
  }

  // Helper function to update card widget content without animation
  function updateCardWidgetContent(cardWidget, value) {
    console.log(`Updating content for widget ${cardWidget.id} to ${value}`);

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
      console.log(
        `Skipping animation for ${cardWidget.id} - not yet initialized with real value`
      );
      return;
    }

    console.log(
      `Forcing animation update for ${cardWidget.id} with value ${currentValue} and animation ${animationType}`
    );

    // Temporarily set a different value to force a change
    const tempValue = currentValue === "00" ? "01" : "00";
    cardWidget.setAttribute("data-current-value", tempValue);

    // Then set it back to trigger the animation
    setTimeout(() => {
      if (animationHandlers[animationType]) {
        animationHandlers[animationType](cardWidget, tempValue, currentValue);
      } else {
        console.warn(
          `No handler for animation type: ${animationType}, using default`
        );
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
      console.log(
        `Value change detected for ${cardWidget.id}: ${currentValue} -> ${newValue} (animation: ${animationType})`
      );

      // Call the appropriate animation handler
      if (animationHandlers[animationType]) {
        animationHandlers[animationType](cardWidget, currentValue, newValue);
      } else {
        console.warn(
          `No handler for animation type: ${animationType}, using default`
        );
        animationHandlers["default"](cardWidget, currentValue, newValue);
      }
    }
  }

  // Set up a single mutation observer for all card widgets
  function setupObserver() {
    console.log("Setting up central mutation observer for all card widgets");

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
        console.log(`Widget ${widget.id} already observed, skipping`);
        return;
      }

      console.log(`Setting up observer for widget ${widget.id}`);
      observer.observe(widget, { attributes: true });
      observedWidgets.add(widget.id);
      widget.setAttribute("data-has-observer", "true");

      // Don't force animation on initial setup - wait for real values
    }

    // Observe all existing card widgets
    const cardWidgets = document.querySelectorAll('div[id^="card-widget-"]');
    console.log(`Found ${cardWidgets.length} card widgets to observe`);
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
                console.log(
                  `Found ${widgets.length} new card widgets to observe`
                );
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
        console.log(
          `Found ${unobservedWidgets.length} unobserved card widgets`
        );
        unobservedWidgets.forEach(observeWidget);
      }
    }, 2000);
  }

  // Initialize the animation system
  function initialize() {
    console.log("Initializing card widget animation system");
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
