using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using TmfLib.Pathable;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Behavior.Modifier {
    public class ToggleModifier : Behavior<StandardMarker>, ICanInteract {

        public const  string PRIMARY_ATTR_NAME = "toggle";

        private readonly IPackState _packstate;

        public PathingCategory Category { get; set; }

        public ToggleModifier(PathingCategory category, StandardMarker marker, IPackState packState) : base(marker) {
            _packstate = packState;

            this.Category = category;
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes, StandardMarker marker, IPackState packState) {
            return new ToggleModifier(packState.RootCategory.GetOrAddCategoryFromNamespace(attributes[PRIMARY_ATTR_NAME].GetValueAsString()),
                                      marker,
                                      packState);
        }

        public void Interact(bool autoTriggered) {
            _packstate.CategoryStates.SetInactive(this.Category,
                                                  !_packstate.CategoryStates.GetCategoryInactive(this.Category));
        }

    }
}
