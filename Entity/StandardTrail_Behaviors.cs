using BhModule.Community.Pathing.Behavior;
using BhModule.Community.Pathing.Behavior.Filter;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        private void AddBehavior(IBehavior behavior) {
            if (behavior == null) return;

            this.Behaviors.Add(behavior);
        }

        private void Populate_Behaviors(AttributeCollection collection, IPackResourceManager resourceManager) {
            // Filters
            { if (collection.TryGetSubset(FestivalFilter.PRIMARY_ATTR_NAME,       out var attributes)) AddBehavior(FestivalFilter.BuildFromAttributes(attributes)); }
            { if (collection.TryGetSubset(MountFilter.PRIMARY_ATTR_NAME,          out var attributes)) AddBehavior(MountFilter.BuildFromAttributes(attributes)); }
            { if (collection.TryGetSubset(ProfessionFilter.PRIMARY_ATTR_NAME,     out var attributes)) AddBehavior(ProfessionFilter.BuildFromAttributes(attributes)); }
            { if (collection.TryGetSubset(RaceFilter.PRIMARY_ATTR_NAME,           out var attributes)) AddBehavior(RaceFilter.BuildFromAttributes(attributes)); }
            { if (collection.TryGetSubset(SpecializationFilter.PRIMARY_ATTR_NAME, out var attributes)) AddBehavior(SpecializationFilter.BuildFromAttributes(attributes)); }
            { if (collection.TryGetSubset(MapTypeFilter.PRIMARY_ATTR_NAME,        out var attributes)) AddBehavior(MapTypeFilter.BuildFromAttributes(attributes)); }

            { if (collection.TryGetSubset(AchievementFilter.PRIMARY_ATTR_NAME, out var attributes)) AddBehavior(AchievementFilter.BuildFromAttributes(attributes, _packState)); }
        }

    }
}
