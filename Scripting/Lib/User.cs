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

        public bool SetClipboard(string value) {
            Blish_HUD.ClipboardUtil.WindowsClipboardService.SetTextAsync(value);

            return true; // TODO: Indicate if the value was set or not.
        }

    }
}
