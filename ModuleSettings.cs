using Blish_HUD.Input;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Input;

namespace BhModule.Community.Pathing {

    public enum MarkerClipboardConsentLevel {
        Always,
        OnlyWhenInteractedWith,
        Never
    }
    
    public enum MapVisibilityLevel {
        Default,
        Always,
        Never
    }

    public class ModuleSettings {

        public ModuleSettings(SettingCollection settings) {
            InitGlobalSettings(settings);
            InitPackSettings(settings);
            InitMapSettings(settings);
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
        public SettingEntry<bool>                        PackAllowInfoText                        { get; private set; }
        public SettingEntry<bool>                        PackAllowMarkersToAnimate                { get; private set; }
        public SettingEntry<bool>                        PackEnableSmartCategoryFilter            { get; private set; }
        public SettingEntry<bool>                        PackShowWhenCategoriesAreFiltered        { get; private set; }

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
            this.PackAllowInfoText                        = this.PackSettings.DefineSetting(nameof(this.PackAllowInfoText),                        true, () => "Allow Markers to Show Info Text On-Screen", () => "If enabled, certain markers will be able to show information when your character is nearby to the marker.");
            this.PackAllowMarkersToAnimate                = this.PackSettings.DefineSetting(nameof(this.PackAllowMarkersToAnimate),                true, () => Strings.Setting_PackAllowMarkersToAnimate, () => "Allows animations such as 'bounce' and trail movements.");
            this.PackEnableSmartCategoryFilter            = this.PackSettings.DefineSetting(nameof(this.PackEnableSmartCategoryFilter),            true, () => "Enable Smart Categories", () => "If a category doesn't contain markers or trails relevant to the current map, the category is hidden.");
            this.PackShowWhenCategoriesAreFiltered        = this.PackSettings.DefineSetting(nameof(this.PackShowWhenCategoriesAreFiltered),        true, () => "Indicate when categories are hidden", () => "Shows a note at the bottom of the menu indicating if categories have been hidden.  Clicking the note will show the hidden categories temporarily.");

            this.PackMaxOpacityOverride.SetRange(0f, 1f);
            this.PackMaxViewDistance.SetRange(25f, 50000f);
            this.PackMaxTrailAnimationSpeed.SetRange(0f, 10f);
            this.PackMarkerScale.SetRange(0.1f, 4f);
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

        private void InitMapSettings(SettingCollection settings) {
            this.MapSettings = settings.AddSubCollection(MAP_SETTINGS);

            // TODO: Add string to strings.resx for localization.
            // TODO: Add description to settings.
            this.MapPathablesEnabled                   = this.MapSettings.DefineSetting(nameof(this.MapPathablesEnabled),                   true,                       () => "Show Markers on Maps");
            this.MapMarkerVisibilityLevel              = this.MapSettings.DefineSetting(nameof(this.MapMarkerVisibilityLevel),              MapVisibilityLevel.Default, () => Strings.Setting_MapShowMarkersOnFullscreen,  () => "");
            this.MapTrailVisibilityLevel               = this.MapSettings.DefineSetting(nameof(this.MapTrailVisibilityLevel),               MapVisibilityLevel.Default, () => Strings.Setting_MapShowTrailsOnFullscreen,   () => "");
            this.MapDrawOpacity                        = this.MapSettings.DefineSetting(nameof(this.MapDrawOpacity),                        1f,                         () => "Opacity on Fullscreen Map",                 () => "");
            this.MiniMapMarkerVisibilityLevel          = this.MapSettings.DefineSetting(nameof(this.MiniMapMarkerVisibilityLevel),          MapVisibilityLevel.Default, () => Strings.Setting_MapShowMarkersOnCompass,     () => "");
            this.MiniMapTrailVisibilityLevel           = this.MapSettings.DefineSetting(nameof(this.MiniMapTrailVisibilityLevel),           MapVisibilityLevel.Default, () => Strings.Setting_MapShowTrailsOnCompass,      () => "");
            this.MiniMapDrawOpacity                    = this.MapSettings.DefineSetting(nameof(this.MiniMapDrawOpacity),                    1f,                         () => "Opacity on the Minimap",                    () => "");
            this.MapShowAboveBelowIndicators           = this.MapSettings.DefineSetting(nameof(this.MapShowAboveBelowIndicators),           true,                       () => Strings.Setting_MapShowAboveBelowIndicators, () => "");
            this.MapFadeVerticallyDistantTrailSegments = this.MapSettings.DefineSetting(nameof(this.MapFadeVerticallyDistantTrailSegments), true,                       () => "Fade Trail Segments Which Are High Above or Below", () => "");
            this.MapTrailWidth                         = this.MapSettings.DefineSetting(nameof(this.MapTrailWidth),                         2f,                         () => "Trail Width on Maps", () => "The thickness of trails shown on the map.");

            this.MapDrawOpacity.SetRange(0f, 1f);
            this.MiniMapDrawOpacity.SetRange(0f, 1f);
            this.MapTrailWidth.SetRange(0.5f, 4.5f);
        }

        #endregion

        #region Keybind Settings

        private const string KEYBIND_SETTINGS = "keybind-settings";

        public SettingCollection KeyBindSettings { get; private set; }

        public SettingEntry<KeyBinding> KeyBindTogglePathables { get; private set; }
        public SettingEntry<KeyBinding> KeyBindToggleWorldPathables { get; private set; }
        public SettingEntry<KeyBinding> KeyBindToggleMapPathables   { get; private set; }

        private void InitKeyBindSettings(SettingCollection settings) {
            this.KeyBindSettings = settings.AddSubCollection(KEYBIND_SETTINGS);

            // TODO: Add strings to strings.resx for localization.
            // TODO: Add description to settings.
            this.KeyBindTogglePathables      = this.KeyBindSettings.DefineSetting(nameof(this.KeyBindTogglePathables), new KeyBinding(ModifierKeys.Shift | ModifierKeys.Alt, Keys.OemPipe),              () => "Toggle Markers",          () => "");
            this.KeyBindToggleWorldPathables = this.KeyBindSettings.DefineSetting(nameof(this.KeyBindToggleWorldPathables), new KeyBinding(ModifierKeys.Shift | ModifierKeys.Alt, Keys.OemOpenBrackets), () => "Toggle Markers in World", () => "");
            this.KeyBindToggleMapPathables   = this.KeyBindSettings.DefineSetting(nameof(this.KeyBindToggleMapPathables), new KeyBinding(ModifierKeys.Shift | ModifierKeys.Alt, Keys.OemCloseBrackets),  () => "Toggle Markers on Map",   () => "");

            HandleInternalKeyBinds();
        }

        private void HandleInternalKeyBinds() {
            this.KeyBindTogglePathables.Value.Enabled      = true;
            this.KeyBindToggleWorldPathables.Value.Enabled = true;
            this.KeyBindToggleMapPathables.Value.Enabled   = true;

            this.KeyBindTogglePathables.Value.Activated += async delegate {
                this.GlobalPathablesEnabled.Value = !this.GlobalPathablesEnabled.Value; 

                //await MapNavUtil.NavigateToPosition(20806.8, 16337.3, 1);
            };
            this.KeyBindToggleWorldPathables.Value.Activated += delegate { this.PackWorldPathablesEnabled.Value = !this.PackWorldPathablesEnabled.Value; };
            this.KeyBindToggleMapPathables.Value.Activated   += delegate { this.MapPathablesEnabled.Value       = !this.MapPathablesEnabled.Value; };
        }

        #endregion

    }
}
