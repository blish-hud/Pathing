using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.Entity.Effects;
using BhModule.Community.Pathing.State;
using Blish_HUD;
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
        public CategoryStates     CategoryStates     { get; private set; }
        public MapStates          MapStates          { get; private set; }
        public UserResourceStates UserResourceStates { get; private set; }
        public UiStates           UiStates           { get; private set; }

        private readonly SynchronizedCollection<IPathingEntity> _entities = new();
        public IEnumerable<IPathingEntity> Entities => _entities;

        private bool _initialized = false;

        public SharedPackState(ModuleSettings moduleSettings) {
            this.UserConfiguration = moduleSettings;

            InitShaders();

            Blish_HUD.Common.Gw2.KeyBindings.Interact.Activated += OnInteractPressed;
        }

        private ManagedState[] _managedStates;

        private async Task ReloadStates() {
            await Task.WhenAll(_managedStates.Select(state => state.Reload()));
        }

        private async Task InitStates() {
            _managedStates = new[] {
                await (this.CategoryStates     = new CategoryStates(this)).Start(),
                await (this.BehaviorStates     = new BehaviorStates(this)).Start(),
                await (this.MapStates          = new MapStates(this)).Start(), 
                await (this.UserResourceStates = new UserResourceStates(this)).Start(),
                await (this.UiStates           = new UiStates(this)).Start()
            };

            _initialized = true;
        }

        public void UnloadPacks() {
            lock (_entities.SyncRoot) {
                foreach (var pathingEntity in _entities) {
                    lock (pathingEntity.Behaviors.SyncRoot) pathingEntity.Behaviors.Clear();
                }

                GameService.Graphics.World.RemoveEntities(_entities);

                _entities.Clear();
            }

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
            this.RootCategory = collection.Categories;

            if (!_initialized) {
                await InitStates();
            } else {
                await ReloadStates();
            }

            await InitPointsOfInterest(collection.PointsOfInterest);
        }

        private async Task InitPointsOfInterest(IEnumerable<PointOfInterest> pois) {
            // TODO: Resolve load / unload deadlock
            //lock (_entities.SyncRoot) {
            pois.AsParallel()
                .Select(BuildEntity)
                .ForAll(newEntity => {
                            _entities.Add(newEntity);
                            GameService.Graphics.World.AddEntity(newEntity);
                            newEntity.FadeIn(); });
            //}

            await Task.CompletedTask;
        }

        private Effect GetEffect(string effectPath) {
            byte[] compiledShader = Utility.TwoMGFX.ShaderCompilerUtil.CompileShader(effectPath);

            System.IO.File.WriteAllBytes(@"C:\Users\dade\source\repos\Pathing\ref\hlsl\marker.mgfx2", compiledShader);

            return new Effect(GameService.Graphics.GraphicsDevice, compiledShader, 0, compiledShader.Length);
        }

        private void InitShaders() {
            //this.SharedMarkerEffect = new MarkerEffect(GetEffect(@"C:\Users\dade\source\repos\Pathing\ref\hlsl\marker.hlsl"));
            //this.SharedTrailEffect  = new TrailEffect(GetEffect(@"C:\Users\dade\source\repos\Pathing\ref\hlsl\trail.hlsl"));
            this.SharedMarkerEffect = new MarkerEffect(PathingModule.Instance.ContentsManager.GetEffect(@"hlsl\marker.mgfx"));
            this.SharedTrailEffect  = new TrailEffect(PathingModule.Instance.ContentsManager.GetEffect(@"hlsl\trail.mgfx"));
        }

        private void OnInteractPressed(object sender, EventArgs e) {
            // TODO: OnInteractPressed needs a better place.
            lock (_entities.SyncRoot) {
                foreach (var entity in _entities) {
                    if (entity is StandardMarker {Focused: true} marker) {
                        marker.Interact(false);
                    }
                }
            }
        }

        public void Update(GameTime gameTime) {
            GameService.Debug.StartTimeFunc("Pathing States");
            foreach (var state in _managedStates) {
                state.Update(gameTime);
            }
            GameService.Debug.StopTimeFunc("Pathing States");
        }

    }
}
