using System.Linq;
using System.Windows.Forms;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.Editor {
    public class CategoryNode : TreeNode {
        public PathingCategory PathingCategory { get; private set; }

        public CategoryNode(PathingCategory category, IPackState rootPackState) {
            this.PathingCategory  = category;
            this.SelectedImageKey = this.ImageKey = "category";
            this.Text             = $"{category.DisplayName} [{category.Name}]";
            
            if (category.Any() || CategoryUtil.GetAssociatedPathingEntities(category, rootPackState.Entities.ToArray()).Any()) {
                this.Nodes.Add("Loading...");
            }
        }

    }
}
