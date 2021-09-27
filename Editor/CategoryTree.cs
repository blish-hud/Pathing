using System.Linq;
using System.Windows.Forms;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;

// ReSharper disable CoVariantArrayConversion

namespace BhModule.Community.Pathing.Editor {
    public class CategoryTree : TreeView {

        public IPackState _packState;

        public IPackState PackState {
            get => _packState;
            set {
                if (Equals(_packState, value)) return;

                _packState = value;
                UpdateTreeView();
            }
        }

        public CategoryTree() {

            

        }

        protected override void OnAfterExpand(TreeViewEventArgs e) {
            if (e.Node is CategoryNode categoryNode) {
                e.Node.Nodes.Clear();
                e.Node.Nodes.AddRange(categoryNode.PathingCategory.Where(category => CategoryUtil.GetCategoryIsNotFiltered(category, _packState.Entities.ToArray(), CategoryUtil.CurrentMapCategoryFilter)).Select(childCategory => new CategoryNode(childCategory, _packState)).ToArray());
                e.Node.Nodes.AddRange(_packState.Entities.Where(pathable => string.Equals(pathable.CategoryNamespace, categoryNode.PathingCategory.Namespace)).Select(pathable => new PathableNode(pathable)).ToArray());
            }

            base.OnAfterExpand(e);
        }

        protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e) {
            if (e.Node is CategoryNode categoryNode) {
                MarkerEditWindow.SetPathingCategory(_packState, categoryNode.PathingCategory);
            } else if (e.Node is PathableNode pathableNode) {
                MarkerEditWindow.SetPathingEntity(_packState, pathableNode.PathingEntity);
            }

            base.OnNodeMouseClick(e);
        }

        private void UpdateTreeView() {
            this.Nodes.Clear();

            if (_packState != null) {
                this.Nodes.AddRange(_packState.RootCategory.Where(category => CategoryUtil.GetCategoryIsNotFiltered(category, _packState.Entities.ToArray(), CategoryUtil.CurrentMapCategoryFilter)).Select(childCategory => new CategoryNode(childCategory, _packState)).ToArray());
            }
        }

    }
}
