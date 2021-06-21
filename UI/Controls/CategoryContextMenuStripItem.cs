using System.Linq;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BhModule.Community.Pathing.UI.Controls {
    public class CategoryContextMenuStripItem : ContextMenuStripItem {

        protected override void OnClick(MouseEventArgs e) {
            if (base.CanCheck) {
                // If CTRL is held down when clicked, uncheck all adjacent menu items except for this one.
                if (GameService.Input.Keyboard.ActiveModifiers.HasFlag(ModifierKeys.Ctrl)) {
                    foreach (var childMenuItem in this.Parent?.Children.Cast<CategoryContextMenuStripItem>() ?? Enumerable.Empty<CategoryContextMenuStripItem>()) {
                        childMenuItem.Checked = (childMenuItem == this);
                    }

                    return;
                }

                if (GameService.Input.Keyboard.ActiveModifiers.HasFlag(ModifierKeys.Shift)) {
                    this.Checked = true;
                }
            }

            base.OnClick(e);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            base.Paint(spriteBatch, bounds);

            //spriteBatch.DrawOnCtrl(this, ContentService.Textures.Error, bounds);
        }

    }
}
