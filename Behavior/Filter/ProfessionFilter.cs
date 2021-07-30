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
    public class ProfessionFilter : IBehavior, ICanFilter {

        public const string PRIMARY_ATTR_NAME = "profession";

        private ObservableCollection<ProfessionType> AllowedProfessions { get; }

        private bool _isFiltered = false;

        public bool IsFiltered() => _isFiltered;

        public ProfessionFilter(IEnumerable<ProfessionType> allowedProfessions) : this(allowedProfessions.ToArray()) { /* NOOP */ }

        public ProfessionFilter(params ProfessionType[] allowedProfessions) {
            this.AllowedProfessions = new ObservableCollection<ProfessionType>(allowedProfessions);

            this.AllowedProfessions.CollectionChanged         += AllowedProfessionsOnCollectionChanged;
            GameService.Gw2Mumble.PlayerCharacter.NameChanged += PlayerCharacterOnNameChanged;

            UpdateFiltered();
        }

        private void UpdateFiltered() {
            _isFiltered = this.AllowedProfessions.All(p => p != GameService.Gw2Mumble.PlayerCharacter.Profession);
        }

        private void PlayerCharacterOnNameChanged(object sender, ValueEventArgs<string> e) {
            UpdateFiltered();
        }

        private void AllowedProfessionsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            UpdateFiltered();
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes) {
            return attributes.TryGetAttribute(PRIMARY_ATTR_NAME, out var attribute)
                       ? new ProfessionFilter(attribute.GetValueAsEnums<ProfessionType>())
                       : null;
        }

        public void Update(GameTime gameTime) { /* NOOP */ }

        public void Unload() { /* NOOP */ }

    }
}
