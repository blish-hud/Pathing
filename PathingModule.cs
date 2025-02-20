using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.Scripting;
using BhModule.Community.Pathing.Scripting.Console;
using BhModule.Community.Pathing.UI.Views;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework.Input;
using Blish_HUD.Graphics.UI;
using Blish_HUD.GameIntegration;
using Blish_HUD.Content;

namespace BhModule.Community.Pathing {
    [Export(typeof(Module))]
    public class PathingModule : Module {

        private static readonly Logger Logger = Logger.GetLogger<PathingModule>();

        #region Service Managers
        internal SettingsManager    SettingsManager    => this.ModuleParameters.SettingsManager;
        internal ContentsManager    ContentsManager    => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager      Gw2ApiManager      => this.ModuleParameters.Gw2ApiManager;
        #endregion

        internal static PathingModule Instance { get; private set; }

        public ScriptEngine                  ScriptEngine   { get; private set; }
        public ModuleSettings                Settings       { get; private set; }
        public TabbedWindow2                 SettingsWindow { get; private set; }
        public PackInitiator                 PackInitiator  { get; private set; }
        public MarkerPackRepo.MarkerPackRepo MarkerPackRepo { get; private set; }

        private CornerIcon _pathingIcon;

        private bool _packsLoading = false;

        public Tab MarkerTreeTab { get; private set; }
        public Tab PackSettingsTab { get; private set; }
        public Tab MapSettingsTab { get; private set; }
        public Tab KeybindSettingsTab { get; private set; }
        public Tab ScriptSettingsTab { get; private set; }
        public Tab MarkerRepoTab { get; private set; }

        private ConsoleWindow _scriptConsoleWindow;

        [ImportingConstructor]
        public PathingModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) {
            Instance = this;
        }

        protected override void DefineSettings(SettingCollection settings) {
            this.Settings = new ModuleSettings(this, settings);
        }

        private IEnumerable<ContextMenuStripItem> GetPathingMenuItems() {
            // Pack initiator gets to inject some menu items, first:
            if (this.PackInitiator != null) {
                foreach (var menuItem in this.PackInitiator.GetPackMenuItems()) {
                    menuItem.Enabled = menuItem.Enabled && !_packsLoading;

                    yield return menuItem;
                }
            }

            // Script Console - we only show if it's enabled or if the user is holding down shift.
            if (this.Settings.ScriptsConsoleEnabled.Value || GameService.Input.Keyboard.ActiveModifiers.HasFlag(ModifierKeys.Shift)) {
                var scriptConsole = new ContextMenuStripItem() {
                    Text = "Script Console" // TODO: Localize "Script Console"
                };

                scriptConsole.Click += (_, _) => ShowScriptWindow();
                yield return scriptConsole;
            }

            // Open Settings
            var openSettings = new ContextMenuStripItem() {
                Text = "Pathing Module Settings" // TODO: Localize "Pathing Module Settings"
            };

            openSettings.Click += (_, _) => {
                if (SettingsWindow.SelectedTab == MarkerRepoTab) {
                    SettingsWindow.SelectedTab = PackSettingsTab;

                    if (SettingsWindow.Visible) return;
                }

                SettingsWindow.ToggleWindow();
            };

            yield return openSettings;
        }

        protected override void Initialize() {
            // SOTO Fix
            if (DateTime.UtcNow.Date >= new DateTime(2023, 8, 22, 0, 0, 0, DateTimeKind.Utc) && Program.OverlayVersion < new SemVer.Version(1, 1, 0)) {
                try {
                    var tacoActive = typeof(TacOIntegration).GetProperty(nameof(TacOIntegration.TacOIsRunning)).GetSetMethod(true);
                    tacoActive?.Invoke(GameService.GameIntegration.TacO, new object[] { true });
                } catch { /* NOOP */ }
            }

            _pathingIcon = new CornerIcon() {
                IconName = Strings.General_UiName,
                Icon     = ContentsManager.GetTexture(@"png\pathing-icon.png"),
                Priority = "Markers & Trails".GetHashCode()
            };

            SettingsWindow = new TabbedWindow2(
                                                ContentsManager.GetTexture(@"png\controls\156006.png"),
                                                new Rectangle(35, 36, 900,      640),
                                                new Rectangle(95, 42, 783 + 38, 592)
                                               ) {
                Title         = Strings.General_UiName,
                Parent        = GameService.Graphics.SpriteScreen,
                Location      = new Point(100, 100),
                Emblem        = this.ContentsManager.GetTexture(@"png\controls\1615829.png"),
                Id            = $"{this.Namespace}_SettingsWindow",
                SavesPosition = true,
            };

            MarkerTreeTab      = new Tab(AsyncTexture2D.FromAssetId(1654244),                  () => new MarkerTreeView(this),                        "Marker Tree View");
            PackSettingsTab    = new Tab(ContentsManager.GetTexture(@"png\156740+155150.png"), () => new SettingsView(this.Settings.PackSettings),    Strings.Window_MainSettingsTab);
            MapSettingsTab     = new Tab(ContentsManager.GetTexture(@"png\157123+155150.png"), () => new SettingsView(this.Settings.MapSettings),     Strings.Window_MapSettingsTab);
            ScriptSettingsTab  = new Tab(AsyncTexture2D.FromAssetId(156701),                   () => new SettingsView(this.Settings.ScriptSettings),  "Script Options");
            KeybindSettingsTab = new Tab(ContentsManager.GetTexture(@"png\156734+155150.png"), () => new SettingsView(this.Settings.KeyBindSettings), Strings.Window_KeyBindSettingsTab);
            MarkerRepoTab      = new Tab(AsyncTexture2D.FromAssetId(156909),                   () => new PackRepoView(this),                          Strings.Window_DownloadMarkerPacks);

            SettingsWindow.Tabs.Add(MarkerTreeTab);
            SettingsWindow.Tabs.Add(PackSettingsTab);
            SettingsWindow.Tabs.Add(MapSettingsTab);
            SettingsWindow.Tabs.Add(ScriptSettingsTab);
            SettingsWindow.Tabs.Add(KeybindSettingsTab);
            SettingsWindow.Tabs.Add(MarkerRepoTab);

            _pathingIcon.Click += delegate {
                if (GameService.Input.Keyboard.ActiveModifiers.HasFlag(ModifierKeys.Ctrl)) {
                    this.Settings.GlobalPathablesEnabled.Value = !this.Settings.GlobalPathablesEnabled.Value;
                } else if (_pathingIcon.Enabled) {
                    TogglePathingContextMenu();
                }
            };

            _pathingIcon.RightMouseButtonPressed += delegate {
                if (_pathingIcon.Enabled) {
                    TogglePathingContextMenu();
                }
            };
        }

        private void ShowScriptWindow() {
            _scriptConsoleWindow ??= new ConsoleWindow(this);
            _scriptConsoleWindow.Show();
            _scriptConsoleWindow.BringToFront();

            _scriptConsoleWindow.FormClosed += (_,_) => _scriptConsoleWindow = null;
        }

        private void TogglePathingContextMenu() {
            var pathingContextMenuStrip = new ContextMenuStrip();
            pathingContextMenuStrip.AddMenuItems(GetPathingMenuItems());

            pathingContextMenuStrip.Show(_pathingIcon);
        }

        private void UpdateModuleLoading(string loadingMessage) {
            _pathingIcon.LoadingMessage = loadingMessage;
            if (this.RunState == ModuleRunState.Loaded && _pathingIcon != null) {
                _pathingIcon.LoadingMessage = loadingMessage;
                _packsLoading               = !string.IsNullOrWhiteSpace(loadingMessage);

                if (!_packsLoading) {
                    _pathingIcon.BasicTooltipText = Strings.General_UiName;
                }
            }
        }

        public IProgress<string> GetModuleProgressHandler() {
            // TODO: Consider enforcing a source so that multiple items can be shown in the loading tooltip.
            return new Progress<string>(UpdateModuleLoading);
        }

        protected override async Task LoadAsync() {
            var sw = Stopwatch.StartNew();
            this.ScriptEngine   = new ScriptEngine(this);
            this.MarkerPackRepo = new MarkerPackRepo.MarkerPackRepo(this);
            this.MarkerPackRepo.Init();
            this.PackInitiator  = new PackInitiator(DirectoriesManager.GetFullDirectoryPath("markers"), this, GetModuleProgressHandler());
            await this.PackInitiator.Init();

            sw.Stop();
            Logger.Debug($"Took {sw.ElapsedMilliseconds} ms to complete loading Pathing module...");
        }

        public override IView GetSettingsView() {
            return new SettingsHintView((() => {
                SettingsWindow.SelectedTab = PackSettingsTab;
                SettingsWindow.Show();
            }, 
            () => {
                SettingsWindow.SelectedTab = MarkerRepoTab;
                SettingsWindow.Show();
            }, this.PackInitiator));
        }

        protected override void Update(GameTime gameTime) {
            this.ScriptEngine?.Update(gameTime);
            this.PackInitiator?.Update(gameTime);
        }

        private void UnloadPathingElements() {
            var entities = GameService.Graphics.World.Entities as IEntity[];

            foreach (var entity in entities) {
                if (entity is IPathingEntity) {
                    GameService.Graphics.World.RemoveEntity(entity);
                }
            }
        }

        protected override void Unload() {
            this.ScriptEngine?.Unload();
            this.Settings?.Unload();
            this.PackInitiator?.Unload();
            _pathingIcon?.Dispose();
            SettingsWindow?.Dispose();
            _scriptConsoleWindow?.Dispose();

            // Help to ensure that all pathing entities
            // are removed regardless of our current state.
            UnloadPathingElements();

            Instance = null;
        }

    }

}
