using BhModule.Community.Pathing.Behavior;
using BhModule.Community.Pathing.Behavior.Filter;
using BhModule.Community.Pathing.Behavior.Modifier;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

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

            // TacO Behaviors
            { if (collection.TryGetSubset(StandardBehaviorFilter.PRIMARY_ATTR_NAME, out var attributes)) AddBehavior(StandardBehaviorFilter.BuildFromAttributes(attributes, _packState.BehaviorStates, this)); }
            { if (collection.TryGetSubset(AchievementFilter.PRIMARY_ATTR_NAME,      out var attributes)) AddBehavior(AchievementFilter.BuildFromAttributes(attributes, _packState.BehaviorStates)); }
            { if (collection.TryGetSubset(InfoModifier.PRIMARY_ATTR_NAME,           out var attributes)) AddBehavior(InfoModifier.BuildFromAttributes(attributes, this, _packState)); }

            // Modifiers
            { if (collection.TryGetSubset(BounceModifier.PRIMARY_ATTR_NAME,        out var attributes)) AddBehavior(BounceModifier.BuildFromAttributes(attributes, this, _packState)); }
            { if (collection.TryGetSubset(CopyModifier.PRIMARY_ATTR_NAME,          out var attributes)) AddBehavior(CopyModifier.BuildFromAttributes(attributes, this, _packState)); }
            { if (collection.TryGetSubset(ToggleModifier.PRIMARY_ATTR_NAME,        out var attributes)) AddBehavior(ToggleModifier.BuildFromAttributes(attributes, this, _packState)); }
            { if (collection.TryGetSubset(ShowHideModifier.SHOW_PRIMARY_ATTR_NAME, out var attributes)) AddBehavior(ShowHideModifier.BuildFromAttributes(attributes, this, _packState)); }
            { if (collection.TryGetSubset(ShowHideModifier.HIDE_PRIMARY_ATTR_NAME, out var attributes)) AddBehavior(ShowHideModifier.BuildFromAttributes(attributes, this, _packState)); }
        }

    }
}
