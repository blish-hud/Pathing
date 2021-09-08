using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        private const string ATTR_RESETLENGTH = "resetlength";

        // TacO default is 0.
        public float ResetLength { get; set; } = 0f;

        /// <summary>
        /// resetlength
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_TacOMisc(AttributeCollection collection, IPackResourceManager resourceManager) {
            { if (collection.TryPopAttribute(ATTR_RESETLENGTH, out var attribute)) this.ResetLength = attribute.GetValueAsFloat(); }
        }

    }
}
