# QR King

A minimal Windows QR generator by Boris Vacula. Generate from clipboard or edited text, customize QR settings, use Wi-Fi/contact templates, and export PNG, JPG, or SVG — all locally.

This is a non-commercial project. Its source remains MIT licensed, including permission for downstream commercial reuse. Dependencies retain their own licenses and copyrights.

[Privacy and saved text](PRIVACY.md) · [License inventory and name-review record](docs/LICENSE-REVIEW.md)

## Download for Windows

[Download QR King v0.2.0 for Windows x64](https://github.com/Boriz0/QR_King/releases/download/v0.2.0/QR_King-0.2.0-win-x64.zip)

Extract the ZIP and run `QR_King.exe`. No separate .NET installation is required. The build is unsigned.

Keep the complete extracted folder, including `licenses` and `PRIVACY.md`, together. The release includes a SHA-256 checksum. The corrected v0.1.0 package remains available in the release history.

[All releases and release notes](https://github.com/Boriz0/QR_King/releases)

<img width="642" height="1197" alt="image" src="https://github.com/user-attachments/assets/df57928c-6a74-4b7f-9831-f46e941c88f8" />


## Run

Open `QR_King/QR_King.sln` in Visual Studio 2022, allow NuGet restore, then press F5. If Visual Studio asks to reload changed files, accept.

The existing project targets .NET 8. Before release, move to a supported framework: .NET 8 support ends November 10, 2026. .NET 10 requires Visual Studio 2026; a .NET Framework port is an alternative for Visual Studio 2022.

## Features

- Reads clipboard text on startup and updates automatically while running.
- Shows exact text, including whitespace, below the code.
- Exports lossless PNG (default), JPG at quality 100, and scalable SVG. The quiet zone is at least four QR squares wide.
- Copies a standard bitmap, PNG, and JPEG data (JFIF). The receiving app selects the format. Copying the app's image preserves its preview.
- Clears the preview for other non-text or empty clipboard content.
- Handles oversized text and retries temporarily busy clipboard reads.
- About credits Boris Vacula, QRCoder, and ZXing.Net and links to https://github.com/Boriz0/QR_King.
- Live updates starts checked. Uncheck to freeze the preview and cancel pending reads; check again to read the current clipboard and resume monitoring.
- With Live updates off, edit or type multiline text in Clipboard Content and press Regenerate. Editing does not alter the clipboard or regenerate automatically. Save/Copy are disabled until regeneration so an old QR is not exported for new text. Empty or oversized manual input shows corrective guidance.
- Unsupported or empty clipboard content and oversized text show inline errors with suggestions. Copy text/a URL, or shorten the payload, to recover.
- Current version is 0.2.0; revision comes from assembly informational metadata. Without Git metadata, About explicitly says local build. A release can set it with `-p:SourceRevisionId=YOUR_COMMIT`.

QR generation is local; no clipboard text is transmitted to a service.

### New in v0.2.0

The v0.2.0 download includes these features and the privacy and license-packaging updates.

- **Fit window to QR code** resizes after generation or when enabled, preserves controls, and clamps to the current monitor's work area. Export dimensions remain independent of the preview.
- **Advanced** provides error correction L/M/Q/H, Auto or 2–16 pixels per QR square, a 4–16 square border, Auto or fixed version 1–40, foreground/background hex colours, default export format, and Reset defaults. Invalid settings show guidance; fixed versions may be too small for the payload. Reset defaults resets the Advanced panel, including text-storage opt-in.
- **Always on top** pins the window above other apps.
- **Ctrl+Enter** regenerates in manual mode; **Ctrl+S** saves the current image.
- **Clear** removes the displayed text and QR without changing the Windows clipboard. Live mode stays enabled if selected and responds to the next clipboard change.
- **Templates** creates Wi-Fi codes (WPA/WPA2, WEP, open or hidden networks) and vCard contacts. Using a template switches to manual mode.
- **Scan verification** decodes generated images and raster exports and checks for an exact payload match. SVG uses the same verified matrix. This is an internal consistency check, not a guarantee every phone camera or printed copy will scan.
- **Preferences** remembers window bounds, live updates, fit, pin, and QR/export settings in `%LOCALAPPDATA%/QR King/settings.json`. Text is not stored by default. Advanced has an explicit Remember text opt-in; stored text is restored in manual mode. Disabling it removes saved text. Corrupt settings fall back to defaults. Templates, including Wi-Fi passwords, are ordinary text for this option.
- **Delete saved text** in Advanced immediately removes the saved value and disables remembering, even if the dialog is subsequently cancelled. Text storage is unencrypted. See [Privacy](PRIVACY.md) for deletion instructions and limitations.

## Dependency

QRCoder 1.6.0 (MIT) generates codes; ZXing.Net 0.16.11 (Apache 2.0) verifies them. See THIRD-PARTY-NOTICES.txt and ThirdParty/ZXing-LICENSE.txt.

## Standalone Windows build

    dotnet publish QR_King/QR_King.csproj -p:PublishProfile=Standalone

    powershell -NoProfile -ExecutionPolicy Bypass -File scripts/New-ReleaseArchive.ps1 -PublishDirectory artifacts/standalone/win-x64 -DependencyFile QR_King/bin/Release/net8.0-windows/win-x64/QR_King.deps.json -OutputPath artifacts/QR_King-0.2.0-win-x64.zip

The profile publishes a Release, self-contained Windows x64 executable to `artifacts/standalone/win-x64`. No separate .NET installation is required. Native runtime files extract automatically on first launch. Trimming is disabled for WPF compatibility.

Self-contained publishing validates exact dependency versions and license-file checksums and creates a `licenses` folder with an index and inventory. Missing notices, changed checksums, or unaudited dependency versions fail publishing/packaging. Update the versioned inventory when updating dependencies; do not bypass the check. Include the entire publish folder, including `PRIVACY.md` and `licenses`, when distributing. This build is unsigned and uses the project's current .NET 8 target.

The PowerShell execution-policy flag applies only to that script process; it does not change the machine's saved execution policy. Review scripts before running them.

QR Code is a registered trademark of DENSO WAVE INCORPORATED. This project is not affiliated with DENSO WAVE. The separate name QR King has not been cleared for trademark availability; see the preliminary [name review](docs/LICENSE-REVIEW.md#name-check).
