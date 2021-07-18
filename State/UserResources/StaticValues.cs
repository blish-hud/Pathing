namespace BhModule.Community.Pathing.State.UserResources {

    /// <summary>
    /// Static values which define normally static values within the module.
    /// </summary>
    public class StaticValues {

        public const string FILENAME = "static.yaml";

        /// <summary>
        /// The error used when calculating trail segments for the compass map.
        /// </summary>
        public float MapTrailDouglasPeuckerError { get; set; } = 0.2f;

        /// <summary>
        /// The number of pixels to sample in the texture to decide what color to use.
        /// </summary>
        public int MapTrailColorSamples { get; set; } = 24;

    }
}
