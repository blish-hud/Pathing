using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using Humanizer;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Behavior.Modifier {
    public class ResetGuidModifier : Behavior<StandardMarker>, ICanInteract, ICanFocus {

        public const  string PRIMARY_ATTR_NAME = "resetguid";

        private readonly IPackState _packState;

        public List<Guid> TargetGuids { get; set; }

        public ResetGuidModifier(IEnumerable<Guid> targetGuids, StandardMarker marker, IPackState packState) : base(marker) {
            _packState = packState;

            this.TargetGuids = targetGuids.ToList();
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes, StandardMarker marker, IPackState packState) {
            return new ResetGuidModifier(attributes.TryGetAttribute(PRIMARY_ATTR_NAME, out var valueAttr) ? valueAttr.GetValueAsGuids() : Enumerable.Empty<Guid>(),
                                         marker,
                                         packState);
        }

        public void Interact(bool autoTriggered) {
            if (_pathingEntity.BehaviorFiltered) {
                return;
            }

            foreach (var guid in this.TargetGuids) {
                _packState.BehaviorStates.ClearHiddenBehavior(guid);
            }
        }


        public void Focus() {
            // TODO: Translate "Will unhide {0} marker(s)"
            _packState.UiStates.Interact.ShowInteract(_pathingEntity, $"Will unhide {this.TargetGuids.Count} marker{(this.TargetGuids.Count == 1 ? "" : "s")} {{0}}");
        }

        public void Unfocus() {
            _packState.UiStates.Interact.DisconnectInteract(_pathingEntity);
        }

    }
}
