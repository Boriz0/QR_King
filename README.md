# QR King

[![Windows build](https://github.com/Boriz0/QR_King/actions/workflows/windows-build.yml/badge.svg)](https://github.com/Boriz0/QR_King/actions/workflows/windows-build.yml)

A minimal Windows QR generator by Boris Vacula. Generate from clipboard or edited text, customize QR settings, use Wi-Fi/contact templates, and export PNG, JPG, or SVG — all locally.

This is a non-commercial project. Its source remains MIT licensed, including permission for downstream commercial reuse. Dependencies retain their own licenses and copyrights.

[Privacy](PRIVACY.md) · [Security policy](SECURITY.md) · [Build verification and releases](docs/RELEASING.md) · [License inventory and name-review record](docs/LICENSE-REVIEW.md)

## Download for Windows

[Download QR King v0.2.2 for Windows x64](https://github.com/Boriz0/QR_King/releases/download/v0.2.2/QR_King-0.2.2-win-x64.zip)

Extract the ZIP and run `QR_King.exe`. No separate .NET installation is required. The build is unsigned.

Keep the complete extracted folder, including `licenses` and `PRIVACY.md`, together. The release includes a SHA-256 checksum. The corrected v0.1.0 package remains available in the release history.

[All releases and release notes](https://github.com/Boriz0/QR_King/releases)

## Screenshots

Click any screenshot to view it at full size.

| Main window | Advanced settings |
| :---: | :---: |
| <a href="https://github.com/user-attachments/assets/f8ce772c-166c-4e7c-bf9f-e274a587d0c7"><img src="https://github.com/user-attachments/assets/f8ce772c-166c-4e7c-bf9f-e274a587d0c7" alt="QR King main window with QR preview and clipboard content" width="280"></a> | <a href="https://github.com/user-attachments/assets/7342738a-cc5e-4176-b06a-0c02071b96cf"><img src="https://github.com/user-attachments/assets/7342738a-cc5e-4176-b06a-0c02071b96cf" alt="Advanced QR generation and export settings" width="280"></a> |

| Wi-Fi template | Contact template | About |
| :---: | :---: | :---: |
| <a href="https://github.com/user-attachments/assets/a19ac0ad-98f7-457f-9d5a-bbbf5d2b8ad9"><img src="https://github.com/user-attachments/assets/a19ac0ad-98f7-457f-9d5a-bbbf5d2b8ad9" alt="Wi-Fi QR template" width="210"></a> | <a href="https://github.com/user-attachments/assets/6618bc5e-2d03-4688-9ad8-e8886b551628"><img src="https://github.com/user-attachments/assets/6618bc5e-2d03-4688-9ad8-e8886b551628" alt="Contact QR template" width="210"></a> | <a href="https://github.com/user-attachments/assets/ddace9b1-ef68-49b5-94a6-8a7af774a4ed"><img src="https://github.com/user-attachments/assets/ddace9b1-ef68-49b5-94a6-8a7af774a4ed" alt="About QR King with version and credits" width="210"></a> |

## Run

Open `QR_King/QR_King.sln` in an up-to-date Visual Studio 2026 with the .NET desktop development workload and the SDK selected by `global.json`, allow NuGet restore, then press F5.

The app and smoke tests target .NET 10 LTS. Visual Studio 2022 does not support targeting .NET 10. You can also use the .NET 10 SDK directly without upgrading the IDE:

    dotnet run --project QR_King/QR_King.csproj
    dotnet run --project tests/QR_King.SmokeTests -c Release

global.json selects SDK 10.0.403 and allows later patches in that feature band. On the maintainer's current workstation, the verified SDK is installed locally at .dotnet/sdk10; use ./.dotnet/sdk10/dotnet.exe in place of dotnet and set DOTNET_ROOT to that directory when running the smoke tests. This local SDK directory is ignored by Git.

.NET 10 LTS support runs through November 14, 2028. Standalone builds bundle runtime 10.0.14 and do not require users to install .NET. Future runtime security fixes require rebuilding and distributing a new standalone package.

## Features

- Reads clipboard text on startup and updates automatically while running.
- Shows exact text, including whitespace, below the code.
- Exports lossless PNG (default), JPG at quality 100, and scalable SVG. The quiet zone is at least four QR squares wide.
- Copies a standard bitmap, PNG, and JPEG data (JFIF). The receiving app selects the format. Copying the app's image preserves its preview.
- Clears the preview for other non-text or empty clipboard content.
- Handles oversized text and retries temporarily busy clipboard reads.
- About credits Boris Vacula, QRCoder, and ZXing.Net and links to https://github.com/Boriz0/QR_King.
- Live updates starts checked. Uncheck to freeze the preview and cancel pending reads; check again to read the current clipboard and resume monitoring.
- With Live updates off, edit or type multiline text in Clipboard Content and press Regenerate. Editing does not alter the clipboard or regenerate automatically. A visible stale-preview notice covers the old QR and Save/Copy are disabled until regeneration (or until the text is reverted to match). Empty or oversized manual input shows corrective guidance.
- Unsupported or empty clipboard content and oversized text show inline errors with suggestions. Copy text/a URL, or shorten the payload, to recover.
- Current version is 0.2.2; revision comes from assembly informational metadata. Without Git metadata, About explicitly says local build. A release can set it with `-p:SourceRevisionId=YOUR_COMMIT`.

QR generation is local; no clipboard text is transmitted to a service.

### New in v0.2.2

- Migrated the application and smoke tests from .NET 8 to .NET 10 LTS.
- Updated the standalone runtime to 10.0.14 and pinned it in the publish profile.
- Reviewed and bundled the exact runtime licenses/notices; retained packaging checks and historical inventory.
- QRCoder and ZXing.Net remain unchanged after compatibility tests and a NuGet vulnerability check.

### Features introduced in v0.2.1

Removes text persistence and both related Advanced controls. It migrates older settings to remove stored text. The previous v0.2.0 release had an opt-in text-storage feature; v0.2.1 removes it.

- **Drop a .txt file** anywhere in the main window to load and generate in manual mode. The visible hint explains this. UTF-8 and BOM-marked UTF-16/UTF-32 are supported, up to 64 KB; invalid/binary files produce guidance and preserve the current content. QR capacity still limits how much text can be encoded.
- **Stale preview** covers an outdated QR while manual edits await Regenerate, preventing accidental scanning or exporting of the previous payload.
- **Export sizes** in Advanced offer Auto, exact 256/512/1024/2048 px canvases, or custom pixels per QR square. Raster modules use whole pixels, with extra background padding as needed. A size too small for the content is rejected with guidance. The main window displays the actual dimensions; resizing its preview does not change the export.
- **Advanced help** explains each option through tooltips, including disabled custom sizing. **Open settings folder** opens the local preferences folder.
- **Windows theme** follows the system's app light/dark preference, including changes while running, across the main window and dialogs. High contrast uses system colours. QR foreground/background colours are independent of the interface theme.

### Features introduced in v0.2.0

The v0.2.0 download includes these features and the privacy and license-packaging updates.

- **Fit window to QR code** resizes after generation or when enabled, preserves controls, and clamps to the current monitor's work area. Export dimensions remain independent of the preview.
- **Advanced** provides error correction L/M/Q/H, Auto or 2–16 pixels per QR square, a 4–16 square border, Auto or fixed version 1–40, foreground/background hex colours, default export format, and Reset defaults. Invalid settings show guidance; fixed versions may be too small for the payload. Reset defaults resets the Advanced panel, including QR and export options.
- **Always on top** pins the window above other apps.
- **Ctrl+Enter** regenerates in manual mode; **Ctrl+S** saves the current image.
- **Clear** removes the displayed text and QR without changing the Windows clipboard. Live mode stays enabled if selected and responds to the next clipboard change.
- **Templates** creates Wi-Fi codes (WPA/WPA2, WEP, open or hidden networks) and vCard contacts. Using a template switches to manual mode.
- **Scan verification** decodes generated images and raster exports and checks for an exact payload match. SVG uses the same verified matrix. This is an internal consistency check, not a guarantee every phone camera or printed copy will scan.
- **Preferences** saves only window bounds, live updates, fit, pin, and QR/export settings. The current version never saves or restores clipboard, manually entered, or template text. Existing legacy text fields and abandoned temporary settings are cleaned up at startup; failures are reported.

## Dependency

QRCoder 1.6.0 (MIT) generates codes; ZXing.Net 0.16.11 (Apache 2.0) verifies them. See THIRD-PARTY-NOTICES.txt and ThirdParty/ZXing-LICENSE.txt.

## Standalone Windows build

    dotnet publish QR_King/QR_King.csproj -p:PublishProfile=Standalone

    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/New-ReleaseArchive.ps1 -PublishDirectory artifacts/standalone/win-x64 -DependencyFile QR_King/bin/Release/net10.0-windows/win-x64/QR_King.deps.json -OutputPath artifacts/QR_King-0.2.2-win-x64.zip

The profile publishes a Release, self-contained Windows x64 executable to `artifacts/standalone/win-x64`. No separate .NET installation is required. Native runtime files extract automatically on first launch. Trimming is disabled for WPF compatibility.

Self-contained publishing validates exact dependency versions and license-file checksums and creates a `licenses` folder with an index and inventory. Missing notices, changed checksums, or unaudited dependency versions fail publishing/packaging. Update the versioned inventory when updating dependencies; do not bypass the check. Include the entire publish folder, including `PRIVACY.md` and `licenses`, when distributing. This build is unsigned and targets .NET 10. The standalone profile pins runtime 10.0.14; update this pin together with the reviewed license inventory for future servicing releases.

The PowerShell execution-policy flag applies only to that script process; it does not change the machine's saved execution policy. Review scripts before running them.

QR Code is a registered trademark of DENSO WAVE INCORPORATED. This project is not affiliated with DENSO WAVE. The separate name QR King has not been cleared for trademark availability; see the preliminary [name review](docs/LICENSE-REVIEW.md#name-check).
