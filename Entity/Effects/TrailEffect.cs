using Blish_HUD;
using Blish_HUD.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace BhModule.Community.Pathing.Entity.Effects {
    public class TrailEffect : SharedEffect {

        // Cached parameter handles
        private readonly EffectParameter
            _pWorldViewProjection, _pPlayerView, _pPlayerPosition, _pCameraPosition, _pTotalMilliseconds,
            _pRace, _pMount,
            _pTexture, _pFadeTexture, _pFlowSpeed, _pFadeNear, _pFadeFar, _pOpacity,
            _pTintColor, _pPlayerFadeRadius, _pFadeCenter;

        // Backing fields
        private Matrix _worldViewProjection;
        private Matrix _playerView;
        private Vector3 _playerPosition;
        private Vector3 _cameraPosition;
        private float _totalMilliseconds;

        private int _race;
        private int _mount;

        private Texture2D _texture;
        private Texture2D _fadeTexture;
        private float _flowSpeed;
        private float _fadeNear, _fadeFar;
        private float _opacity;
        private float _playerFadeRadius;
        private bool _fadeCenter;
        private Color _tintColor;

        public Matrix WorldViewProjection {
            get => _worldViewProjection;
            set => SetParam(ref _worldViewProjection, value, _pWorldViewProjection);
        }

        public Matrix PlayerView {
            get => _playerView;
            set => SetParam(ref _playerView, value, _pPlayerView);
        }

        public Vector3 PlayerPosition {
            get => _playerPosition;
            set => SetParam(ref _playerPosition, value, _pPlayerPosition);
        }

        public Vector3 CameraPosition {
            get => _cameraPosition;
            set => SetParam(ref _cameraPosition, value, _pCameraPosition);
        }

        public float TotalMilliseconds {
            get => _totalMilliseconds;
            set => SetParam(ref _totalMilliseconds, value, _pTotalMilliseconds);
        }

        public int Race {
            get => _race;
            set => SetParam(ref _race, value, _pRace);
        }

        public int Mount {
            get => _mount;
            set => SetParam(ref _mount, value, _pMount);
        }

        public Texture2D Texture {
            get => _texture;
            set => SetParam(ref _texture, value, _pTexture);
        }

        public Texture2D FadeTexture {
            get => _fadeTexture;
            set => SetParam(ref _fadeTexture, value, _pFadeTexture);
        }

        public float FlowSpeed {
            get => _flowSpeed;
            set => SetParam(ref _flowSpeed, value, _pFlowSpeed);
        }

        public float FadeNear {
            get => _fadeNear;
            set => SetParam(ref _fadeNear, value, _pFadeNear);
        }

        public float FadeFar {
            get => _fadeFar;
            set => SetParam(ref _fadeFar, value, _pFadeFar);
        }

        public float Opacity {
            get => _opacity;
            set => SetParam(ref _opacity, value, _pOpacity);
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


        #region ctors

        public TrailEffect(Effect cloneSource) : base(cloneSource) {
            _pWorldViewProjection = Parameters["WorldViewProjection"];
            _pPlayerView = Parameters["PlayerView"];
            _pPlayerPosition = Parameters["PlayerPosition"];
            _pCameraPosition = Parameters["CameraPosition"];
            _pTotalMilliseconds = Parameters["TotalMilliseconds"];
            _pRace = Parameters["Race"];
            _pMount = Parameters["Mount"];
            _pTexture = Parameters["Texture"];
            _pFadeTexture = Parameters["FadeTexture"];
            _pFlowSpeed = Parameters["FlowSpeed"];
            _pFadeNear = Parameters["FadeNear"];
            _pFadeFar = Parameters["FadeFar"];
            _pOpacity = Parameters["Opacity"];
            _pTintColor = Parameters["TintColor"];
            _pPlayerFadeRadius = Parameters["PlayerFadeRadius"];
            _pFadeCenter = Parameters["FadeCenter"];
        }

        public TrailEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode) {
            _pWorldViewProjection = Parameters["WorldViewProjection"];
            _pPlayerView = Parameters["PlayerView"];
            _pPlayerPosition = Parameters["PlayerPosition"];
            _pCameraPosition = Parameters["CameraPosition"];
            _pTotalMilliseconds = Parameters["TotalMilliseconds"];
            _pRace = Parameters["Race"];
            _pMount = Parameters["Mount"];
            _pTexture = Parameters["Texture"];
            _pFadeTexture = Parameters["FadeTexture"];
            _pFlowSpeed = Parameters["FlowSpeed"];
            _pFadeNear = Parameters["FadeNear"];
            _pFadeFar = Parameters["FadeFar"];
            _pOpacity = Parameters["Opacity"];
            _pTintColor = Parameters["TintColor"];
            _pPlayerFadeRadius = Parameters["PlayerFadeRadius"];
            _pFadeCenter = Parameters["FadeCenter"];
        }

        public TrailEffect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count) {
            _pWorldViewProjection = Parameters["WorldViewProjection"];
            _pPlayerView = Parameters["PlayerView"];
            _pPlayerPosition = Parameters["PlayerPosition"];
            _pCameraPosition = Parameters["CameraPosition"];
            _pTotalMilliseconds = Parameters["TotalMilliseconds"];
            _pRace = Parameters["Race"];
            _pMount = Parameters["Mount"];
            _pTexture = Parameters["Texture"];
            _pFadeTexture = Parameters["FadeTexture"];
            _pFlowSpeed = Parameters["FlowSpeed"];
            _pFadeNear = Parameters["FadeNear"];
            _pFadeFar = Parameters["FadeFar"];
            _pOpacity = Parameters["Opacity"];
            _pTintColor = Parameters["TintColor"];
            _pPlayerFadeRadius = Parameters["PlayerFadeRadius"];
            _pFadeCenter = Parameters["FadeCenter"];
        }

        #endregion

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

        public void SetEntityState(Texture2D texture, float flowSpeed, float fadeNear, float fadeFar, float opacity, float playerFadeRadius, bool fadeCenter, Color tintColor) {
            this.Texture          = texture;
            this.FlowSpeed        = flowSpeed;
            this.FadeNear         = fadeNear;
            this.FadeFar          = fadeFar;
            this.Opacity          = opacity;
            this.PlayerFadeRadius = playerFadeRadius;
            this.FadeCenter       = fadeCenter;
            this.TintColor        = tintColor;
        }

        protected override void Update(GameTime gameTime) {
            this.TotalMilliseconds = (float)gameTime.TotalGameTime.TotalMilliseconds;
            this.PlayerPosition    = GameService.Gw2Mumble.PlayerCharacter.Position;
            this.CameraPosition    = GameService.Gw2Mumble.PlayerCamera.Position;

            // Universal
            this.Mount = (int)GameService.Gw2Mumble.PlayerCharacter.CurrentMount;
            this.Race  = (int)GameService.Gw2Mumble.PlayerCharacter.Race;

            // TODO: Move to Graphics pipeline
            this.WorldViewProjection = GameService.Gw2Mumble.PlayerCamera.WorldViewProjection;
            this.PlayerView          = GameService.Gw2Mumble.PlayerCamera.PlayerView;
        }

    }
}
