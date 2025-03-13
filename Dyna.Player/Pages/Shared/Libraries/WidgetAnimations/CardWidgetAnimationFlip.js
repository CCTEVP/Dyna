// CardWidgetAnimationFlip.js - Handles flip animation for card widgets
console.log("LOADING: CardWidgetAnimationFlip.js");

(function () {
  console.log("INITIALIZING: Flip Animation Module");

  // ===== FLIP ANIMATION =====
  function animateCardWidgetFlip(cardWidget, oldValue, newValue) {
    console.log(
      `Animating flip from ${oldValue} to ${newValue} for widget ${cardWidget.id}`
    );

    // Find the elements
    const flipCard = cardWidget.querySelector('[data-role="flip-card"]');
    const staticTop = cardWidget.querySelector('[data-role="static-top"]');
    const topDigit = cardWidget.querySelector('[data-role="top-digit"]');
    const backDigit = cardWidget.querySelector('[data-role="back-digit"]');

    if (!flipCard || !staticTop || !topDigit || !backDigit) {
      console.error(
        `Required elements for flip animation not found in widget ${cardWidget.id}`
      );
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
    console.log("Flip animation handler registered with animation system");
  } else {
    console.error(
      "CardWidgetAnimationSystem not found. Make sure CardWidgetAnimation.js is loaded first."
    );
  }
})();
