using IPA;
using IPALogger = IPA.Logging.Logger;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace FizzyUtils {
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin {
        internal const string Name = "FizzyUtils";
        internal const string BaseURL = "fizzyutils.fizzyapple12.com";
        internal const bool secure = true;
        public static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        public static bool track = true;
        

        UsageTracker.UsageTrackerUser usageTrackerUser;

        [Init]
        public Plugin(IPALogger logger, Config config) {
            Instance = this;
            Log = logger;

            track = config.Generated<PluginConfig>().sendData;

            Utils.usageTracker = new UsageTracker(BaseURL, secure);
            usageTrackerUser = Utils.usageTracker.AddUser(Name);

            Utils.leaderBoarder = new LeaderBoarder(BaseURL, secure);
        }

        [OnExit]
        public void OnApplicationQuit() {
            Utils.usageTracker.Stop();
            Utils.leaderBoarder.Stop();
        }
    }

    public static class Utils {
        public static UsageTracker usageTracker;
        public static LeaderBoarder leaderBoarder;
    }

    public class PluginConfig {
        public static PluginConfig Instance { get; set; }

        public virtual bool sendData { get; set; } = true;
    }
}
