using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        private const string ATTR_TRIGGERRANGE = "triggerrange";

        // TacO default is 2.
        public override float TriggerRange { get; set; } = 2f;

        /// <summary>
        /// triggerrange
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_Triggers(AttributeCollection collection, IPackResourceManager resourceManager) {
            { if (collection.TryPopAttribute(ATTR_TRIGGERRANGE, out var attribute)) this.TriggerRange = attribute.GetValueAsFloat(); }
        }

    }
}
