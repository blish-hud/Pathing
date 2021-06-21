using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Microsoft.Xna.Framework;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Behavior.Filter {
    public class SpecializationFilter : IBehavior, ICanFilter {

        public const string PRIMARY_ATTR_NAME = "specialization";

        private ObservableCollection<int> AllowedSpecializations { get; }

        private bool _isFiltered = false;

        public bool IsFiltered() => _isFiltered;

        public SpecializationFilter(IEnumerable<int> allowedSpecializations) : this(allowedSpecializations.ToArray()) { /* NOOP */ }

        public SpecializationFilter(params int[] allowedSpecializations) {
            this.AllowedSpecializations = new ObservableCollection<int>(allowedSpecializations);

            this.AllowedSpecializations.CollectionChanged               += AllowedSpecializationsOnCollectionChanged;
            GameService.Gw2Mumble.PlayerCharacter.SpecializationChanged += PlayerCharacterOnSpecializationChanged;

            UpdateFiltered();
        }

        private void UpdateFiltered() {
            _isFiltered = this.AllowedSpecializations.All(p => p != GameService.Gw2Mumble.PlayerCharacter.Specialization);
        }

        private void PlayerCharacterOnSpecializationChanged(object sender, ValueEventArgs<int> e) {
            UpdateFiltered();
        }

        private void AllowedSpecializationsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            UpdateFiltered();
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes) {
            return new SpecializationFilter(attributes[PRIMARY_ATTR_NAME].GetValueAsInts());
        }

        public void Update(GameTime gameTime) { /* NOOP */ }

    }
}
