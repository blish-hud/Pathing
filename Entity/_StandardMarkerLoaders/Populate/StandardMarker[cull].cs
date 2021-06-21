using System;
using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using Microsoft.Xna.Framework.Graphics;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_CULL = "cull";

        public RasterizerState CullDirection { get; set; } = RasterizerState.CullNone;

        /// <summary>
        /// cull
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_Cull(AttributeCollection collection, IPackResourceManager resourceManager) {
            {
                var cullDirection = _packState.UserResourceStates.Population.MarkerPopulationDefaults.Cull;

                if (collection.TryPopAttribute(ATTR_CULL, out var attribute)) {
                    cullDirection = attribute.GetValueAsEnum<CullDirection>();
                }

                this.CullDirection = cullDirection switch {
                    Entity.CullDirection.None => RasterizerState.CullNone,
                    Entity.CullDirection.Clockwise => RasterizerState.CullClockwise,
                    Entity.CullDirection.CounterClockwise => RasterizerState.CullCounterClockwise,
                    _ => this.CullDirection
                };
            }
        }

    }
}
