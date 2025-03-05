using Blish_HUD.Controls;

namespace BhModule.Community.Pathing.UI.Extensions
{
    internal static class PanelExtensions
    {
        public static int ContainsChildPosition(this Container container, Control child)
        {
            foreach (var panelChild in container.Children)
            {
                if (panelChild == child)
                {
                    return child.Top;
                }

                if (panelChild is Container childContainer)
                {
                    int childPosition = ContainsChildPosition(childContainer, child);

                    if (childPosition != -1)
                    {
                        return childPosition + childContainer.Top;
                    }
                }
            }
            return -1;
        }
    }
}
