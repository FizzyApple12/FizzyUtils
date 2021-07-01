using IPA;
using IPALogger = IPA.Logging.Logger;

namespace FizzyUtils {
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin {
        internal const string Name = "FizzyUtils";
        internal const string BaseURL = "fizzyutils.fizzyapple12.com";
        internal const bool secure = true;
        public static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        

        UsageTracker.UsageTrackerUser usageTrackerUser;

        [Init]
        public Plugin(IPALogger logger) {
            Instance = this;
            Log = logger;

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
}
