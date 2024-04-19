using System.Collections.Generic;
using System.IO;

namespace ObsidianLauncher.ViewModels
{
    public static class ObsdiianPathHelper
    {
        public static string[] GetObsidianPath()
        {
            string[] defaultPaths =
            {
                @"C:\Program Files (x86)\Steam\steamapps\common\Resonite\",
                @"C:\Resonite\app\",
                @"/.steam/steam/steamapps/common/Resonite/",
                @"/mnt/LocalDisk/SteamLibrary/steamapps/common/Resonie/"
            };

            List<string> existingPaths = new List<string>();
            foreach (string path in defaultPaths)
            {
                if (Directory.Exists(path))
                {
                    existingPaths.Add(path);
                }
            }

            // Check if CustomInstallDir is set in the configuration
            Config config = Config.LoadConfig();
            if (!string.IsNullOrEmpty(config.CustomInstallDir) && Directory.Exists(config.CustomInstallDir))
            {
                existingPaths.Insert(0, config.CustomInstallDir);
            }

            return existingPaths.ToArray();
        }
    }
}
