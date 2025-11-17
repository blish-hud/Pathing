using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using System;
using TmfLib.Pathable;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Behavior.Modifier {
    public class ShowHideModifier : Behavior<StandardMarker>, ICanInteract, ICanFocus {

        private static readonly Logger Logger = Logger.GetLogger<ShowHideModifier>();

        public const string SHOW_PRIMARY_ATTR_NAME = "show";
        public const string HIDE_PRIMARY_ATTR_NAME = "hide";

        private readonly IPackState _packState;

        public PathingCategory Category       { get; set; }
        public bool            ShowOnInteract { get; set; }

        public ShowHideModifier(PathingCategory category, bool showOnInteract, StandardMarker marker, IPackState packState) : base(marker) {
            _packState = packState;

            this.Category       = category;
            this.ShowOnInteract = showOnInteract;
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes, StandardMarker marker, IPackState packState) {
            // Create show modifier.
            if (attributes.TryGetAttribute(SHOW_PRIMARY_ATTR_NAME, out var showAttr)) {
                string attrValue = showAttr.GetValueAsString();

                if (!string.IsNullOrWhiteSpace(attrValue)) {
                    PathingCategory category = null;

                    try {
                        category = packState.RootCategory.GetOrAddCategoryFromNamespace(attrValue);
                    } catch (Exception e) {
                        Logger.Warn(e, $"Failed to load {showAttr.Name}=\"{attrValue}\".");
                    }

                    if (category != null) {
                        return new ShowHideModifier(category, true, marker, packState);
                    }
                }
            }

            // Create hide modifier.
            if (attributes.TryGetAttribute(HIDE_PRIMARY_ATTR_NAME, out var hideAttr)) {
                string attrValue = hideAttr.GetValueAsString();

                if (!string.IsNullOrWhiteSpace(attrValue)) {
                    PathingCategory category = null;

                    try {
                        category = packState.RootCategory.GetOrAddCategoryFromNamespace(attrValue);
                    } catch (Exception e) {
                        Logger.Warn(e, $"Failed to load {hideAttr.Name}=\"{attrValue}\".");
                    }

                    if (category != null) {
                        return new ShowHideModifier(category, false, marker, packState);
                    }
                }
            }

            return null;
        }

        public void Interact(bool autoTriggered) {
            if (_pathingEntity.BehaviorFiltered) {
                return;
            }

            _packState.CategoryStates.SetInactive(this.Category,
                                                  !this.ShowOnInteract);
        }

        public void Focus() {
            _packState.UiStates.Interact.ShowInteract(_pathingEntity, $"{(this.ShowOnInteract ? "Show" : "Hide")} '{this.Category.Namespace}' category {{0}}");
        }

        public void Unfocus() {
            _packState.UiStates.Interact.DisconnectInteract(_pathingEntity);
        }

        public override void Unload() {
            _packState.UiStates.Interact.DisconnectInteract(_pathingEntity);
        }

    }
}
