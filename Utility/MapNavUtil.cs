using System;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls.Intern;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.Utility {
    public static class MapNavUtil {

        private static readonly Logger Logger = Logger.GetLogger(typeof(MapNavUtil));

        private static double GetDistance(double x1, double y1, double x2, double y2) {
            return GetDistance(x2 - x1, y2 - y1);
        }

        private static double GetDistance(double offsetX, double offsetY) {
            return Math.Sqrt(Math.Pow(offsetX, 2) + Math.Pow(offsetY, 2));
        }

        private static async Task WaitForTick() {
            int tick = GameService.Gw2Mumble.Tick;
            while (GameService.Gw2Mumble.Tick - tick < 2) {
                await Task.Delay(10);
            }
        }

        public static async Task<bool> NavigateToPosition(double x, double y, double zoom) {
            var mapPos  = GameService.Gw2Mumble.UI.MapCenter;
            var mapZoom = GameService.Gw2Mumble.UI.MapScale;

            double offsetX    = 0;
            double offsetY    = 0;
            double offsetZoom = 0;

            Mouse.SetPosition(GameService.Graphics.WindowWidth / 2, GameService.Graphics.WindowHeight / 2);

            var startPos = Mouse.GetPosition();

            while (GameService.Gw2Mumble.UI.MapScale < 12) {
                Mouse.RotateWheel(-int.MaxValue);
                Mouse.RotateWheel(-int.MaxValue);
                Mouse.RotateWheel(-int.MaxValue);
                Mouse.RotateWheel(-int.MaxValue);
                await WaitForTick();
            }

            double totalDist  = GetDistance(mapPos.X, mapPos.Y, x, y) / GameService.Gw2Mumble.UI.MapScale;

            Logger.Debug($"Distance: {totalDist}");

            if (Math.Sqrt(Math.Pow(mapPos.X - x, 2)) / GameService.Gw2Mumble.UI.MapScale > GameService.Graphics.WindowWidth / 2f) {
                Logger.Debug("Point is off horizontally");
            }

            if (Math.Sqrt(Math.Pow(mapPos.Y - y, 2)) / GameService.Gw2Mumble.UI.MapScale > GameService.Graphics.WindowHeight / 2f) {
                Logger.Debug("Point is off vertically");
            }

            return true;

            double targetDist = 0;

            while (true) {
                mapPos  = GameService.Gw2Mumble.UI.MapCenter;

                offsetX = mapPos.X - x;
                offsetY = mapPos.Y - y;

                targetDist = Math.Sqrt(Math.Pow(offsetX, 2) + Math.Pow(offsetY, 2));

                Logger.Debug($"Distance remaining: {GetDistance(mapPos.X, mapPos.Y, x, y)}");
                Logger.Debug($"Map Position: {GameService.Gw2Mumble.UI.MapPosition.X}, {GameService.Gw2Mumble.UI.MapPosition.Y}");

                if (targetDist < 5) break;
                //Logger.Debug($"Distance remaining: {offsetX}, {offsetY}");

                Mouse.SetPosition(GameService.Graphics.WindowWidth / 2, GameService.Graphics.WindowHeight / 2);

                Mouse.Press(MouseButton.RIGHT);
                Mouse.SetPosition(startPos.X + (int)MathHelper.Clamp((float)offsetX / (float)GameService.Gw2Mumble.UI.MapScale, -100000, 100000),
                                  startPos.Y + (int)MathHelper.Clamp((float)offsetY / (float)GameService.Gw2Mumble.UI.MapScale, -100000, 100000));

                await WaitForTick();

                Mouse.SetPosition(startPos.X + (int)MathHelper.Clamp((float)offsetX / (float)GameService.Gw2Mumble.UI.MapScale, -100000, 100000),
                                  startPos.Y + (int)MathHelper.Clamp((float)offsetY / (float)GameService.Gw2Mumble.UI.MapScale, -100000, 100000));

                Mouse.Release(MouseButton.RIGHT);

                await Task.Delay(1000);
            }

            Mouse.Click(MouseButton.LEFT);

            return true;
        }

    }
}
