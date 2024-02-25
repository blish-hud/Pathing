using BhModule.Community.Pathing.Utility;
using System.Runtime.CompilerServices;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_OCCLUDE = "occlude";

        public bool Occlude { get; set; }

        /// <summary>
        /// mask
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_Occlude(AttributeCollection collection, IPackResourceManager resourceManager) {
            { if (collection.TryPopAttribute(ATTR_OCCLUDE, out var attribute)) this.Occlude = attribute.GetValueAsBool(); }
        }

    }
}
