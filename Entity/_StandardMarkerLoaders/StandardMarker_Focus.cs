using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Behavior;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_TRIGGERRANGE = "triggerrange";
        
        public override float TriggerRange { get; set; }

        private bool _focused;
        public bool Focused {
            get => _focused;
            set {
                if (_focused == value) return;

                _focused = value;

                lock (this.Behaviors.SyncRoot) {
                    for (int i = 0; i < this.Behaviors.Count; i++) {
                        if (this.Behaviors[i] is ICanFocus focusable) {
                            if (_focused) {
                                focusable.Focus();
                            } else {
                                focusable.Unfocus();
                            }
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

            { if (collection.TryPopAttribute(ATTR_TRIGGERRANGE, out var attribute)) this.TriggerRange = attribute.GetValueAsFloat(); }
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
            lock (this.Behaviors.SyncRoot) {
                for (int i = 0; i < this.Behaviors.Count; i++) {
                    if (this.Behaviors[i] is ICanInteract interactable) {
                        interactable.Interact(autoTriggered);
                    }
                }
            }
        }

    }
}
