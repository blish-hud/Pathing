using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Behavior.Modifier {
    public class InfoModifier : Behavior<StandardMarker>, ICanFocus {

        public const  string PRIMARY_ATTR_NAME = "info";

        private readonly IPackState _packState;

        private string _infoValue;
        public string InfoValue {
            get => _infoValue;
            set {
                if (string.Equals(_infoValue, value)) return;

                _packState.UiStates.RemoveInfoString(_infoValue);
                _infoValue = value;

                if (_inFocus) {
                    Focus();
                }
            }
        }

        private bool _inFocus = false;

        public InfoModifier(StandardMarker pathingEntity, string value, IPackState packState) : base(pathingEntity) {
            _packState = packState;

            this.InfoValue = value;
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes, StandardMarker marker, IPackState packState) {
            var hasInfoAttr = attributes.TryGetAttribute(PRIMARY_ATTR_NAME, out var valueAttr);

            return hasInfoAttr ? new InfoModifier(marker, valueAttr.GetValueAsString(), packState)
                               : null;
        }

        public void Focus() {
            if (_pathingEntity.BehaviorFiltered) {
                return;
            }

            _inFocus = true;
            _packState.UiStates.AddInfoString(this.InfoValue);
        }

        public void Unfocus() {
            _inFocus = false;
            _packState.UiStates.RemoveInfoString(this.InfoValue);
        }

        public override void Unload() {
            _packState.UiStates.RemoveInfoString(this.InfoValue);
        }

    }
}
