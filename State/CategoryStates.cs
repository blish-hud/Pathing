using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BhModule.Community.Pathing.UI.Events;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.State {
    public class CategoryStates : ManagedState {

        private static readonly Logger Logger = Logger.GetLogger<CategoryStates>();

        private const string NEW_STATE_FILE = "category_preferences.txt";
        private const string OLD_STATE_FILE = "categories.txt";
        private const string OLD_INVERTED_FILE = "invcategories.txt";

        private const double INTERVAL_SAVESTATE = 5000; // 5.0 seconds
        private const double INTERVAL_UPDATEINACTIVECATEGORIES = 100;  // 0.1 seconds

        // Key: Namespace, Value: True (Explicitly Enabled) | False (Explicitly Disabled)
        private readonly ConcurrentDictionary<string, bool> _explicitStates = new(StringComparer.OrdinalIgnoreCase);

        // Indexed by category id within _evaluatedRoot's index.
        private bool[]          _evaluatedInactiveCategories = Array.Empty<bool>();
        private PathingCategory _evaluatedRoot;

        private int _inactiveVersion;

        /// <summary>
        /// Incremented after any change to the evaluated inactive categories.
        /// </summary>
        public int InactiveVersion => Volatile.Read(ref _inactiveVersion);

        private double _lastSaveState                     = 0;
        private double _lastInactiveCategoriesCalculation = 0;

        private bool _stateDirty       = false;
        private bool _calculationDirty = false;

        public event EventHandler<PathingCategoryEventArgs> CategoryInactiveChanged;
        public event EventHandler<EventArgs> CategoryStatesOptimized;

        public CategoryStates(IRootPackState packState) : base(packState) { /* NOOP */ }

        protected override async Task<bool> Initialize() {
            await LoadStates();
            return true;
        }

        public override async Task Reload() {
            await SaveStates(null);
            _explicitStates.Clear();
            _evaluatedInactiveCategories = Array.Empty<bool>();
            Interlocked.Increment(ref _inactiveVersion);
            await LoadStates();
        }

        private async Task LoadStates() {
            string dataDir = DataDirUtil.GetSafeDataDir(DataDirUtil.COMMON_STATE);
            string newStatePath = Path.Combine(dataDir, NEW_STATE_FILE);

            Logger.Debug($"Loading {nameof(CategoryStates)} state.");

            if (File.Exists(newStatePath)) {
                await LoadUnifiedState(newStatePath);
            } else {
                await MigrateOldStates(dataDir);
            }

            _calculationDirty = true;
        }

        private async Task LoadUnifiedState(string filePath) {
            try {
                string[] lines = await FileUtil.ReadLinesAsync(filePath);
                foreach (string line in lines) {
                    if (string.IsNullOrWhiteSpace(line) || line.Length < 2) continue;

                    bool explicitActive = line[0] == '+';
                    string categoryNamespace = line.Substring(1);

                    _explicitStates[categoryNamespace] = explicitActive;
                }
            } catch (Exception e) {
                Logger.Warn(e, $"Failed to read unified category states from {filePath}.");
            }
        }

        private async Task MigrateOldStates(string dataDir) {
            string oldStatePath = Path.Combine(dataDir, OLD_STATE_FILE);
            string oldInvertedPath = Path.Combine(dataDir, OLD_INVERTED_FILE);

            // Legacy Categories: Everything listed here was explicitly UNCHECKED (Disabled)
            if (File.Exists(oldStatePath)) {
                try {
                    string[] lines = await FileUtil.ReadLinesAsync(oldStatePath);
                    foreach (string ns in lines) _explicitStates[ns] = false;
                } catch (Exception e) { Logger.Warn(e, "Failed to migrate legacy standard categories."); }
            }

            // Legacy Inverted Categories: Everything listed here was explicitly CHECKED (Enabled)
            if (File.Exists(oldInvertedPath)) {
                try {
                    string[] lines = await FileUtil.ReadLinesAsync(oldInvertedPath);
                    foreach (string ns in lines) _explicitStates[ns] = true;
                } catch (Exception e) { Logger.Warn(e, "Failed to migrate legacy inverted categories."); }
            }

            if (_explicitStates.Count > 0) {
                _stateDirty = true; // Queue a write to the new unified format
                Logger.Info($"Successfully migrated {_explicitStates.Count} legacy category states to the unified layout.");
            }
        }

        private async Task SaveStates(GameTime gameTime) {
            if (!_stateDirty) return;

            Logger.Debug($"Saving {nameof(CategoryStates)} preferences.");
            string newStatePath = Path.Combine(DataDirUtil.GetSafeDataDir(DataDirUtil.COMMON_STATE), NEW_STATE_FILE);

            try {
                // Format: +namespace means Explicitly On, -namespace means Explicitly Off
                var lines = _explicitStates.Select(kvp => $"{(kvp.Value ? "+" : "-")}{kvp.Key}");
                await FileUtil.WriteLinesAsync(newStatePath, lines);
                _stateDirty = false;
            } catch (Exception e) {
                Logger.Warn(e, $"Failed to write unified category states to {newStatePath}.");
            }
        }

        /// <summary>
        /// Flags the evaluated states to be recalculated (e.g. after the categories were reloaded).
        /// </summary>
        public void Invalidate() {
            _calculationDirty = true;
        }

        private void CalculateOptimizedCategoryStates(GameTime gameTime) {
            var root = _rootPackState.RootCategory;

            // Categories can be added by reference (e.g. from scripts) after the last calculation.
            if (root != null && root.Index.Count > _evaluatedInactiveCategories.Length) _calculationDirty = true;

            if (!_calculationDirty || root == null) return;

            var preCalcInactive = new bool[root.Index.Count];
            var remainingCategories = new Queue<PathingCategory>();
            remainingCategories.Enqueue(root);

            while (remainingCategories.Count > 0) {
                var category = remainingCategories.Dequeue();

                // Check explicit override preference first. 
                // Fallback to the intrinsic DefaultToggle of the pack if no override exists.
                bool isCategoryActive = _explicitStates.TryGetValue(category.Namespace, out bool explicitActive)
                    ? explicitActive
                    : category.DefaultToggle;

                if (!isCategoryActive) {
                    // This node is disabled; instantly flag it and all subcategories as inactive without diving deeper
                    if (category.Id < preCalcInactive.Length) preCalcInactive[category.Id] = true;
                    AddAllSubCategories(preCalcInactive, category);
                    continue;
                }

                foreach (var subCategory in category) {
                    remainingCategories.Enqueue(subCategory);
                }
            }

            _evaluatedInactiveCategories = preCalcInactive;
            _evaluatedRoot               = root;
            Interlocked.Increment(ref _inactiveVersion);
            this.CategoryStatesOptimized?.Invoke(this, EventArgs.Empty);
            _calculationDirty = false;
        }

        private void AddAllSubCategories(bool[] categories, PathingCategory topCategory) {
            var remainingCategories = new Queue<PathingCategory>(topCategory);

            while (remainingCategories.Count > 0) {
                var category = remainingCategories.Dequeue();
                if (category.Id < categories.Length) categories[category.Id] = true;

                foreach (var subCategory in category) {
                    remainingCategories.Enqueue(subCategory);
                }
            }
        }

        private Action<GameTime>     _calculateOptimizedCategoryStatesCall;
        private Func<GameTime, Task> _saveStatesCall;

        public override void Update(GameTime gameTime) {
            UpdateCadenceUtil.UpdateWithCadence(_calculateOptimizedCategoryStatesCall ??= CalculateOptimizedCategoryStates, gameTime, INTERVAL_UPDATEINACTIVECATEGORIES, ref _lastInactiveCategoriesCalculation);
            UpdateCadenceUtil.UpdateAsyncWithCadence(_saveStatesCall ??= SaveStates, gameTime, INTERVAL_SAVESTATE, ref _lastSaveState);
        }

        public override async Task Unload() {
            await SaveStates(null);
        }

        public bool GetNamespaceInactive(string categoryNamespace) {
            var category = _evaluatedRoot?.Find(categoryNamespace);
            return category != null && IsInactive(category);
        }

        /// <summary>
        /// Indicates the category or one of its parents is disabled.
        /// </summary>
        public bool IsInactive(PathingCategory category) {
            var evaluated = _evaluatedInactiveCategories;

            if (category.Index == _evaluatedRoot?.Index && category.Id < evaluated.Length) {
                return evaluated[category.Id];
            }

            // Not yet evaluated (e.g. added since the last calculation).
            for (var parent = category; parent != null && !parent.Root; parent = parent.Parent) {
                if (GetCategoryInactive(parent)) return true;
            }

            return false;
        }

        public bool GetRawNamespaceInactive(string categoryNamespace) {
            return _explicitStates.TryGetValue(categoryNamespace, out bool active) && !active;
        }

        public bool GetCategoryInactive(PathingCategory category) {
            if (_explicitStates.TryGetValue(category.Namespace, out bool explicitActive)) {
                return !explicitActive;
            }
            return !category.DefaultToggle;
        }

        public void SetInactive(PathingCategory category, bool isInactive) {
            bool targetActiveState = !isInactive;

            // Cleanup Optimization: If user choice perfectly matches the marker pack's default,
            // we can remove the override entirely to keep the text save-file clean.
            if (category.DefaultToggle == targetActiveState) {
                _explicitStates.TryRemove(category.Namespace, out _);
            } else {
                _explicitStates[category.Namespace] = targetActiveState;
            }

            CategoryInactiveChanged?.Invoke(this, new PathingCategoryEventArgs(category) { Active = targetActiveState });

            _stateDirty       = true; // Ensure that we save the new state.
            _calculationDirty = true; // Ensure that the hashset is recalculated.
        }

        public void SetInactive(string categoryNamespace, bool isInactive) {
            var liveCategory = _rootPackState?.RootCategory?.Find(categoryNamespace);

            if (liveCategory != null) {
                SetInactive(liveCategory, isInactive);
            } else {
                // Make sure we store the state even if set through a Lua script while the pack isn't active on the map.
                _explicitStates[categoryNamespace] = !isInactive;
                _stateDirty = true;
                _calculationDirty = true;
            }
        }

        public event EventHandler<PathingCategoryEventArgs> TriggerOpenCategoryView;

        public void TriggerOpenCategory(PathingCategory category) {
            TriggerOpenCategoryView?.Invoke(this, new PathingCategoryEventArgs(category));
        }
    }
}