using System;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;

namespace BhModule.Community.Pathing.Utility {
    internal static class ClipboardHelper {

        private static readonly Logger Logger = Logger.GetLogger(typeof(ClipboardHelper));

        private const int NOTIFICATION_DURATION = 2;

        public static async Task<bool> TryCopyAsync(string value) {
            try {
                return await ClipboardUtil.WindowsClipboardService.SetTextAsync(value).ConfigureAwait(false);
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to copy value to the clipboard.");
                return false;
            }
        }
        public static void CopyWithNotification(string value, string notificationMessage) {
            _ = CopyWithNotificationAsync(value, notificationMessage);
        }

        private static async Task CopyWithNotificationAsync(string value, string notificationMessage) {
            try {
                if (!await TryCopyAsync(value).ConfigureAwait(false) || string.IsNullOrWhiteSpace(notificationMessage)) {
                    return;
                }

                ThreadUtil.RunOnMainThread(() => ScreenNotification.ShowNotification(notificationMessage,
                                                                                      ScreenNotification.NotificationType.Info,
                                                                                      null,
                                                                                      NOTIFICATION_DURATION));
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to show clipboard notification.");
            }
        }

        public static string FormatMessage(string messageFormat, string value) {
            if (string.IsNullOrEmpty(messageFormat)) return messageFormat;

            try {
                return string.Format(messageFormat, value);
            } catch (FormatException) {
                return messageFormat;
            }
        }

    }
}
