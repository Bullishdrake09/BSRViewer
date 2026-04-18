using System;
using System.Threading;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BSRViewer.Services;
using HMUI;
using IPA.Utilities.Async;
using TMPro;
using UnityEngine;
using Zenject;

namespace BSRViewer.UI
{
    /// <summary>
    /// The main ViewController that shows BSR info for the currently selected level.
    /// It is placed in the right panel of the main menu via BSRViewerFlowCoordinator.
    /// </summary>
    [ViewDefinition("BSRViewer.UI.BSRViewerView.bsml")]
    [HotReload(RelativePathToLayout = @"BSRViewerView.bsml")]
    public class BSRViewerViewController : BSMLAutomaticViewController
    {
        // ── Injected ──────────────────────────────────────────────────────────
        private BeatSaverService _beatSaverService = null!;

        [Inject]
        internal void Construct(BeatSaverService beatSaverService)
        {
            _beatSaverService = beatSaverService;
        }

        // ── BSML UI component bindings ────────────────────────────────────────

        [UIComponent("status-text")]
        private TextMeshProUGUI _statusText = null!;

        [UIComponent("info-panel")]
        private Transform _infoPanel = null!;

        [UIComponent("error-text")]
        private TextMeshProUGUI _errorText = null!;

        [UIComponent("song-name-text")]
        private TextMeshProUGUI _songNameText = null!;

        [UIComponent("song-author-text")]
        private TextMeshProUGUI _songAuthorText = null!;

        [UIComponent("level-author-text")]
        private TextMeshProUGUI _levelAuthorText = null!;

        [UIComponent("bsr-key-text")]
        private TextMeshProUGUI _bsrKeyText = null!;

        // ── Private state ─────────────────────────────────────────────────────
        private BeatSaverMapInfo? _currentMapInfo;
        private CancellationTokenSource? _cts;

        // ── Public API called by FlowCoordinator ──────────────────────────────

        /// <summary>
        /// Called when the user selects a level. Kicks off API lookup.
        /// </summary>
        public void SetSelectedLevel(BeatmapLevel level)
        {
            // Cancel any in-flight request
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            _currentMapInfo = null;

            ShowStatus("Loading BeatSaver info…");
            HideInfo();
            HideError();

            // Extract level hash — CustomLevels always embed the hash in the levelID
            // Format: "custom_level_XXXX..." where XXXX... is the SHA1 hash (upper-case hex)
            var levelId = level.levelID;
            if (!levelId.StartsWith("custom_level_", StringComparison.OrdinalIgnoreCase))
            {
                ShowStatus("This is an OST level — no BSR key available.");
                return;
            }

            var hash = levelId.Substring("custom_level_".Length);
            if (hash.Length != 40)
            {
                ShowStatus("Could not determine map hash.");
                return;
            }

            _ = FetchAndDisplayAsync(hash, _cts.Token);
        }

        /// <summary>
        /// Reset the panel to idle state (no map selected).
        /// </summary>
        public void ClearDisplay()
        {
            _cts?.Cancel();
            _currentMapInfo = null;
            ShowStatus("Select a map to see its BSR key.");
            HideInfo();
            HideError();
        }

        // ── Internal async fetch ──────────────────────────────────────────────

        private async Task FetchAndDisplayAsync(string hash, CancellationToken ct)
        {
            try
            {
                var info = await _beatSaverService.GetMapByHashAsync(hash, ct).ConfigureAwait(false);

                // Marshal display updates back to the Unity main thread
                await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory
                    .StartNew(() => ApplyMapInfo(info, hash))
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Normal — user navigated away; do nothing
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"[BSRViewerViewController] Unexpected error: {ex}");
                await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory
                    .StartNew(() =>
                    {
                        ShowStatus("");
                        ShowError("An unexpected error occurred.");
                    })
                    .ConfigureAwait(false);
            }
        }

        private void ApplyMapInfo(BeatSaverMapInfo? info, string hash)
        {
            if (info == null)
            {
                HideInfo();
                ShowStatus("");
                ShowError($"Map not found on BeatSaver.\n(Hash: {hash.Substring(0, 8)}…)");
                return;
            }

            _currentMapInfo = info;

            _songNameText.text = info.SongName;
            _songAuthorText.text = info.SongAuthor;
            _levelAuthorText.text = info.LevelAuthor;
            _bsrKeyText.text = $"!bsr {info.Key}";

            HideStatus();
            HideError();
            ShowInfo();
        }

        // ── Button handlers ───────────────────────────────────────────────────

        [UIAction("CopyBsrKey")]
        private void CopyBsrKey()
        {
            if (_currentMapInfo == null) return;
            UnityEngine.GUIUtility.systemCopyBuffer = $"!bsr {_currentMapInfo.Key}";
            Plugin.Log.Info($"[BSRViewer] Copied to clipboard: !bsr {_currentMapInfo.Key}");

            // Brief visual feedback
            var originalText = _bsrKeyText.text;
            _bsrKeyText.text = "Copied!";
            _ = ResetBsrTextAsync(originalText);
        }

        [UIAction("OpenBeatSaverUrl")]
        private void OpenBeatSaverUrl()
        {
            if (_currentMapInfo == null) return;
            Application.OpenURL(_currentMapInfo.BeatSaverUrl);
            Plugin.Log.Info($"[BSRViewer] Opening URL: {_currentMapInfo.BeatSaverUrl}");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private async Task ResetBsrTextAsync(string originalText)
        {
            await Task.Delay(1500).ConfigureAwait(false);
            await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory
                .StartNew(() =>
                {
                    if (_currentMapInfo != null)
                        _bsrKeyText.text = originalText;
                })
                .ConfigureAwait(false);
        }

        private void ShowStatus(string msg)
        {
            if (_statusText == null) return;
            _statusText.text = msg;
            _statusText.gameObject.SetActive(!string.IsNullOrEmpty(msg));
        }

        private void HideStatus() => _statusText?.gameObject.SetActive(false);

        private void ShowInfo() => _infoPanel?.gameObject.SetActive(true);
        private void HideInfo() => _infoPanel?.gameObject.SetActive(false);

        private void ShowError(string msg)
        {
            if (_errorText == null) return;
            _errorText.text = msg;
            _errorText.gameObject.SetActive(true);
        }

        private void HideError() => _errorText?.gameObject.SetActive(false);

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            _cts?.Cancel();
        }
    }
}
