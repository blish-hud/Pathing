using System;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using Blish_HUD;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BasicTooltipView = BhModule.Community.Pathing.UI.Tooltips.BasicTooltipView;

namespace BhModule.Community.Pathing.UI.Controls {
    public class SmallInteract : Control {

        private const int DRAW_WIDTH  = 64;
        private const int DRAW_HEIGHT = 64;

        private const float LEFT_OFFSET = 0.62f;
        private const float TOP_OFFSET  = 0.58f;

        private const double SUBTLE_DELAY  = 0.103d;
        private const double SUBTLE_DAMPER = 0.05d;

        private static readonly Texture2D _interact1 = PathingModule.Instance.ContentsManager.GetTexture("png/controls/102390.png");

        private readonly IRootPackState _packState;

        private Vector3 _lastPlayerPosition = Vector3.Zero;
        private double  _subtleTimer         = 0;

        private double _showStart = 0;

        private Color _tint = Color.White;

        private IPathingEntity _activePathingEntity;

        public SmallInteract(IRootPackState packState) {
            _packState = packState;

            this.Visible = false;
            this.Size    = new Point(DRAW_WIDTH, DRAW_HEIGHT);
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.DoNotBlock | CaptureType.Mouse;
        }

        public void ShowInteract(IPathingEntity pathingEntity, string interactMessage) {
            ShowInteract(pathingEntity, new BasicTooltipView(string.Format(interactMessage, $"[{Blish_HUD.Common.Gw2.KeyBindings.Interact.GetBindingDisplayText()}]")));
        }

        public void ShowInteract(IPathingEntity pathingEntity, string interactMessage, Color tint) {
            ShowInteract(pathingEntity, new BasicTooltipView(string.Format(interactMessage, $"[{Blish_HUD.Common.Gw2.KeyBindings.Interact.GetBindingDisplayText()}]")), tint);
        }

        public void ShowInteract(IPathingEntity pathingEntity, ITooltipView tooltipView, Color tint) {
            _tint = tint;

            _activePathingEntity = pathingEntity;

            if (_packState.UserResourceStates.Advanced.InteractGearAnimation) {
                _showStart = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalSeconds;
            }

            this.Tooltip = new Tooltip(tooltipView);
            this.Visible = true;
        }

        public void ShowInteract(IPathingEntity pathingEntity, ITooltipView tooltipView) {
            if (pathingEntity.BehaviorFiltered) {
                return;
            }

            ShowInteract(pathingEntity, tooltipView, Color.FromNonPremultiplied(255, 142, 50, 255));
        }

        protected override void OnClick(MouseEventArgs e) {
            base.OnClick(e);

            _activePathingEntity?.Interact(false);
        }

        public void DisconnectInteract(IPathingEntity pathingEntity) {
            if (_activePathingEntity == pathingEntity) {
                _activePathingEntity = null;
                this.Visible         = false;
                this.Tooltip         = null;
            }
        }

        public override void DoUpdate(GameTime gameTime) {
            base.DoUpdate(gameTime);

            if (GameService.Gw2Mumble.PlayerCharacter.Position != _lastPlayerPosition || GameService.Gw2Mumble.PlayerCharacter.IsInCombat) {
                _lastPlayerPosition = GameService.Gw2Mumble.PlayerCharacter.Position;

                _subtleTimer = gameTime.TotalGameTime.TotalSeconds;
            }

            if (this.Parent != null) {
                this.Location = new Point((int)(this.Parent.Width * _packState.UserResourceStates.Advanced.InteractGearXOffset), (int)(this.Parent.Height * _packState.UserResourceStates.Advanced.InteractGearYOffset));
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            if (!GameService.GameIntegration.Gw2Instance.IsInGame) return;
            if (!_packState.UserConfiguration.PackAllowInteractIcon.Value) return;

            if (_activePathingEntity == null || _activePathingEntity.IsFiltered(EntityRenderTarget.World)) return;

            float  baseOpacity = 0.4f;
            double tCTS        = Math.Max(GameService.Overlay.CurrentGameTime.TotalGameTime.TotalSeconds - _subtleTimer, SUBTLE_DAMPER);
            float  opacity     = (this.MouseOver ? baseOpacity : 0.3f) + (float)Math.Min((tCTS / SUBTLE_DELAY - SUBTLE_DAMPER) * 0.6f, 0.6f);

            spriteBatch.DrawOnCtrl(this, _interact1, bounds.OffsetBy(DRAW_WIDTH / 2, DRAW_HEIGHT / 2), null, _tint * opacity, Math.Min((float)(GameService.Overlay.CurrentGameTime.TotalGameTime.TotalSeconds - _showStart) * 20f, MathHelper.TwoPi), new Vector2(DRAW_WIDTH / 2, DRAW_HEIGHT / 2));
        }

    }
}
