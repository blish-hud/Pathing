using TmfLib.Pathable;

namespace BhModule.Community.Pathing.Editor {
    public class PathingCategoryEditWrapper {

        private readonly PathingCategory _innerPathingCategory;

        public string DisplayName {
            get => _innerPathingCategory.DisplayName;
            set => _innerPathingCategory.DisplayName = value;
        }

        public bool IsSeparator {
            get => _innerPathingCategory.IsSeparator;
            set => _innerPathingCategory.IsSeparator = value;
        }

        public bool DefaultToggle {
            get => _innerPathingCategory.DefaultToggle;
            set => _innerPathingCategory.DefaultToggle = value;
        }

        public PathingCategoryEditWrapper(PathingCategory innerPathingCategory) {
            _innerPathingCategory = innerPathingCategory;
        }

    }
}
