using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.Content;
using BhModule.Community.Pathing.State;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Neo.IronLua;
using TmfLib.Attributes;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail : PathingEntity {

        private static readonly Logger Logger = Logger.GetLogger<StandardTrail>();

        public override float DrawOrder => float.MaxValue;

        public TextureResourceManager TextureResourceManager { get; }

        internal Vector3[][] _sectionPoints;

        public StandardTrail(IPackState packState, PathableSource source) : base(packState, source) {
            this.TextureResourceManager = TextureResourceManager.GetTextureResourceManager(source.ResourceManager);

            Initialize(source);
        }

        private void Populate(AttributeReader collection, TextureResourceManager resourceManager) {
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

        private void Initialize(PathableSource source) {
            Populate(source.Attributes, this.TextureResourceManager);

            var trailData = source.TrailData;

            if (trailData != null) {
                var rawSections   = new Vector3[trailData.SectionCount][];
                var trailSections = new Vector3[trailData.SectionCount][];

                for (int s = 0; s < rawSections.Length; s++) {
                    var points  = trailData.GetSection(s);
                    var section = new Vector3[points.Length];

                    for (int i = 0; i < points.Length; i++) {
                        section[i] = new Vector3(points[i].X, points[i].Y, points[i].Z);
                    }

                    rawSections[s]   = section;
                    trailSections[s] = PostProcessing_DouglasPeucker(section, _packState.UserResourceStates.Advanced.MapTrailDouglasPeuckerError).ToArray();
                }

                _sectionPoints = trailSections;

                BuildBuffers(rawSections);
            } else {
                _sectionBuffers = Array.Empty<SectionBuffer>();
            }

            this.FadeIn();
        }

    }
}
