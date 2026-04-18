using Zenject;
using BSRViewer.UI;
using BSRViewer.Services;

namespace BSRViewer.Installers
{
    public class BSRViewerMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<BeatSaverService>().AsSingle();
            Container.BindInterfacesAndSelfTo<BSRViewerViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<BSRViewerFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
        }
    }
}
