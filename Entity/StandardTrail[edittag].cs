using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using TmfLib.Attributes;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        private const string ATTR_EDITTAG = "edittag";

        /// <summary>
        /// edittag
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_EditTag(AttributeReader collection, IPackResourceManager resourceManager) {
            { if (collection.TryPopAttribute(ATTR_EDITTAG, out var attribute)) this.EditTag = attribute.GetValueAsInt(); }
        }

    }
}
