# BSR Viewer — Beat Saber 1.40.8 Mod

A BSIPA plugin for **Beat Saber 1.40.8** that shows the **BeatSaver BSR key** and map metadata for any custom level you select in the song browser — right inside the game.

---

## ✨ Features

| Feature | Details |
|---------|---------|
| **Live BSR lookup** | Fetches `!bsr <key>` from `api.beatsaver.com` the moment you click a song |
| **Copy to clipboard** | One button copies `!bsr <key>` — paste it straight into a stream chat |
| **Map metadata** | Shows song name, artist, mapper name, and uploader |
| **Open BeatSaver** | Button opens the map's BeatSaver page in your default browser |
| **Draggable panel** | Floating screen can be repositioned with the grip handle |
| **OST awareness** | Gracefully ignores non-custom (OST) levels |

---

## 📋 Requirements

| Dependency | Version | Where to get |
|------------|---------|--------------|
| Beat Saber | **1.40.8** | Steam / Oculus |
| BSIPA | ^4.3.0 | BSManager → Mods tab, or [GitHub](https://github.com/bsmg/BeatSaber-IPA-Reloaded/releases) |
| BeatSaberMarkupLanguage | ^1.12.0 | BSManager → Mods tab |
| SiraUtil | ^3.1.0 | BSManager → Mods tab |

> **Note:** BSML and SiraUtil are available directly through **BSManager** for 1.40.8.
> They are listed under `legacy1.40.8_unity_v2021.3.16f1` in the branch selector.

---

## 🔨 Building

### Prerequisites
- Visual Studio 2022 **or** Rider (with .NET SDK 6+)
- .NET Framework 4.7.2 targeting pack installed
- Beat Saber 1.40.8 installed and modded (BSIPA patched)

### Steps

1. **Clone the repo**
   ```
   git clone https://github.com/yourname/BSRViewer.git
   cd BSRViewer
   ```

2. **Create your local config file** (`BSRViewer/BSRViewer.csproj.user`):
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
     <PropertyGroup>
       <BeatSaberDir>C:\Program Files (x86)\Steam\steamapps\common\Beat Saber</BeatSaberDir>
     </PropertyGroup>
   </Project>
   ```
   Adjust the path to your actual Beat Saber install directory.

3. **Open `BSRViewer.sln`** in Visual Studio or Rider.

4. **Build → Build Solution** (`Ctrl+Shift+B`).
   - The post-build event automatically copies `BSRViewer.dll` into your `Beat Saber/Plugins/` folder.

5. **Launch Beat Saber.** The floating BSR panel will appear on the right side of the main menu.

---

## 📦 Manual Install (pre-built release)

1. Install the dependencies listed above via BSManager.
2. Copy `BSRViewer.dll` into `<BeatSaberDir>\Plugins\`.
3. Launch the game.

---

## 🎮 Usage

1. Open the **Solo** mode song browser.
2. Click on any **custom level**.
3. The **BSR Viewer** panel appears on the right — it shows:
   - Song name, artist, and mapper
   - The `!bsr <key>` command in large text
4. Press **Copy** to put `!bsr <key>` on your clipboard.
5. Press **Open BeatSaver Page** to open the map in your browser.

---

## 🗂 Project Structure

```
BSRViewer/
├── BSRViewer.sln
└── BSRViewer/
    ├── Plugin.cs                       ← BSIPA entry point
    ├── manifest.json                   ← BSIPA manifest (gameVersion: 1.40.8)
    ├── BSRViewer.csproj
    ├── Installers/
    │   └── BSRViewerMenuInstaller.cs   ← Zenject bindings for Menu scene
    ├── Services/
    │   └── BeatSaverService.cs         ← HTTP client → api.beatsaver.com
    └── UI/
        ├── BSRViewerView.bsml          ← BSML layout (embedded resource)
        ├── BSRViewerViewController.cs  ← BSML ViewController
        └── BSRViewerFlowCoordinator.cs ← Hooks into song selection events
```

---

## 🔍 How it works

1. **Level selection event** — `LevelCollectionViewController.didSelectLevelEvent` fires when you click a song.
2. **Hash extraction** — Custom level IDs are formatted `custom_level_<SHA1HASH>`. The hash is parsed out.
3. **BeatSaver API call** — `GET https://api.beatsaver.com/maps/hash/<hash>` returns the map's JSON, from which the `id` field (the BSR key) is extracted.
4. **BSML display** — Results are marshalled back to the Unity main thread and applied to the BSML view components.

---

## ⚠️ Notes

- Requires an **internet connection** to resolve hashes.
- The BeatSaver API has a rate limit of ~10 req/s; rapidly scrolling through songs may briefly show "loading" until the request completes.
- OST levels have no BSR key and display an appropriate message.
- The floating screen position is **not** persisted between sessions in v1.0.0 — it resets to the default position on restart. Position persistence can be added in a future version using BSIPA's config system.

---

## 📜 License

MIT — do whatever you like, attribution appreciated.
