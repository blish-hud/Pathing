using System.Collections.Generic;
using BhModule.Community.Pathing.UI.Models;
using Blish_HUD.Content;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.UI.Controls.TreeNodes
{
    public class LabelNode : PathingNode
    {
        public LabelNode(string text, AsyncTexture2D icon = null) : base(text)
        {
            IconPaddingTop = 2;
            IconSize       = new Point(25, 25);

            IconTextures = [
                new PathingTexture() {
                    Icon = icon
                }
            ];

            this.ShowBackground   = true;
            this.PanelHeight      = 30;
            this.Checkable        = false;
            BackgroundOpacity     = 0.05f;
            BackgroundOpaqueColor = Color.LightYellow;
            DoBuildContextMenu    = false;
        }

        protected override void BuildContextMenu() {
            //Don't build context menu
        }

    }
}
