using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_RESETLENGTH = "resetlength";
        private const string ATTR_AUTOTRIGGER = "autotrigger";

        /// <summary>
        /// Used by behavior 4 to indicate how long the timer should last.
        /// </summary>
        public float ResetLength { get; set; } = 0f;

        /// <summary>
        /// Calls <see cref="Interact"/> when focused.
        /// </summary>
        public bool AutoTrigger { get; set; } = false;

        /// <summary>
        /// resetlength
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_TacOMisc(AttributeCollection collection, IPackResourceManager resourceManager) {
            { if (collection.TryPopAttribute(ATTR_RESETLENGTH, out var attribute)) this.ResetLength = attribute.GetValueAsFloat(); }
            { if (collection.TryPopAttribute(ATTR_AUTOTRIGGER, out var attribute)) this.AutoTrigger = attribute.GetValueAsBool(); }
        }

    }
}
