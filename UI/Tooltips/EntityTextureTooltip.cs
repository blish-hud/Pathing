using System;
using System.Collections.Generic;
using BhModule.Community.Pathing.UI.Controls.TreeNodes;
using BhModule.Community.Pathing.UI.Models;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.UI.Tooltips
{
    internal class EntityTextureTooltip : View, ITooltipView
    {
        private List<PathingTexture> _textures;

        private List<Image> _imgIcons = new List<Image>();

        public EntityTextureTooltip(List<PathingTexture> textures)
        {
            _textures = textures;
        }

        protected override void Build(Container buildPanel)
        {
            if (_textures.Count <= 0) return;
            
            var xPosition = 0;

            foreach (var texture in _textures)
            {
                var (newWidth, newHeight) = GetResizedDimensions(texture.Icon, 200, 200);

                _imgIcons.Add(new Image(texture.Icon)
                {
                    Size     = new Point(newWidth, newHeight),
                    Tint     = texture.Tint,
                    Location = new Point(xPosition, 0),
                    Parent   = buildPanel
                });

                xPosition += newWidth;
            }
        }

        private (int width, int height) GetResizedDimensions(AsyncTexture2D texture, int maxWidth, int maxHeight)
        {
            var originalWidth  = texture.Width;
            var originalHeight = texture.Height;

            if (originalWidth <= maxWidth && originalHeight <= maxHeight)
            {
                return (originalWidth, originalHeight);
            }

            float widthRatio  = (float)maxWidth  / originalWidth;
            float heightRatio = (float)maxHeight / originalHeight;
            float scale       = Math.Min(widthRatio, heightRatio);

            int newWidth  = (int)(originalWidth  * scale);
            int newHeight = (int)(originalHeight * scale);

            return (newWidth, newHeight);
        }

        protected override void Unload()
        {
            DisposeControls(_imgIcons);

            base.Unload();
        }


        private void DisposeControls(IEnumerable<Control> controls)
        {
            var controlsQueue = new Queue<Control>(controls);

            while (controlsQueue.Count > 0)
            {
                var control = controlsQueue.Dequeue();

                control.Parent = null;
                control.Dispose();
            }
        }
    }
}