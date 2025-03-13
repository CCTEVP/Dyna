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
