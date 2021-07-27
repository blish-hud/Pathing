﻿using System.Linq;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace BhModule.Community.Pathing.UI.Common {
    public class AchievementTooltipView : View {

        private Achievement _achievement;
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

        public AchievementTooltipView(int acheivementId) {
            this.WithPresenter(new AchievementPresenter(this, acheivementId));
        }

        protected override void Build(Panel buildPanel) {
            //buildPanel.Size = new Point(300, 256);
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
                Font = GameService.Content.DefaultFont16,
                Parent              = buildPanel
            };

            _achievementDescriptionLabel = new Label() {
                AutoSizeHeight      = false,
                AutoSizeWidth       = true,
                Location            = new Point(_achievementNameLabel.Left, _categoryIconImage.Top + _categoryIconImage.Height / 2),
                Height              = _categoryIconImage.Height / 2,
                Padding             = new Thickness(0, 8, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment   = VerticalAlignment.Middle,
                TextColor           = Control.StandardColors.DisabledText,
                Parent              = buildPanel
            };

            _achievementRequirementLabel = new Label() {
                AutoSizeHeight = true,
                AutoSizeWidth  = false,
                Location       = new Point(_categoryIconImage.Left, _categoryIconImage.Bottom + 16),
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
            _achievementDescriptionLabel.Text = _achievement.Description;
            _achievementRequirementLabel.Text = _achievement.Requirement;

            _achievementNameLabel.Height = string.IsNullOrEmpty(_achievement.Description) ? _categoryIconImage.Height : _categoryIconImage.Height / 2;

            _achievementRequirementLabel.Width = new[] { _achievementNameLabel.Right + 8, _achievementDescriptionLabel.Right + 8, 300 }.Max();
        }

    }
}