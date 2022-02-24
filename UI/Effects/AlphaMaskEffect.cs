using Blish_HUD.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.UI.Effects {
    public class AlphaMaskEffect : SharedEffect {

        private static readonly AlphaMaskEffect _sharedInstance = new AlphaMaskEffect(PathingModule.Instance.ContentsManager.GetEffect(@"hlsl\alphamask.mgfx"));

        public static AlphaMaskEffect SharedInstance => _sharedInstance;

        // Per-instance parameters
        private const string PARAMETER_MASK = "Mask";

        private Texture2D _mask;

        public Texture2D Mask {
            get => _mask;
            set => SetParameter(PARAMETER_MASK, ref _mask, value);
        }

        #region ctors

        public AlphaMaskEffect(Effect cloneSource) : base(cloneSource) { }

        public AlphaMaskEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode) { }

        public AlphaMaskEffect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count) { }

        #endregion

        public void SetEffectState(Texture2D mask) {
            this.Mask = mask;
        }

        protected override void Update(GameTime gameTime) { /* NOOP */ }
        
    }
}
