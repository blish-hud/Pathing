using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace BhModule.Community.Pathing.UI.Views {
    public class IndividualPackSettingsView : View {

        private const int LOADOPTION_DROPDOWNWIDTH = 128;

        private Label _packNameLabel;

        private Dropdown _packLoadOptionDropdown;

        protected override void Build(Container buildPanel) {
            _packNameLabel = new Label() {
                Text                = "",
                Width               = LOADOPTION_DROPDOWNWIDTH,
                Height              = buildPanel.Height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment   = VerticalAlignment.Middle,
                Parent              = buildPanel
            };

            _packLoadOptionDropdown = new Dropdown() {
                Width  = LOADOPTION_DROPDOWNWIDTH,
                Left   = _packNameLabel.Right + 16,
                Parent = buildPanel,
            };
        }

    }
}
