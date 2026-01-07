using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.Content;
using BhModule.Community.Pathing.State;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Neo.IronLua;
using TmfLib.Pathable;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail : PathingEntity {

        private static readonly Logger Logger = Logger.GetLogger<StandardTrail>();

        public override float DrawOrder => float.MaxValue;

        public TextureResourceManager TextureResourceManager { get; }

        internal Vector3[][] _sectionPoints;

        public StandardTrail(IPackState packState, ITrail trail) : base(packState, trail) {
            this.TextureResourceManager = TextureResourceManager.GetTextureResourceManager(trail.ResourceManager);

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
            // Populate_EditTag(collection, resourceManager);
        }

        private void Initialize(ITrail trail) {
            Populate(trail.GetAggregatedAttributes(), TextureResourceManager.GetTextureResourceManager(trail.ResourceManager));

            if (trail.TrailSections != null) { 
                var trailSections = new List<Vector3[]>(trail.TrailSections.Count());
                foreach (var trailSection in trail.TrailSections) {
                    trailSections.Add(PostProcessing_DouglasPeucker(trailSection.TrailPoints.Select(v => new Vector3(v.X, v.Y, v.Z)), _packState.UserResourceStates.Advanced.MapTrailDouglasPeuckerError).ToArray());
                }

                _sectionPoints = trailSections.ToArray();

                BuildBuffers(trail);
            } else {
                _sectionBuffers = Array.Empty<VertexBuffer>();
            }

            this.FadeIn();
        }

    }
}
