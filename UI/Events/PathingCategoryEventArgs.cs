using System;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.UI.Events
{
    public class PathingCategoryEventArgs : EventArgs
    {
        public PathingCategory Category { get; }
        public PathingCategoryEventArgs(PathingCategory category)
        {
            this.Category = category;
        }
    }
}
