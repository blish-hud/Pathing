using System;

namespace BhModule.Community.Pathing.State.UserResources {

    /// <summary>
    /// Additional advanced settings which do not have a place in the UI yet.
    /// </summary>
    public class AdvancedDefaults {

        public const string FILENAME = "advanced.yaml";

        public string[] MarkerLoadPaths { get; set; } = Array.Empty<string>();

        public double CopyAttributeRechargeMs = 8000d;

    }
}
