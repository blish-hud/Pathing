using System;
using System.Linq;
using BhModule.Community.Pathing.UI.Extensions;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.UI.Controls.TreeView;

public class CustomFlowPanel : FlowPanel
{
    public Scrollbar Scrollbar => this.Parent?.Children.OfType<Scrollbar>().FirstOrDefault(s => s.AssociatedContainer == this);

    private float _targetScrollDistance = 0f;
    private float _scrollTarget = 0f;

    public float ScrollDistance
    {
        get => this.Scrollbar?.ScrollDistance ?? 0f;
        set
        {
            if (!this.CanScroll || this.Scrollbar == null)
                return;

            this.Scrollbar.ScrollDistance = MathHelper.Clamp(value, 0f, 1f);
        }
    }

    public void SetTargetScrollDistance(float distance) {
        _scrollTarget = MathHelper.Clamp(distance, 0f, 1);
    }

    public void SaveScrollDistance(int height)
    {
        var scrollbarDistance = this.Scrollbar.ScrollDistance;

        if (this.Scrollbar != null && !float.IsNaN(scrollbarDistance)) {
            
            _targetScrollDistance = scrollbarDistance * (float)(height - this.Scrollbar.Height);
        }
    }

    public void UpdateScrollDistance() {
        UpdateScrollDistance(_targetScrollDistance);
    }

    public void UpdateScrollDistance(float target)
    {
        if (this.Scrollbar != null && !float.IsNaN(target)) {
            var bottom = this.Children
                             .Where(c => c.Visible)
                             .Max(c => c.Bottom);

            float distance = (float)target / (float)(bottom + (int)this.ControlPadding.Y - this.Scrollbar.Height);

            Scrollbar.ScrollDistance = Math.Min(distance, 1f);
        }
    }

    protected override void OnChildAdded(ChildChangedEventArgs e)
    {
        base.OnChildAdded(e);

        e.ChangedChild.Resized += ChangedChild_Resized;
    }

    protected override void OnChildRemoved(ChildChangedEventArgs e)
    {
        base.OnChildRemoved(e);

        e.ChangedChild.Resized -= ChangedChild_Resized;
    }

    private void ChangedChild_Resized(object sender, ResizedEventArgs e) {

        if (e.PreviousSize.Y == e.CurrentSize.Y) return;

        var otherChildrenSize = Children.Where(c => c != sender)
                                    .Sum(c => c.Height);

        var previousBottomSize = otherChildrenSize + e.PreviousSize.Y - (int)ControlPadding.Y;

        if (this.CanScroll)
            SaveScrollDistance(previousBottomSize);

        UpdateScrollDistance();
    }

    public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
    {
        base.PaintBeforeChildren(spriteBatch, bounds);

        if (_targetScrollDistance > 0f)
        {
            UpdateScrollDistance(_targetScrollDistance);
            _targetScrollDistance = 0f;
        }

        if (_scrollTarget > 0f) {
            ScrollDistance = _scrollTarget;
            _scrollTarget  = 0f;
        }
    }

    public void ScrollToChild(Control child, int paddingTop) {
        var childPosition = this.ContainsChildPosition(child);

        this.ScrollToChild(childPosition - paddingTop);
    }

    public void ScrollToChild(int childYPosition)
    {
        if (childYPosition == -1) return;

        var scrollbar = this.Parent?.Children
                             .OfType<Scrollbar>()
                             .FirstOrDefault(s => s.AssociatedContainer == this);

        if (scrollbar == null)
            return;

        if (childYPosition == 0)
            scrollbar.ScrollDistance = 0f;
        else
        {
            var panelHeight = (float)this.Children.Where(c => c.Visible).Max(c => c.Bottom);

            var scrollPosition = (float)childYPosition / (panelHeight - scrollbar.Height);

            scrollbar.ScrollDistance = scrollPosition;

            this.SetTargetScrollDistance(scrollPosition);
        }
    }

    protected override void DisposeControl()
    {
        foreach (var child in this.Children)
            child.Resized -= ChangedChild_Resized;

        base.DisposeControl();
    }
}