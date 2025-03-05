using Blish_HUD.Common.UI.Views;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.UI.Tooltips
{
    internal class EntityTextureTooltip : View, ITooltipView
    {
        private AsyncTexture2D _texture;

        private Image _imgIcon;

        public EntityTextureTooltip(AsyncTexture2D texture)
        {
            _texture = texture;

        }

        protected override void Build(Container buildPanel)
        {
            if (_texture == null) return;

            _imgIcon = new Image(_texture)
            {
                Size     = new Point(_texture.Width, _texture.Height),
                Location = new Point(0,              0),
                Parent   = buildPanel
            };
        }

        protected override void Unload()
        {
            _imgIcon?.Dispose();
            _texture?.Dispose();

            base.Unload();
        }

    }
}