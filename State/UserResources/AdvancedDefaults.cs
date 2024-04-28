using System;
using YamlDotNet.Serialization;

namespace BhModule.Community.Pathing.State.UserResources {

    /// <summary>
    /// Additional advanced settings which do not have a place in the UI yet.
    /// </summary>
    public class AdvancedDefaults {

        public const string FILENAME = "advanced.yaml";

        // Marker Packs
        [YamlMember(Description = "A set of directory paths to also check for marker packs to load.")]
        public string[] MarkerLoadPaths     { get; set; } = Array.Empty<string>();

        [YamlMember(Description = "If marker packs should be automatically optimized when they are downloaded.")]
        public bool OptimizeMarkerPacks { get; set; } = true;

        // Copy Attribute
        [YamlMember(Description = "The debounce time for the copy attribute to auto trigger in milliseconds.")]
        public double CopyAttributeRechargeMs { get; set; } = 8000d;

        // Interact-Gear
        [YamlMember(Description = "The X offset (% of screen width) of the interact icon.")]
        public float InteractGearXOffset   { get; set; } = 0.62f;

        [YamlMember(Description = "The Y offset (% of screen height) of the interact icon.")]
        public float InteractGearYOffset   { get; set; } = 0.58f;

        [YamlMember(Description = "If the interact icon should spin or not.")]
        public bool InteractGearAnimation { get; set; } = true;

        // Info Window
        [YamlMember(Description = "The X offset of the info window in pixels.")]
        public int InfoWindowXOffsetPixels { get; set; } = 300;

        [YamlMember(Description = "The Y offset of the info window in pixels.")]
        public int InfoWindowYOffsetPixels { get; set; } = 200;

        // Map
        [YamlMember(Description = "If the early hide feature based on camera movement should be used when the map closes.")]
        public bool MapTriggerHideFromCamera { get; set; } = true;

        [YamlMember(Description = "The Douglas Peucker error used by the trails shown on the minimap.")]
        public float MapTrailDouglasPeuckerError { get; set; } = 0.2f;

        // Category DisplayName Truncation
        [YamlMember(Description = "The width in pixels that category names will be truncated to if 'Truncate Long Category Names' is enabled.")]
        public int CategoryNameTruncateWidth { get; set; } = 225;

        // Trails
        [YamlMember(Description = "A multiplier which changes the fade radius around the character.")]
        public float CharacterTrailFadeMultiplier { get; set; } = 1f;

    }
}
