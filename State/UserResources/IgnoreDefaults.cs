using System.Collections.Generic;
using System.Linq;

namespace BhModule.Community.Pathing.State.UserResources {

    /// <summary>
    /// Ignore defaults which defines which maps certain features are ignored on.
    /// </summary>
    public class IgnoreDefaults {

        public const string FILENAME = "ignore.yaml";

        public HashSet<int> Compass { get; set; } = new(new[] {
            935, // SAB Lobby
            895, // SAB World 1
            934, // SAB World 2
        });

        public HashSet<int> Map { get; set; } = new(new[] {
            935, // SAB Lobby
            895, // SAB World 1
            934, // SAB World 2
        });

        public HashSet<int> World { get; set; } = new(Enumerable.Empty<int>());

    }
}
