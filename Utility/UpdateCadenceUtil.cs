using System;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.Utility {
    public static class UpdateCadenceUtil {

        public static void UpdateWithCadence(Action<GameTime> call, GameTime gameTime, double cadence, ref double lastCheck) {
            lastCheck += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (lastCheck >= cadence) {
                call(gameTime);
                lastCheck = 0;
            }
        }

    }
}
