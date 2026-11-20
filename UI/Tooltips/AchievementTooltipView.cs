using System;
using System.Linq;
using System.Text.RegularExpressions;
using Blish_HUD;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace BhModule.Community.Pathing.UI.Tooltips {
    public class AchievementTooltipView : View, ITooltipView {

        private          Achievement _achievement;
        private readonly int         _achievementBit;
        public Achievement Achievement {
            get => _achievement;
            set {
                if (_achievement == value) return;

                _achievement = value;

                if (_achievement != null) {
                    ShowContent();
                }

                UpdateAchievementView();
            }
        }

        private AchievementCategory _achievementCategory;
        public AchievementCategory AchievementCategory {
            get => _achievementCategory;
            set {
                if (_achievementCategory == value) return;

                _achievementCategory = value;
                UpdateCategoryView();
            }
        }

        private const int EDGE_PADDING = 8;

        private Container _buildPanel;

        private Image _categoryIconImage;
        private Label _achievementNameLabel;
        private Label _achievementDescriptionLabel;
        private Label _achievementRequirementLabel;

        private LoadingSpinner _loadingSpinner;
        private Label          _loadingLabel;

        // The tooltip sizes itself to the bottom of its lowest visible child, so this keeps a consistent gap below the content.
        private Control _bottomPaddingSpacer;
        private Control _bottomControl;

        public AchievementTooltipView() { /* NOOP */ }

        public AchievementTooltipView(int achievementId, int achievementBit) {
            _achievementBit = achievementBit;

            this.WithPresenter(new AchievementPresenter(this, achievementId));
        }

        protected override void Build(Container buildPanel) {
            _buildPanel = buildPanel;

            buildPanel.HeightSizingMode = SizingMode.AutoSize;
            buildPanel.WidthSizingMode  = SizingMode.AutoSize;

            _bottomPaddingSpacer = new Panel() {
                Size   = new Point(1, EDGE_PADDING),
                Parent = buildPanel
            };

            if (_achievement != null) {
                // The details finished loading before the tooltip was built.
                ShowContent();
                UpdateCategoryView();
                UpdateAchievementView();
            } else {
                BuildLoading(buildPanel);
            }
        }

        private void BuildLoading(Container buildPanel) {
            _loadingSpinner = new LoadingSpinner() {
                Size     = new Point(32, 32),
                Location = new Point(EDGE_PADDING, EDGE_PADDING),
                Parent   = buildPanel
            };

            _loadingLabel = new Label() {
                Text              = "Loading achievement details...",
                AutoSizeWidth     = true,
                Height            = _loadingSpinner.Height,
                Location          = new Point(_loadingSpinner.Right + EDGE_PADDING, _loadingSpinner.Top),
                VerticalAlignment = VerticalAlignment.Middle,
                TextColor         = Color.FromNonPremultiplied(204, 204, 204, 255),
                ShowShadow        = true,
                Parent            = buildPanel
            };

            TrackBottomControl(_loadingSpinner);
        }

        /// <summary>
        /// Called by the presenter if the achievement details could not be loaded.
        /// </summary>
        public void ShowLoadFailed() {
            if (_loadingLabel == null) return;

            _loadingSpinner?.Dispose();
            _loadingSpinner = null;

            _loadingLabel.Left = EDGE_PADDING;
            _loadingLabel.Text = "Failed to load achievement details.";

            TrackBottomControl(_loadingLabel);
        }

        private void ShowContent() {
            if (_buildPanel == null || _categoryIconImage != null) return;

            _loadingSpinner?.Dispose();
            _loadingSpinner = null;
            _loadingLabel?.Dispose();
            _loadingLabel = null;

            BuildContent(_buildPanel);

            // The category may have been assigned before the content existed.
            UpdateCategoryView();
        }

        private void TrackBottomControl(Control control) {
            if (_bottomControl != null) {
                _bottomControl.Resized -= BottomControlOnChanged;
                _bottomControl.Moved   -= BottomControlOnChanged;
            }

            _bottomControl = control;

            _bottomControl.Resized += BottomControlOnChanged;
            _bottomControl.Moved   += BottomControlOnChanged;

            UpdateBottomPadding();
        }

        private void BottomControlOnChanged(object sender, EventArgs e) => UpdateBottomPadding();

        private void UpdateBottomPadding() {
            if (_bottomPaddingSpacer == null || _bottomControl == null) return;

            _bottomPaddingSpacer.Top = _bottomControl.Bottom;
        }

        private void BuildContent(Container buildPanel) {
            _categoryIconImage = new Image() {
                Size            = new Point(48, 48),
                Location        = new Point(8, 8),
                Parent          = buildPanel
            };

            _achievementNameLabel = new Label() {
                AutoSizeHeight      = false,
                AutoSizeWidth       = true,
                Location            = new Point(_categoryIconImage.Right + 8, _categoryIconImage.Top),
                Height              = _categoryIconImage.Height / 2,
                Padding             = new Thickness(0, 8, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment   = VerticalAlignment.Middle,
                Font                = GameService.Content.DefaultFont16,
                ShowShadow          = true,
                Parent              = buildPanel
            };

            _achievementDescriptionLabel = new Label() {
                AutoSizeHeight      = true,
                AutoSizeWidth       = false,
                Location            = new Point(_achievementNameLabel.Left, _categoryIconImage.Top + _categoryIconImage.Height / 2),
                Width               = Math.Max(_achievementNameLabel.Width, 200),
                Padding             = new Thickness(0, 8, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment   = VerticalAlignment.Middle,
                TextColor           = Color.FromNonPremultiplied(204, 204, 204, 255),
                ShowShadow          = true,
                WrapText            = true,
                Parent              = buildPanel
            };

            _achievementRequirementLabel = new Label() {
                AutoSizeHeight = true,
                AutoSizeWidth  = false,
                Location       = new Point(_categoryIconImage.Left, _achievementDescriptionLabel.Bottom + 8),
                ShowShadow     = true,
                WrapText       = true,
                Parent         = buildPanel
            };

            TrackBottomControl(_achievementRequirementLabel);
        }

        private void UpdateCategoryView() {
            if (_achievementCategory == null) return;
            if (_categoryIconImage == null) return;

            _categoryIconImage.Texture = GameService.Content.GetRenderServiceTexture(_achievementCategory.Icon);
        }

        private void UpdateAchievementView() {
            if (_achievement == null) return;
            if (_achievementNameLabel == null) return;

            try {
                _achievementNameLabel.Text = _achievement.Name;
                _achievementDescriptionLabel.Text = CleanMessage(_achievement.Description);
                _achievementRequirementLabel.Text = CleanMessage(_achievement.Requirement);

                if (_achievementBit != -1 && _achievement.Bits != null) {
                    var bit = _achievement.Bits[_achievementBit];
                    if (bit.Type == AchievementBitType.Text) {
                        _achievementRequirementLabel.Text = CleanMessage(((AchievementTextBit)bit).Text);
                    }
                }

                _achievementNameLabel.Height = string.IsNullOrEmpty(_achievement.Description) ? _categoryIconImage.Height : _categoryIconImage.Height / 2;
                _achievementDescriptionLabel.Width = Math.Max(_achievementNameLabel.Width, 200);
                _achievementRequirementLabel.Width = new[] { _achievementNameLabel.Right + 8, _achievementDescriptionLabel.Right + 8, 300 }.Max();

                _achievementRequirementLabel.Top = Math.Max(_achievementDescriptionLabel.Bottom + 8, _categoryIconImage.Bottom + 8);
            } catch (Exception) { /* NOOP */ }

            // Pad below whichever control is actually lowest (the requirement label is empty for some achievements).
            bool hasRequirement = !string.IsNullOrWhiteSpace(_achievementRequirementLabel.Text);
            _achievementRequirementLabel.Visible = hasRequirement;

            if (hasRequirement) {
                TrackBottomControl(_achievementRequirementLabel);
            } else {
                TrackBottomControl(_achievementDescriptionLabel.Bottom > _categoryIconImage.Bottom
                                       ? _achievementDescriptionLabel
                                       : _categoryIconImage);
            }
        }

        static string CleanMessage(string message) {
            // Perform manipulations on the cleaned message
            string cleanedMessage = message;

            // This Regex catches both the color and raw text of colored segments
            string pattern = @"<c[=@][@=]?([^>]+)>(.*?)(<\/?c\/?>|$)";
            MatchCollection colorCatches = Regex.Matches(cleanedMessage, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match match in colorCatches) {
                GroupCollection groups = match.Groups;
                string rawText = groups[2].Value;

                // Just strip out the codes entirely
                cleanedMessage = cleanedMessage.Replace(match.Value, rawText);
            }

            // Next, replace any <br> tags with \n
            // Also crushes multiple linebreaks into a single linebreak
            cleanedMessage = Regex.Replace(cleanedMessage, @"(<br ?\/?>)+", "\n", RegexOptions.IgnoreCase);
            return cleanedMessage;
        }

    }
}
