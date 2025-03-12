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
