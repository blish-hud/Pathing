using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace BhModule.Community.Pathing.State.UserResources {

    /// <summary>
    /// Ignore defaults which defines which maps certain features are ignored on.
    /// </summary>
    public class IgnoreDefaults {

        public const string FILENAME = "ignore.yaml";

        [YamlMember(Description = "A set of maps to not render marker/trails onto the minimap.")]
        public HashSet<int> Compass { get; set; } = new(new[] {
            935, // SAB Lobby
            895, // SAB World 1
            934, // SAB World 2
        });

        [YamlMember(Description = "A set of maps to not render marker/trails onto the fullscreen map.")]
        public HashSet<int> Map { get; set; } = new(new[] {
            935, // SAB Lobby
            895, // SAB World 1
            934, // SAB World 2
        });

        [YamlMember(Description = "A set of maps to not render marker/trails in-game.")]
        public HashSet<int> World { get; set; } = new(Enumerable.Empty<int>());

    }
}
