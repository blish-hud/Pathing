using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blish_HUD;
using Gw2Sharp.WebApi.Exceptions;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Rectangle = Gw2Sharp.WebApi.V2.Models.Rectangle;

namespace BhModule.Community.Pathing.State {
    public class MapStates : ManagedState {

        private static readonly Logger Logger = Logger.GetLogger<MapStates>();

        public readonly struct MapDetails {

            public Rectangle ContinentRect { get; }
            public Rectangle MapRect       { get; }

            public MapDetails(Rectangle continentRect, Rectangle mapRect) {
                this.ContinentRect = continentRect;
                this.MapRect       = mapRect;
            }

        }

        private readonly Dictionary<int, MapDetails> _mapDetails = new();

        public MapStates(IRootPackState rootPackState) : base(rootPackState) { /* NOOP */ }

        protected override async Task<bool> Initialize() {
            await LoadMapData();

            return true;
        }

        private async Task LoadMapData(int remainingAttempts = 2) {
            IApiV2ObjectList<Map> maps = null;

            try {
                maps = await PathingModule.Instance.Gw2ApiManager.Gw2ApiClient.V2.Maps.AllAsync();
            } catch (Exception ex) {
                if (remainingAttempts > 0) {
                    Logger.Warn(ex, "Failed to pull map data from the Gw2 API.  Trying again in 30 seconds.");
                    await Task.Yield();
                    await Task.Delay(30000);
                    await LoadMapData(remainingAttempts - 1);
                } else if (ex is TooManyRequestsException) {
                    Logger.Warn(ex, "After multiple attempts no map data could be loaded due to being rate limited by the API.");
                } else {
                    Logger.Error(ex, "Final attempt to pull map data from the Gw2 API failed.  This session won't have map data.");
                }
            }

            if (maps == null)
                return; // We failed to load any map data.

            lock (_mapDetails) {
                foreach (var map in maps) {
                    _mapDetails[map.Id] = new MapDetails(map.ContinentRect, map.MapRect);
                }
            }

            await Reload();
        }

        private const double METERCONVERSION = 1d / 254d * 10000d;

        private MapDetails? _currentMapDetails = null;

        public void EventCoordsToMapCoords(double eventCoordsX, double eventCoordsY, out double outX, out double outY) {
            if (_currentMapDetails.HasValue) {
                outX = _currentMapDetails.Value.ContinentRect.TopLeft.X + (eventCoordsX * METERCONVERSION - _currentMapDetails.Value.MapRect.TopLeft.X) / _currentMapDetails.Value.MapRect.Width * _currentMapDetails.Value.ContinentRect.Width;
                outY = _currentMapDetails.Value.ContinentRect.TopLeft.Y + -(eventCoordsY * METERCONVERSION - _currentMapDetails.Value.MapRect.TopLeft.Y) / _currentMapDetails.Value.MapRect.Height * _currentMapDetails.Value.ContinentRect.Height;

                return;
            }

            outX = 0;
            outY = 0;
        }

        public override Task Reload() {
            lock (_mapDetails) {
                if (_mapDetails.TryGetValue(_rootPackState.CurrentMapId, out var mapDetails)) {
                    _currentMapDetails = mapDetails;
                } else {
                    _currentMapDetails = null;
                }
            }

            return Task.CompletedTask;
        }

        public override void Update(GameTime gameTime) { /* NOOP */ }

        public override Task Unload() {
            _currentMapDetails = null;
            return Task.CompletedTask;
        }

    }
}
