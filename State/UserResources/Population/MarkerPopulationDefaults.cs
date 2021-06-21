using System;
using BhModule.Community.Pathing.Entity;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.State.UserResources.Population {
    public class MarkerPopulationDefaults {

        public float         Alpha             { get; set; } = 1.0f;
        public Color         Tint              { get; set; } = Color.White;
        public CullDirection Cull              { get; set; } = CullDirection.None;
        public float         FadeNear          { get; set; } = -1f;
        public float         FadeFar           { get; set; } = -1f;
        public Guid          Guid              { get; set; } = Guid.Empty;
        public float         HeightOffset      { get; set; } = 1.5f;
        public float         IconSize          { get; set; } = 1.0f;
        public bool          MiniMapVisibility { get; set; } = true;
        public bool          MapVisibility     { get; set; } = true;
        public bool          InGameVisibility  { get; set; } = true;
        public float         MinSize           { get; set; } = 5f;
        public float         MaxSize           { get; set; } = 2048f;
        public Vector3       RotateXyz         { get; set; } = Vector3.Zero;
        public Color         TitleColor        { get; set; } = Color.White;
        public bool          CanFade           { get; set; } = true;

    }
}
