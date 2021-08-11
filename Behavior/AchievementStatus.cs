using System.Collections.Generic;
using System.Diagnostics;

namespace BhModule.Community.Pathing.Behavior {

    public readonly struct AchievementStatus {

        public bool Done { get; }

        public HashSet<int> AchievementBits { get; }

        public bool Unlocked { get; }

        public AchievementStatus(bool done, IEnumerable<int> achievementBits, bool unlocked) {
            this.Done            = done;
            this.AchievementBits = new HashSet<int>(achievementBits);
            this.Unlocked        = unlocked;
        }

    }

}