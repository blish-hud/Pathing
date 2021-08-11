using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Gw2Sharp.WebApi.V2.Models;

namespace BhModule.Community.Pathing.Behavior {

    public readonly struct AchievementStatus {

        public bool Done { get; }

        public HashSet<int> AchievementBits { get; }

        public bool Unlocked { get; }

        public AchievementStatus(AccountAchievement accountAchievement) {
            this.Done            = accountAchievement.Done;
            this.AchievementBits = new HashSet<int>(accountAchievement.Bits ?? Enumerable.Empty<int>());
            this.Unlocked        = accountAchievement.Unlocked ?? true;
        }

    }

}