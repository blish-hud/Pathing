using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using Microsoft.Xna.Framework;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Behavior.Filter {
    public class AchievementFilter : IBehavior, ICanFilter, ICanInteract {

        public const  string PRIMARY_ATTR_NAME = "achievement";
        private const string ATTR_ID           = PRIMARY_ATTR_NAME + "id";
        private const string ATTR_BIT          = PRIMARY_ATTR_NAME + "bit";

        private readonly BehaviorStates _behaviorStates;

        private bool _triggered = false;

        public int AchievementId  { get; set; }
        public int AchievementBit { get; set; }

        public AchievementFilter(int achievementId, int achievementBit, BehaviorStates behaviorStates) {
            this.AchievementId  = achievementId;
            this.AchievementBit = achievementBit;

            _behaviorStates = behaviorStates;
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes, BehaviorStates behaviorStates) {
            return new AchievementFilter(attributes.TryGetAttribute(ATTR_ID,  out var idAttr) ? idAttr.GetValueAsInt() : 0,
                                         attributes.TryGetAttribute(ATTR_BIT, out var bitAttr) ? bitAttr.GetValueAsInt() : -1,
                                         behaviorStates);
        }

        public void Update(GameTime gameTime) { /* NOOP */ }

        public bool IsFiltered() {
            return _triggered || _behaviorStates.IsAchievementHidden(this.AchievementId, this.AchievementBit);
        }

        public void Interact(bool autoTriggered) {
            _triggered = true;
        }

    }
}
