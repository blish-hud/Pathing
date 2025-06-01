using System.Linq;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.UI.Controls;
using BhModule.Community.Pathing.UI.Controls.TreeView;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using TmfLib.Pathable;
using Panel = Blish_HUD.Controls.Panel;
using View = Blish_HUD.Graphics.UI.View;

namespace BhModule.Community.Pathing.UI.Views {
    internal class ConfirmationView : View {
        protected FormattedLabel Text { get; set; }

        protected StandardButton ConfirmButton { get; set; }
        
        protected BlueButton OpenButton { get; set; }
        protected StandardButton DenyButton { get; set; }

        private TreeView _treeView { get; }
        private PathingCategory _category { get; }

        private IPackState _packState { get; }

        public ConfirmationView(TreeView treeView, PathingCategory category, IPackState packState) {
            this._treeView  = treeView;
            this._category  = category;
            this._packState = packState;
        }

        protected override void Build(Container buildPanel) {
            var container = new Panel() {
                Parent            = buildPanel,
                ShowBorder        = true,
                BackgroundTexture = GameService.Content.GetTexture("tooltip"),
                Size              = buildPanel.Size,
                ClipsBounds       = false,
                HeightSizingMode  = SizingMode.AutoSize,
                ZIndex            = int.MaxValue - 2
            };

            var builder = new FormattedLabelBuilder()
                         .CreatePart("Do you wish to activate all the parents of this category?",                           b => b.SetFontSize(ContentService.FontSize.Size18).SetTextColor(Color.Orange).MakeBold())
                         .CreatePart("\n \nThe category you selected is not active because one or more of the parent categories listed below is not active.\n \n ", b => b.SetFontSize(ContentService.FontSize.Size16));

            var parents = this._category.GetParentsDesc().ToList();

            var firstParent = true;

            foreach (var parent in parents) {
                var color = firstParent ? Color.Orange : Color.LightYellow;

                var inactive = _packState.CategoryStates.GetCategoryInactive(parent);
                builder = builder.CreatePart($" {parent.DisplayName}\n ", b => b.SetFontSize(ContentService.FontSize.Size16).SetTextColor(color).SetPrefixImage(AsyncTexture2D.FromAssetId(inactive ? 154982 : 154979)).SetPrefixImageSize(new Point(20, 20)));

                firstParent = false;
            }

            Text = builder
                  .SetWidth(344)
                  .AutoSizeHeight()
                  .Wrap()
                  .Build();

            Text.Location = new Point(15, 15);
            Text.Parent   = container;
            
            ConfirmButton = new StandardButton()
            {
                Parent   = container,
                Text     = "Yes",
                Width    = 85,
                Location = new Point(15, Text.Height + 30)
            };

            ConfirmButton.Click += (_, _) =>
            {
                foreach(var parent in parents)
                {
                    _packState.CategoryStates.SetInactive(parent, false);
                }

                //if (_treeView != null)
                //    _treeView.UpdateSearchResultsCheckState(_packState);

                buildPanel.Dispose();
            };

            DenyButton = new StandardButton()
            {
                Parent   = container,
                Text     = "Cancel",
                Width    = 85,
                Location = new Point(ConfirmButton.Right + 5, Text.Height + 30)
            };

            DenyButton.Click += (_, _) => buildPanel.Dispose();


            OpenButton = new BlueButton()
            {
                Parent   = container,
                Text     = "Open In Explorer",
                Width    = 150,
                Location = new Point(DenyButton.Right + 50, Text.Height + 30)
            };

            OpenButton.Click += (_, _) => {
                _packState.CategoryStates.TriggerOpenCategory(_category);
                buildPanel.Dispose();
            };

        }
    }
}
