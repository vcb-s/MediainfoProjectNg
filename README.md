# mediainfo project ng

A simple rewrite of mediainfo project using WPF. The original project is written by mori.

## Usage

### Prepare MediaInfo DLL

Use the latest version of MediaInfo. Install [MediaInfo](https://mediaarea.net/en/MediaInfo), then copy `MediaInfo.dll` from the MediaInfo installation directory to the application directory, or create a link to the DLL there.

### Load Files

Drag files or a whole directory into the window. The application lists media information, runs checks, and reports issues. Double-click a row to open the detail window.

When loading a directory, the application scans files recursively, skips `CDs` and `Scans` directories, and ignores `.txt`, `.log`, and `.torrent` files. Files already in the list are skipped.

### Detail Checks

The detail window shows these check results:

| Level | Check | Message |
| --- | --- | --- |
| `Error` | Matroska files should use `.mkv`, `.mka`, or `.mks`; MPEG-4 files should use `.mp4`, `.m4a`, or `.m4v`. | `文件后缀和与容器不符。后缀：{extension}，容器{format}` |
| `Error` | For matching VCB-S or VCB-Studio style `.mkv` names, the filename description should match the detected profile, video encoder, and audio encoders. | `内容物和文件名描述不符。` |
| `Warning` | Any video or audio track has a non-zero delay. | `容器中含有延时非 0 的轨道。` |
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
| Row text | Files with more than one subtitle track use blue text. |
| FPS | VFR or unusual frame rates are colored. |
| Color space | Non-`YUV420` color space is colored orange. |
| Chapter language | Mixed or missing chapter languages are highlighted. |

## Build

```powershell
dotnet restore --runtime win-x64
dotnet build MediainfoProjectNg --configuration Release --no-restore --runtime win-x64
dotnet publish MediainfoProjectNg --configuration Release --no-build --runtime win-x64
```

## License

BSD License 2.0
