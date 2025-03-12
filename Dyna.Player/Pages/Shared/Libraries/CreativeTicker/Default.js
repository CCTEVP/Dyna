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
