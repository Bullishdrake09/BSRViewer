using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using Zenject;
using UnityEngine;

namespace BSRViewer.UI
{
    /// <summary>
    /// Creates a BSML FloatingScreen and hooks into Beat Saber's level selection
    /// events to drive the BSRViewerViewController.
    /// </summary>
    public class BSRViewerFlowCoordinator : MonoBehaviour, IInitializable, IDisposable
    {
        // ── Injected ──────────────────────────────────────────────────────────
        private BSRViewerViewController _viewController = null!;
        private LevelCollectionViewController _levelCollectionViewController = null!;

        [Inject]
        internal void Construct(
            BSRViewerViewController viewController,
            LevelCollectionViewController levelCollectionViewController)
        {
            _viewController = viewController;
            _levelCollectionViewController = levelCollectionViewController;
        }

        // ── Floating screen ───────────────────────────────────────────────────
        private FloatingScreen? _floatingScreen;

        // ── IInitializable ────────────────────────────────────────────────────
        public void Initialize()
        {
            try
            {
                _floatingScreen = FloatingScreen.CreateFloatingScreen(
                    new Vector2(100f, 60f),
                    true,
                    new Vector3(3.4f, 1.2f, 2.5f),
                    Quaternion.Euler(0f, -30f, 0f)
                );

                _floatingScreen.name = "BSRViewerFloatingScreen";
                _floatingScreen.SetRootViewController(_viewController, ViewController.AnimationType.In);

                _levelCollectionViewController.didSelectLevelEvent += OnLevelSelected;

                Plugin.Log.Info("[BSRViewerFlowCoordinator] Initialized.");
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"[BSRViewerFlowCoordinator] Initialization failed: {ex}");
            }
        }

        // ── IDisposable ───────────────────────────────────────────────────────
        public void Dispose()
        {
            if (_levelCollectionViewController != null)
                _levelCollectionViewController.didSelectLevelEvent -= OnLevelSelected;

            if (_floatingScreen != null)
                Destroy(_floatingScreen.gameObject);
        }

        // ── Event handlers ────────────────────────────────────────────────────
        private void OnLevelSelected(LevelCollectionViewController _, BeatmapLevel level)
        {
            _viewController.SetSelectedLevel(level);
        }
    }
}
