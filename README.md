# QR King

A minimal Windows clipboard QR generator by Boris Vacula.

## Download for Windows

[Download QR King v0.1.0 for Windows x64](https://github.com/Boriz0/QR_King/releases/download/v0.1.0/QR_King-0.1.0-win-x64.zip)

Extract the ZIP and run `QR_King.exe`. No separate .NET installation is required. The build is unsigned.

[All releases and release notes](https://github.com/Boriz0/QR_King/releases)

<img width="642" height="1197" alt="image" src="https://github.com/user-attachments/assets/df57928c-6a74-4b7f-9831-f46e941c88f8" />


## Run

Open `QR_King/QR_King.sln` in Visual Studio 2022, allow NuGet restore, then press F5. If Visual Studio asks to reload changed files, accept.

The existing project targets .NET 8. Before release, move to a supported framework: .NET 8 support ends November 10, 2026. .NET 10 requires Visual Studio 2026; a .NET Framework port is an alternative for Visual Studio 2022.

## Features

- Reads clipboard text on startup and updates automatically while running.
- Shows exact text, including whitespace, below the code.
- Saves JPG at quality 100 with a white quiet zone.
- Copies both a standard bitmap and JPEG data (JFIF). The receiving app selects the format. Copying the app's image preserves its preview.
- Clears the preview for other non-text or empty clipboard content.
- Handles oversized text and retries temporarily busy clipboard reads.
- About credits Boris Vacula and QRCoder and links to https://github.com/Boriz0/QR_King.
- Live updates starts checked. Uncheck to freeze the preview and cancel pending reads; check again to read the current clipboard and resume monitoring.
- With Live updates off, edit or type multiline text in Clipboard Content and press Regenerate. Editing does not alter the clipboard or regenerate automatically. Save/Copy are disabled until regeneration so an old QR is not exported for new text. Empty or oversized manual input shows corrective guidance.
- Unsupported or empty clipboard content and oversized text show inline errors with suggestions. Copy text/a URL, or shorten the payload, to recover.
- Version is 0.1.0; revision comes from assembly informational metadata. Without Git metadata, About explicitly says local build. A release can set it with `-p:SourceRevisionId=YOUR_COMMIT`.

QR generation is local; no clipboard text is transmitted to a service.

## Build and tests

    dotnet build QR_King/QR_King.sln -c Release
    dotnet run --project tests/QR_King.SmokeTests -c Release

The smoke tests independently decode generated QR images and JPEG exports with ZXing.Net. They cover URLs, normal text, accented characters, emoji, newlines, whitespace and a long payload, reject oversized data, and construct the main/About views. They do not alter the clipboard.

Manual checks: copy a URL before launch, scan with a phone, copy new text, save and scan a JPG, copy the image into Paint, then copy fresh text. Check non-text clipboard handling and the About dialog. Live clipboard interaction, paste compatibility and file dialogs require interactive verification.

## Dependency

QRCoder 1.6.0, MIT license; see THIRD-PARTY-NOTICES.txt. ZXing.Net is a test-only dependency and is not shipped with the app.

## Standalone Windows build

    dotnet publish QR_King/QR_King.csproj -p:PublishProfile=Standalone

The profile publishes a Release, self-contained Windows x64 executable to `artifacts/standalone/win-x64`. No separate .NET installation is required. Native runtime files extract automatically on first launch. Trimming is disabled for WPF compatibility.

Run `QR_King.exe`. When distributing, include `THIRD-PARTY-NOTICES.txt`. The packaged ZIP contains both files. This build is unsigned and uses the project's current .NET 8 target.
