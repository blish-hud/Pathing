using System.Linq;
using System.Windows.Forms;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.Editor {
    public class CategoryNode : TreeNode {

        public PathingCategory PathingCategory { get; private set; }

        public CategoryNode(PathingCategory category) : base() {
            this.PathingCategory = category;

            this.SelectedImageKey = this.ImageKey = "category";

            this.Text = $"{category.DisplayName} [{category.Name}]";

            if (category.Any() || category.Pathables.Any()) {
                this.Nodes.Add("Loading...");
            }
        }

    }
}
