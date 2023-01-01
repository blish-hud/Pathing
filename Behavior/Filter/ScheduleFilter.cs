using System;
using BhModule.Community.Pathing.Utility;
using Cronos;
using Microsoft.Xna.Framework;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Behavior.Filter {
    class ScheduleFilter : IBehavior, ICanFilter {

        public const string PRIMARY_ATTR_NAME = "schedule";
        public const string ATTR_DURATION     = PRIMARY_ATTR_NAME + "-duration";

        /// <summary>
        /// The cron expression that represents the start of the schedule.
        /// </summary>
        public CronExpression CronExpression { get; set; }

        /// <summary>
        /// The duration after the schedule that the marker or trail should not be filtered.
        /// </summary>
        public TimeSpan Duration { get; set; }

        private DateTime _nextTrigger = DateTime.MinValue;
        private bool     _isFiltered  = false;

        public ScheduleFilter(CronExpression cronExpression, TimeSpan duration) {
            this.CronExpression = cronExpression;
            this.Duration       = duration;

            UpdateNextTrigger(true);
        }

        private void UpdateNextTrigger(bool accountForDuration) {
            var nextTrigger = accountForDuration
                                  ? this.CronExpression.GetNextOccurrence(DateTime.UtcNow - this.Duration, true)
                                  : this.CronExpression.GetNextOccurrence(DateTime.UtcNow, true);

            if (nextTrigger.HasValue) {
                _nextTrigger = nextTrigger.Value;
            }
        }

        public void Update(GameTime gameTime) {
            var now = DateTime.UtcNow;

            if (!_isFiltered && now > _nextTrigger + this.Duration) {
                // If showing and we need to reset and calculate next occurrence.
                UpdateNextTrigger(false);

                _isFiltered = true;
            } else if (now >= _nextTrigger && now < _nextTrigger + this.Duration) {
                // If not showing and we need to start showing.
                _isFiltered = false;
            } else {
                _isFiltered = true;
            }
        }

        public string FilterReason() {
            return $"Hidden because it is only scheduled to show at certain times of day.";
        }

        public void Unload() { /* NOOP */ }

        public bool IsFiltered() => _isFiltered;

        public static IBehavior BuildFromAttributes(AttributeCollection attributes) {
            if (attributes.TryGetAttribute(PRIMARY_ATTR_NAME, out var expressionAttr) && attributes.TryGetAttribute(ATTR_DURATION, out var durationAttr)) {
                var   expression = expressionAttr.GetValueAsCronExpression();
                float duration   = durationAttr.GetValueAsFloat();

                if (expression != null && duration > 0) {
                    return new ScheduleFilter(expression, TimeSpan.FromMinutes(duration));
                }
            }

            return null;
        }

    }
}
