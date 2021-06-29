using IPA;
using IPALogger = IPA.Logging.Logger;

namespace FizzyUtils {
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin {
        internal const string Name = "FizzyUtils";
        public static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        

        UsageTracker.UsageTrackerUser usageTrackerUser;

        [Init]
        public Plugin(IPALogger logger) {
            Instance = this;
            Log = logger;

            Utils.usageTracker = new UsageTracker("127.0.0.1");
            usageTrackerUser = Utils.usageTracker.AddUser(Name);
        }

        [OnExit]
        public void OnApplicationQuit() {
            Utils.usageTracker.Stop();
        }
    }

    public static class Utils {
        public static UsageTracker usageTracker;
    }
}
