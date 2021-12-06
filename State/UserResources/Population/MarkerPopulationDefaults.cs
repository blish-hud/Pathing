using System;
using BhModule.Community.Pathing.Entity;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.State.UserResources.Population {
    public class MarkerPopulationDefaults {

        public float         Alpha              { get; set; } = 1f;          // https://github.com/BoyC/GW2TacO/blob/a10c305105a2a81cd15c443e2c00a6fda8355231/gw2tactical.h#L66
        public Color         Tint               { get; set; } = Color.White; // https://github.com/BoyC/GW2TacO/blob/a10c305105a2a81cd15c443e2c00a6fda8355231/gw2tactical.h#L78
        public float         FadeNear           { get; set; } = -1f;         // https://github.com/BoyC/GW2TacO/blob/a10c305105a2a81cd15c443e2c00a6fda8355231/gw2tactical.h#L67
        public float         FadeFar            { get; set; } = -1f;         // https://github.com/BoyC/GW2TacO/blob/a10c305105a2a81cd15c443e2c00a6fda8355231/gw2tactical.h#L68
        public float         HeightOffset       { get; set; } = 1.5f;        // https://github.com/BoyC/GW2TacO/blob/a10c305105a2a81cd15c443e2c00a6fda8355231/gw2tactical.h#L69
        public float         IconSize           { get; set; } = 1f;          // https://github.com/BoyC/GW2TacO/blob/a10c305105a2a81cd15c443e2c00a6fda8355231/gw2tactical.h#L65
        public float         MinSize            { get; set; } = 5f;          // https://github.com/BoyC/GW2TacO/blob/a10c305105a2a81cd15c443e2c00a6fda8355231/gw2tactical.h#L81
        public float         MaxSize            { get; set; } = 2048f;       // https://github.com/BoyC/GW2TacO/blob/a10c305105a2a81cd15c443e2c00a6fda8355231/gw2tactical.h#L82
        public float         TriggerRange       { get; set; } = 2f;          // https://github.com/BoyC/GW2TacO/blob/a10c305105a2a81cd15c443e2c00a6fda8355231/gw2tactical.h#L70
        public float         MapDisplaySize     { get; set; } = 20f;         // https://github.com/BoyC/GW2TacO/blob/a10c305105a2a81cd15c443e2c00a6fda8355231/gw2tactical.h#L73
        public bool          MapVisibility      { get; set; } = true;
        public CullDirection Cull               { get; set; } = CullDirection.None;
        public Guid          Guid               { get; set; } = Guid.Empty;
        public bool          MiniMapVisibility  { get; set; } = true;
        public bool          ScaleOnMapWithZoom { get; set; } = true;
        public bool          InGameVisibility   { get; set; } = true;
        public Vector3       RotateXyz          { get; set; } = Vector3.Zero;
        public Color         TitleColor         { get; set; } = Color.White;
        public bool          CanFade            { get; set; } = true;

    }
}
