using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.UI.Tooltips;
using BhModule.Community.Pathing.Utility;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Behavior.Filter {
    public class AchievementFilter : Behavior<IPathingEntity>, ICanFilter, ICanInteract, ICanFocus {

        public const string PRIMARY_ATTR_NAME = "achievement";
        public const string ATTR_ID           = PRIMARY_ATTR_NAME + "id";
        public const string ATTR_BIT          = PRIMARY_ATTR_NAME + "bit";

        private readonly IPackState _packState;

        private bool _triggered = false;

        public int AchievementId  { get; set; }
        public int AchievementBit { get; set; }

        public AchievementFilter(int achievementId, int achievementBit, IPathingEntity pathingEntity, IPackState packState) : base(pathingEntity) {
            this.AchievementId  = achievementId;
            this.AchievementBit = achievementBit;

            _packState = packState;
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes, IPathingEntity pathingEntity, IPackState packState) {
            return new AchievementFilter(attributes.TryGetAttribute(ATTR_ID,  out var idAttr) ? idAttr.GetValueAsInt() : 0,
                                         attributes.TryGetAttribute(ATTR_BIT, out var bitAttr) ? bitAttr.GetValueAsInt() : -1,
                                         pathingEntity,
                                         packState);
        }

        public bool IsFiltered() {
            return _triggered || _packState.AchievementStates.IsAchievementHidden(this.AchievementId, this.AchievementBit);
        }

        public void Interact(bool autoTriggered) {
            _packState.UiStates.Interact.DisconnectInteract(_pathingEntity);
            _triggered = true;
        }

        public void Focus() {
            _packState.UiStates.Interact.ShowInteract(_pathingEntity, new AchievementTooltipView(this.AchievementId));
        }

        public void Unfocus() {
            _packState.UiStates.Interact.DisconnectInteract(_pathingEntity);
        }

        public override void Unload() {
            _packState.UiStates.Interact.DisconnectInteract(_pathingEntity);
        }

    }
}
