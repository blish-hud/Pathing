using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.UI.Controls {
    public class HorizontalCompass : Control {

        private readonly IRootPackState _packState;

        protected override CaptureType CapturesInput() {
            return CaptureType.DoNotBlock;
        }

        public HorizontalCompass(IRootPackState packState) {
            this.ZIndex = int.MinValue / 2;

            _packState = packState;

            this.Location = new Point(0, 50);
            this.Height   = 50;
        }

        public override void DoUpdate(GameTime gameTime) {
            base.DoUpdate(gameTime);

            if (this.Parent != null) {
                this.Width = this.Parent.Width;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            if (!GameService.GameIntegration.Gw2Instance.IsInGame) return;

            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.LightBlue);

            List<StandardMarker> entities = _packState.Entities.OfType<StandardMarker>().OrderBy(marker => marker.DistanceToPlayer).ToList();

            bounds = new Rectangle(Location.X, Location.Y, bounds.Width, bounds.Height);

            foreach (var pathable in entities) {
                pathable.RenderToHorizontalCompass(spriteBatch, bounds);
            }
        }

    }
}
