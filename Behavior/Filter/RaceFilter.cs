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
    public class RaceFilter : IBehavior, ICanFilter {
        
        public const string PRIMARY_ATTR_NAME = "race";

        private ObservableCollection<RaceType> AllowedRaces { get; }

        private bool _isFiltered = false;

        public bool IsFiltered() => _isFiltered;

        public RaceFilter(IEnumerable<RaceType> allowedRaces) : this(allowedRaces.ToArray()) { /* NOOP */ }

        public RaceFilter(params RaceType[] allowedRaces) {
            this.AllowedRaces = new ObservableCollection<RaceType>(allowedRaces);

            this.AllowedRaces.CollectionChanged               += AllowedRacesOnCollectionChanged;
            GameService.Gw2Mumble.PlayerCharacter.NameChanged += PlayerCharacterOnNameChanged;

            UpdateFiltered();
        }

        private void UpdateFiltered() {
            _isFiltered = this.AllowedRaces.All(r => r != GameService.Gw2Mumble.PlayerCharacter.Race);
        }

        private void PlayerCharacterOnNameChanged(object sender, ValueEventArgs<string> e) {
            UpdateFiltered();
        }

        private void AllowedRacesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            UpdateFiltered();
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes) {
            return attributes.TryGetAttribute(PRIMARY_ATTR_NAME, out var attribute)
                       ? new RaceFilter(attribute.GetValueAsEnums<RaceType>())
                       : null;
        }

        public void Update(GameTime gameTime) { /* NOOP */ }

    }
}
