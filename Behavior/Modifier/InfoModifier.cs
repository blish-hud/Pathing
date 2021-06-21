using System;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.UI.Controls;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Behavior.Modifier {
    public class InfoModifier : Behavior<StandardMarker>, ICanFocus {

        public const  string PRIMARY_ATTR_NAME = "info";
        private const string ATTR_RANGE        = PRIMARY_ATTR_NAME + "range";

        private const float  DEFAULT_INFORANGE = 2f;

        private IPackState _packState;

        public string InfoValue { get; set; }

        public InfoModifier(StandardMarker pathingEntity, string value, float range, IPackState packState) : base(pathingEntity) {
            _packState = packState;

            this.InfoValue = value;

            // TODO: Find a way to make this its own range.
            pathingEntity.TriggerRange = range;
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes, StandardMarker marker, IPackState packState) {
            return new InfoModifier(marker,
                                    attributes.TryGetAttribute(PRIMARY_ATTR_NAME, out var valueAttr) ? valueAttr.GetValueAsString() : "",
                                    attributes.TryGetAttribute(ATTR_RANGE, out var rangeAttr) ? rangeAttr.GetValueAsFloat(DEFAULT_INFORANGE) : DEFAULT_INFORANGE,
                                    packState);
        }

        public void Focus() {
            _packState.UiStates.AddInfoString(this.InfoValue);
        }

        public void Unfocus() {
            _packState.UiStates.RemoveInfoString(this.InfoValue);
        }

    }
}
