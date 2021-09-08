using System.Collections.Generic;
using System.Threading.Tasks;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Rectangle = Gw2Sharp.WebApi.V2.Models.Rectangle;

namespace BhModule.Community.Pathing.State {
    public class MapStates : ManagedState {

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

        private async Task LoadMapData() {
            IApiV2ObjectList<Map> maps = await PathingModule.Instance.Gw2ApiManager.Gw2ApiClient.V2.Maps.AllAsync();

            foreach (var map in maps) {
                _mapDetails[map.Id] = new MapDetails(map.ContinentRect, map.MapRect);
            }
        }

        private const double METERCONVERSION = 1d / 254d * 10000d;

        public (double X, double Y) EventCoordsToMapCoords(double eventCoordsX, double eventCoordsY, int map = -1) {
            if (map < 0) map = _rootPackState.CurrentMapId;

            if (_mapDetails.TryGetValue(map, out var mapDetails)) {
                return (mapDetails.ContinentRect.TopLeft.X +  (eventCoordsX * METERCONVERSION - mapDetails.MapRect.TopLeft.X) / mapDetails.MapRect.Width  * mapDetails.ContinentRect.Width,
                        mapDetails.ContinentRect.TopLeft.Y + -(eventCoordsY * METERCONVERSION - mapDetails.MapRect.TopLeft.Y) / mapDetails.MapRect.Height * mapDetails.ContinentRect.Height);
            }

            return (0, 0);
        }

        public override async Task Reload() { }

        public override void Update(GameTime gameTime) { /* NOOP */ }

        public override Task Unload() {
            return Task.CompletedTask;
        }

    }
}
