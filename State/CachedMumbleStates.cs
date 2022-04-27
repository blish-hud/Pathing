using System;
using System.Threading.Tasks;
using Blish_HUD;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.State {
    public class CachedMumbleStates : ManagedState {

        private int  _lastTick = -1;
        private bool _needsUpdate = true;

        private double _mapCenterX;
        private double _mapCenterY;
        public  double MapCenterX => GetTickCachedValue(ref _mapCenterX);
        public  double MapCenterY => GetTickCachedValue(ref _mapCenterY);

        private bool _isMapOpen;
        public  bool IsMapOpen => GetTickCachedValue(ref _isMapOpen);

        private bool _isCompassRotationEnabled;
        public  bool IsCompassRotationEnabled => GetTickCachedValue(ref _isCompassRotationEnabled);

        private double _compassRotation;
        public  double CompassRotation => GetTickCachedValue(ref _compassRotation);

        private static bool UpdateIsMapOpen() => GameService.Gw2Mumble.UI.IsMapOpen;

        private T GetTickCachedValue<T>(ref T value) {
            if (_needsUpdate) {
                UpdateCache();
            }

            return value;
        }

        private void UpdateCache() {
            var mapCenter = GameService.Gw2Mumble.UI.MapCenter;
            _mapCenterX = mapCenter.X;
            _mapCenterY = mapCenter.Y;

            _isMapOpen                = GameService.Gw2Mumble.UI.IsMapOpen;
            _isCompassRotationEnabled = GameService.Gw2Mumble.UI.IsCompassRotationEnabled;
            _compassRotation          = GameService.Gw2Mumble.UI.CompassRotation;

            _needsUpdate = false;
        }

        public CachedMumbleStates(IRootPackState rootPackState) : base(rootPackState) { }

        public override Task Reload() {
            return Task.CompletedTask;
        }

        public override void Update(GameTime gameTime) {
            int newTick = GameService.Gw2Mumble.Tick;

            if (GameService.Gw2Mumble.Tick != _lastTick) {
                _lastTick    = newTick;
                _needsUpdate = true;
            }
        }

        protected override Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public override Task Unload() {
            return Task.CompletedTask;
        }

    }
}
