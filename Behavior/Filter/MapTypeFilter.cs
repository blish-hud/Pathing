using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Behavior.Filter {
    public class MapTypeFilter : IBehavior, ICanFilter {

        public const string PRIMARY_ATTR_NAME = "maptype";

        private ObservableCollection<MapType> AllowedMapTypes { get; }

        private bool _isFiltered;

        public bool IsFiltered() => _isFiltered;

        public MapTypeFilter(IEnumerable<MapType> allowedMapTypes) : this(allowedMapTypes.ToArray()) { /* NOOP */ }

        public MapTypeFilter(params MapType[] allowedMapTypes) {
            this.AllowedMapTypes = new ObservableCollection<MapType>(allowedMapTypes);

            this.AllowedMapTypes.CollectionChanged += AllowedMapTypesOnCollectionChanged;

            UpdateFiltered();
        }

        private void AllowedMapTypesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            UpdateFiltered();
        }

        private void UpdateFiltered() {
            _isFiltered = this.AllowedMapTypes.All(m => m != GameService.Gw2Mumble.CurrentMap.Type);
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes) {
            return attributes.TryGetAttribute(PRIMARY_ATTR_NAME, out var attribute)
                       ? new MapTypeFilter(attribute.GetValueAsEnums<MapType>())
                       : null;
        }

        public void Update(GameTime gameTime) { /* NOOP */ }
        public void Unload() { /* NOOP */ }

    }
}
