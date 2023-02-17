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

        private Image _categoryIconImage;
        private Label _achievementNameLabel;
        private Label _achievementDescriptionLabel;
        private Label _achievementRequirementLabel;

        public AchievementTooltipView() { /* NOOP */ }

        public AchievementTooltipView(int achievementId, int achievementBit) {
            _achievementBit = achievementBit;

            this.WithPresenter(new AchievementPresenter(this, achievementId));
        }

        protected override void Build(Container buildPanel) {
            buildPanel.HeightSizingMode = SizingMode.AutoSize;
            buildPanel.WidthSizingMode  = SizingMode.AutoSize;

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
        }

        private void UpdateCategoryView() {
            if (_achievementCategory == null) return;

            _categoryIconImage.Texture = GameService.Content.GetRenderServiceTexture(_achievementCategory.Icon);
        }

        private void UpdateAchievementView() {
            if (_achievement == null) return;

            _achievementNameLabel.Text        = _achievement.Name;
            _achievementDescriptionLabel.Text = CleanMessage(_achievement.Description);
            _achievementRequirementLabel.Text = CleanMessage(_achievement.Requirement);
           
            if (_achievementBit != -1 && _achievement.Bits != null) {
                var bit = _achievement.Bits[_achievementBit];
                if (bit.Type == AchievementBitType.Text) {
                    _achievementRequirementLabel.Text = CleanMessage(((AchievementTextBit) bit).Text);
                }
            }

            _achievementNameLabel.Height       = string.IsNullOrEmpty(_achievement.Description) ? _categoryIconImage.Height : _categoryIconImage.Height / 2;
            _achievementDescriptionLabel.Width = Math.Max(_achievementNameLabel.Width, 200);
            _achievementRequirementLabel.Width = new[] { _achievementNameLabel.Right + 8, _achievementDescriptionLabel.Right + 8, 300 }.Max();

            _achievementRequirementLabel.Top = Math.Max(_achievementDescriptionLabel.Bottom + 8, _categoryIconImage.Bottom + 8);
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
