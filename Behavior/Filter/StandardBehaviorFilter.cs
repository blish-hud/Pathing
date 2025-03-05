using System;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using TmfLib.Prototype;
using Humanizer;

namespace BhModule.Community.Pathing.Behavior.Filter {
    public class StandardBehaviorFilter : Behavior<StandardMarker>, ICanFilter, ICanInteract, ICanFocus {

        public const string PRIMARY_ATTR_NAME = "behavior";

        private readonly StandardPathableBehavior _behaviorMode;
        private readonly IPackState               _packState;

        public StandardBehaviorFilter(StandardPathableBehavior behaviorMode, StandardMarker marker, IPackState packState) : base(marker) {
            _behaviorMode = behaviorMode;
            _packState    = packState;
        }

        public bool IsFiltered() {
            if (_pathingEntity.InvertBehavior) {
                return _behaviorMode != StandardPathableBehavior.OnceDailyPerCharacter
                           ? !_packState.BehaviorStates.IsBehaviorHidden(_behaviorMode, _pathingEntity.Guid)
                           : !_packState.BehaviorStates.IsBehaviorHidden(_behaviorMode, _pathingEntity.Guid.Xor(GameService.Gw2Mumble.PlayerCharacter.Name.ToGuid()));
            } else {
                return _behaviorMode != StandardPathableBehavior.OnceDailyPerCharacter
                           ? _packState.BehaviorStates.IsBehaviorHidden(_behaviorMode, _pathingEntity.Guid)
                           : _packState.BehaviorStates.IsBehaviorHidden(_behaviorMode, _pathingEntity.Guid.Xor(GameService.Gw2Mumble.PlayerCharacter.Name.ToGuid()));
            }
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes, StandardMarker marker, IPackState packState) {
            attributes.TryGetAttribute(PRIMARY_ATTR_NAME, out var attribute);
            
            if(attribute == null) return null;

            var behaviorId = (StandardPathableBehavior)attribute.GetValueAsInt(0);

            return behaviorId > 0
                ? new StandardBehaviorFilter(behaviorId, marker, packState)
                : null;
        }

        public string FilterReason() {
            return (_behaviorMode) switch {
                StandardPathableBehavior.ReappearOnMapChange => "Hidden until the next map change because you interacted with it.",
                StandardPathableBehavior.ReappearOnDailyReset => "Hidden until daily reset because you interacted with it.",
                StandardPathableBehavior.OnlyVisibleBeforeActivation => "Hidden permanently because you interacted with it.",
                StandardPathableBehavior.ReappearAfterTimer => "Hidden for a period of time because you interacted with it.",
                StandardPathableBehavior.ReappearOnMapReset => "Hidden until map reset because you interacted with it.",
                StandardPathableBehavior.OncePerInstance => "Hidden until you change map instances because you interacted with it.",
                StandardPathableBehavior.OnceDailyPerCharacter => "Hidden for this character until daily reset because you interacted with it.",
                StandardPathableBehavior.ReappearOnWeeklyReset => "Hidden until weekly reset because you interacted with it.",
                _ => "Unknown"
            };
        }

        public void Interact(bool autoTriggered) {
            switch (_behaviorMode) {
                case StandardPathableBehavior.AlwaysVisible:
                    return;
                case StandardPathableBehavior.ReappearOnMapChange:         // TacO Behavior 1
                case StandardPathableBehavior.OnlyVisibleBeforeActivation: // TacO Behavior 3
                case StandardPathableBehavior.OncePerInstance:             // TacO Behavior 6
                    _packState.BehaviorStates.AddFilteredBehavior(_behaviorMode, _pathingEntity.Guid);
                    break;
                case StandardPathableBehavior.ReappearOnDailyReset: // TacO Behavior 2
                    _packState.BehaviorStates.AddFilteredBehavior(_pathingEntity.Guid, DateTime.UtcNow.Date.AddDays(1));
                    break;
                case StandardPathableBehavior.ReappearAfterTimer: // TacO Behavior 4
                    _packState.BehaviorStates.AddFilteredBehavior(_pathingEntity.Guid, DateTime.UtcNow.AddSeconds(_pathingEntity.ResetLength));
                    break;
                case StandardPathableBehavior.OnceDailyPerCharacter: // TacO Behavior 7
                    _packState.BehaviorStates.AddFilteredBehavior(_pathingEntity.Guid.Xor(GameService.Gw2Mumble.PlayerCharacter.Name.ToGuid()), DateTime.UtcNow.Date.AddDays(1));
                    break;
                case StandardPathableBehavior.ReappearOnWeeklyReset: // Blish HUD Behavior 101
                    var now           = DateTime.UtcNow;
                    var sevenThirtyAm = new TimeSpan(7, 30, 0);

                    int daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;

                    if (daysUntilMonday == 0 && now.TimeOfDay > sevenThirtyAm) {
                        // account for when today is Monday.
                        daysUntilMonday = 7;
                    }

                    _packState.BehaviorStates.AddFilteredBehavior(_pathingEntity.Guid, now.Date.AddDays(daysUntilMonday).Add(sevenThirtyAm));
                    break;
            }

            _packState.UiStates.Interact.DisconnectInteract(_pathingEntity);
        }

        public void Focus() {
            string interactText = null;

            // TODO: Add localization for behavior interact text.
            switch (_behaviorMode) {
                case StandardPathableBehavior.AlwaysVisible:
                    return;
                case StandardPathableBehavior.ReappearOnMapChange:
                    interactText = "Hide marker until map change {0}";
                    break;
                case StandardPathableBehavior.ReappearOnDailyReset:
                    interactText = $"Hide marker until daily reset ({(DateTime.UtcNow.Date.AddDays(1) - DateTime.UtcNow).Humanize(2, minUnit: Humanizer.Localisation.TimeUnit.Second)}) {{0}}";
                    break;
                case StandardPathableBehavior.OnlyVisibleBeforeActivation:
                    interactText = "Hide marker permanently {0}";
                    break;
                case StandardPathableBehavior.ReappearAfterTimer:
                    interactText = $"Hide marker for {TimeSpan.FromSeconds(_pathingEntity.ResetLength).Humanize(4)} {{0}}";
                    break;
                case StandardPathableBehavior.ReappearOnMapReset:
                    interactText = "Hide marker until map reset {0}";
                    break;
                case StandardPathableBehavior.OncePerInstance:
                    interactText = "Hide marker until you join a new instance of this map {0}";
                    break;
                case StandardPathableBehavior.OnceDailyPerCharacter:
                    interactText = "Hide marker for this character until daily reset {0}";
                    break;
                case StandardPathableBehavior.ReappearOnWeeklyReset:
                    interactText = "Hide marker until weekly reset {0}";
                    break;
                default:
                    interactText = "Interact with behavior {0}";
                    break;
            }

            _packState.UiStates.Interact.ShowInteract(_pathingEntity, interactText);
        }

        public void Unfocus() {
            _packState.UiStates.Interact.DisconnectInteract(_pathingEntity);
        }

        public override void Unload() {
            _packState.UiStates.Interact.DisconnectInteract(_pathingEntity);
        }

    }
}
