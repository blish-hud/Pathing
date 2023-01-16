using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BhModule.Community.Pathing.Scripting.Extensions {
    internal static class GuidExtensions {

        public static string ToBase64(this Guid guid) {
            return Utility.GuidExtensions.ToBase64String(guid);
        }

    }
}
