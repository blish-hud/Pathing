using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Editor.TypeConverters;
using BhModule.Community.Pathing.Editor.TypeEditors;
using BhModule.Community.Pathing.Utility;
using Microsoft.Xna.Framework;
using TmfLib;
using AttributeCollection = TmfLib.Prototype.AttributeCollection;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_TEXT       = "text"; // TacO uses this for WvW markers
        private const string ATTR_TITLE      = "title";
        private const string ATTR_TITLECOLOR = "title-color";

        [DisplayName("Title")]
        [Category("Appearance")]
        public string BillboardText { get; set; }

        [DisplayName("Title-Color")]
        [Category("Appearance")]
        [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ColorConverter))]
        public Color  BillboardTextColor { get; set; }

        /// <summary>
        /// text, title, title-color
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_Title(AttributeCollection collection, IPackResourceManager resourceManager) {
            this.BillboardTextColor = _packState.UserResourceStates.Population.MarkerPopulationDefaults.TitleColor;

            { if (collection.TryPopAttribute(ATTR_TEXT,       out var attribute)) this.BillboardText      = attribute.GetValueAsString(); }
            { if (collection.TryPopAttribute(ATTR_TITLE,      out var attribute)) this.BillboardText      = attribute.GetValueAsString(); }
            { if (collection.TryPopAttribute(ATTR_TITLECOLOR, out var attribute)) this.BillboardTextColor = attribute.GetValueAsColor(this.BillboardTextColor); }
        }

    }
}
