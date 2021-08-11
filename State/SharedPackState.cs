using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Editor.Entity;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.Entity.Effects;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TmfLib;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing {
    public class SharedPackState : IRootPackState {
        
        public ModuleSettings UserConfiguration { get; }

        public int CurrentMapId { get; set; }

        public PathingCategory RootCategory { get; private set; }

        public MarkerEffect SharedMarkerEffect { get; private set; }
        public TrailEffect  SharedTrailEffect  { get; private set; }

        public BehaviorStates     BehaviorStates     { get; private set; }
        public AchievementStates  AchievementStates  { get; private set; }
        public CategoryStates     CategoryStates     { get; private set; }
        public MapStates          MapStates          { get; private set; }
        public UserResourceStates UserResourceStates { get; private set; }
        public UiStates           UiStates           { get; private set; }

        private readonly SafeList<IPathingEntity> _entities = new();
        public IEnumerable<IPathingEntity> Entities => _entities;

        private bool _initialized = false;
        private bool _loadingPack = false;

        public SharedPackState(ModuleSettings moduleSettings) {
            this.UserConfiguration = moduleSettings;

            InitShaders();

            Blish_HUD.Common.Gw2.KeyBindings.Interact.Activated += OnInteractPressed;

            #if SHOWINDEV
                GameService.Input.Mouse.LeftMouseButtonPressed += MouseOnLeftMouseButtonPressed;
            #endif
        }

        private async void MouseOnLeftMouseButtonPressed(object sender, MouseEventArgs e) {
            var mouseRay = PickingUtil.CalculateRay(e.MousePosition,
                                                    GameService.Gw2Mumble.PlayerCamera.View,
                                                    GameService.Gw2Mumble.PlayerCamera.Projection);

            foreach (var entity in GameService.Graphics.World.Entities.OrderBy(entity => entity.DrawOrder)) {
                if (entity is ICanPick pickEntity) {
                    if (pickEntity.RayIntersects(mouseRay)) {
                        switch (entity) {
                            case StandardMarker marker:
                                marker.DebugRender = !marker.DebugRender;

                                await Editor.MarkerEditWindow.SetMarker(marker);
                                break;
                            case IAxisHandle handle:
                                handle.HandleActivated(mouseRay);
                                break;
                            default:
                                continue;
                        }

                        break;
                    }
                }
            }
        }

        private ManagedState[] _managedStates;

        private async Task ReloadStates() {
            await Task.WhenAll(_managedStates.Select(state => state.Reload()));
        }

        private async Task InitStates() {
            _managedStates = new[] {
                await (this.CategoryStates     = new CategoryStates(this)).Start(),
                await (this.AchievementStates  = new AchievementStates(this)).Start(),
                await (this.BehaviorStates     = new BehaviorStates(this)).Start(),
                await (this.MapStates          = new MapStates(this)).Start(), 
                await (this.UserResourceStates = new UserResourceStates(this)).Start(),
                await (this.UiStates           = new UiStates(this)).Start()
            };

            _initialized = true;
        }

        public void UnloadPacks() {
            foreach (var pathingEntity in _entities) {
                pathingEntity.Unload();
            }

            GameService.Graphics.World.RemoveEntities(_entities);

            _entities.Clear();

            this.RootCategory = null;
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
                await Task.Delay(1000);
            }

            _loadingPack = true;

            this.RootCategory = collection.Categories;

            if (!_initialized) {
                await InitStates();
            } else {
                await ReloadStates();
            }

            await InitPointsOfInterest(collection.PointsOfInterest);

            _loadingPack = false;
        }

        private async Task InitPointsOfInterest(IEnumerable<PointOfInterest> pois) {
            var poiBag = new ConcurrentBag<IPathingEntity>();

            pois.AsParallel()
                .Select(BuildEntity)
                .ForAll(poiBag.Add);

            _entities.AddRange(poiBag);
            GameService.Graphics.World.AddEntities(poiBag);

            await Task.CompletedTask;
        }

        private void InitShaders() {
            this.SharedMarkerEffect = new MarkerEffect(PathingModule.Instance.ContentsManager.GetEffect(@"hlsl\marker.mgfx"));
            this.SharedTrailEffect  = new TrailEffect(PathingModule.Instance.ContentsManager.GetEffect(@"hlsl\trail.mgfx"));
        }

        private void OnInteractPressed(object sender, EventArgs e) {
            // TODO: OnInteractPressed needs a better place.
            foreach (var entity in _entities) {
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

    }
}
