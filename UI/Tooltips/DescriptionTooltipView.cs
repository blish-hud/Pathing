using Blish_HUD;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.UI.Tooltips {
    public class DescriptionTooltipView : View, ITooltipView {

        public string Title {
            get => _titleLabel.Text;
            set {
                _titleLabel.Text = value;
                UpdateLayout();
            }
        }

        public string Description {
            get => _descriptionLabel.Text;
            set {
                _descriptionLabel.Text = value;
                UpdateLayout();
            }
        }

        private readonly Label _titleLabel       = new();
        private readonly Label _descriptionLabel = new();

        public DescriptionTooltipView() { /* NOOP */ }

        public DescriptionTooltipView(string title, string description) {
            this.Title       = title;
            this.Description = description;
        }

        private void UpdateLayout() {
            _descriptionLabel.Location = new Point(0, this.Title == null || this.Description == null
                                                          ? 0
                                                          : _titleLabel.Bottom + 8);

            // Poor mans "max width" implementation.  Needs a max width property in core.
            _titleLabel.AutoSizeWidth       = true;
            _descriptionLabel.AutoSizeWidth = true;

            if (_titleLabel.Width > 300 || _descriptionLabel.Width > 300) {
                _titleLabel.AutoSizeWidth       = false;
                _descriptionLabel.AutoSizeWidth = false;

                _titleLabel.Width       = 300;
                _descriptionLabel.Width = 300;
            }
        }

        protected override void Build(Container buildPanel) {
            buildPanel.HeightSizingMode = SizingMode.AutoSize;
            buildPanel.WidthSizingMode  = SizingMode.AutoSize;

            _titleLabel.Location       = Point.Zero;
            _titleLabel.AutoSizeHeight = true;
            _titleLabel.Width          = 300;
            _titleLabel.ShowShadow     = true;
            _titleLabel.TextColor      = Color.FromNonPremultiplied(255, 204, 119, 255);
            _titleLabel.Font           = GameService.Content.DefaultFont16;
            _titleLabel.Parent         = buildPanel;

            _descriptionLabel.AutoSizeHeight = true;
            _descriptionLabel.Width          = 300;
            _descriptionLabel.WrapText       = true;
            _descriptionLabel.ShowShadow     = true;
            _descriptionLabel.Parent         = buildPanel;

            UpdateLayout();
        }

    }
}
