/* Bundle generated in debug mode on 12/03/2025 16:26:11 */
/* Contains 8 js assets */

/* ===== BEGIN library/CreativeTicker.js ===== */
window.creativeTicker = window.creativeTicker || {
  interval: null,
  tickRate: 100, // 10ms tick rate
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

/* ===== END library/CreativeTicker.js ===== */

/* ===== BEGIN widget/ImageWidget.js ===== */
function renderImageWidgets() {
    console.log("Rendering ImageWidgets");
    const imageWidgets = document.querySelectorAll('[id^="image-widget-"]');

    imageWidgets.forEach(imageWidget => {
        
    });
}
/* ===== END widget/ImageWidget.js ===== */

/* ===== BEGIN widget/TextWidget.js ===== */
function renderTextWidgets() {
    console.log("Rendering TextWidgets");
    const textWidgets = document.querySelectorAll('[id^="text-widget-"]');

    textWidgets.forEach(textWidget => {

    });
}
/* ===== END widget/TextWidget.js ===== */

/* ===== BEGIN widget/CardWidget.js ===== */
function renderCardWidgets() {
  console.log("Rendering CardWidgets");
  const cardWidgets = document.querySelectorAll('[id^="card-widget-"]');

  // Check if any card widgets have animations
  let hasAnimations = false;

  cardWidgets.forEach((cardWidget) => {
    const animationType = cardWidget.getAttribute("data-animation");
    if (animationType && animationType !== "") {
      hasAnimations = true;
    }

    const observer = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        if (
          mutation.type === "attributes" &&
          mutation.attributeName === "data-value"
        ) {
          const changedCardWidget = mutation.target;
          const newValue = changedCardWidget.getAttribute("data-value");
          handleAttributeChange(
            changedCardWidget,
            mutation.attributeName,
            newValue
          );
        }
      });
    });
    observer.observe(cardWidget, { attributes: true });

    const initialValue = cardWidget.getAttribute("data-value");
    if (initialValue) {
      // Clear any existing content first
      cardWidget.textContent = "";
      updateCardWidgetContent(cardWidget, initialValue);
    }
  });

  // If any card widgets have animations, load the animation library
  if (hasAnimations) {
    // Register the animation library
    if (
      typeof Dyna !== "undefined" &&
      Dyna.Player &&
      Dyna.Player.TagHelpers &&
      Dyna.Player.TagHelpers.AssetTagHelper
    ) {
      Dyna.Player.TagHelpers.AssetTagHelper.AddPresentAsset(
        "CardWidgetAnimations",
        "library"
      );
    }
  }
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
            console.warn(
              "Roll animation function not found. Make sure CardWidgetAnimations library is loaded."
            );
            updateCardWidgetContent(cardWidget, newValue);
          }
          break;
        case "flip":
          // Check if the animation function exists (loaded from the library)
          if (typeof animateCardWidgetFlip === "function") {
            animateCardWidgetFlip(cardWidget, currentValue, newValue);
          } else {
            console.warn(
              "Flip animation function not found. Make sure CardWidgetAnimations library is loaded."
            );
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
  console.log(`Animating flip from ${oldValue} to ${newValue}`);

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

/* ===== BEGIN library/CardWidgetAnimations.js ===== */
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
  console.log(`Animating flip from ${oldValue} to ${newValue}`);

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

/* ===== END library/CardWidgetAnimations.js ===== */

/* ===== BEGIN widget/CountdownWidget.js ===== */
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


/* ===== END widget/CountdownWidget.js ===== */

/* ===== BEGIN layout/SlideLayout.js ===== */
function renderSlideLayouts() {
    console.log("Rendering SlideLayouts");
    const slideLayouts = document.querySelectorAll('[id^="slide-layout-"]');

    slideLayouts.forEach(slideWidget => {

    });
}
/* ===== END layout/SlideLayout.js ===== */

/* ===== BEGIN widget/VideoWidget.js ===== */
function renderVideoWidgets() {
    console.log("Rendering VideoWidgets");
    const videoWidgets = document.querySelectorAll('[id^="video-widget-"]');

    videoWidgets.forEach(videoWidget => {

        // Create the image element within the imageWidget
        const vid = document.createElement('video');
        const src = document.createElement('source');
        src.href = videoWidget.getAttribute('data-src');
        vid.appendChild(src);
        videoWidget.appendChild(vid);
    });
}
/* ===== END widget/VideoWidget.js ===== */

