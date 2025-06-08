using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.State;
using Blish_HUD;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.UI.Tooltips
{
    internal class CategoryPathTooltip : View, ITooltipView
    {
        private readonly PathingCategory _category;
        private readonly IPackState _packState;

        protected FormattedLabel Text { get; set; }
        public CategoryPathTooltip(PathingCategory category, IPackState packState) {
            _category       = category;
            _packState = packState;
        }

        protected override void Build(Container buildPanel)
        {
            var container = new Panel()
            {
                Parent            = buildPanel,
                ShowBorder        = false,
                BackgroundTexture = GameService.Content.GetTexture("tooltip"),
                HeightSizingMode  = SizingMode.AutoSize,
                WidthSizingMode   = SizingMode.AutoSize,
                ClipsBounds       = false,
                ZIndex            = int.MaxValue - 2
            };

            var builder = new FormattedLabelBuilder();

            var parents = this._category
                              .GetParentsDesc();

            var firstParent = true;

            foreach (var parent in parents)
            {
                var color = parent == this._category ? Color.LightBlue : firstParent ? Color.Orange : Color.LightYellow;

                var inactive = _packState.CategoryStates.GetCategoryInactive(parent);
                builder = builder.CreatePart($" {parent.DisplayName}\n ", b => b.SetFontSize(ContentService.FontSize.Size16).SetTextColor(color).SetPrefixImage(AsyncTexture2D.FromAssetId(inactive ? 154982 : 154979)).SetPrefixImageSize(new Point(20, 20)));

                firstParent = false;
            }

            Text = builder
                  .SetWidth(420)
                  .AutoSizeHeight()
                  .Wrap()
                  .Build();

            Text.Location = new Point(15, 15);
            Text.Parent   = container;
        }

        protected override void Unload()
        {
            Text?.Dispose();

            base.Unload();
        }
    }
}