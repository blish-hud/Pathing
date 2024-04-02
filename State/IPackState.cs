using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.Entity.Effects;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.State {
    public interface IPackState {

        PathingModule Module { get; }
        ModuleSettings UserConfiguration { get; }

        int CurrentMapId { get; }

        PathingCategory RootCategory { get; }

        MarkerEffect SharedMarkerEffect { get; }
        TrailEffect SharedTrailEffect { get; }

        BehaviorStates     BehaviorStates     { get; }
        AchievementStates  AchievementStates  { get; }
        RaidStates         RaidStates         { get; }
        CategoryStates     CategoryStates     { get; }
        MapStates          MapStates          { get; }
        UserResourceStates UserResourceStates { get; }
        UiStates           UiStates           { get; }
        CachedMumbleStates CachedMumbleStates { get; }
        KvStates           KvStates           { get; }

        SafeList<IPathingEntity> Entities { get; }

        // TODO: Needs unload

    }
}
