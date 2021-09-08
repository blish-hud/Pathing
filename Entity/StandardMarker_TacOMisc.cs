using System.ComponentModel;
using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using AttributeCollection = TmfLib.Prototype.AttributeCollection;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_RESETLENGTH = "resetlength";
        private const string ATTR_AUTOTRIGGER = "autotrigger";

        /// <summary>
        /// Used by behavior 4 to indicate how long the timer should last.
        /// </summary>
        [Description("When using behavior 4 (reappear after timer) this value defines, in seconds, the duration until the marker is reset after being activated.")]
        [Category("Behavior")]
        public float ResetLength { get; set; } = 0f;

        /// <summary>
        /// Calls <see cref="Interact"/> when focused.
        /// </summary>
        [Description("If enabled, attributes and behaviors which would normally require an interaction to activate will instead activate automatically when within TriggerRange.")]
        [Category("Behavior")]
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
