using System.Linq;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.UI.Controls;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.State {
    public class UiStates : ManagedState {

        // Map
        public FlatMap Map { get; private set; }

        // Horizontal Compass
        public HorizontalCompass HorizontalCompass { get; private set; }

        // Interact
        public SmallInteract Interact { get; private set; }

        // Info
        private InfoWindow       _info;
        private Label            _infoLabel;
        private SafeList<string> _infoList;

        private ScreenDraw _screenDraw;

        public UiStates(IRootPackState rootPackState) : base(rootPackState) { /* NOOP */ }

        public override async Task Reload() {
            await Unload();

            await Initialize();
        }

        protected override Task<bool> Initialize() {
            InitInfo();
            InitMap();
            //InitHorizontalCompass();
            InitInteract();
            //InitScreenDraw();

            return Task.FromResult(true);
        }

        private void InitScreenDraw() {
            _screenDraw = new ScreenDraw() {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = this.Map.ZIndex - 1
            };
        }

        private void InitMap() {
            GameService.Gw2Mumble.CurrentMap.MapChanged += CurrentMapChanged;
            GameService.Gw2Mumble.UI.IsMapOpenChanged   += MapOpenedChanged;

            if (this.Map != null) return;
            
            this.Map = new FlatMap(_rootPackState) {
                Parent  = GameService.Graphics.SpriteScreen
            };
        }

        private void InitHorizontalCompass() {
            if (this.HorizontalCompass != null) return;

            this.HorizontalCompass = new HorizontalCompass(_rootPackState) {
                Parent = GameService.Graphics.SpriteScreen
            };
        }

        private void UpdateMapState() {
            if (this.Map == null) return;

            Map.Visible = !(GameService.Gw2Mumble.UI.IsMapOpen
                                 ? _rootPackState.UserResourceStates.Ignore.Map
                                 : _rootPackState.UserResourceStates.Ignore.Compass).Contains(GameService.Gw2Mumble.CurrentMap.Id);
        }

        private void MapOpenedChanged(object  sender, ValueEventArgs<bool> e) => UpdateMapState();
        private void CurrentMapChanged(object sender, ValueEventArgs<int>  e) => UpdateMapState();

        private void InitInfo() {
            if (_info != null) return;
            
            _info = new InfoWindow(_rootPackState) {
                Parent  = GameService.Graphics.SpriteScreen,
                Visible = false
            };

            _info.Hide(false);

            _infoLabel = new Label() {
                Width               = 350,
                WrapText            = true,
                AutoSizeHeight      = true,
                Location            = new Point(70, 60),
                VerticalAlignment   = VerticalAlignment.Middle,
                HorizontalAlignment = HorizontalAlignment.Left,
                Font                = GameService.Content.DefaultFont18,
                Parent              = _info
            };

            _infoList = new SafeList<string>();
        }

        private void UpdateInfoText() {
            string currentInfo = _infoList.ToList().LastOrDefault() ?? string.Empty;

            if (string.IsNullOrEmpty(currentInfo)) {
                _info.Hide(true);
            } else {
                // Add spacing to make font a little more readable.
                _infoLabel.Text = currentInfo.Replace(" ", "  ");
                _info.Show();
            }

        }

        public void AddInfoString(string info) {
            _infoList.Add(info);

            UpdateInfoText();
        }

        public void RemoveInfoString(string info) {
            _infoList.Remove(info);

            UpdateInfoText();
        }

        private void InitInteract() {
            this.Interact = new SmallInteract(_rootPackState) {
                Parent = GameService.Graphics.SpriteScreen
            };
        }

        public override Task Unload() {
            GameService.Gw2Mumble.CurrentMap.MapChanged -= CurrentMapChanged;
            GameService.Gw2Mumble.UI.IsMapOpenChanged   -= MapOpenedChanged;

            _info?.Hide();

            _infoList.ToList().ForEach(RemoveInfoString);
            _infoList.Clear();

            this.Interact?.Dispose();

            _screenDraw?.Dispose();

            return Task.CompletedTask;
        }

        public override void Update(GameTime gameTime) {
            if (!_rootPackState.UserConfiguration.GlobalPathablesEnabled.Value) {
                _info?.Hide();
            }
        }

    }
}
