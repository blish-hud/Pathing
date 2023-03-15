using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BhModule.Community.Pathing.Scripting.Lib {
    public class User {

        private readonly PathingGlobal _global;

        public User(PathingGlobal global) {
            _global = global;
        }

        public async Task<bool> SetClipboard(string value) {

            // TODO: Check if user has copy to clipboard enabled
            //IPackState _packState = idk how to get pack state here :)
            //if (_packState.UserConfiguration.PackMarkerConsentToClipboard.Value != MarkerClipboardConsentLevel.Always) return false;

            try {
                Task<bool> copyTask = Blish_HUD.ClipboardUtil.WindowsClipboardService.SetTextAsync(value).ContinueWith(t => {
                     if (t.IsCompleted && t.Result) 
                     {
                         ScreenNotification.ShowNotification(
                                                             $"Copied {value} to clipboard",
                                                             ScreenNotification.NotificationType.Info,
                                                             null,
                                                             2
                                                            );
                         return true;
                     }

                     return false;
                });

                copyTask.Wait();

                return copyTask.Result;

            } catch (Exception ex) {
                //needs exception cleanup
                return false;
            }
        }

    }
}
