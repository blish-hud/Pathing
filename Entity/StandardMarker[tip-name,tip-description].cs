using System.ComponentModel;
using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using AttributeCollection = TmfLib.Prototype.AttributeCollection;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker : IHasMapInfo {

        private const string ATTR_TIP         = "tip";
        private const string ATTR_NAME        = ATTR_TIP + "-name";
        private const string ATTR_DESCRIPTION = ATTR_TIP + "-description";

        [DisplayName("Tip-Name")]
        [Category("Appearance")]
        public string TipName { get; set; }

        [DisplayName("Tip-Description")]
        [Category("Appearance")]
        public string TipDescription { get; set; }

        /// <summary>
        /// tip-name, tip-description
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_Tip(AttributeCollection collection, IPackResourceManager resourceManager) {
            { if (collection.TryPopAttribute(ATTR_NAME,        out var attribute)) this.TipName        = attribute.GetValueAsString(); }
            { if (collection.TryPopAttribute(ATTR_DESCRIPTION, out var attribute)) this.TipDescription = attribute.GetValueAsString(); }
        }

    }
}
