using System.Linq;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Editor.Entity;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BhModule.Community.Pathing.State {
    public class EditorStates : ManagedState {

        // TODO: Make this configurable somehow.
        private const ModifierKeys EDITOR_MODIFIERKEY = ModifierKeys.Shift;
        private const Keys         EDITOR_KEYKEY      = Keys.LeftShift | Keys.RightShift;

        /// <summary>
        /// List of entities acitvely being edited.
        /// </summary>
        public SafeList<IPathingEntity> SelectedPathingEntities { get; private set; } = new();

        private bool _multiSelect = false;

        public EditorStates(IRootPackState rootPackState) : base(rootPackState) { }
        
        protected override Task<bool> Initialize() {
            #if SHOWINDEV
                GameService.Input.Mouse.LeftMouseButtonPressed += MouseOnLeftMouseButtonPressed;
                GameService.Input.Keyboard.KeyReleased += KeyboardOnKeyReleased;
            #endif

            return Task.FromResult(true);
        }

        private void KeyboardOnKeyReleased(object sender, KeyboardEventArgs e) {
            if (EDITOR_KEYKEY.HasFlag(e.Key)) {
                _multiSelect = false;
            }
        }

        private void MouseOnLeftMouseButtonPressed(object sender, MouseEventArgs e) {
            // TODO: Move mouse picking to Core.

            if (!GameService.Input.Keyboard.ActiveModifiers.HasFlag(EDITOR_MODIFIERKEY)) return;

            var mouseRay = PickingUtil.CalculateRay(e.MousePosition,
                                                    GameService.Gw2Mumble.PlayerCamera.View,
                                                    GameService.Gw2Mumble.PlayerCamera.Projection);

            ICanPick pickedEntity = null;

            foreach (var entity in GameService.Graphics.World.Entities.OrderBy(entity => entity.DrawOrder)) {
                if (entity is ICanPick pickEntity) {
                    if (pickEntity.RayIntersects(mouseRay)) {
                        pickedEntity = pickEntity;

                        switch (entity) {
                            case IPathingEntity pathable:
                                if (this.SelectedPathingEntities.Contains(pathable)) {
                                    this.SelectedPathingEntities.Remove(pathable);
                                } else if (_multiSelect) {
                                        this.SelectedPathingEntities.Add(pathable);
                                } else {
                                    this.SelectedPathingEntities.SetRange(new[] { pathable });
                                }

                                //Editor.MarkerEditWindow.SetPathingEntity(this, marker);
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

            if (pickedEntity == null) {
                this.SelectedPathingEntities.Clear();
            }

            _multiSelect = true;
        }

        public override Task Reload() {
            this.SelectedPathingEntities = new();

            return Task.CompletedTask;
        }

        public override void Update(GameTime gameTime) {
            
        }

        public override Task Unload() {
            this.SelectedPathingEntities = new();

            return Task.CompletedTask;
        }

    }
}
