﻿using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.Content;
using BhModule.Community.Pathing.State;
using Blish_HUD;
using Microsoft.Xna.Framework;
using TmfLib.Pathable;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail : PathingEntity {

        private static readonly Logger Logger = Logger.GetLogger<StandardTrail>();

        public override float DrawOrder => float.MaxValue;

        private Vector3[][] _sectionPoints;

        public StandardTrail(IPackState packState, ITrail trail) : base(packState, trail) {
            Initialize(trail);
        }

        private void Populate(AttributeCollection collection, TextureResourceManager resourceManager) {
            Populate_Guid(collection, resourceManager);

            Populate_Alpha(collection, resourceManager);
            Populate_AnimationSpeed(collection, resourceManager);
            Populate_Tint(collection, resourceManager);
            Populate_TrailScale(collection, resourceManager);
            Populate_FadeNearAndFar(collection, resourceManager);
            Populate_Texture(collection, resourceManager);
            Populate_Cull(collection, resourceManager);
            Populate_MapVisibility(collection, resourceManager);
            Populate_CanFade(collection, resourceManager);
            Populate_IsWall(collection, resourceManager);

            Populate_Behaviors(collection, resourceManager);

            // Editor Specific
            Populate_EditTag(collection, resourceManager);
        }
        
        private void Initialize(ITrail trail) {
            var trailSections = new List<Vector3[]>(trail.TrailSections.Count());
            foreach (var trailSection in trail.TrailSections) {
                trailSections.Add(PostProcessing_DouglasPeucker(trailSection.TrailPoints.Select(v => new Vector3(v.X, v.Y, v.Z)), _packState.UserResourceStates.Advanced.MapTrailDouglasPeuckerError).ToArray());
            }

            _sectionPoints = trailSections.ToArray();

            Populate(trail.GetAggregatedAttributes(), TextureResourceManager.GetTextureResourceManager(trail.ResourceManager));

            BuildBuffers(trail);
            
            UpdateMapGlowOpacity(_packState.UserConfiguration.MapDrawOpacity.Value);
            UpdateMiniMapGlowOpacity(_packState.UserConfiguration.MiniMapDrawOpacity.Value);
            _packState.UserConfiguration.MapDrawOpacity.SettingChanged += (_, args) => UpdateMapGlowOpacity(args.NewValue);
            _packState.UserConfiguration.MiniMapDrawOpacity.SettingChanged += (_, args) => UpdateMiniMapGlowOpacity(args.NewValue);

            this.FadeIn();
        }

    }
}
