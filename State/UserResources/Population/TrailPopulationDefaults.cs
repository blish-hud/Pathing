using BhModule.Community.Pathing.Entity;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.State.UserResources.Population {
    public class TrailPopulationDefaults {

        public float         Alpha             { get; set; } = 1f;
        public float         AnimSpeed         { get; set; } = 1f;
        public Color         Tint              { get; set; } = Color.White;
        public CullDirection Cull              { get; set; } = CullDirection.None;
        public float         FadeNear          { get; set; } = 700f;
        public float         FadeFar           { get; set; } = 900f;
        public bool          MiniMapVisibility { get; set; } = true;
        public bool          MapVisibility     { get; set; } = true;
        public bool          InGameVisibility  { get; set; } = true;
        public float         TrailScale        { get; set; } = 1.0f;

    }
}
