using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_TYPE = "type";

        public override string CategoryNamespace { get; set; }

        /// <summary>
        /// type
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_Type(AttributeCollection collection, IPackResourceManager resourceManager) {
            { if (collection.TryPopAttribute(ATTR_TYPE, out var attribute)) this.CategoryNamespace = attribute.GetValueAsString(); }
        }

    }
}
