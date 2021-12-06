using System;
using System.Linq;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.UI.Tooltips;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Gw2Sharp.Models;
using Gw2Sharp.Mumble.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BhModule.Community.Pathing.Entity {
    public class FlatMap : Control {

        private const int MAPWIDTH_MAX  = 362;
        private const int MAPHEIGHT_MAX = 338;
        private const int MAPWIDTH_MIN  = 170;
        private const int MAPHEIGHT_MIN = 170;
        private const int MAPOFFSET_MIN = 19;

        private readonly SpriteBatchParameters _pathableParameters;

        private readonly IRootPackState _packState;

        private double _lastMapViewChanged = 0;
        private float  _lastCameraPos      = 0f;

        private IPathingEntity _activeEntity;

        private readonly DescriptionTooltipView _tooltipView;
        private readonly Tooltip                _activeTooltip;

        private ContextMenuStrip _activeContextMenu;

        private ContextMenuStrip BuildPathableMenu(IPathingEntity pathingEntry) {
            var newMenu = new ContextMenuStrip();

            newMenu.AddMenuItem("Hide Parent Category").Click += delegate {
                _packState.CategoryStates.SetInactive(pathingEntry.Category.Namespace, true);
            };

            if (GameService.Input.Keyboard.ActiveModifiers.HasFlag(ModifierKeys.Shift)) {
                newMenu.AddMenuItem("Copy Parent Category Namespace").Click += async delegate { await ClipboardUtil.WindowsClipboardService.SetTextAsync(pathingEntry.Category.Namespace); };
                newMenu.AddMenuItem("Edit Marker").Click                    += delegate { Editor.MarkerEditWindow.SetPathingEntity(_packState, pathingEntry); };
                newMenu.AddMenuItem("Delete Marker").Click                  += delegate { ScreenNotification.ShowNotification("Not yet supported", ScreenNotification.NotificationType.Warning, null, 5); };
            }

            newMenu.Hidden += delegate { _activeContextMenu.Dispose(); _activeContextMenu = null; };

            _activeContextMenu = newMenu;

            return newMenu;
        }

        public FlatMap(IRootPackState packState) {
            this.ZIndex = int.MinValue / 2;

            _packState = packState;

            this.Location    = new Point(1);
            // TODO: Decide if FlatMap should clip or not (the game does not clip).
            // this.ClipsBounds = false;

            _pathableParameters        = this.SpriteBatchParameters;
            this.SpriteBatchParameters = new SpriteBatchParameters(SpriteSortMode.Deferred, BlendState.Opaque);

            UpdateBounds();

            _tooltipView   = new DescriptionTooltipView();
            _activeTooltip = new Tooltip(_tooltipView);
            this.Tooltip   = _activeTooltip;

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
            if (_activeContextMenu != null) { _activeContextMenu.Visible = false; }

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

        protected override CaptureType CapturesInput() => CaptureType.Mouse | CaptureType.DoNotBlock;

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

        private void UpdateTooltip(IPathingEntity pathable, bool isAlternativeMenu = false) {
            string tooltipTitle;
            string tooltipDescription = "";

            if (pathable != null && isAlternativeMenu) {
                tooltipTitle       = string.Join("\n > ", pathable.Category.GetParentsDesc().Select(category => category.DisplayName.Trim()));
                tooltipDescription = null;
            } else if (pathable is IHasMapInfo mapPathable) {
                if ((tooltipTitle = mapPathable.TipName) == "") {
                    this.Tooltip = null;
                    _activeTooltip.Hide();
                    return;
                }

                if (!string.IsNullOrWhiteSpace(mapPathable.TipDescription)) {
                    tooltipDescription = mapPathable.TipDescription + "\n\n";
                }

                tooltipDescription += $"{WorldUtil.WorldToGameCoord(pathable.DistanceToPlayer):##,###} away";
            } else {
                this.Tooltip = null;
                _activeTooltip.Hide();
                return;
            }

            _tooltipView.Title       = tooltipTitle;
            _tooltipView.Description = tooltipDescription;
            this.Tooltip             = _activeTooltip;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            if (!GameService.GameIntegration.Gw2Instance.IsInGame) return;

            bounds = new Rectangle(this.Location, bounds.Size);

            // We clear the bounds and start the spritebatch again
            // to mask 3D elements from showing above the flatmap.
            spriteBatch.Draw(ContentService.Textures.TransparentPixel, bounds, Color.Transparent);
            spriteBatch.End();
            spriteBatch.Begin(_pathableParameters);

            double scale   = GameService.Gw2Mumble.UI.MapScale * 0.897d; //Workaround to fix pixel to coordinate scaling - Blish HUD scale of 1 is "Larger" but game is "Normal".
            double offsetX = bounds.X + (bounds.Width  / 2d);
            double offsetY = bounds.Y + (bounds.Height / 2d);

            float opacity = MathHelper.Clamp((float)(GameService.Overlay.CurrentGameTime.TotalGameTime.TotalSeconds - _lastMapViewChanged) / 0.65f, 0f, 1f) * 0.8f;

            // TODO: Make this more based on relative vertical axis difference.
            // TODO: Revise how we do this - tons of memory allocations.
            var entities = _packState.Entities.ToList().OrderBy(poi => -poi.DrawOrder);

            bool showModTooltip = (GameService.Input.Keyboard.ActiveModifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

            _activeEntity = null;

            // Used to allow users to reduce the opacity of markers on either the compass or fullscreen map.
            float overrideOpacity = GameService.Gw2Mumble.UI.IsMapOpen 
                                        ? _packState.UserConfiguration.MapDrawOpacity.Value 
                                        : _packState.UserConfiguration.MiniMapDrawOpacity.Value;

            foreach (var pathable in entities) {
                var hint = pathable.RenderToMiniMap(spriteBatch, bounds, (offsetX, offsetY), scale, overrideOpacity * opacity);

                if (this.MouseOver && hint.HasValue && hint.Value.Contains(GameService.Input.Mouse.Position)) {
                    _activeEntity = pathable;
                }
            }

            UpdateTooltip(_activeEntity, showModTooltip);
        }

        protected override void DisposeControl() {
            GameService.Gw2Mumble.UI.UISizeChanged    -= UIOnUISizeChanged;
            GameService.Gw2Mumble.UI.IsMapOpenChanged -= UIOnIsMapOpenChanged;

            base.DisposeControl();
        }

    }
}
