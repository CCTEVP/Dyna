/**
 * CardWidget/Default.js
 * Handles the rendering and functionality of card widgets
 */

// Function to render all card widgets
function renderCardWidgets() {
    console.log("Initializing CardWidgets");
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
