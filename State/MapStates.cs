using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Gw2Sharp.WebApi.Exceptions;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Rectangle = Gw2Sharp.WebApi.V2.Models.Rectangle;

namespace BhModule.Community.Pathing.State {
    public class MapStates : ManagedState {

        private static readonly Logger Logger = Logger.GetLogger<MapStates>();

        public struct MapDetails {

            public double ContinentRectTopLeftX { get; set; }
            public double ContinentRectTopLeftY { get; set; }

            public double MapRectTopLeftX { get; set; }
            public double MapRectTopLeftY { get; set; }

            public double MapRectWidth  { get; set; }
            public double MapRectHeight { get; set; }

            public double ContinentRectWidth  { get; set; }
            public double ContinentRectHeight { get; set; }

            public MapDetails() { }

            public MapDetails(Rectangle continentRect, Rectangle mapRect) {
                ContinentRectTopLeftX = continentRect.TopLeft.X;
                ContinentRectTopLeftY = continentRect.TopLeft.Y;
                MapRectTopLeftX       = mapRect.TopLeft.X;
                MapRectTopLeftY       = mapRect.TopLeft.Y;
                MapRectWidth          = mapRect.Width;
                MapRectHeight         = mapRect.Height;
                ContinentRectWidth    = continentRect.Width;
                ContinentRectHeight   = continentRect.Height;
            }

        }

        private Dictionary<int, MapDetails> _mapDetails = new();

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
                    Logger.Warn(ex, "Failed to pull map data from the Gw2 API.  Trying again in 2 seconds.");
                    await Task.Yield();
                    await Task.Delay(500);
                    await LoadMapData(remainingAttempts - 1);
                } else if (ex is TooManyRequestsException) {
                    Logger.Warn(ex, "After multiple attempts no map data could be loaded due to being rate limited by the API.");
                } else {
                    Logger.Warn(ex, "Final attempt to pull map data from the Gw2 API failed.  This session won't have map data.");
                }
            }

            if (maps == null && !_mapDetails.Any()) {
                try {
                    using var fs            = _rootPackState.Module.ContentsManager.GetFileStream("fallback/mapDetails.json");
                    using var sr            = new StreamReader(fs);
                    string    rawMapDetails = await sr.ReadToEndAsync();

                    _mapDetails = JsonConvert.DeserializeObject<Dictionary<int, MapDetails>>(rawMapDetails);
                } catch (Exception ex) {
                    Logger.Warn(ex, "Loadding fallback/mapDetails.json failed!");
                }
            } else if (maps != null) {
                lock (_mapDetails) {
                    foreach (var map in maps) {
                        _mapDetails[map.Id] = new MapDetails(map.ContinentRect, map.MapRect);
                    }
                }
            }

            // Used to prepare the fallback
            //System.IO.File.WriteAllText("mapDetails.json", JsonConvert.SerializeObject(_mapDetails));

            await Reload();
        }

        private const float METERCONVERSION = 39.37f;

        private MapDetails? _currentMapDetails = null;

        public void EventCoordsToMapCoords(double eventCoordsX, double eventCoordsY, out double outX, out double outY) {
            if (_currentMapDetails.HasValue) {
                outX = _currentMapDetails.Value.ContinentRectTopLeftX + (eventCoordsX * METERCONVERSION - _currentMapDetails.Value.MapRectTopLeftX) / _currentMapDetails.Value.MapRectWidth * _currentMapDetails.Value.ContinentRectWidth;
                outY = _currentMapDetails.Value.ContinentRectTopLeftY + -(eventCoordsY * METERCONVERSION - _currentMapDetails.Value.MapRectTopLeftY) / _currentMapDetails.Value.MapRectHeight * _currentMapDetails.Value.ContinentRectHeight;

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
