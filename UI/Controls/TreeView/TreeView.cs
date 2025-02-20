using System;
using Blish_HUD.Controls;

namespace BhModule.Community.Pathing.UI.Controls.TreeView
{
    public class TreeView : Container
    {
        protected override void OnChildRemoved(ChildChangedEventArgs e)
        {
	        base.OnChildRemoved(e);

            e.ChangedChild.Dispose();
        }
    }
}
