using System.ComponentModel;
using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Behavior;
using BhModule.Community.Pathing.Utility;
using Microsoft.Xna.Framework;
using TmfLib;
using AttributeCollection = TmfLib.Prototype.AttributeCollection;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_TRIGGERRANGE = "triggerrange";
        private const string ATTR_INFORANGE    = "inforange";

        [Description("This attribute is used by multiple other attributes to define a distance from the marker in which those attributes will activate their functionality or behavior.")]
        [Category("Behavior")]
        public override float TriggerRange { get; set; }

        private bool _focused;
        [Description("The focused state indicates if the player is within the trigger range which may activate a behavior.")]
        [Category("State Debug")]
        public bool Focused {
            get => _focused;
            private set {
                if (_focused == value) return;

                _focused = value;
                
                foreach (var behavior in this.Behaviors) {
                    if (behavior is ICanFocus focusable) {
                        if (_focused) {
                            focusable.Focus();
                        } else {
                            focusable.Unfocus();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// triggerrange, inforange
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_Triggers(AttributeCollection collection, IPackResourceManager resourceManager) {
            this.TriggerRange = _packState.UserResourceStates.Population.MarkerPopulationDefaults.TriggerRange;

            { if (collection.TryPopAttribute(ATTR_TRIGGERRANGE, out var attribute)) this.TriggerRange = attribute.GetValueAsFloat(_packState.UserResourceStates.Population.MarkerPopulationDefaults.TriggerRange); }

            // We treat inforange as an alias for triggerrange.
            // We want to make sure we only set this value if it is actually specified and triggerrange is still the default.
            if (this.TriggerRange == _packState.UserResourceStates.Population.MarkerPopulationDefaults.TriggerRange) {
                if (collection.TryPopAttribute(ATTR_INFORANGE, out var rangeAttr)) {
                    this.TriggerRange = rangeAttr.GetValueAsFloat(_packState.UserResourceStates.Population.MarkerPopulationDefaults.TriggerRange);
                }
            }
        }

        public override void Focus() {
            if (this.Focused) return;

            this.Focused = true;

            if (this.AutoTrigger) {
                this.Interact(true);
            }

            if (this.BehaviorFiltered && _packState.UserConfiguration.PackShowHiddenMarkersReducedOpacity.Value) {
                // Allow users to see that they have a hidden marker and can unhide it.
                foreach (var behavior in this.Behaviors) {
                    if (behavior is ICanFilter filter && filter.IsFiltered()) {
                        _packState.UiStates.Interact.ShowInteract(this, $"{filter.FilterReason()}\n\nPress {{0}} or click this gear to unhide the marker.", Color.LightBlue);
                    }
                }
            }
        }

        public override void Unfocus() {
            this.Focused = false;
            _packState.UiStates.Interact.DisconnectInteract(this);
        }

        public override void Interact(bool autoTriggered) {
            if (!autoTriggered && this.BehaviorFiltered && _packState.UserConfiguration.PackShowHiddenMarkersReducedOpacity.Value) {
                // Allow users to clear hidden markers.
                foreach (var behavior in this.Behaviors.ToArray()) {
                    if (behavior is ICanFilter filter) {
                        this.Behaviors.Remove(behavior);
                    }
                }

                // This is the only behavior type that is stored that we can easily clear.
                _packState.BehaviorStates.ClearHiddenBehavior(this.Guid);

                Unfocus();
                return;
            } else if (this.BehaviorFiltered) {
                return;
            }

            foreach (var behavior in this.Behaviors) {
                if (behavior is ICanInteract interactable) {
                    Logger.Debug($"{(autoTriggered ? "Automatically" : "Manually")} interacted with marker '{this.Guid.ToBase64String()}': {behavior.GetType().Name}");
                    interactable.Interact(autoTriggered);
                }
            }
        }

    }
}
