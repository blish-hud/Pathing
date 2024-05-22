using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.UI.Controls {
    public class CategoryContextMenuStrip : ContextMenuStrip {

        private readonly IPackState      _packState;
        private readonly PathingCategory _pathingCategory;

        private static readonly Texture2D _textureContinueMenu = Content.GetTexture("156057");

        private readonly Color _backColor = Color.FromNonPremultiplied(37, 36, 37, 255);

        private bool _forceShowAll = false;

        public CategoryContextMenuStrip(IPackState packState, PathingCategory pathingCategory, bool forceShowAll) {
            _packState       = packState;
            _pathingCategory = pathingCategory;

            _forceShowAll = forceShowAll;
        }

        // TODO: Make category filtering less janky.

        private (IEnumerable<PathingCategory> SubCategories, int Skipped) GetSubCategories(bool forceShowAll = false) {
            // We only show subcategories with a non-empty DisplayName (explicitly setting it to "" will hide it) and
            // was loaded by one of the packs (since those still around from unloaded packs will remain).
            var subCategories = _pathingCategory.Where(cat => cat.LoadedFromPack && cat.DisplayName != "" && !cat.IsHidden);

            if (!_packState.UserConfiguration.PackEnableSmartCategoryFilter.Value || forceShowAll) {
                return (subCategories, 0);
            }

            var filteredSubCategories = new List<PathingCategory>();

            PathingCategory lastCategory = null;

            bool lastIsSeparator = false;

            int skipped = 0;

            // We go bottom to top to check if the categories are potentially relevant to categories below.
            foreach (var subCategory in subCategories.Reverse()) {
                if (subCategory.IsSeparator && ((!lastCategory?.IsSeparator ?? false) || lastIsSeparator)) {
                    // If separator was relevant to this category, we include it.
                    filteredSubCategories.Add(subCategory);
                    lastIsSeparator = true;
                } else if (CategoryUtil.UiCategoryIsNotFiltered(subCategory, _packState)) {
                    // If category was not filtered, we include it.
                    filteredSubCategories.Add(subCategory);
                    lastIsSeparator = false;
                } else {
                    lastIsSeparator = false;
                    if (!subCategory.IsSeparator) skipped++;
                    continue;
                }

                lastCategory = subCategory;
            }
            
            return (Enumerable.Reverse(filteredSubCategories), skipped);
        }

        protected override void OnShown(EventArgs e) {
            (IEnumerable<PathingCategory> subCategories, int skipped) = GetSubCategories(_forceShowAll);

            foreach (var subCategory in subCategories) {
                this.AddMenuItem(new CategoryContextMenuStripItem(_packState, subCategory, _forceShowAll));
            }

            if (skipped > 0 && _packState.UserConfiguration.PackShowWhenCategoriesAreFiltered.Value) {
                var showAllSkippedCategories = new ContextMenuStripItem() {
                    // LOCALIZE: Skipped categories menu item
                    Text = $"{skipped} Categories Are Hidden",
                    Enabled = false,
                    CanCheck = true,
                    BasicTooltipText = string.Format(Strings.Info_HiddenCategories, _packState.UserConfiguration.PackEnableSmartCategoryFilter.DisplayName)
                };

                this.AddMenuItem(showAllSkippedCategories);

                // The control is disabled, so the .Click event won't fire.  We cheat by just doing LeftMouseButtonReleased.
                showAllSkippedCategories.LeftMouseButtonReleased += ShowAllSkippedCategories_LeftMouseButtonReleased;
            }

            if (skipped == 0 && !subCategories.Any()) {
                this.AddMenuItem(new ContextMenuStripItem() {
                    Text = "No marker packs loaded...",
                    Enabled = false,
                });
            }

            base.OnShown(e);

            // Behold: the effort I'm willing to put towards making huge category listings visible.

            if (this.Bottom > GameService.Graphics.SpriteScreen.Bottom) {
                this.Bottom = GameService.Graphics.SpriteScreen.Bottom;
            }

            if (this.Top < 0) {
                this.Top = 0;
            }

            if (this.Right > GameService.Graphics.SpriteScreen.Right) {
                this.Right = GameService.Graphics.SpriteScreen.Right;
            }

            if (this.Left < 0) {
                this.Left = 0;
            }

            // See that is it very little.
        }

        private void ShowAllSkippedCategories_LeftMouseButtonReleased(object sender, MouseEventArgs e) {
            this.ClearChildren();

            (IEnumerable<PathingCategory> subCategories, int skipped) = GetSubCategories(true);

            foreach (var subCategory in subCategories) {
                this.AddMenuItem(new CategoryContextMenuStripItem(_packState, subCategory, true));
            }
        }

        protected override void OnHidden(EventArgs e) {
            foreach (var cmsiChild in this.Children.Select(otherChild => otherChild as ContextMenuStripItem)) {
                cmsiChild?.Submenu?.Hide();
            }

            this.ClearChildren();

            base.OnHidden(e);
        }

        private const int SCROLLHINT_HEIGHT = 20;

        protected override void OnMouseMoved(MouseEventArgs e) {
            base.OnMouseMoved(e);

            // The remaining effort I have for this silly issue.  Please be mindful of your category use in marker packs.
            // Friends don't let friends create ridiculous and poorly structure category structures.

            if (this.Bottom > GameService.Graphics.SpriteScreen.Bottom && GameService.Graphics.SpriteScreen.Bottom - e.MousePosition.Y < SCROLLHINT_HEIGHT) {
                this.Bottom = GameService.Graphics.SpriteScreen.Bottom;
            }

            if (this.Top < 0 && e.MousePosition.Y < SCROLLHINT_HEIGHT) {
                this.Top = 0;
            }
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            if (this.Bottom > GameService.Graphics.SpriteScreen.Bottom) {
                var scrollBounds = new Rectangle(4, GameService.Graphics.SpriteScreen.Bottom - SCROLLHINT_HEIGHT, this.Width - 8, SCROLLHINT_HEIGHT);

                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, scrollBounds,                                                                                                          _backColor);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(2, GameService.Graphics.SpriteScreen.Bottom                     - SCROLLHINT_HEIGHT, this.Width - 6, 1), Color.DarkGray);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(2, GameService.Graphics.SpriteScreen.Bottom - SCROLLHINT_HEIGHT + 1,                 this.Width - 6, 1), Color.LightGray);

                spriteBatch.DrawOnCtrl(this, _textureContinueMenu, new Rectangle(this.Width / 2 - _textureContinueMenu.Width / 2, scrollBounds.Bottom - scrollBounds.Height / 2 - _textureContinueMenu.Height / 2, _textureContinueMenu.Width, _textureContinueMenu.Height));
            }

            if (this.Top < 0) {
                var scrollBounds = new Rectangle(4, -this.Top, this.Width - 8, SCROLLHINT_HEIGHT);

                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, scrollBounds, _backColor);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(2, -this.Top + SCROLLHINT_HEIGHT                                            - 1,                 this.Width - 6, 1), Color.DarkGray);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(2, -this.Top                                                                + SCROLLHINT_HEIGHT, this.Width - 6, 1), Color.LightGray);

                spriteBatch.DrawOnCtrl(this, _textureContinueMenu, new Rectangle(this.Width / 2 - _textureContinueMenu.Width / 2, scrollBounds.Bottom - scrollBounds.Height / 2 - _textureContinueMenu.Height / 2, _textureContinueMenu.Width, _textureContinueMenu.Height), null, Color.White, 0, Vector2.Zero, SpriteEffects.FlipVertically);
            }

            base.PaintAfterChildren(spriteBatch, bounds);
        }

    }
}
