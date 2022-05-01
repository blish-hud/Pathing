using System;

namespace BhModule.Community.Pathing.State.UserResources {

    /// <summary>
    /// Additional advanced settings which do not have a place in the UI yet.
    /// </summary>
    public class AdvancedDefaults {

        public const string FILENAME = "advanced.yaml";

        // Marker Packs
        public string[] MarkerLoadPaths { get; set; } = Array.Empty<string>();

        // Copy Attribute
        public double CopyAttributeRechargeMs { get; set; } = 8000d;

        // Interact-Gear
        public float InteractGearXOffset   { get; set; } = 0.62f;
        public float InteractGearYOffset   { get; set; } = 0.58f;
        public bool  InteractGearAnimation { get; set; } = true;

        // Info Window
        public int InfoWindowXOffsetPixels { get; set; } = 300;
        public int InfoWindowYOffsetPixels { get; set; } = 200;

    }
}
