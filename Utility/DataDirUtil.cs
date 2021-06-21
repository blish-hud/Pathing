using System.IO;

namespace BhModule.Community.Pathing.Utility {
    public static class DataDirUtil {

        /// <summary>
        /// Where state information is kept.
        /// </summary>
        public const string COMMON_STATE = "states";

        /// <summary>
        /// Where user config information is kept.
        /// </summary>
        public const string COMMON_USER  = "user";

        private const string MARKER_DIR = "markers";
        private const string DATA_DIR   = "data";

        public static string MarkerDir => PathingModule.Instance.DirectoriesManager.GetFullDirectoryPath(MARKER_DIR);

        public static string GetSafeDataDir(string dir) {
            string combined = Path.Combine(MarkerDir, DATA_DIR, dir);

            if (!Directory.Exists(combined)) {
                Directory.CreateDirectory(combined);
            }

            return combined;
        }

    }
}
