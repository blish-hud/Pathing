using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        private const string ATTR_TRAILSCALE = "trailscale";

        public float TrailScale { get; set; }

        private void Populate_TrailScale(AttributeCollection collection, IPackResourceManager resourceManager) {
            this.TrailScale = _packState.UserResourceStates.Population.TrailPopulationDefaults.TrailScale;

            { if (collection.TryPopAttribute(ATTR_TRAILSCALE, out var attribute)) this.TrailScale = attribute.GetValueAsFloat(this.TrailScale); }
        }

    }
}
