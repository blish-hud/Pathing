using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Controls;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Behavior.Modifier {
    public class CopyModifier : Behavior<StandardMarker>, ICanInteract, ICanFocus {

        public const string PRIMARY_ATTR_NAME = "copy";
        public const string ATTR_MESSAGE      = PRIMARY_ATTR_NAME + "-message";

        public const string DEFAULT_COPYMESSAGE = "'{0}' copied to clipboard.";

        private readonly IPackState _packState;

        public string CopyValue   { get; set; }
        public string CopyMessage { get; set; }

        private double _lastTrigger = 0;

        public CopyModifier(string value, string message, StandardMarker marker, IPackState packState) : base(marker) {
            _packState = packState;

            this.CopyValue   = value;
            this.CopyMessage = message;
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes, StandardMarker marker, IPackState packState) {
            return new CopyModifier(attributes.TryGetAttribute(PRIMARY_ATTR_NAME, out var valueAttr) ? valueAttr.GetValueAsString() : "",
                                    attributes.TryGetAttribute(ATTR_MESSAGE,      out var messageAttr) ? messageAttr.GetValueAsString() : DEFAULT_COPYMESSAGE,
                                    marker,
                                    packState);
        }

        public void Interact(bool autoTriggered) {
            if (_packState.UserConfiguration.PackMarkerConsentToClipboard.Value == MarkerClipboardConsentLevel.Never
             || (_packState.UserConfiguration.PackMarkerConsentToClipboard.Value == MarkerClipboardConsentLevel.OnlyWhenInteractedWith && autoTriggered))
                return;

            if (_pathingEntity.BehaviorFiltered) {
                return;
            }

            // Provide a bit of a debounce
            if (autoTriggered && GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds - _lastTrigger < _packState.UserResourceStates.Advanced.CopyAttributeRechargeMs) return;

            _lastTrigger = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;

            if (string.IsNullOrEmpty(this.CopyValue)) {
                return;
            }

            ClipboardUtil.WindowsClipboardService.SetTextAsync(this.CopyValue).ContinueWith(t => {
                  if (t.IsCompleted && t.Result) {
                      ScreenNotification.ShowNotification(string.Format(this.CopyMessage, this.CopyValue),
                                                          ScreenNotification.NotificationType.Info,
                                                          null,
                                                          2);
                  }
            });
        }


        public void Focus() {
            // TODO: Translate "Copy Text {0}"
            _packState.UiStates.Interact.ShowInteract(_pathingEntity, $"Copy '{this.CopyValue}' to clipboard {{0}}");
        }

        public void Unfocus() {
            _packState.UiStates.Interact.DisconnectInteract(_pathingEntity);
        }

        public override void Unload() {
            _packState.UiStates.Interact.DisconnectInteract(_pathingEntity);
        }

    }
}
