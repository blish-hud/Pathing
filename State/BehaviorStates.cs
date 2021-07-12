using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Behavior;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using File = System.IO.File;

namespace BhModule.Community.Pathing.State {

    public class BehaviorStates : ManagedState {

        private static readonly Logger Logger = Logger.GetLogger<BehaviorStates>();

        private const string STATE_FILE = "timers.txt";

        private const double INTERVAL_CHECKTIMERS       = 5000;   // 5 seconds
        private const double INTERVAL_CHECKACHIEVEMENTS = 150000; // 2.5 minutes
        private const double INTERVAL_SAVESTATE         = 10000;  // 10 seconds

        /// <summary>
        /// TacO Behavior 1
        /// </summary>
        private readonly HashSet<Guid> _hiddenUntilMapChange = new();

        /// <summary>
        /// TacO Behavior 3
        /// </summary>
        private readonly HashSet<Guid> _hiddenStatic = new();

        /// <summary>
        /// TacO Behavior 2, 4, and 7
        /// Blish HUD Behavior 101
        /// </summary>
        private readonly HashSet<Guid> _hiddenUntilTimer = new();
        private readonly List<(DateTime timerExpiration, Guid guid)> _timerMetadata = new();

        /// <summary>
        /// TacO Behavior 6
        /// </summary>
        private readonly ConcurrentDictionary<ulong, HashSet<Guid>> _hiddenInShard = new();

        /// <summary>
        /// TacO Achievement Attributes
        /// </summary>
        private readonly ConcurrentDictionary<int, AchievementStatus> _achievementStates = new();

        // Some last checks are intentionally set to target wait times to force them to run on the first loop.
        private double _lastTimerCheck       = INTERVAL_CHECKTIMERS;
        private double _lastAchievementCheck = INTERVAL_CHECKACHIEVEMENTS;

        private double _lastSaveState = 0;

        private bool _stateDirty = false;

        public BehaviorStates(IRootPackState packState) : base(packState) {
            GameService.Gw2Mumble.CurrentMap.MapChanged += CurrentMapOnMapChanged;
        }

        private bool SyncedCollectionContainsGuid(HashSet<Guid> collection, Guid guid) {
            if (collection == null) return false;

            lock (collection) return collection.Contains(guid);
        }

        private ulong GetMapInstanceKey() {
            ulong mapIndex = (ulong)_rootPackState.CurrentMapId << 32;
            return mapIndex + GameService.Gw2Mumble.Info.ShardId;
        }

        public bool IsBehaviorHidden(StandardPathableBehavior behavior, Guid guid) {
            return behavior switch {
                // TacO
                StandardPathableBehavior.AlwaysVisible => false,
                StandardPathableBehavior.ReappearOnMapChange => SyncedCollectionContainsGuid(_hiddenUntilMapChange,                                                           guid),
                StandardPathableBehavior.ReappearOnDailyReset => SyncedCollectionContainsGuid(_hiddenUntilTimer,                                                              guid),
                StandardPathableBehavior.OnlyVisibleBeforeActivation => SyncedCollectionContainsGuid(_hiddenStatic,                                                           guid),
                StandardPathableBehavior.ReappearAfterTimer => SyncedCollectionContainsGuid(_hiddenUntilTimer,                                                                guid),
                StandardPathableBehavior.OncePerInstance => SyncedCollectionContainsGuid(_hiddenInShard.TryGetValue(GetMapInstanceKey(), out var guidList) ? guidList : null, guid),
                StandardPathableBehavior.OnceDailyPerCharacter => SyncedCollectionContainsGuid(_hiddenUntilTimer,                                                             guid),

                // Blish HUD
                StandardPathableBehavior.ReappearOnWeeklyReset => SyncedCollectionContainsGuid(_hiddenUntilTimer, guid),

                // Not implemented.
                StandardPathableBehavior.ReappearOnMapReset => false,

                // Other?
                _ => UnsupportedBehavior(behavior),
            };
        }

        public bool IsAchievementHidden(int achievementId, int achievementBit) {
            AchievementStatus achievement;

            // If the achievement is not found, we show it.
            lock (_achievementStates) if (!_achievementStates.TryGetValue(achievementId, out achievement)) return false;

            // If the achievement reports that it has been completed, we hide it.
            if (achievement.Done) return true;

            // If the achievement is partially done and this bit has been completed, we hide it.
            return achievement.AchievementBits.Contains(achievementBit);
        }

        private bool UnsupportedBehavior(StandardPathableBehavior behavior) {
            return false;
        }

        private void CurrentMapOnMapChanged(object sender, ValueEventArgs<int> e) {
            lock (_hiddenUntilMapChange) _hiddenUntilMapChange.Clear();

            _stateDirty = true;
        }

        public void AddFilteredBehavior(StandardPathableBehavior behavior, Guid guid) {
            switch (behavior) {
                case StandardPathableBehavior.ReappearOnMapChange: // Behavior 1
                    lock (_hiddenUntilMapChange) _hiddenUntilMapChange.Add(guid);
                    break;
                case StandardPathableBehavior.OnlyVisibleBeforeActivation: // Behavior 3
                    lock (_hiddenStatic) _hiddenStatic.Add(guid);
                    break;
                case StandardPathableBehavior.OncePerInstance: // Behavior 6
                    ulong instanceKey = GetMapInstanceKey();

                    lock (_hiddenInShard) {
                        if (!_hiddenInShard.ContainsKey(instanceKey)) {
                            _hiddenInShard[instanceKey] = new HashSet<Guid>();
                        }

                        lock (_hiddenInShard[instanceKey]) _hiddenInShard[instanceKey].Add(guid);
                    }

                    break;
                default:
                    Logger.Warn($"TacO behavior {behavior} can not have its filtering handled this way.");
                    break;
            }

            _stateDirty = true;
        }

        public void AddFilteredBehavior(Guid guid, DateTime expire) {
            // Behaviors 2, 4, 7, and 101
            lock (_hiddenUntilTimer) lock (_timerMetadata) {
                _hiddenUntilTimer.Add(guid);
                _timerMetadata.Add((expire, guid));
            }

            _stateDirty = true;
        }

        protected override async Task<bool> Initialize() {
            await LoadState();

            return true;
        }

        public override Task Reload() { return Task.CompletedTask; /* NOOP */ }

        public override void Update(GameTime gameTime) {
            UpdateCadenceUtil.UpdateWithCadence(UpdateTimers,       gameTime, INTERVAL_CHECKTIMERS,       ref _lastTimerCheck);
            UpdateCadenceUtil.UpdateWithCadence(UpdateAchievements, gameTime, INTERVAL_CHECKACHIEVEMENTS, ref _lastAchievementCheck);
            UpdateCadenceUtil.UpdateAsyncWithCadence(SaveState,          gameTime, INTERVAL_SAVESTATE,         ref _lastSaveState);
        }

        protected override void Unload() {
            SaveState(null);
        }

        private async Task LoadState() {
            // Load timer states and metadata.
            string timerStatesPath = Path.Combine(DataDirUtil.GetSafeDataDir(DataDirUtil.COMMON_STATE), STATE_FILE);

            if (!File.Exists(timerStatesPath)) return;

            string[] recordedTimerMetadata = Array.Empty<string>();

            try {
                recordedTimerMetadata = await FileUtil.ReadLinesAsync(timerStatesPath);
            } catch (Exception e) {
                Logger.Error(e, $"Failed to read {STATE_FILE} ({timerStatesPath}).");
            }

            lock (_timerMetadata)
            lock (_hiddenUntilTimer) {
                foreach (string line in recordedTimerMetadata) {
                    string[] lineParts = line.Split(',');

                    try {
                        var markerGuid   = Guid.ParseExact(lineParts[0], "D");
                        var markerExpire = DateTime.Parse(lineParts[1]);

                        _hiddenUntilTimer.Add(markerGuid);
                        _timerMetadata.Add((markerExpire, markerGuid));
                    } catch (Exception ex) {
                        Logger.Warn(ex, $"Failed to parse behavior state '{line}' from {timerStatesPath}");
                    }
                }
            }
        }

        private async Task SaveState(GameTime gameTime) {
            if (!_stateDirty) return;

            Logger.Debug($"Saving {nameof(CategoryStates)} state.");

            (DateTime timerExpiration, Guid guid)[] timerMetadata;

            lock (_timerMetadata) timerMetadata = _timerMetadata.ToArray();

            string timerStatesPath = Path.Combine(DataDirUtil.GetSafeDataDir(DataDirUtil.COMMON_STATE), STATE_FILE);

            try {
                await FileUtil.WriteLinesAsync(timerStatesPath, timerMetadata.Select(metadata => $"{metadata.guid},{metadata.timerExpiration}"));
            } catch (Exception e) {
                Logger.Error(e, $"Failed to write {STATE_FILE} ({timerStatesPath}).");
            }

            _stateDirty = false;
        }

        private Task HandleAchievementUpdate(Task<IApiV2ObjectList<AccountAchievement>> accountAchievementTask) {
            lock (_achievementStates) {
                foreach (var achievement in accountAchievementTask.Result) {
                    _achievementStates.AddOrUpdate(achievement.Id,
                                                   new AchievementStatus(achievement.Done, achievement.Bits ?? Enumerable.Empty<int>()),
                                                   (_, _) => new AchievementStatus(achievement.Done, achievement.Bits ?? Enumerable.Empty<int>()));
                }
            }

            return Task.CompletedTask;
        }

        private void UpdateAchievements(GameTime gameTime) {
            if (!this.Running) return;

            try {
                // v2/account/achivements requires "account" and "progression" permissions.
                if (PathingModule.Instance.Gw2ApiManager.HavePermissions(new[] {TokenPermission.Account, TokenPermission.Progression})) {
                    PathingModule.Instance.Gw2ApiManager.Gw2ApiClient.V2.Account.Achievements.GetAsync().ContinueWith(HandleAchievementUpdate, TaskContinuationOptions.NotOnFaulted);
                }
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to load account achievements.");
            }
        }

        private void UpdateTimers(GameTime gameTime) {
            lock (_timerMetadata) lock (_hiddenUntilTimer) {
                foreach (var guidDetails in _timerMetadata.ToArray()) {
                    if (guidDetails.timerExpiration < DateTime.UtcNow) {
                        _timerMetadata.Remove(guidDetails);
                        _hiddenUntilTimer.Remove(guidDetails.guid);

                        _stateDirty = true;
                    }
                }
            }
        }

    }
}
