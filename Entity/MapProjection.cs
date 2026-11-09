using System;
using BhModule.Community.Pathing.State;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.Entity {
    public readonly struct MapProjection {

        private readonly MapStates.MapDetails _mapDetails;
        private readonly bool                 _hasMapDetails;

        private readonly double _mapCenterX;
        private readonly double _mapCenterY;
        private readonly double _scale;

        private readonly bool  _rotate;
        private readonly float _cos;
        private readonly float _sin;

        private readonly Vector2 _offset;

        public MapProjection(IPackState packState, double scale, double offsetX, double offsetY) {
            _hasMapDetails = packState.MapStates.TryGetCurrentMapDetails(out _mapDetails);

            var mumble = packState.CachedMumbleStates;

            _mapCenterX = mumble.MapCenterX;
            _mapCenterY = mumble.MapCenterY;
            _scale      = scale;

            _rotate = !mumble.IsMapOpen && mumble.IsCompassRotationEnabled;

            // Matches Matrix.CreateRotationZ
            float radians = (float)mumble.CompassRotation;
            _cos = (float)Math.Cos(radians);
            _sin = (float)Math.Sin(radians);

            _offset = new Vector2((float)offsetX, (float)offsetY);
        }

        public Vector2 Project(double x, double y) {
            double mapX = 0;
            double mapY = 0;

            if (_hasMapDetails) {
                MapStates.EventCoordsToMapCoords(in _mapDetails, x, y, out mapX, out mapY);
            }

            var location = new Vector2((float)((mapX - _mapCenterX) / _scale),
                                       (float)((mapY - _mapCenterY) / _scale));

            if (_rotate) {
                location = new Vector2(location.X * _cos + location.Y * -_sin,
                                       location.X * _sin + location.Y * _cos);
            }

            return location + _offset;
        }

    }
}
