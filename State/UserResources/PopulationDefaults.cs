using BhModule.Community.Pathing.State.UserResources.Population;

namespace BhModule.Community.Pathing.State.UserResources {

    /// <summary>
    /// Population defaults which defines the default values for pathable attributes when a value isn't provided.
    /// </summary>
    public class PopulationDefaults {

        public const string FILENAME = "populate.yaml";

        public MarkerPopulationDefaults MarkerPopulationDefaults { get; set; } = new();

        public TrailPopulationDefaults TrailPopulationDefaults { get; set; } = new();

    }
}
