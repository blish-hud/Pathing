using BhModule.Community.Pathing.Behavior.Modifier;
using Blish_HUD;
using Blish_HUD.Controls;

namespace BhModule.Community.Pathing.Scripting.Lib {
    public class User {

        private readonly PathingGlobal _global;

        public User(PathingGlobal global) {
            _global = global;
        }

        public bool SetClipboard(string value) {
            return SetClipboard(value, string.Format(CopyModifier.DEFAULT_COPYMESSAGE, value));
        }

        public bool SetClipboard(string value, string message) {
            if (_global.ScriptEngine.Module.Settings.PackMarkerConsentToClipboard.Value == MarkerClipboardConsentLevel.Never) {
                // The player has disabled clipboard access.
                return false;
            }

            ClipboardUtil.WindowsClipboardService.SetTextAsync(value).ContinueWith(t => {
                if (t.IsCompleted && t.Result) {
                    ScreenNotification.ShowNotification(message,
                                                        ScreenNotification.NotificationType.Info,
                                                        null,
                                                        2);
                }
            });

            // TODO: Evaluate our options for handling async methods that get called from Lua.  Currenly we just fire it and let it go.
            return true;
        }

    }
}
