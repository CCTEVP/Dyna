/**
 * CardWidgetAnimationRoll.js
 * Handles the roll animation for card widgets
 */
console.log("LOADING: CardWidgetAnimationRoll.js");

(function () {
  console.log("INITIALIZING: Roll Animation Module");

  // Function to animate a card widget with roll animation
  function animateCardWidgetRoll(cardWidget, oldValue, newValue) {
    if (oldValue === newValue) return;

    console.log(
      `Animating roll from ${oldValue} to ${newValue} for widget ${cardWidget.id}`
    );

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
    console.log("Roll animation handler registered with animation system");
  } else {
    console.error(
      "CardWidgetAnimationSystem not found. Make sure CardWidgetAnimation.js is loaded first."
    );
  }
})();
