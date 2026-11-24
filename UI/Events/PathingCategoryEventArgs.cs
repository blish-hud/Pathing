using System;

namespace BhModule.Community.Pathing.UI.Events
{
    public class PathingCategoryEventArgs : EventArgs
    {
        public PathingCategory Category { get; }
        
        public bool Active { get; set; }
        
        public PathingCategoryEventArgs(PathingCategory category)
        {
            this.Category = category;
        }
    }
}
