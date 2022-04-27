﻿using BhModule.Community.Pathing.Utility;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        private const string ATTR_TRAILSCALE = "trailscale";

        public float TrailScale { get; set; }

        /// <summary>
        /// trailscale
        /// </summary>
        private void Populate_TrailScale(AttributeCollection collection, IPackResourceManager resourceManager) {
            this.TrailScale = _packState.UserResourceStates.Population.TrailPopulationDefaults.TrailScale;

            { if (collection.TryPopAttribute(ATTR_TRAILSCALE, out var attribute)) this.TrailScale = attribute.GetValueAsFloat(this.TrailScale); }
        }

    }
}
