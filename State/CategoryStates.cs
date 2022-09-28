using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Microsoft.Xna.Framework;
using TmfLib.Pathable;

// ReSharper disable InconsistentlySynchronizedField

namespace BhModule.Community.Pathing.State {
    public class CategoryStates : ManagedState {

        private static readonly Logger Logger = Logger.GetLogger<CategoryStates>();

        private const string STATE_FILE         = "categories.txt";
        private const string INVERTEDSTATE_FILE = "invcategories.txt";

        private const double INTERVAL_SAVESTATE                = 5000; // 5.0 seconds
        private const double INTERVAL_UPDATEINACTIVECATEGORIES = 100;  // 0.1 seconds

        private HashSet<string> _inactiveCategories = new(StringComparer.OrdinalIgnoreCase);

        private readonly SafeList<PathingCategory> _rawInactiveCategories = new(); // Contains all categories which have been explicitly unchecked.
        private readonly SafeList<PathingCategory> _rawInvertedCategories = new(); // Contains all categories which have been explicitly checked and by default are toggled off.

        private double _lastSaveState                     = 0;
        private double _lastInactiveCategoriesCalculation = 0;

        private bool _stateDirty       = false;
        private bool _calculationDirty = false;

        public CategoryStates(IRootPackState packState) : base(packState) { /* NOOP */ }

        private async Task LoadCategoryState(string stateFileName, SafeList<PathingCategory> rawCategoriesList, PathingCategory rootCategory) {
            string categoryStatePath = Path.Combine(DataDirUtil.GetSafeDataDir(DataDirUtil.COMMON_STATE), stateFileName);

            if (!File.Exists(categoryStatePath))
                return; // Early skip if this state file doesn't exist yet.

            string[] recordedCategories = Array.Empty<string>();

            try {
                recordedCategories = await FileUtil.ReadLinesAsync(categoryStatePath);
            } catch (Exception e) {
                Logger.Error(e, $"Failed to read {STATE_FILE} ({categoryStatePath}).");
            }

            rawCategoriesList.Clear();

            foreach (string categoryNamespace in recordedCategories) {
                // TODO: Consider the case where a category no longer exists - this will create it.
                // We end up ignoring it, though, as it is known that it was not pulled from a pack based on the LoadedFromPack property.
                rawCategoriesList.Add(rootCategory.GetOrAddCategoryFromNamespace(categoryNamespace));
            }
        }

        private void CleanTwinStates(SafeList<PathingCategory> categories, SafeList<PathingCategory> invertedCategories) {
            var twins = categories.ToArray().Intersect(invertedCategories.ToArray());

            foreach (var twin in twins) {
                // We'll remove from both since we honestly have no idea at this time which it should be in.
                // It'll sort itself out the next time the user toggles it.
                categories.Remove(twin);
                invertedCategories.Remove(twin);
            }
        }

        private async Task LoadStates() {
            var rootCategory = _rootPackState.RootCategory;

            if (rootCategory == null)
                return; // Early skip if the pack is already getting repopulated.

            Logger.Debug($"Loading {nameof(CategoryStates)} state.");

            await LoadCategoryState(STATE_FILE,         _rawInactiveCategories, rootCategory);
            await LoadCategoryState(INVERTEDSTATE_FILE, _rawInvertedCategories, rootCategory);

            // Avoids an edge case where a category ends up in both files (a pack is updated with defaultToggle).
            // REF: https://discord.com/channels/531175899588984842/534492173362528287/1010130170625138729
            CleanTwinStates(_rawInactiveCategories, _rawInvertedCategories);

            _calculationDirty = true;
        }

        private async Task SaveCategoryState(string stateFileName, SafeList<PathingCategory> rawCategoriesList) {
            PathingCategory[] toggledCategories = rawCategoriesList.ToArray();

            string categoryStatePath = Path.Combine(DataDirUtil.GetSafeDataDir(DataDirUtil.COMMON_STATE), stateFileName);

            try {
                await FileUtil.WriteLinesAsync(categoryStatePath, toggledCategories.Select(c => c.Namespace));
            } catch (Exception e) {
                Logger.Warn(e, $"Failed to write {stateFileName} ({categoryStatePath}).");
            }
        }

        private async Task SaveStates(GameTime gameTime) {
            if (!_stateDirty) return;

            Logger.Debug($"Saving {nameof(CategoryStates)} state.");

            await SaveCategoryState(STATE_FILE,         _rawInactiveCategories); // Standard categories.
            await SaveCategoryState(INVERTEDSTATE_FILE, _rawInvertedCategories); // Inverted categories.
            
            _stateDirty = false;
        }

        private void AddAllSubCategories(HashSet<string> categories, PathingCategory topCategory) {
            var remainingCategories = new Queue<PathingCategory>(topCategory);

            while (remainingCategories.Count > 0) {
                var category = remainingCategories.Dequeue();

                categories.Add(category.Namespace);

                foreach (var subCategory in category) {
                    remainingCategories.Enqueue(subCategory);
                }
            }
        }

        private void CalculateOptimizedCategoryStates(GameTime gameTime) {
            if (!_calculationDirty) return;

            if (_rootPackState.RootCategory == null) return;

            PathingCategory[] inactiveCategories       = _rawInactiveCategories.ToArray();
            PathingCategory[] activeInvertedCategories = _rawInvertedCategories.ToArray();

            var remainingCategories = new Queue<PathingCategory>();
            remainingCategories.Enqueue(_rootPackState.RootCategory);

            var preCalcInactiveCategories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            while (remainingCategories.Count > 0) {
                var category = remainingCategories.Dequeue();

                if (inactiveCategories.Contains(category) // Standard toggled categories.
                  || (!category.DefaultToggle && !activeInvertedCategories.Contains(category))) { // Inverted toggled categories.
                    preCalcInactiveCategories.Add(category.Namespace);
                    AddAllSubCategories(preCalcInactiveCategories, category);
                    continue;
                }

                foreach (var subCategory in category) {
                    remainingCategories.Enqueue(subCategory);
                }
            }

            _inactiveCategories = preCalcInactiveCategories;
            _calculationDirty = false;
        }

        protected override async Task<bool> Initialize() {
            await LoadStates();

            return true;
        }

        public override async Task Reload() {
            _inactiveCategories.Clear();
            _rawInactiveCategories.Clear();
            _rawInvertedCategories.Clear();

            await LoadStates();
        }

        public override void Update(GameTime gameTime) {
            UpdateCadenceUtil.UpdateWithCadence(CalculateOptimizedCategoryStates, gameTime, INTERVAL_UPDATEINACTIVECATEGORIES, ref _lastInactiveCategoriesCalculation);
            UpdateCadenceUtil.UpdateAsyncWithCadence(SaveStates, gameTime, INTERVAL_SAVESTATE, ref _lastSaveState);
        }

        public override async Task Unload() {
            await SaveStates(null);
        }

        public bool GetNamespaceInactive(string categoryNamespace) {
            return _inactiveCategories.Contains(categoryNamespace);
        }

        private bool GetCategoryInactive(PathingCategory category, SafeList<PathingCategory> rawCategoriesList) {
            return rawCategoriesList.Contains(category);
        }

        public bool GetCategoryInactive(PathingCategory category) {
            if (category.DefaultToggle) {
                return GetCategoryInactive(category, _rawInactiveCategories);
            } else {
                return !GetCategoryInactive(category, _rawInvertedCategories);
            }
        }

        private void SetInactive(PathingCategory category, bool isInactive, SafeList<PathingCategory> rawCategoriesList) {
            rawCategoriesList.Remove(category);

            if (isInactive) {
                rawCategoriesList.Add(category);
            }

            _stateDirty       = true; // Ensure that we save the new state.
            _calculationDirty = true; // Ensure that the hashset is recalculated.
        }

        public void SetInactive(PathingCategory category, bool isInactive) {
            if (category.DefaultToggle) {
                SetInactive(category, isInactive, _rawInactiveCategories);
            } else {
                SetInactive(category, !isInactive, _rawInvertedCategories);
            }
        }

        public void SetInactive(string categoryNamespace, bool isInactive) => SetInactive(_rootPackState.RootCategory.GetOrAddCategoryFromNamespace(categoryNamespace), isInactive);

    }
}
