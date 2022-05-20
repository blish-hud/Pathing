using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Behavior.Filter {
    public class RaidFilter : Behavior<IPathingEntity>, ICanFilter {

        public const string PRIMARY_ATTR_NAME = "raid";

        private readonly IPackState _packState;

        public List<string> Raids { get; set; }

        public RaidFilter(IEnumerable<string> raids, IPathingEntity pathingEntity, IPackState packState) : base(pathingEntity) {
            this.Raids = raids.Select(raid => raid.ToLowerInvariant()).ToList();

            _packState = packState;
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes, IPathingEntity pathingEntity, IPackState packState) {
            return new RaidFilter(attributes.TryGetAttribute(PRIMARY_ATTR_NAME,  out var idAttr) ? idAttr.GetValueAsStrings() : Enumerable.Empty<string>(),
                                  pathingEntity,
                                  packState);
        }

        public bool IsFiltered() {
            return this.Raids.Any() && !_packState.RaidStates.AreRaidsComplete(this.Raids);
        }

    }
}
