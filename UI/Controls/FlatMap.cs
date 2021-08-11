using System;
using System.Linq;
using BhModule.Community.Pathing.State;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Gw2Sharp.Models;
using Gw2Sharp.Mumble.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace BhModule.Community.Pathing.Entity {
    public class FlatMap : Control {

        private const int MAPWIDTH_MAX  = 362;
        private const int MAPHEIGHT_MAX = 338;
        private const int MAPWIDTH_MIN  = 170;
        private const int MAPHEIGHT_MIN = 170;
        private const int MAPOFFSET_MIN = 19;

        private readonly IRootPackState _packState;

        private double _lastMapViewChanged = 0;
        private float  _lastCameraPos      = 0f;

        private IPathingEntity _activeEntity;

        private ContextMenuStrip BuildPathableMenu(IPathingEntity pathingEntry) {
            var newMenu = new ContextMenuStrip();

            newMenu.AddMenuItem("Hide Parent Category").Click += delegate {
                _packState.CategoryStates.SetInactive(pathingEntry.CategoryNamespace, true);
            };

            if (GameService.Input.Keyboard.ActiveModifiers.HasFlag(ModifierKeys.Shift)) {
                newMenu.AddMenuItem("Copy Parent Category Namespace").Click += async delegate { await ClipboardUtil.WindowsClipboardService.SetTextAsync(pathingEntry.CategoryNamespace); };
                newMenu.AddMenuItem("Edit Marker").Click                    += async delegate { await Editor.MarkerEditWindow.SetMarker(pathingEntry as StandardMarker); };
                newMenu.AddMenuItem("Delete Marker").Click                  += delegate { ScreenNotification.ShowNotification("Not yet supported", ScreenNotification.NotificationType.Warning, null, 5); };
            }

            newMenu.Hidden += delegate { newMenu.Dispose(); };

            return newMenu;
        }

        public FlatMap(IRootPackState packState) {
            this.ZIndex = int.MinValue;

            _packState = packState;

            this.Location    = new Point(1);
            // TODO: Decide if FlatMap should clip or not (the game does not clip).
            // this.ClipsBounds = false;

            UpdateBounds();

            GameService.Gw2Mumble.UI.UISizeChanged    += UIOnUISizeChanged;
            GameService.Gw2Mumble.UI.IsMapOpenChanged += UIOnIsMapOpenChanged;
        }

        private void TriggerFadeIn() {
            _lastMapViewChanged = (GameService.Overlay.CurrentGameTime?.TotalGameTime.TotalSeconds ?? 0);
        }

        private void UIOnUISizeChanged(object sender, ValueEventArgs<UiSize> e) {
            TriggerFadeIn();
        }

        private void UIOnIsMapOpenChanged(object sender, ValueEventArgs<bool> e) {
            TriggerFadeIn();

            _lastCameraPos = GameService.Gw2Mumble.PlayerCamera.Position.Z;
        }

        protected override void OnRightMouseButtonPressed(MouseEventArgs e) {
            base.OnRightMouseButtonPressed(e);

            if (_activeEntity != null) {
                BuildPathableMenu(_activeEntity).Show(e.MousePosition);
            }
        }

        private int GetOffset(float curr, float max, float min, float val) {
            return (int)Math.Round((curr - min) / (max - min) * (val - MAPOFFSET_MIN) + MAPOFFSET_MIN, 0);
        }

        private void UpdateBounds() {
            if (GameService.Gw2Mumble.UI.CompassSize.Width < 1 ||
                GameService.Gw2Mumble.UI.CompassSize.Height < 1) return;

            Point newSize;

            if (GameService.Gw2Mumble.UI.IsMapOpen) {
                this.Location = Point.Zero;

                newSize = GameService.Graphics.SpriteScreen.Size;
            } else {
                int offsetWidth  = GetOffset(GameService.Gw2Mumble.UI.CompassSize.Width,  MAPWIDTH_MAX,  MAPWIDTH_MIN,  40);
                int offsetHeight = GetOffset(GameService.Gw2Mumble.UI.CompassSize.Height, MAPHEIGHT_MAX, MAPHEIGHT_MIN, 40);

                if (GameService.Gw2Mumble.UI.IsCompassTopRight) {
                    this.Location = new Point(GameService.Graphics.SpriteScreen.ContentRegion.Width - GameService.Gw2Mumble.UI.CompassSize.Width - offsetWidth + 1, 1);
                } else {
                    this.Location = new Point(GameService.Graphics.SpriteScreen.ContentRegion.Width  - GameService.Gw2Mumble.UI.CompassSize.Width  - offsetWidth,
                                              GameService.Graphics.SpriteScreen.ContentRegion.Height - GameService.Gw2Mumble.UI.CompassSize.Height - offsetHeight - 40);
                }

                newSize = new Point(GameService.Gw2Mumble.UI.CompassSize.Width  + offsetWidth,
                                    GameService.Gw2Mumble.UI.CompassSize.Height + offsetHeight);
            }

            this.Size = newSize;
        }

        protected override CaptureType CapturesInput() => CaptureType.ForceNone;

        public override void DoUpdate(GameTime gameTime) {
            UpdateBounds();

            if (GameService.Gw2Mumble.UI.IsMapOpen) {
                if (GameService.Gw2Mumble.PlayerCharacter.CurrentMount == MountType.None
                    && GameService.Overlay.CurrentGameTime?.TotalGameTime.TotalSeconds - _lastMapViewChanged > 1f
                    && Math.Abs(_lastCameraPos - GameService.Gw2Mumble.PlayerCamera.Position.Z) > 0.1f) {
                    this.Hide();
                }

                _lastCameraPos = GameService.Gw2Mumble.PlayerCamera.Position.Z;
            }

            base.DoUpdate(gameTime);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            if (!GameService.GameIntegration.IsInGame) return;

            bounds = new Rectangle(this.Location, bounds.Size);

            double scale   = GameService.Gw2Mumble.UI.MapScale * Graphics.GetScaleRatio(UiSize.Normal); //Workaround to fix pixel to coordinate scaling - Blish HUD scale of 1 is "Larger" but game is "Normal".
            double offsetX = bounds.X + (bounds.Width  / 2d);
            double offsetY = bounds.Y + (bounds.Height / 2d);

            float opacity = MathHelper.Clamp((float)(GameService.Overlay.CurrentGameTime.TotalGameTime.TotalSeconds - _lastMapViewChanged) / 0.65f, 0f, 1f) * 0.8f;

            IPathingEntity[] entities = _packState.Entities.OrderBy(poi => -poi.DrawOrder).ToArray();
            
            string finalTooltip = string.Empty;
            
            foreach (var pathable in entities) {
                RectangleF? hint = pathable.RenderToMiniMap(spriteBatch, bounds, (offsetX, offsetY), scale, opacity);

                if (this.MouseOver && hint.HasValue && hint.Value.Contains(GameService.Input.Mouse.Position)) {
                    _activeEntity = pathable;

                    if (GameService.Input.Keyboard.ActiveModifiers.HasFlag(ModifierKeys.Shift)) {
                        finalTooltip = pathable.CategoryNamespace;
                    } else if (pathable is IHasMapInfo mapPathable) {
                        finalTooltip = mapPathable.TipName;
                    }
                }
            }

            this.BasicTooltipText = finalTooltip;

            if (finalTooltip == string.Empty) {
                _activeEntity = null;
            }
        }

        protected override void DisposeControl() {
            GameService.Gw2Mumble.UI.UISizeChanged    -= UIOnUISizeChanged;
            GameService.Gw2Mumble.UI.IsMapOpenChanged -= UIOnIsMapOpenChanged;

            base.DisposeControl();
        }

    }
}
