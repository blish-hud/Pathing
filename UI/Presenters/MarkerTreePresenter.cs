using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BhModule.Community.Pathing.UI.Controls.TreeView;
using BhModule.Community.Pathing.UI.Views;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.UI.Presenter {

    public class MarkerTreePresenter : Presenter<MarkerTreeView, PackInitiator> {

        private readonly PathingModule _module;

        private MarkerNode _primaryMarkerNode;


        public MarkerTreePresenter(MarkerTreeView view, PathingModule module) : base(view, module.PackInitiator) {
            _module = module;
        }

        protected override Task<bool> Load(IProgress<string> progress) {
            _module.ModuleLoaded += (sender, args) => {
                UpdateView();
            };

            return Task.FromResult(true);
        }

        public IEnumerable<PathingCategory> FlattenCategories(PathingCategory category)
        {
            var categories = new List<PathingCategory> { category };

            foreach (var subCategory in category)
            {
                categories.AddRange(FlattenCategories(subCategory));
            }

            return categories;
        }

        public IEnumerable<PathingCategory> Search(string input) {
            if(input.Length < 3) return new List<PathingCategory>();

            var categories = FlattenCategories(_module.PackInitiator.GetAllMarkersCategories());

            string normalizedInput = input.ToLower().Replace(" ", "");

            return categories.Where(c => !string.IsNullOrWhiteSpace(c.DisplayName) && c.DisplayName.ToLower().Replace(" ", "").Contains(normalizedInput));
        }

        protected override void UpdateView() {
            if(this.View.TreeView == null) return;

            this.View.TreeView.ClearChildren();

            if (_module.PackInitiator == null) return;

            _primaryMarkerNode = new MarkerNode(_module.PackInitiator.PackState, _module.PackInitiator.GetAllMarkersCategories(), false, "All Markers")
            {
                Parent = this.View.TreeView,
                Width = this.View.TreeView.Width - 30
            };

            _primaryMarkerNode.Build();

            _primaryMarkerNode.Expand();
        }

        public void ResetView() {
            UpdateView();
        }

        public void SetSearchResults(IList<PathingCategory> categories) {
            _primaryMarkerNode.ClearChildNodes();

            foreach (var category in categories) {
                var categoryNode = new MarkerNode(_module.PackInitiator.PackState, category, false)
                {
                    Parent                = _primaryMarkerNode,
                    Width                 = _primaryMarkerNode.Width - 14,
                    BackgroundOpaqueColor = Color.Yellow,
                    BackgroundOpacity     = 0.2f
                };

                categoryNode.Build();
            }
        }

    }
}
