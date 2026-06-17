using Blish_HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BhModule.Community.Pathing.Utility {
    internal static class ThreadUtil {

        public static void RunOnMainThread(Action call) {
            if (Program.IsMainThread) {
                call();
            } else {
                GameService.Overlay.QueueMainThreadUpdate((_) => call());
            }
        }

    }
}
