# mediainfo project ng

A simple rewrite of mediainfo project using WPF. The original project is written by mori.

## Requirements

- Windows x64.
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-10.0.7-windows-x64-installer) to run the published app, or [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-10.0.203-windows-x64-installer) to build it.
- A compatible MediaInfo library.

## Usage

### Prepare MediaInfo DLL

Use the latest version of MediaInfo. Install [MediaInfo](https://mediaarea.net/en/MediaInfo), then copy `MediaInfo.dll` from the MediaInfo installation directory to the application directory, or create a link to the DLL there. The loaded MediaInfoLib version is shown in the window title and status bar.

### Load Files

Drag files or a whole directory into the window. The application lists media information, runs checks, and reports issues. Double-click a row to open the detail window.

When loading a directory, the application scans files recursively, skips `CDs` and `Scans` directories, and ignores `.txt`, `.log`, and `.torrent` files. Files already in the list are skipped.

The main table shows the first video track, the first two audio tracks, the first subtitle track, chapter status/language, and the full path. Select a row to show the raw MediaInfo summary in the right panel. Use `Clear!` to empty the list, or the hide/show button to collapse and restore the right panel.

### Export Screenshot

Click `截图` to save a PNG table snapshot. The export captures every loaded row, not just the visible viewport, and includes the visible table columns before `完整路径` plus a status bar with the MediaInfoLib/version title and file count.

### Detail Checks

The detail window shows these check results:

| Level | Check | Message |
| --- | --- | --- |
| `Error` | Matroska files should use `.mkv`, `.mka`, or `.mks`; MPEG-4 files should use `.mp4`, `.m4a`, or `.m4v`. | `文件后缀和与容器不符。后缀：{extension}，容器{format}` |
| `Error` | For matching VCB-S or VCB-Studio style `.mkv` names, the filename description should match the detected profile tag, resolution tag, video encoder, and audio encoders. Supported video tags include `x264`, `x265`, and `svtav1`; supported profile tags include `Ma10p`, `Ma444-10p`, `Hi10p`, `Hi444pp`, and `Pro10p`. | `内容物和文件名描述不符。` |
| `Warning` | Any video or audio track has a non-zero delay. | `容器中含有延时非 0 的轨道。` |
| `Error` | Any video track has a language other than `UND`. | `视频轨道语言非 UND。` |
| `Warning` | The longest and shortest video/audio tracks differ by more than 600 ms. | `轨道间长度相差过大。` |
| `Warning` | A file has exactly one chapter. | `文件只有一个章节。` |
| `Warning` | A file has multiple chapter groups. | `文件存在多组章节。` |
| `Warning` | The last chapter starts within 1100 ms of the end of the longest video/audio track. | `文件末尾有无用章节。` |
| `Warning` | The first chapter does not start at 0. | `首个章节时间戳非零。` |
| `Info` | The file has more than two audio tracks. | `文件含有多条音轨。` |

### List Highlights

The main list also highlights fields that do not add messages to the detail window:

| Field | Highlight |
| --- | --- |
| Row background | Uses the color of the first detail check result, or white when no issue is found. |
| Row text | Files with more than one subtitle track use blue text. |
| FPS | VFR and unusual frame rates are colored. |
| Color space | Non-`YUV420` color space is colored orange. |
| Chapter language | Mixed or missing chapter languages are highlighted. |

## Build

The project targets `net10.0-windows`, publishes for `win-x64`, and uses a framework-dependent single-file executable (`SelfContained=false`).

```powershell
dotnet restore --runtime win-x64
dotnet build MediainfoProjectNg --configuration Release --no-restore --runtime win-x64
dotnet publish MediainfoProjectNg --configuration Release --no-build --runtime win-x64
```

The GitHub workflow uploads `MediainfoProjectNg\bin\Release\net10.0-windows\win-x64\publish\MediainfoProjectNg.exe`.

## License

[BSD License 2.0](LICENSE)
