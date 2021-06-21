using System;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Behavior.Filter {
    public class StandardBehaviorFilter : Behavior<StandardMarker>, ICanFilter, ICanInteract {

        public const string PRIMARY_ATTR_NAME = "behavior";

        private readonly StandardPathableBehavior _behaviorMode;
        private readonly BehaviorStates           _behaviorStates;

        public StandardBehaviorFilter(StandardPathableBehavior behaviorMode, BehaviorStates behaviorStates, StandardMarker marker) : base(marker) {
            _behaviorMode   = behaviorMode;
            _behaviorStates = behaviorStates;
        }

        public bool IsFiltered() {
            return _behaviorMode != StandardPathableBehavior.OnceDailyPerCharacter
                       ? _behaviorStates.IsBehaviorHidden(_behaviorMode, _pathingEntity.Guid)
                       : _behaviorStates.IsBehaviorHidden(_behaviorMode, _pathingEntity.Guid.Xor(GameService.Gw2Mumble.PlayerCharacter.Name.ToGuid()));
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes, BehaviorStates behaviorStates, StandardMarker marker) {
            return new StandardBehaviorFilter(attributes[PRIMARY_ATTR_NAME].GetValueAsEnum<StandardPathableBehavior>(), behaviorStates, marker);
        }

        public void Interact(bool autoTriggered) {
            switch (_behaviorMode) {
                case StandardPathableBehavior.ReappearOnMapChange:         // TacO Behavior 1
                case StandardPathableBehavior.OnlyVisibleBeforeActivation: // TacO Behavior 3
                case StandardPathableBehavior.OncePerInstance:             // TacO Behavior 6
                    _behaviorStates.AddFilteredBehavior(_behaviorMode, _pathingEntity.Guid);
                    break;
                case StandardPathableBehavior.ReappearOnDailyReset: // TacO Behavior 2
                    _behaviorStates.AddFilteredBehavior(_pathingEntity.Guid, DateTime.UtcNow.Date.AddDays(1));
                    break;
                case StandardPathableBehavior.ReappearAfterTimer: // TacO Behavior 4
                    _behaviorStates.AddFilteredBehavior(_pathingEntity.Guid, DateTime.UtcNow.AddSeconds(_pathingEntity.ResetLength));
                    break;
                case StandardPathableBehavior.OnceDailyPerCharacter: // TacO Behavior 7
                    _behaviorStates.AddFilteredBehavior(_pathingEntity.Guid.Xor(GameService.Gw2Mumble.PlayerCharacter.Name.ToGuid()), DateTime.UtcNow.Date.AddDays(1));
                    break;
                case StandardPathableBehavior.ReappearOnWeeklyReset: // Blish HUD Behavior 101
                    var now           = DateTime.UtcNow;
                    var sevenThirtyAm = new TimeSpan(7, 30, 0);

                    int daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;

                    if (daysUntilMonday == 0 && now.TimeOfDay > sevenThirtyAm) {
                        // account for when today is Monday.
                        daysUntilMonday = 7;
                    }

                    _behaviorStates.AddFilteredBehavior(_pathingEntity.Guid, now.Date.AddDays(daysUntilMonday).Add(sevenThirtyAm));
                    break;
            }
        }

    }
}
