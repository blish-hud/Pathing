using System;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Graphics.UI;
using Gw2Sharp.WebApi.V2.Models;

namespace BhModule.Community.Pathing.UI.Common {
    class AchievementPresenter : Presenter<AchievementTooltipView, int> {

        private static readonly Logger Logger = Logger.GetLogger<AchievementPresenter>();

        private const int LOAD_ATTEMPTS = 3;

        private AchievementCategory _achievementCategories;
        private Achievement         _achievement;
        
        public AchievementPresenter(AchievementTooltipView view, int achievementId) : base(view, achievementId) { }

        private async Task<bool> AttemptLoadAchievement(IProgress<string> progress, int attempt = LOAD_ATTEMPTS) {
            progress.Report("Loading achievement details...");

            try {
                _achievement = await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Achievements.GetAsync(this.Model);
            } catch (Exception ex) {
                if (attempt > 0) {
                    return await AttemptLoadAchievement(progress, --attempt);
                } else {
                    Logger.Error(ex, "Failed to load achievement details.");
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> AttemptLoadAchievementCategory(IProgress<string> progress, int attempt = LOAD_ATTEMPTS) {
            progress.Report("Loading achievement group details...");

            try {
                var categories = await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Achievements.Categories.AllAsync();
                _achievementCategories = categories.FirstOrDefault(category => category.Achievements.Contains(this.Model));
            } catch (Exception ex) {
                if (attempt > 0) {
                    return await AttemptLoadAchievementCategory(progress, --attempt);
                } else {
                    Logger.Error(ex, "Failed to load achievement details.");
                    return false;
                }
            }

            return true;
        }

        protected override async Task<bool> Load(IProgress<string> progress) {
            if (!await AttemptLoadAchievement(progress) && _achievement != null) {
                progress.Report("Failed to load achievement details.");
                return false;
            }

            await AttemptLoadAchievementCategory(progress);

            return true;
        }

        protected override void UpdateView() {
            this.View.Achievement         = _achievement;
            this.View.AchievementCategory = _achievementCategories;
        }

    }
}
