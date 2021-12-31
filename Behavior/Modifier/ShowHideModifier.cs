using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using TmfLib.Pathable;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Behavior.Modifier {
    public class ShowHideModifier : Behavior<StandardMarker>, ICanInteract, ICanFocus {

        public const string SHOW_PRIMARY_ATTR_NAME = "show";
        public const string HIDE_PRIMARY_ATTR_NAME = "hide";

        private readonly IPackState _packstate;

        public PathingCategory Category       { get; set; }
        public bool            ShowOnInteract { get; set; }

        public ShowHideModifier(PathingCategory category, bool showOnInteract, StandardMarker marker, IPackState packState) : base(marker) {
            _packstate = packState;

            this.Category       = category;
            this.ShowOnInteract = showOnInteract;
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes, StandardMarker marker, IPackState packState) {
            // Create show modifier.
            if (attributes.TryGetAttribute(SHOW_PRIMARY_ATTR_NAME, out var showAttr)) {
                return new ShowHideModifier(packState.RootCategory.GetOrAddCategoryFromNamespace(showAttr.GetValueAsString()),
                                            true,
                                            marker,
                                            packState);
            }

            // Create hide modifier.
            if (attributes.TryGetAttribute(HIDE_PRIMARY_ATTR_NAME, out var hideAttr)) {
                return new ShowHideModifier(packState.RootCategory.GetOrAddCategoryFromNamespace(hideAttr.GetValueAsString()),
                                            false,
                                            marker,
                                            packState);
            }

            return null;
        }

        public void Interact(bool autoTriggered) {
            if (_pathingEntity.BehaviorFiltered) {
                return;
            }

            _packstate.CategoryStates.SetInactive(this.Category,
                                                  !this.ShowOnInteract);
        }

        public void Focus() { /* NOOP */ }

        public void Unfocus() { /* NOOP */ }

    }
}
