using BhModule.Community.Pathing.State;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TmfLib;
using TmfLib.Pathable;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker : PathingEntity {

        private static readonly Logger Logger = Logger.GetLogger<StandardMarker>();

        private readonly IPackState _packState;

        private VertexPositionTexture[] _verts;
        private DynamicVertexBuffer     _vertexBuffer;

        private Vector2 _size { get; set; } = Vector2.Zero;

        public Vector2 Size {
            get => new(MathHelper.Clamp(_size.X, this.MinSize, this.MaxSize), MathHelper.Clamp(_size.Y, this.MinSize, this.MaxSize));
            set => _size = value;
        }

        public override float DrawOrder => Vector3.DistanceSquared(this.Position, GameService.Gw2Mumble.PlayerCamera.Position);

        public StandardMarker(IPackState packState, IPointOfInterest pointOfInterest) {
            _packState = packState;

            Populate(pointOfInterest.GetAggregatedAttributes(), pointOfInterest.ResourceManager);

            Initialize();
        }

        private void Populate(AttributeCollection collection, IPackResourceManager resourceManager) {
            Populate_Guid(collection, resourceManager);

            Populate_Type(collection, resourceManager);
            Populate_Position(collection, resourceManager);
            Populate_Triggers(collection, resourceManager);
            //Populate_MinMaxSize(collection, resourceManager);
            Populate_IconSize(collection, resourceManager);
            Populate_IconFile(collection, resourceManager);
            // Populate_Title(collection, resourceManager);
            Populate_Tint(collection, resourceManager);
            Populate_Rotation(collection, resourceManager);
            Populate_HeightOffset(collection, resourceManager);
            Populate_Alpha(collection, resourceManager);
            Populate_FadeNearAndFar(collection, resourceManager);
            Populate_Cull(collection, resourceManager);
            Populate_MapVisibility(collection, resourceManager);
            Populate_CanFade(collection, resourceManager);

            Populate_TacOMisc(collection, resourceManager);

            Populate_Behaviors(collection, resourceManager);
        }

        private void Initialize() {
            //this.Size = new Vector2(MathHelper.Clamp(this.Size.X, this.MinSize, this.MaxSize),
            //                        MathHelper.Clamp(this.Size.Y, this.MinSize, this.MaxSize));

            InitializeMiniMap();
            InitializeWorld();

            if (true) {
                this.FadeIn();
            }
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            this.DistanceToPlayer = Vector3.Distance(GameService.Gw2Mumble.PlayerCharacter.Position, this.Position);
        }

    }
}
