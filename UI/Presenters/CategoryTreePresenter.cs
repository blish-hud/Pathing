using System;
using System.Linq;
using System.Threading.Tasks;
using BhModule.Community.Pathing.UI.Events;
using BhModule.Community.Pathing.UI.Views;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Graphics.UI;

namespace BhModule.Community.Pathing.UI.Presenter {

    public class CategoryTreePresenter : Presenter<CategoryTreeView, PackInitiator> {

        private readonly        PathingModule _module;
        private                 bool          _packEventsInitialized;
        private                 bool          _updatingView;
        private static readonly Logger        _logger = Logger.GetLogger<CategoryTreePresenter>();

        public CategoryTreePresenter(CategoryTreeView view, PathingModule module) : base(view, module.PackInitiator) {
            _module = module;
        }

        protected override Task<bool> Load(IProgress<string> progress) {
            if (!_module.Loaded)
                _module.ModuleLoaded += _module_ModuleLoaded;
            else
                Initialize();

            return Task.FromResult(true);
        }

        protected override void UpdateView()
        {
            if (_updatingView || this.View.TreeView == null) return;

            this.View.TreeView.ClearChildNodes();

            if (_module.PackInitiator == null || _module.PackInitiator.IsLoading) return;

            if(!this.View.ValidateMarkerPacksState())
                return;

            this.View.TreeView.SetPackInitiator(_module.PackInitiator);

            try
            {
                _updatingView = true;
                this.View.TreeView.LoadNodes();

                if (this.View.TargetCategory != null)
                {
                    this.View.TreeView.NavigateToPath(this.View.TargetCategory.GetPath());
                    this.View.TargetCategory = null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to update view.");
            }
            finally
            {
                _updatingView = false;
            }

            if (!_packEventsInitialized)
                InitalizePackEvents();
        }

        private void _module_ModuleLoaded(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Initialize() {
            this.View.TreeView.SetPackInitiator(_module.PackInitiator);

            //Handle pack events
            InitalizePackEvents();

            if (!_module.PackInitiator.IsLoading) {
                this.View.SetLoading(true);

                UpdateView();

                this.View.SetLoading(false);
            }
        }

        private void InitalizePackEvents()
        {
            _module.PackInitiator.LoadMapFromEachPackStarted                       += PackInitiatorOnLoadMapFromEachPackStarted;
            _module.PackInitiator.LoadMapFromEachPackFinished                      += PackInitiatorOnLoadMapFromEachPackFinished;
            _module.PackInitiator.PackState.CategoryStates.CategoryInactiveChanged += CategoryStatesOnCategoryInactiveChanged;
            _module.PackInitiator.PackState.CategoryStates.CategoryStatesOptimized += CategoryStatesOnCategoryStatesOptimized;
            _packEventsInitialized                                                     =  true;
        }

        private void CategoryStatesOnCategoryStatesOptimized(object sender, EventArgs e) {
            this.View.TreeView?.UpdateSearchResultsCheckState(_module.PackInitiator.PackState);
        }

        private void CategoryStatesOnCategoryInactiveChanged(object sender, PathingCategoryEventArgs e) {
            this.View.TreeView?.UpdateCheckedState(e.Category, e.Active);
        }

        private void PackInitiatorOnLoadMapFromEachPackStarted(object sender, EventArgs e) {
            this.View.SetLoading(true);
            this.View.TreeView.ClearChildNodes();
        }

        private void PackInitiatorOnLoadMapFromEachPackFinished(object sender, EventArgs e) {
            UpdateView();
            this.View.SetLoading(false);
        }

        protected override void Unload() {
            _module.ModuleLoaded                                                   -= _module_ModuleLoaded;
            _module.PackInitiator.LoadMapFromEachPackStarted                       -= PackInitiatorOnLoadMapFromEachPackStarted;
            _module.PackInitiator.LoadMapFromEachPackFinished                      -= PackInitiatorOnLoadMapFromEachPackFinished;
            _module.PackInitiator.PackState.CategoryStates.CategoryInactiveChanged -= CategoryStatesOnCategoryInactiveChanged;

            _packEventsInitialized =  false;

            base.Unload();
        }

    }
}
