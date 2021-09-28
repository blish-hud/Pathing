using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_MINSIZE = "minsize";
        private const string ATTR_MAXSIZE = "maxsize";

        public float MinSize { get; set; } = 0f;
        public float MaxSize { get; set; } = float.MaxValue;

        /// <summary>
        /// minsize, maxsize
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_MinMaxSize(AttributeCollection collection, IPackResourceManager resourceManager) {
            this.MinSize = _packState.UserResourceStates.Population.MarkerPopulationDefaults.MinSize;
            this.MaxSize = _packState.UserResourceStates.Population.MarkerPopulationDefaults.MaxSize;

            { if (collection.TryPopAttribute(ATTR_MINSIZE, out var attribute)) this.MinSize = attribute.GetValueAsFloat(this.MinSize); }
            { if (collection.TryPopAttribute(ATTR_MAXSIZE, out var attribute)) this.MaxSize = attribute.GetValueAsFloat(this.MaxSize); }
        }

    }
}
