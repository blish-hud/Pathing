using Blish_HUD;
using Blish_HUD.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.Entity.Effects {
    public class MarkerEffect : SharedEffect {

        // Per-effect parameters
        private const string PARAMETER_VIEW           = "View";
        private const string PARAMETER_PROJECTION     = "Projection";
        private const string PARAMETER_PLAYERVIEW     = "PlayerView";
        private const string PARAMETER_PLAYERPOSITION = "PlayerPosition";
        private const string PARAMETER_CAMERAPOSITION = "CameraPosition";

        private Matrix _view, _projection, _playerView;
        private Vector3 _playerPosition;
        private Vector3 _cameraPosition;

        public Matrix View {
            get => _view;
            set => SetParameter(PARAMETER_VIEW, ref _view, value);
        }

        public Matrix Projection {
            get => _projection;
            set => SetParameter(PARAMETER_PROJECTION, ref _projection, value);
        }

        public Matrix PlayerView {
            get => _playerView;
            set => SetParameter(PARAMETER_PLAYERVIEW, ref _playerView, value);
        }

        public Vector3 PlayerPosition {
            get => _playerPosition;
            set => SetParameter(PARAMETER_PLAYERPOSITION, ref _playerPosition, value);
        }

        public Vector3 CameraPosition {
            get => _cameraPosition;
            set => SetParameter(PARAMETER_CAMERAPOSITION, ref _cameraPosition, value);
        }

        // Universal

        private const string PARAMETER_RACE           = "Race";
        private const string PARAMETER_MOUNT          = "Mount";
        private const string PARAMETER_FADENEARCAMERA = "FadeNearCamera";

        private int  _race;
        private int  _mount;
        private bool _fadeNearCamera;

        public int Race {
            get => _race;
            set => SetParameter(PARAMETER_RACE, ref _race, value);
        }

        public int Mount {
            get => _mount;
            set => SetParameter(PARAMETER_MOUNT, ref _mount, value);
        }

        // Entity-unique parameters
        private const string PARAMETER_WORLD              = "World";
        private const string PARAMETER_TEXTURE            = "Texture";
        private const string PARAMETER_FADETEXTURE        = "FadeTexture";
        private const string PARAMETER_OPACITY            = "Opacity";
        private const string PARAMETER_FADENEAR           = "FadeNear";
        private const string PARAMETER_FADEFAR            = "FadeFar";
        private const string PARAMETER_PLAYERFADERADIUS   = "PlayerFadeRadius";
        private const string PARAMETER_FADECENTER         = "FadeCenter";
        private const string PARAMETER_TINTCOLOR          = "TintColor";
        private const string PARAMETER_SHOWDEBUGWIREFRAME = "ShowDebugWireframe";

        private Matrix    _world;
        private Texture2D _texture;
        private Texture2D _fadeTexture;
        private float     _opacity;
        private float     _fadeNear, _fadeFar;
        private float     _playerFadeRadius;
        private bool      _fadeCenter;
        private Color     _tintColor;
        private bool      _showDebugWireframe;

        public Matrix World {
            get => _world;
            set => SetParameter(PARAMETER_WORLD, ref _world, value);
        }

        public Texture2D Texture {
            get => _texture;
            set => SetParameter(PARAMETER_TEXTURE, ref _texture, value);
        }

        public Texture2D FadeTexture {
            get => _fadeTexture;
            set => SetParameter(PARAMETER_FADETEXTURE, ref _fadeTexture, value);
        }

        public float Opacity {
            get => _opacity;
            set => SetParameter(PARAMETER_OPACITY, ref _opacity, value);
        }

        public float FadeNear {
            get => _fadeNear;
            set => SetParameter(PARAMETER_FADENEAR, ref _fadeNear, value);
        }

        public float FadeFar {
            get => _fadeFar;
            set => SetParameter(PARAMETER_FADEFAR, ref _fadeFar, value);
        }

        public float PlayerFadeRadius {
            get => _playerFadeRadius;
            set => SetParameter(PARAMETER_PLAYERFADERADIUS, ref _playerFadeRadius, value);
        }

        public bool FadeCenter {
            get => _fadeCenter;
            set => SetParameter(PARAMETER_FADECENTER, ref _fadeCenter, value);
        }

        public bool FadeNearCamera {
            get => _fadeNearCamera;
            set => SetParameter(PARAMETER_FADENEARCAMERA, ref _fadeNearCamera, value);
        }

        public Color TintColor {
            get => _tintColor;
            set => SetParameter(PARAMETER_TINTCOLOR, ref _tintColor, value);
        }

        public bool ShowDebugWireframe {
            get => _showDebugWireframe;
            set => SetParameter(PARAMETER_SHOWDEBUGWIREFRAME, ref _showDebugWireframe, value);
        }

        #region ctors

        public MarkerEffect(Effect baseEffect) : base(baseEffect) { }

        private MarkerEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode) { }

        private MarkerEffect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count) { }

        #endregion

        public void SetEntityState(Matrix world, Texture2D texture, float opacity, float fadeNear, float fadeFar, bool fadeNearCamera, Color tintColor, bool showDebugWireframe) {
            this.World              = world;
            this.Texture            = texture;
            this.Opacity            = opacity;
            this.FadeNear           = fadeNear;
            this.FadeFar            = fadeFar;
            this.FadeNearCamera     = fadeNearCamera;
            this.TintColor          = tintColor;
            this.ShowDebugWireframe = showDebugWireframe;
        }
        
        protected override void Update(GameTime gameTime) {
            this.PlayerPosition = GameService.Gw2Mumble.PlayerCharacter.Position;
            this.CameraPosition = GameService.Gw2Mumble.PlayerCamera.Position;

            // Universal
            this.Mount = (int)GameService.Gw2Mumble.PlayerCharacter.CurrentMount;
            this.Race  = (int)GameService.Gw2Mumble.PlayerCharacter.Race;

            this.View       = GameService.Gw2Mumble.PlayerCamera.View;
            this.Projection = GameService.Gw2Mumble.PlayerCamera.Projection;
            this.PlayerView = GameService.Gw2Mumble.PlayerCamera.PlayerView;
        }

    }
}
