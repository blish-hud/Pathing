using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using TmfLib.Pathable;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Behavior.Modifier {
    public class ToggleModifier : Behavior<StandardMarker>, ICanInteract, ICanFocus {

        public const  string PRIMARY_ATTR_NAME = "toggle";

        private readonly IPackState _packState;

        public PathingCategory Category { get; set; }

        public ToggleModifier(PathingCategory category, StandardMarker marker, IPackState packState) : base(marker) {
            _packState = packState;

            this.Category = category;
        }
        
        public static IBehavior BuildFromAttributes(AttributeCollection attributes, StandardMarker marker, IPackState packState) {
            return attributes.TryGetAttribute(PRIMARY_ATTR_NAME, out var attribute)
                       ? new ToggleModifier(packState.RootCategory.GetOrAddCategoryFromNamespace(attribute.GetValueAsString()),
                                            marker,
                                            packState)
                       : null;
        }

        public void Interact(bool autoTriggered) {
            if (_pathingEntity.BehaviorFiltered) {
                return;
            }

            _packState.CategoryStates.SetInactive(this.Category,
                                                  !_packState.CategoryStates.GetCategoryInactive(this.Category));
        }

        public void Focus() {
            // TODO: Add localization for Toggle Category text.
            _packState.UiStates.Interact.ShowInteract(_pathingEntity, $"Toggle '{this.Category.Namespace}' category {{0}}");
        }

        public void Unfocus() {
            _packState.UiStates.Interact.DisconnectInteract(_pathingEntity);
        }

        public override void Unload() {
            _packState.UiStates.Interact.DisconnectInteract(_pathingEntity);
        }

    }
}
