using Blish_HUD;
using Blish_HUD.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace BhModule.Community.Pathing.Entity.Effects {
    public class MarkerEffect : SharedEffect {

        // Cached parameter handles
        private readonly EffectParameter
            _pView, _pProj, _pPlayerView,
            _pPlayerPos, _pCameraPos,
            _pRace, _pMount, _pFadeNearCamera,
            _pWorld, _pTexture, _pFadeTexture,
            _pOpacity, _pFadeNear, _pFadeFar,
            _pPlayerFadeRadius, _pFadeCenter,
            _pTintColor, _pShowDebugWireframe;

        private Matrix _view, _projection, _playerView;
        private Vector3 _playerPosition;
        private Vector3 _cameraPosition;

        private int  _race;
        private int  _mount;
        private bool _fadeNearCamera;

        private Matrix    _world;
        private Texture2D _texture;
        private Texture2D _fadeTexture;
        private float     _opacity;
        private float     _fadeNear, _fadeFar;
        private float     _playerFadeRadius;
        private bool      _fadeCenter;
        private Color     _tintColor;
        private bool      _showDebugWireframe;

        #region ctors

        public MarkerEffect(Effect baseEffect) : base(baseEffect) {
            // Cache once
            _pView = Parameters["View"];
            _pProj = Parameters["Projection"];
            _pPlayerView = Parameters["PlayerView"];
            _pPlayerPos = Parameters["PlayerPosition"];
            _pCameraPos = Parameters["CameraPosition"];
            _pRace = Parameters["Race"];
            _pMount = Parameters["Mount"];
            _pFadeNearCamera = Parameters["FadeNearCamera"];
            _pWorld = Parameters["World"];
            _pTexture = Parameters["Texture"];
            _pFadeTexture = Parameters["FadeTexture"];
            _pOpacity = Parameters["Opacity"];
            _pFadeNear = Parameters["FadeNear"];
            _pFadeFar = Parameters["FadeFar"];
            _pPlayerFadeRadius = Parameters["PlayerFadeRadius"];
            _pFadeCenter = Parameters["FadeCenter"];
            _pTintColor = Parameters["TintColor"];
            _pShowDebugWireframe = Parameters["ShowDebugWireframe"];
        }

        private MarkerEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode) {
            // Cache once
            _pView = Parameters["View"];
            _pProj = Parameters["Projection"];
            _pPlayerView = Parameters["PlayerView"];
            _pPlayerPos = Parameters["PlayerPosition"];
            _pCameraPos = Parameters["CameraPosition"];
            _pRace = Parameters["Race"];
            _pMount = Parameters["Mount"];
            _pFadeNearCamera = Parameters["FadeNearCamera"];
            _pWorld = Parameters["World"];
            _pTexture = Parameters["Texture"];
            _pFadeTexture = Parameters["FadeTexture"];
            _pOpacity = Parameters["Opacity"];
            _pFadeNear = Parameters["FadeNear"];
            _pFadeFar = Parameters["FadeFar"];
            _pPlayerFadeRadius = Parameters["PlayerFadeRadius"];
            _pFadeCenter = Parameters["FadeCenter"];
            _pTintColor = Parameters["TintColor"];
            _pShowDebugWireframe = Parameters["ShowDebugWireframe"];
        }

        private MarkerEffect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count) {
            // Cache once
            _pView = Parameters["View"];
            _pProj = Parameters["Projection"];
            _pPlayerView = Parameters["PlayerView"];
            _pPlayerPos = Parameters["PlayerPosition"];
            _pCameraPos = Parameters["CameraPosition"];
            _pRace = Parameters["Race"];
            _pMount = Parameters["Mount"];
            _pFadeNearCamera = Parameters["FadeNearCamera"];
            _pWorld = Parameters["World"];
            _pTexture = Parameters["Texture"];
            _pFadeTexture = Parameters["FadeTexture"];
            _pOpacity = Parameters["Opacity"];
            _pFadeNear = Parameters["FadeNear"];
            _pFadeFar = Parameters["FadeFar"];
            _pPlayerFadeRadius = Parameters["PlayerFadeRadius"];
            _pFadeCenter = Parameters["FadeCenter"];
            _pTintColor = Parameters["TintColor"];
            _pShowDebugWireframe = Parameters["ShowDebugWireframe"];
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SetParam(ref Matrix f, Matrix v, EffectParameter p) {
            if (f == v) return false; f = v; p.SetValue(v); return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SetParam(ref Vector3 f, Vector3 v, EffectParameter p) {
            if (f == v) return false; f = v; p.SetValue(v); return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SetParam(ref Texture2D f, Texture2D v, EffectParameter p) {
            if (ReferenceEquals(f, v)) return false; f = v; p.SetValue(v); return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SetParam(ref float f, float v, EffectParameter p) {
            if (f == v) return false; f = v; p.SetValue(v); return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SetParam(ref int f, int v, EffectParameter p) {
            if (f == v) return false; f = v; p.SetValue(v); return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SetParam(ref bool f, bool v, EffectParameter p) {
            if (f == v) return false; f = v; p.SetValue(v); return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SetParam(ref Color f, Color v, EffectParameter p) {
            if (f == v) return false; f = v; p.SetValue(v.ToVector4()); return true;
        }

        // Override property setters to use cached params
        public Matrix View {
            get => _view;
            set => SetParam(ref _view, value, _pView);
        }
        public Matrix Projection {
            get => _projection;
            set => SetParam(ref _projection, value, _pProj);
        }
        public Matrix PlayerView {
            get => _playerView;
            set => SetParam(ref _playerView, value, _pPlayerView);
        }
        public Vector3 PlayerPosition {
            get => _playerPosition;
            set => SetParam(ref _playerPosition, value, _pPlayerPos);
        }
        public Vector3 CameraPosition {
            get => _cameraPosition;
            set => SetParam(ref _cameraPosition, value, _pCameraPos);
        }
        public int Race {
            get => _race;
            set => SetParam(ref _race, value, _pRace);
        }
        public int Mount {
            get => _mount;
            set => SetParam(ref _mount, value, _pMount);
        }
        public bool FadeNearCamera {
            get => _fadeNearCamera;
            set => SetParam(ref _fadeNearCamera, value, _pFadeNearCamera);
        }
        public Matrix World {
            get => _world;
            set => SetParam(ref _world, value, _pWorld);
        }
        public Texture2D Texture {
            get => _texture;
            set => SetParam(ref _texture, value, _pTexture);
        }
        public Texture2D FadeTexture {
            get => _fadeTexture;
            set => SetParam(ref _fadeTexture, value, _pFadeTexture);
        }
        public float Opacity {
            get => _opacity;
            set => SetParam(ref _opacity, value, _pOpacity);
        }
        public float FadeNear {
            get => _fadeNear;
            set => SetParam(ref _fadeNear, value, _pFadeNear);
        }
        public float FadeFar {
            get => _fadeFar;
            set => SetParam(ref _fadeFar, value, _pFadeFar);
        }
        public float PlayerFadeRadius {
            get => _playerFadeRadius;
            set => SetParam(ref _playerFadeRadius, value, _pPlayerFadeRadius);
        }
        public bool FadeCenter {
            get => _fadeCenter;
            set => SetParam(ref _fadeCenter, value, _pFadeCenter);
        }
        public Color TintColor {
            get => _tintColor;
            set => SetParam(ref _tintColor, value, _pTintColor);
        }
        public bool ShowDebugWireframe {
            get => _showDebugWireframe;
            set => SetParam(ref _showDebugWireframe, value, _pShowDebugWireframe);
        }

    }
}
