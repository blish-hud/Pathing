using BhModule.Community.Pathing.Entity;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.State.UserResources.Population {
    public class TrailPopulationDefaults {

        public float         Alpha             { get; set; } = 1f;          // https://github.com/BoyC/GW2TacO/blob/a10c305105a2a81cd15c443e2c00a6fda8355231/gw2tactical.h#L66
        public float         AnimSpeed         { get; set; } = 1f;          // https://github.com/BoyC/GW2TacO/blob/a10c305105a2a81cd15c443e2c00a6fda8355231/gw2tactical.h#L71
        public Color         Tint              { get; set; } = Color.White; // https://github.com/BoyC/GW2TacO/blob/a10c305105a2a81cd15c443e2c00a6fda8355231/gw2tactical.h#L78
        public CullDirection Cull              { get; set; } = CullDirection.None;
        public float         FadeNear          { get; set; } = 700f;
        public float         FadeFar           { get; set; } = 900f;
        public bool          MiniMapVisibility { get; set; } = true;
        public bool          MapVisibility     { get; set; } = true;
        public bool          InGameVisibility  { get; set; } = true;
        public float         TrailScale        { get; set; } = 1.0f;

    }
}
