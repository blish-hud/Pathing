using System.ComponentModel;
using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Behavior;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using AttributeCollection = TmfLib.Prototype.AttributeCollection;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_TRIGGERRANGE = "triggerrange";

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
        /// triggerrange
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_Triggers(AttributeCollection collection, IPackResourceManager resourceManager) {
            this.TriggerRange = _packState.UserResourceStates.Population.MarkerPopulationDefaults.TriggerRange;

            { if (collection.TryPopAttribute(ATTR_TRIGGERRANGE, out var attribute)) this.TriggerRange = attribute.GetValueAsFloat(_packState.UserResourceStates.Population.MarkerPopulationDefaults.TriggerRange); }
        }

        public override void Focus() {
            if (this.Focused) return;

            this.Focused = true;

            if (this.AutoTrigger) {
                this.Interact(true);
            }
        }

        public override void Unfocus() {
            this.Focused = false;
        }

        public override void Interact(bool autoTriggered) {
            foreach (var behavior in this.Behaviors) {
                if (behavior is ICanInteract interactable) {
                    Logger.Debug($"{(autoTriggered ? "Automatically" : "Manually")} interacted with marker '{this.Guid.ToBase64String()}': {behavior.GetType().Name}");
                    interactable.Interact(autoTriggered);
                }
            }
        }

    }
}
