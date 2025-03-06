using System;
using System.Threading.Tasks;
using BhModule.Community.Pathing.UI.Events;
using BhModule.Community.Pathing.UI.Views;
using BhModule.Community.Pathing.Utility;
using Blish_HUD.Graphics.UI;

namespace BhModule.Community.Pathing.UI.Presenter {

    public class CategoryTreePresenter : Presenter<CategoryTreeView, PackInitiator> {

        private readonly PathingModule _module;
        private          bool          _isEventSubscribed;

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

        private void _module_ModuleLoaded(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Initialize() {
            this.View.TreeView.PackInitiator = _module.PackInitiator;

            _module.PackInitiator.LoadMapFromEachPackStarted                       += PackInitiatorOnLoadMapFromEachPackStarted;
            _module.PackInitiator.LoadMapFromEachPackFinished                      += PackInitiatorOnLoadMapFromEachPackFinished;
            _module.PackInitiator.PackState.CategoryStates.CategoryInactiveChanged += CategoryStatesOnCategoryInactiveChanged;
            _isEventSubscribed                                                     =  true;
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
        }

        protected override void UpdateView() {
            if(this.View.TreeView == null) return;

            this.View.TreeView.ClearChildNodes();

            if (_module.PackInitiator == null || _module.PackInitiator.IsLoading) return;

            this.View.TreeView.PackInitiator ??= _module.PackInitiator;

            this.View.TreeView.LoadNodes();

            if (this.View.TargetCategory != null) {
                this.View.TreeView.NavigateToPath(this.View.TargetCategory.GetPath());
                this.View.TargetCategory = null;
            }
            
            if (!_isEventSubscribed)
            {
                _module.PackInitiator.LoadMapFromEachPackFinished += PackInitiatorOnLoadMapFromEachPackFinished;
                _isEventSubscribed                                 =  true;
            }

        }

        protected override void Unload() {
            _module.ModuleLoaded                                                   -= _module_ModuleLoaded;
            _module.PackInitiator.LoadMapFromEachPackStarted                       -= PackInitiatorOnLoadMapFromEachPackStarted;
            _module.PackInitiator.LoadMapFromEachPackFinished                      -= PackInitiatorOnLoadMapFromEachPackFinished;
            _module.PackInitiator.PackState.CategoryStates.CategoryInactiveChanged -= CategoryStatesOnCategoryInactiveChanged;

            _isEventSubscribed =  false;

            base.Unload();
        }

    }
}
