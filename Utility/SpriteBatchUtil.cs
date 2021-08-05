using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace BhModule.Community.Pathing {
    public static class SpriteBatchUtil {

        public static void Draw(this SpriteBatch spriteBatch, Texture2D texture, RectangleF destinationRectangle, Color tint) {
            // Hacky trick to let us use RectangleF with spritebatch.

            var scale = new Vector2(destinationRectangle.Width / texture.Width, destinationRectangle.Height / texture.Height);

            spriteBatch.Draw(texture, destinationRectangle.Center, null, tint, 0f, new Vector2(destinationRectangle.Width / 2f, destinationRectangle.Height / 2f), scale, SpriteEffects.None, 0f);
        }

    }
}
