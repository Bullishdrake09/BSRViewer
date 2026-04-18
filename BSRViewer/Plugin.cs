using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using SiraUtil.Zenject;
using BSRViewer.Installers;
using IPALogger = IPA.Logging.Logger;

namespace BSRViewer
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; } = null!;

        [Init]
        public Plugin(IPALogger logger, Config config, Zenjector zenjector)
        {
            Log = logger;
            Log.Info("BSRViewer initializing...");

            zenjector.UseLogger(logger);
            zenjector.UseMetadataBinder<Plugin>();

            // Install into the game's main menu / standard gameplay scenes
            zenjector.Install<BSRViewerMenuInstaller>(Location.Menu);
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Log.Info("BSRViewer started.");
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Info("BSRViewer exited.");
        }
    }
}
