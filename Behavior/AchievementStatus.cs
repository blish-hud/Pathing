using System.Collections.Generic;

namespace BhModule.Community.Pathing.Behavior {

    public readonly struct AchievementStatus {

        public bool Done { get; }

        public HashSet<int> AchievementBits { get; }

        public AchievementStatus(bool done, IEnumerable<int> achievementBits) {
            this.Done            = done;
            this.AchievementBits = new HashSet<int>(achievementBits);
        }

    }

}