using System;
using System.Runtime.CompilerServices;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Input;

namespace BhModule.Community.Pathing {

    public enum MarkerClipboardConsentLevel {
        Always,
        OnlyWhenInteractedWith,
        Never
    }

    public enum MarkerInfoDisplayMode {
        Default = 0,
        WithoutBackground = 1,
        NeverDisplay = 100
    }


    public enum MapVisibilityLevel {
        Default,
        Always,
        Never
    }

    public class ModuleSettings {

        private readonly PathingModule _module;

        public ModuleSettings(PathingModule module, SettingCollection settings) {
            _module = module;

            InitGlobalSettings(settings);
            InitPackSettings(settings);
            InitMapSettings(settings);
            InitScriptSettings(settings);
            InitKeyBindSettings(settings);
        }

        #region Global Settings

        private const string GLOBAL_SETTINGS = "global-settings";

        public SettingCollection GlobalSettings { get; private set; }

        public SettingEntry<bool> GlobalPathablesEnabled      { get; private set; }

        private void InitGlobalSettings(SettingCollection settings) {
            this.GlobalSettings = settings.AddSubCollection(GLOBAL_SETTINGS);

            this.GlobalPathablesEnabled = this.GlobalSettings.DefineSetting(nameof(this.GlobalPathablesEnabled), true);
        }

        #endregion

        #region Pack Settings

        private const string PACK_SETTINGS = "pack-settings";

        public SettingCollection PackSettings { get; private set; }

        public SettingEntry<bool>                        PackWorldPathablesEnabled                { get; private set; }
        public SettingEntry<float>                       PackMaxOpacityOverride                   { get; private set; }
        public SettingEntry<float>                       PackMaxViewDistance                      { get; private set; }
        public SettingEntry<float>                       PackMaxTrailAnimationSpeed               { get; private set; }
        public SettingEntry<float>                       PackMarkerScale                          { get; private set; }
        public SettingEntry<bool>                        PackFadeTrailsAroundCharacter            { get; private set; }
        public SettingEntry<bool>                        PackFadePathablesDuringCombat            { get; private set; }
        public SettingEntry<bool>                        PackFadeMarkersBetweenCharacterAndCamera { get; private set; }
        public SettingEntry<bool>                        PackAllowMarkersToAutomaticallyHide      { get; private set; }
        public SettingEntry<MarkerClipboardConsentLevel> PackMarkerConsentToClipboard             { get; private set; }
        public SettingEntry<MarkerInfoDisplayMode>       PackInfoDisplayMode                      { get; private set; }
        public SettingEntry<bool>                        PackAllowInfoText                        { get; private set; }
        public SettingEntry<bool>                        PackAllowInteractIcon                    { get; private set; }
        public SettingEntry<bool>                        PackAllowMarkersToAnimate                { get; private set; }
        public SettingEntry<bool>                        PackEnableSmartCategoryFilter            { get; private set; }
        public SettingEntry<bool>                        PackShowWhenCategoriesAreFiltered        { get; private set; }
        public SettingEntry<bool>                        PackTruncateLongCategoryNames            { get; private set; }
        public SettingEntry<bool>                        PackShowHiddenMarkersReducedOpacity      { get; private set; }
        public SettingEntry<bool>                        PackShowTooltipsOnAchievements           { get; private set; }

        private void InitPackSettings(SettingCollection settings) {
            this.PackSettings = settings.AddSubCollection(PACK_SETTINGS);

            // TODO: Add string to strings.resx for localization.
            // TODO: Add description to settings.
            this.PackWorldPathablesEnabled                = this.PackSettings.DefineSetting(nameof(this.PackWorldPathablesEnabled),                true, () => "Show Markers in World");
            this.PackMaxOpacityOverride                   = this.PackSettings.DefineSetting(nameof(this.PackMaxOpacityOverride),                   1f, () => Strings.Setting_PackMaxOpacityOverride, () => "");
            this.PackMaxViewDistance                      = this.PackSettings.DefineSetting(nameof(this.PackMaxViewDistance),                      25000f, () => Strings.Setting_PackMaxViewDistance, () => "");
            this.PackMaxTrailAnimationSpeed               = this.PackSettings.DefineSetting(nameof(this.PackMaxTrailAnimationSpeed),               10f, () => Strings.Setting_PackMaxTrailAnimationSpeed, () => "");
            this.PackMarkerScale                          = this.PackSettings.DefineSetting(nameof(this.PackMarkerScale),                          1f,   () => "Marker Scale", () => $"Modifies the size of markers in the world.");
            this.PackFadeTrailsAroundCharacter            = this.PackSettings.DefineSetting(nameof(this.PackFadeTrailsAroundCharacter),            true, () => Strings.Setting_PackFadeTrailsAroundCharacter, () => "If enabled, trails will be faded out around your character to make it easier to see your character.");
            this.PackFadePathablesDuringCombat            = this.PackSettings.DefineSetting(nameof(this.PackFadePathablesDuringCombat),            true, () => Strings.Setting_PackFadePathablesDuringCombat, () => "If enabled, markers and trails will be hidden while you're in combat to avoid obscuring the fight.");
            this.PackFadeMarkersBetweenCharacterAndCamera = this.PackSettings.DefineSetting(nameof(this.PackFadeMarkersBetweenCharacterAndCamera), true, () => Strings.Setting_PackFadeMarkersBetweenCharacterAndCamera, () => "If enabled, markers will be drawn with less opacity if they are directly between your character and the camera to avoid obscuring your vision.");
            this.PackAllowMarkersToAutomaticallyHide      = this.PackSettings.DefineSetting(nameof(this.PackAllowMarkersToAutomaticallyHide),      true, () => Strings.Setting_PackAllowMarkersToAutomaticallyHide, () => "If enabled, markers and trails may hide themselves as a result of interactions, API data, current festival, etc.  Disabling this feature forces all markers on the map to be shown.");
            this.PackMarkerConsentToClipboard             = this.PackSettings.DefineSetting(nameof(this.PackMarkerConsentToClipboard),             MarkerClipboardConsentLevel.Always, () => Strings.Setting_PackMarkerConsentToClipboard, () => string.Format(Strings.Setting_PackMarkerConsentToClipboardDescription, Blish_HUD.Common.Gw2.KeyBindings.Interact.GetBindingDisplayText()));
            //this.PackAllowInfoText                        = this.PackSettings.DefineSetting(nameof(this.PackAllowInfoText),                        true, () => "Allow Markers to Show Info Text On-Screen", () => "If enabled, certain markers will be able to show information when your character is nearby to the marker.");
            this.PackInfoDisplayMode                      = this.PackSettings.DefineSetting(nameof(this.PackInfoDisplayMode),                      MarkerInfoDisplayMode.Default, () => "Marker Info Display Mode", () => "Default - Popups with extra info will show when you are near certain markers.\r\n\r\nWithout Background - Extra info will show when you are near certain markers, but there won't be a background behind the text.\r\n\r\nNever Display - Markers will not show popup info text on your screen.");
            this.PackAllowInteractIcon                    = this.PackSettings.DefineSetting(nameof(this.PackAllowInteractIcon),                    true, () => "Allow Markers to Show Interact Gear On-Screen", () => "If enabled, interactable markers will show a small gear icon on-screen to show what the interaction will do.");
            this.PackAllowMarkersToAnimate                = this.PackSettings.DefineSetting(nameof(this.PackAllowMarkersToAnimate),                true, () => Strings.Setting_PackAllowMarkersToAnimate, () => "Allows animations such as 'bounce' and trail movements.");
            this.PackEnableSmartCategoryFilter            = this.PackSettings.DefineSetting(nameof(this.PackEnableSmartCategoryFilter),            true, () => "Enable Smart Categories", () => "If a category doesn't contain markers or trails relevant to the current map, the category is hidden.");
            this.PackShowWhenCategoriesAreFiltered        = this.PackSettings.DefineSetting(nameof(this.PackShowWhenCategoriesAreFiltered),        true, () => "Indicate When Categories Are Hidden", () => "Shows a note at the bottom of the menu indicating if categories have been hidden.  Clicking the note will show the hidden categories temporarily.");
            this.PackTruncateLongCategoryNames            = this.PackSettings.DefineSetting(nameof(this.PackTruncateLongCategoryNames),            false, () => "Truncate Long Category Names", () => "Shortens long category names so that more nested menus can be shown on screen.");
            this.PackShowHiddenMarkersReducedOpacity      = this.PackSettings.DefineSetting(nameof(this.PackShowHiddenMarkersReducedOpacity),      false, () => "Temporarily Show Ghost Markers", () => "Shows hidden markers with a reduced opacity allowing you to unhide them.  This setting automatically disables on startup.");
            this.PackShowTooltipsOnAchievements           = this.PackSettings.DefineSetting(nameof(this.PackShowTooltipsOnAchievements),           false, () => "Show Tooltips for Achievements", () => "Warning: This can cause performance issues when browsing categories.");

            this.PackMaxOpacityOverride.SetRange(0f, 1f);
            this.PackMaxViewDistance.SetRange(25f, 50000f);
            this.PackMaxTrailAnimationSpeed.SetRange(0f, 10f);
            this.PackMarkerScale.SetRange(0.1f, 4f);

            // Reset this one back to false.
            this.PackShowHiddenMarkersReducedOpacity.Value = false;
        }

        #endregion

        #region Map Settings

        private const string MAP_SETTINGS = "map-settings";

        public SettingCollection MapSettings { get; private set; }

        public SettingEntry<bool>               MapPathablesEnabled                   { get; private set; }
        public SettingEntry<MapVisibilityLevel> MapMarkerVisibilityLevel              { get; private set; }
        public SettingEntry<MapVisibilityLevel> MapTrailVisibilityLevel               { get; private set; }
        public SettingEntry<float>              MapDrawOpacity                        { get; private set; }
        public SettingEntry<MapVisibilityLevel> MiniMapMarkerVisibilityLevel          { get; private set; }
        public SettingEntry<MapVisibilityLevel> MiniMapTrailVisibilityLevel           { get; private set; }
        public SettingEntry<float>              MiniMapDrawOpacity                    { get; private set; }
        public SettingEntry<bool>               MapShowAboveBelowIndicators           { get; private set; }
        public SettingEntry<bool>               MapFadeVerticallyDistantTrailSegments { get; private set; }
        public SettingEntry<float>              MapTrailWidth                         { get; private set; }
        public SettingEntry<bool>               MapShowTooltip                        { get; private set; }
        public SettingEntry<bool>               MapTrailGlow                          { get; private set; }
        public SettingEntry<int>                MapTrailGlowSpeed                     { get; private set; }
        public SettingEntry<float>              MapTrailGlowLength                    { get; private set; }
        public SettingEntry<float>              MapTrailGlowOpacity                { get; private set; }
        public SettingEntry<int>                MapTrailGlowBeadCount                 { get; private set; }

        private void InitMapSettings(SettingCollection settings) {
            this.MapSettings = settings.AddSubCollection(MAP_SETTINGS);

            // TODO: Add string to strings.resx for localization.
            // TODO: Add description to settings.
            this.MapPathablesEnabled                   = this.MapSettings.DefineSetting(nameof(this.MapPathablesEnabled),                   true,                       () => "Show Markers on Maps");
            this.MapMarkerVisibilityLevel              = this.MapSettings.DefineSetting(nameof(this.MapMarkerVisibilityLevel),              MapVisibilityLevel.Default, () => Strings.Setting_MapShowMarkersOnFullscreen,          () => "");
            this.MapTrailVisibilityLevel               = this.MapSettings.DefineSetting(nameof(this.MapTrailVisibilityLevel),               MapVisibilityLevel.Default, () => Strings.Setting_MapShowTrailsOnFullscreen,           () => "");
            this.MapDrawOpacity                        = this.MapSettings.DefineSetting(nameof(this.MapDrawOpacity),                        1f,                         () => "Opacity on Fullscreen Map",                         () => "");
            this.MiniMapMarkerVisibilityLevel          = this.MapSettings.DefineSetting(nameof(this.MiniMapMarkerVisibilityLevel),          MapVisibilityLevel.Default, () => Strings.Setting_MapShowMarkersOnCompass,             () => "");
            this.MiniMapTrailVisibilityLevel           = this.MapSettings.DefineSetting(nameof(this.MiniMapTrailVisibilityLevel),           MapVisibilityLevel.Default, () => Strings.Setting_MapShowTrailsOnCompass,              () => "");
            this.MiniMapDrawOpacity                    = this.MapSettings.DefineSetting(nameof(this.MiniMapDrawOpacity),                    1f,                         () => "Opacity on the Minimap",                            () => "");
            this.MapShowAboveBelowIndicators           = this.MapSettings.DefineSetting(nameof(this.MapShowAboveBelowIndicators),           true,                       () => Strings.Setting_MapShowAboveBelowIndicators,         () => "");
            this.MapFadeVerticallyDistantTrailSegments = this.MapSettings.DefineSetting(nameof(this.MapFadeVerticallyDistantTrailSegments), true,                       () => "Fade Trail Segments Which Are High Above or Below", () => "");
            this.MapShowTooltip                        = this.MapSettings.DefineSetting(nameof(this.MapShowTooltip),                        true,                       () => "Show Tooltips on Map",                              () => "If enabled, tooltips will be shown on the map when the cursor is over a marker.");
            this.MapTrailWidth                         = this.MapSettings.DefineSetting(nameof(this.MapTrailWidth),                         2f,                         () => "Trail Width on Maps",                               () => "The thickness of trails shown on the map.");
            this.MapTrailGlowBeadCount                 = this.MapSettings.DefineSetting(nameof(this.MapTrailGlowBeadCount),                 0,                          () => "Trail Glow Bead Count",                             () => "Number of glow beads on the map");
            this.MapTrailGlowSpeed                     = this.MapSettings.DefineSetting(nameof(this.MapTrailGlowSpeed),                     20,                         () => "Trail Glow Speed",                                  () => "The speed at which the glow moves along the trail");
            this.MapTrailGlowLength                    = this.MapSettings.DefineSetting(nameof(this.MapTrailGlowLength),                    1f,                         () => "Trail Glow Length",                                 () => "The maximum length of a glowing segment");
            this.MapTrailGlowOpacity                   = this.MapSettings.DefineSetting(nameof(this.MapTrailGlowOpacity),                   0.3f,                       () => "Trail Glow Opacity",                                () => "");
            
            this.MapDrawOpacity.SetRange(0f, 1f);
            this.MiniMapDrawOpacity.SetRange(0f, 1f);
            this.MapTrailWidth.SetRange(0.5f, 4.5f);
            this.MapTrailGlowBeadCount.SetRange(0, 100);
            this.MapTrailGlowSpeed.SetRange(3, 50);
            this.MapTrailGlowLength.SetRange(0.5f, 10f);
            this.MapTrailGlowOpacity.SetRange(0f, 1f);
        }

        #endregion

        #region Script Settings

        private const string SCRIPT_SETTINGS = "script-settings";

        public SettingCollection ScriptSettings { get; private set; }

        public SettingEntry<bool> ScriptsEnabled        { get; private set; }
        public SettingEntry<bool> ScriptsConsoleEnabled { get; private set; }

        private void InitScriptSettings(SettingCollection settings) {
            this.ScriptSettings = settings.AddSubCollection(SCRIPT_SETTINGS);

            this.ScriptsEnabled        = this.ScriptSettings.DefineSetting(nameof(this.ScriptsEnabled),         true, () => "Enable Lua Scripts",    () => "If enabled, marker packs may load Lua scripts to provide custom functionality.");
            this.ScriptsConsoleEnabled = this.ScriptSettings.DefineSetting(nameof(this.ScriptsConsoleEnabled), false, () => "Enable Script Console", () => "If enabled, the Script Console can be accessed from the Pathing module menu to debug scripts.");
        }

        #endregion

        #region Keybind Settings

        private const string KEYBIND_SETTINGS = "keybind-settings";

        public SettingCollection KeyBindSettings { get; private set; }

        public SettingEntry<KeyBinding> KeyBindTogglePathables      { get; private set; }
        public SettingEntry<KeyBinding> KeyBindToggleWorldPathables { get; private set; }
        public SettingEntry<KeyBinding> KeyBindToggleMapPathables   { get; private set; }
        public SettingEntry<KeyBinding> KeyBindReloadMarkerPacks    { get; private set; }

        private void InitKeyBindSettings(SettingCollection settings) {
            this.KeyBindSettings = settings.AddSubCollection(KEYBIND_SETTINGS);

            // TODO: Add strings to strings.resx for localization.
            // TODO: Add description to settings.
            this.KeyBindTogglePathables      = this.KeyBindSettings.DefineSetting(nameof(this.KeyBindTogglePathables), new KeyBinding(ModifierKeys.Shift | ModifierKeys.Alt, Keys.OemPipe),              () => "Toggle Markers",          () => "");
            this.KeyBindToggleWorldPathables = this.KeyBindSettings.DefineSetting(nameof(this.KeyBindToggleWorldPathables), new KeyBinding(ModifierKeys.Shift | ModifierKeys.Alt, Keys.OemOpenBrackets), () => "Toggle Markers in World", () => "");
            this.KeyBindToggleMapPathables   = this.KeyBindSettings.DefineSetting(nameof(this.KeyBindToggleMapPathables), new KeyBinding(ModifierKeys.Shift | ModifierKeys.Alt, Keys.OemCloseBrackets),  () => "Toggle Markers on Map",   () => "");
            this.KeyBindReloadMarkerPacks    = this.KeyBindSettings.DefineSetting(nameof(this.KeyBindReloadMarkerPacks), new KeyBinding(ModifierKeys.Shift | ModifierKeys.Alt, Keys.R),                  () => "Reload Marker Packs",     () => "");

            HandleInternalKeyBinds();
        }

        private void HandleInternalKeyBinds() {
            this.KeyBindTogglePathables.Value.BlockSequenceFromGw2 = true;
            this.KeyBindTogglePathables.Value.Enabled = true;

            this.KeyBindToggleWorldPathables.Value.BlockSequenceFromGw2 = true;
            this.KeyBindToggleWorldPathables.Value.Enabled = true;

            this.KeyBindToggleMapPathables.Value.BlockSequenceFromGw2 = true;
            this.KeyBindToggleMapPathables.Value.Enabled = true;

            this.KeyBindReloadMarkerPacks.Value.BlockSequenceFromGw2 = true;
            this.KeyBindReloadMarkerPacks.Value.Enabled = true;

            this.KeyBindTogglePathables.Value.Activated      += ToggleGlobalPathablesEnabled;
            this.KeyBindToggleWorldPathables.Value.Activated += TogglePackWorldPathablesEnabled;
            this.KeyBindToggleMapPathables.Value.Activated   += ToggleMapPathablesEnabled;
            this.KeyBindReloadMarkerPacks.Value.Activated    += ReloadMarkerPacks;
        }

        private void ReloadMarkerPacks(object sender, EventArgs e) {
            if (_module.PackInitiator != null) { 
                _module.PackInitiator.ReloadPacks();
            }
        }

        private void ToggleGlobalPathablesEnabled(object sender, EventArgs e) {
            this.GlobalPathablesEnabled.Value = !this.GlobalPathablesEnabled.Value;
        }

        private void TogglePackWorldPathablesEnabled(object sender, EventArgs e) {
            this.PackWorldPathablesEnabled.Value = !this.PackWorldPathablesEnabled.Value;
        }

        private void ToggleMapPathablesEnabled(object sender, EventArgs e) {
            this.MapPathablesEnabled.Value = !this.MapPathablesEnabled.Value;
        }

        #endregion

        public void Unload() {
            this.KeyBindTogglePathables.Value.Enabled      = false;
            this.KeyBindToggleWorldPathables.Value.Enabled = false;
            this.KeyBindToggleMapPathables.Value.Enabled   = false;
            this.KeyBindReloadMarkerPacks.Value.Enabled    = false;

            this.KeyBindTogglePathables.Value.Activated      -= ToggleGlobalPathablesEnabled;
            this.KeyBindToggleWorldPathables.Value.Activated -= TogglePackWorldPathablesEnabled;
            this.KeyBindToggleMapPathables.Value.Activated   -= ToggleMapPathablesEnabled;
            this.KeyBindReloadMarkerPacks.Value.Activated    -= ReloadMarkerPacks;
        }

    }
}
