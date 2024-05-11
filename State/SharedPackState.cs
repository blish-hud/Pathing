using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Content;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.Entity.Effects;
using BhModule.Community.Pathing.State;
using Blish_HUD;
using Microsoft.Xna.Framework;
using TmfLib;
using TmfLib.Pathable;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing {
    public class SharedPackState : IRootPackState {
        
        public PathingModule Module { get; }

        [Obsolete("Use 'ModuleSettings' on the 'Module' property instead.")]
        public ModuleSettings UserConfiguration => this.Module.Settings;

        public int CurrentMapId { get; set; }

        public PathingCategory RootCategory { get; private set; }

        public MarkerEffect SharedMarkerEffect { get; private set; }
        public TrailEffect  SharedTrailEffect  { get; private set; }

        public BehaviorStates     BehaviorStates     { get; private set; }
        public AchievementStates  AchievementStates  { get; private set; }
        public RaidStates         RaidStates         { get; private set; }
        public CategoryStates     CategoryStates     { get; private set; }
        public MapStates          MapStates          { get; private set; }
        public UserResourceStates UserResourceStates { get; private set; }
        public UiStates           UiStates           { get; private set; }
        public CachedMumbleStates CachedMumbleStates { get; private set; }
        public KvStates           KvStates           { get; private set; }

        public  SafeList<IPathingEntity> Entities { get; private set; } = new();

        private ManagedState[] _managedStates;

        private bool _initialized  = false;
        private bool _loadingPack  = false;
        private bool _loadingState = false;

        public SharedPackState(PathingModule module) {
            this.Module = module;

            InitShaders();

            Blish_HUD.Common.Gw2.KeyBindings.Interact.Activated += OnInteractPressed;
        }

        public async Task Load() {
            await InitStates();
        }

        private async Task ReloadStates() {
            if (!_initialized || _loadingState)
                return;

            _loadingState = true;

            await Task.WhenAll(_managedStates.Select(state => state.Reload()));

            _loadingState = false;
        }

        private async Task InitStates() {
            _managedStates = new[] {
                await (this.CategoryStates     = new CategoryStates(this)).Start(),
                await (this.AchievementStates  = new AchievementStates(this)).Start(),
                await (this.RaidStates         = new RaidStates(this)).Start(),
                await (this.BehaviorStates     = new BehaviorStates(this)).Start(),
                await (this.MapStates          = new MapStates(this)).Start(),
                await (this.UserResourceStates = new UserResourceStates(this)).Start(),
                await (this.UiStates           = new UiStates(this)).Start(),
                await (this.CachedMumbleStates = new CachedMumbleStates(this)).Start(),
                await (this.KvStates           = new KvStates(this)).Start(),
            };

            _initialized = true;
        }

        private IPathingEntity BuildEntity(IPointOfInterest pointOfInterest) {
            return pointOfInterest.Type switch {
                PointOfInterestType.Marker => new StandardMarker(this, pointOfInterest),
                PointOfInterestType.Trail => new StandardTrail(this, pointOfInterest as ITrail),
                PointOfInterestType.Route => throw new NotImplementedException("Routes have not been implemented."),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public async Task LoadPackCollection(IPackCollection collection) {
            // TODO: Support cancel instead of spinning like this.
            while (_loadingPack) {
                await Task.Delay(100);
            }

            _loadingPack = true;

            this.RootCategory = collection.Categories;

            await ReloadStates();

            await InitPointsOfInterest(collection.PointsOfInterest);

            _loadingPack = false;
        }

        private static async Task PreloadTextures(IPointOfInterest pointOfInterest) {
            (string texture, bool shouldSample) = pointOfInterest.Type switch {
                PointOfInterestType.Marker => (pointOfInterest.GetAggregatedAttributeValue("iconfile"), false),
                PointOfInterestType.Trail => (pointOfInterest.GetAggregatedAttributeValue("texture"), true)
            };

            if (texture != null) {
                await TextureResourceManager.GetTextureResourceManager(pointOfInterest.ResourceManager).PreloadTexture(texture, shouldSample);
            }
        }

        public IPathingEntity InitPointOfInterest(PointOfInterest pointOfInterest) {
            var entity = BuildEntity(pointOfInterest);

            this.Entities.Add(entity);
            GameService.Graphics.World.AddEntity(entity);

            return entity;
        }

        public void RemovePathingEntity(IPathingEntity entity) {
            this.Entities.Remove(entity);
            GameService.Graphics.World.RemoveEntity(entity);
        }

        private async Task InitPointsOfInterest(IEnumerable<PointOfInterest> pointsOfInterest) {
            var pois = pointsOfInterest as PointOfInterest[] ?? pointsOfInterest.ToArray();

            // Avoid locking things up too much on lower-spec systems.
            foreach (var poi in pois) {
                await PreloadTextures(poi);
                InitPointOfInterest(poi);
            }

            await Task.CompletedTask;
        }

        private void InitShaders() {
            this.SharedMarkerEffect = new MarkerEffect(this.Module.ContentsManager.GetEffect(@"hlsl\marker.mgfx"));
            this.SharedTrailEffect  = new TrailEffect(this.Module.ContentsManager.GetEffect(@"hlsl\trail.mgfx"));

            this.SharedMarkerEffect.FadeTexture = this.Module.ContentsManager.GetTexture(@"png\42975.png");
        }
        
        private void OnInteractPressed(object sender, EventArgs e) {
            // TODO: OnInteractPressed needs a better place.
            foreach (var entity in this.Entities.ToArray()) {
                if (entity is StandardMarker {Focused: true} marker) {
                    marker.Interact(false);
                }
            }
        }

        public void Update(GameTime gameTime) {
            if (_managedStates == null) return;

            foreach (var state in _managedStates) {
                state.Update(gameTime);
            }
        }

        public async Task Unload() {
            foreach (var pathingEntity in this.Entities) {
                pathingEntity.Unload();
            }

            GameService.Graphics.World.RemoveEntities(this.Entities);

            this.Entities = new SafeList<IPathingEntity>();

            this.RootCategory = null;

            await TextureResourceManager.UnloadAsync();
        }

    }
}
