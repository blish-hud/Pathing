using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using Blish_HUD.Controls;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Behavior.Modifier {
    public class CopyModifier : Behavior<StandardMarker>, ICanInteract {

        public const  string PRIMARY_ATTR_NAME = "copy";
        private const string ATTR_MESSAGE      = PRIMARY_ATTR_NAME + "-message";

        private const string DEFAULT_COPYMESSAGE = "'{0}' copied to clipboard.";

        private readonly IPackState _packState;

        public string CopyValue   { get; set; }
        public string CopyMessage { get; set; }

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

            Blish_HUD.ClipboardUtil.WindowsClipboardService.SetTextAsync(this.CopyValue).ContinueWith(t => {
                  if (t.IsCompleted && t.Result) {
                      ScreenNotification.ShowNotification(string.Format(this.CopyMessage, this.CopyValue),
                                                          ScreenNotification.NotificationType.Info,
                                                          null,
                                                          2);
                  }
            });
        }

    }
}
