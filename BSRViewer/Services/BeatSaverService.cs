using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using IPA.Utilities.Async;
using Zenject;

namespace BSRViewer.Services
{
    /// <summary>
    /// Queries the BeatSaver REST API to resolve a map hash to its BSR key and metadata.
    /// Endpoint: GET https://api.beatsaver.com/maps/hash/{hash}
    /// </summary>
    public class BeatSaverService : IInitializable, IDisposable
    {
        private static readonly string BaseUrl = "https://api.beatsaver.com";
        private HttpClient _http = null!;

        public void Initialize()
        {
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Add(
                "User-Agent",
                "BSRViewer/1.0.0 (+https://github.com/Bullishdrake09/BSRViewer)"
            );
            _http.Timeout = TimeSpan.FromSeconds(10);
        }

        public void Dispose()
        {
            _http?.Dispose();
        }

        /// <summary>
        /// Fetches BSR map info by map hash.
        /// Returns null on any error (network, 404, etc.).
        /// </summary>
        public async Task<BeatSaverMapInfo?> GetMapByHashAsync(string hash, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return null;

            var normalizedHash = hash.ToLowerInvariant();
            var url = $"{BaseUrl}/maps/hash/{normalizedHash}";

            try
            {
                Plugin.Log.Debug($"[BeatSaverService] GET {url}");
                var response = await _http.GetAsync(url, ct).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    Plugin.Log.Warn($"[BeatSaverService] Non-success status {response.StatusCode} for hash {normalizedHash}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var obj = JObject.Parse(json);

                var id = obj["id"]?.ToString();
                var name = obj["metadata"]?["songName"]?.ToString()
                        ?? obj["name"]?.ToString()
                        ?? "Unknown";
                var songAuthor = obj["metadata"]?["songAuthorName"]?.ToString() ?? "";
                var levelAuthor = obj["metadata"]?["levelAuthorName"]?.ToString() ?? "";
                var uploader = obj["uploader"]?["name"]?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(id))
                {
                    Plugin.Log.Warn("[BeatSaverService] Map response had no id/key field.");
                    return null;
                }

                return new BeatSaverMapInfo
                {
                    Key = id,
                    SongName = name,
                    SongAuthor = songAuthor,
                    LevelAuthor = levelAuthor,
                    UploaderName = uploader,
                    BeatSaverUrl = $"https://beatsaver.com/maps/{id}"
                };
            }
            catch (OperationCanceledException)
            {
                Plugin.Log.Debug("[BeatSaverService] Request cancelled.");
                return null;
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"[BeatSaverService] Exception: {ex.Message}");
                return null;
            }
        }
    }

    public class BeatSaverMapInfo
    {
        /// <summary>The BSR key (short hex ID, e.g. "1a2b3").</summary>
        public string Key { get; set; } = "";
        public string SongName { get; set; } = "";
        public string SongAuthor { get; set; } = "";
        public string LevelAuthor { get; set; } = "";
        public string UploaderName { get; set; } = "";
        public string BeatSaverUrl { get; set; } = "";
    }
}
