using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using Microsoft.Xna.Framework;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Behavior.Filter {
    public class AchievementFilter : IBehavior, ICanFilter, ICanInteract {

        public const string PRIMARY_ATTR_NAME = "achievement";
        public const string ATTR_ID           = PRIMARY_ATTR_NAME + "id";
        public const string ATTR_BIT          = PRIMARY_ATTR_NAME + "bit";

        private readonly IPackState _packState;

        private bool _triggered = false;

        public int AchievementId  { get; set; }
        public int AchievementBit { get; set; }

        public AchievementFilter(int achievementId, int achievementBit, IPackState packState) {
            this.AchievementId  = achievementId;
            this.AchievementBit = achievementBit;

            _packState = packState;
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes, IPackState packState) {
            return new AchievementFilter(attributes.TryGetAttribute(ATTR_ID,  out var idAttr) ? idAttr.GetValueAsInt() : 0,
                                         attributes.TryGetAttribute(ATTR_BIT, out var bitAttr) ? bitAttr.GetValueAsInt() : -1,
                                         packState);
        }

        public void Update(GameTime gameTime) { /* NOOP */ }

        public void Unload() { /* NOOP */ }

        public bool IsFiltered() {
            return _triggered || _packState.AchievementStates.IsAchievementHidden(this.AchievementId, this.AchievementBit);
        }

        public void Interact(bool autoTriggered) {
            _triggered = true;
        }

    }
}
