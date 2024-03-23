using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Graphics.UI;
using Gw2Sharp.WebApi.V2.Models;

namespace BhModule.Community.Pathing.UI.Tooltips {
    public class AchievementPresenter : Presenter<AchievementTooltipView, int> {

        private static readonly Logger Logger = Logger.GetLogger<AchievementPresenter>();

        private const int LOAD_ATTEMPTS = 1;

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
                    Logger.Warn(ex, "Failed to load details for achievement with ID {achievementId}.", this.Model);
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> AttemptLoadAchievementCategory(IProgress<string> progress, int attempt = LOAD_ATTEMPTS) {
            progress.Report("Loading achievement group details...");

            _achievementCategories = PathingModule.Instance.PackInitiator.PackState.AchievementStates.GetAchievementCategory(this.Model);

            return _achievement != null;
        }

        protected override async Task<bool> Load(IProgress<string> progress) {
            var backgroundThread = new Thread(async () => {
                if (!await AttemptLoadAchievement(progress) && _achievement != null) {
                    progress.Report("Failed to load achievement details.");
                    return;
                }

                await AttemptLoadAchievementCategory(progress);
                UpdateView();
            }) { IsBackground = true };
            backgroundThread.Start();

            return true;
        }

        protected override void UpdateView() {
            this.View.Achievement         = _achievement;
            this.View.AchievementCategory = _achievementCategories;
        }

    }
}
