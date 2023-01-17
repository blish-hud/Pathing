using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Behavior;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Microsoft.Xna.Framework;
using File = System.IO.File;

namespace BhModule.Community.Pathing.State {

    public class BehaviorStates : ManagedState {

        private static readonly Logger Logger = Logger.GetLogger<BehaviorStates>();

        private const string STATE_FILE = "timers.txt";

        private const double INTERVAL_CHECKTIMERS       = 5000;   // 5 seconds
        private const double INTERVAL_SAVESTATE         = 10000;  // 10 seconds

        /// <summary>
        /// TacO Behavior 1
        /// </summary>
        private readonly HashSet<Guid> _hiddenUntilMapChange = new();

        /// <summary>
        /// TacO Behavior 2, 3, 4, and 7
        /// Blish HUD Behavior 801
        /// </summary>
        private readonly HashSet<Guid> _hiddenUntilTimer = new();
        private readonly SafeList<(DateTime timerExpiration, Guid guid)> _timerMetadata = new();

        /// <summary>
        /// TacO Behavior 6
        /// </summary>
        private readonly ConcurrentDictionary<ulong, HashSet<Guid>> _hiddenInShard = new();

        // Some last checks are intentionally set to target wait times to force them to run on the first loop.
        private double _lastTimerCheck = INTERVAL_CHECKTIMERS;

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

        public void ClearHiddenBehavior(Guid guid) {
            lock (_hiddenUntilMapChange) {
                if (_hiddenUntilMapChange.Remove(guid)) {
                    return;
                }
            }

            lock (_hiddenUntilTimer) {
                if (_hiddenUntilTimer.Contains(guid)) {
                    _hiddenUntilTimer.Remove(guid);

                    for (int i = 0; i < _timerMetadata.Count; i++) {
                        if (_timerMetadata[i].guid == guid) {
                            _timerMetadata.RemoveAt(i);
                            _stateDirty = true;
                            return;
                        }
                    }
                }
            }

            lock (_hiddenInShard) {
                foreach (var shard in _hiddenInShard) {
                    if (shard.Value.Contains(guid)) {
                        shard.Value.Remove(guid);
                        return;
                    }
                }
            }
        }

        public bool IsBehaviorHidden(StandardPathableBehavior behavior, Guid guid) {
            return behavior switch {
                // TacO
                StandardPathableBehavior.AlwaysVisible => false,
                StandardPathableBehavior.ReappearOnMapChange => SyncedCollectionContainsGuid(_hiddenUntilMapChange,                                                           guid),
                StandardPathableBehavior.ReappearOnDailyReset => SyncedCollectionContainsGuid(_hiddenUntilTimer,                                                              guid),
                StandardPathableBehavior.OnlyVisibleBeforeActivation => SyncedCollectionContainsGuid(_hiddenUntilTimer,                                                       guid),
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
                    lock (_hiddenUntilTimer) {
                        _hiddenUntilTimer.Add(guid);
                        _timerMetadata.Add((DateTime.MaxValue, guid));
                    }
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
            // Behaviors 2, 4, 7, and 801
            lock (_hiddenUntilTimer) {
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
            UpdateCadenceUtil.UpdateWithCadence(UpdateTimers, gameTime, INTERVAL_CHECKTIMERS, ref _lastTimerCheck);
            UpdateCadenceUtil.UpdateAsyncWithCadence(SaveState, gameTime, INTERVAL_SAVESTATE, ref _lastSaveState);
        }

        public override async Task Unload() {
            await SaveState(null);
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

            lock (_hiddenUntilTimer) {
                foreach (string line in recordedTimerMetadata) {
                    string[] lineParts = line.Split(',');

                    try {
                        var markerGuid   = Guid.ParseExact(lineParts[0], "D");

                        var markerExpire = DateTime.MinValue;

                        // Migrate from Pathing < v1.1.5.  New dates are 8601 and prepended with `t`.
                        if (!lineParts[1].StartsWith("t")) {
                            markerExpire = DateTime.Parse(lineParts[1]);
                        }

                        markerExpire = DateTime.Parse(lineParts[1].TrimStart('t'));

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

            Logger.Debug($"Saving {nameof(BehaviorStates)} state.");

            (DateTime timerExpiration, Guid guid)[] timerMetadata = _timerMetadata.ToArray();

            string timerStatesPath = Path.Combine(DataDirUtil.GetSafeDataDir(DataDirUtil.COMMON_STATE), STATE_FILE);

            try {
                await FileUtil.WriteLinesAsync(timerStatesPath, timerMetadata.Select(metadata => $"{metadata.guid},t{metadata.timerExpiration:s}"));
            } catch (Exception e) {
                Logger.Warn(e, $"Failed to write {STATE_FILE} ({timerStatesPath}).");
            }

            _stateDirty = false;
        }

        private void UpdateTimers(GameTime gameTime) {
            lock (_hiddenUntilTimer) {
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
