using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.Entity.Effects;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.State {
    public class OverridingPackState : IPackState {

        private readonly IPackState _referencePackState;

        public PathingModule  Module            => _referencePackState.Module;
        public ModuleSettings UserConfiguration => _referencePackState.UserConfiguration;

        public int CurrentMapId => _referencePackState.CurrentMapId;

        public PathingCategory RootCategory => _referencePackState.RootCategory;

        public MarkerEffect   SharedMarkerEffect => _referencePackState.SharedMarkerEffect;
        public TrailEffect    SharedTrailEffect  => _referencePackState.SharedTrailEffect;

        public BehaviorStates     BehaviorStates     => _referencePackState.BehaviorStates;
        public AchievementStates  AchievementStates  => _referencePackState.AchievementStates;
        public RaidStates         RaidStates         => _referencePackState.RaidStates;
        public CategoryStates     CategoryStates     => _referencePackState.CategoryStates;
        public MapStates          MapStates          => _referencePackState.MapStates;
        public UserResourceStates UserResourceStates => _referencePackState.UserResourceStates;
        public UiStates           UiStates           => _referencePackState.UiStates;
        public CachedMumbleStates CachedMumbleStates => _referencePackState.CachedMumbleStates;
        public KvStates           KvStates           => _referencePackState.KvStates;

        public SafeList<IPathingEntity> Entities => _referencePackState.Entities;

        public OverridingPackState(IPackState referencePackState) {
            _referencePackState = referencePackState;
        }

    }
}
