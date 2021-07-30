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
    public class MountFilter : IBehavior, ICanFilter {

        public const string PRIMARY_ATTR_NAME = "mount";

        private ObservableCollection<MountType> AllowedMounts { get; }

        private bool _isFiltered;

        public bool IsFiltered() => _isFiltered;

        public MountFilter(IEnumerable<MountType> allowedMounts) : this(allowedMounts.ToArray()) { /* NOOP */ }

        public MountFilter(params MountType[] allowedMounts) {
            this.AllowedMounts = new ObservableCollection<MountType>(allowedMounts);

            this.AllowedMounts.CollectionChanged                      += AllowedMountsOnCollectionChanged;
            GameService.Gw2Mumble.PlayerCharacter.CurrentMountChanged += PlayerCharacterOnCurrentMountChanged;

            UpdateFiltered();
        }

        private void PlayerCharacterOnCurrentMountChanged(object sender, ValueEventArgs<MountType> e) {
            UpdateFiltered();
        }

        private void AllowedMountsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            UpdateFiltered();
        }

        private void UpdateFiltered() {
            _isFiltered = this.AllowedMounts.All(m => m != GameService.Gw2Mumble.PlayerCharacter.CurrentMount);
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes) {
            return attributes.TryGetAttribute(PRIMARY_ATTR_NAME, out var attribute)
                       ? new MountFilter(attribute.GetValueAsEnums<MountType>())
                       : null;
        }

        public void Update(GameTime gameTime) { /* NOOP */ }

        public void Unload() { /* NOOP */ }

    }
}
