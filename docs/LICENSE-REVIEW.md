# License and name review record

Updated 8 November 2026 (runtime migration); initial review 7 November 2026. This is an engineering record, not a legal opinion or trademark clearance. The maintainer describes QR King as a non-commercial project. The MIT license still permits downstream commercial reuse; this project description does not add a non-commercial restriction.

## Source and binary licensing

Original QR King source remains under the root MIT LICENSE. Dependencies retain their own copyrights and terms. The generated licenses/INDEX.md identifies exact versions and their corresponding license/notice files. Redistribution must preserve the applicable files, including any upstream notices; the root license does not relicense third-party components.

## Current v0.2.2 runtime inventory

The application targets net10.0-windows. The standalone profile pins .NET and Windows Desktop runtime 10.0.14; global.json selects SDK 10.0.403 with patch roll-forward. Update the runtime pin and review the corresponding notices together for each runtime servicing update.

- Microsoft.NETCore.App.Runtime.win-x64 10.0.14: the restored NuGet package declares MIT and supplies LICENSE.TXT and THIRD-PARTY-NOTICES.TXT, copied byte-for-byte to ThirdParty/runtime-10.0.14.
- Microsoft.WindowsDesktop.App.Runtime.win-x64 10.0.14: the restored NuGet package declares MIT and supplies LICENSE, but no separate third-party-notices file. The complete supplemental WPF and WinForms notices were copied from src/wpf/THIRD-PARTY-NOTICES.TXT and src/winforms/THIRD-PARTY-NOTICES.TXT at the exact dotnet/dotnet build commit 0427b9abbead3fa64c6655eb4290c537397131fa. Both runtime package metadata files identify this commit.
- Source provenance: https://github.com/dotnet/dotnet/tree/0427b9abbead3fa64c6655eb4290c537397131fa and its src/source-manifest.json. That manifest records WPF 4726a1a94599e1c2aa090f5326e2ca930f016c7d, WinForms 3c92f9f1cc1dcc74779795d518c67aa80f472df7, and Windows Desktop e77cb537e3004a6cfeb8ca69a62476edb0c84a45.
- SDK ZIP SHA-512 verified against Microsoft's release metadata at https://builds.dotnet.microsoft.com/dotnet/release-metadata/10.0/releases.json. SDK 10.0.403 includes runtime 10.0.14.
- QRCoder 1.6.0 and ZXing.Net 0.16.11 remain unchanged and passed the .NET 10 smoke tests. NuGet's vulnerability check reported no vulnerable packages on 8 November 2026. This is a point-in-time result, not a guarantee.
- The .NET 10 resolved dependency graph no longer lists the old System.Drawing.Common 6.0.0 or Microsoft.Win32.SystemEvents 6.0.0 packages separately. Legacy inventory entries remain for the historical-release repair script. Packaging reads the actual publish dependency manifest, so only the current build's dependencies enter its generated index.
- Microsoft.NET.ILLink.Tasks 10.0.14 is build tooling and is not distributed as application runtime code.

The new runtime notices have their own SHA-256 entries. The packaging guard remains enabled and rejects unreviewed versions and missing or modified notices.

## Historical inventory (.NET 8 releases)

Audited versions:

- QRCoder 1.6.0: package declares MIT; LICENSE.txt obtained from the v1.6.0 tag (https://raw.githubusercontent.com/codebude/QRCoder/v1.6.0/LICENSE.txt).
- ZXing.Net 0.16.11: package declares Apache-2.0; COPYING obtained from tag v0.16.11.0 (https://raw.githubusercontent.com/micjahn/ZXing.Net/v0.16.11.0/COPYING). The upstream tag's root tree lists no standalone NOTICE. Unmodified library; copyrights retained in the library and upstream license. https://api.github.com/repos/micjahn/ZXing.Net/git/trees/v0.16.11.0
- Microsoft.NETCore.App.Runtime.win-x64 8.0.33: its downloaded NuGet package explicitly declares MIT and contains LICENSE.TXT and THIRD-PARTY-NOTICES.TXT. Source revision da7a9beabf4cccc624399d583f52f080a4c858c2.
- Microsoft.WindowsDesktop.App.Runtime.win-x64 8.0.33: its NuGet metadata explicitly declares MIT and contains LICENSE. Source revision 148ad015191c8712701bdc59f95e04bed2f89e9f. Neither that package nor the matching SHA-512-verified Microsoft runtime ZIP has a separate notices file. Supplemental notices were retrieved from the exact WPF and WinForms source revisions recorded by that desktop build's eng/Version.Details.xml: WPF 410325ddb32334f300a2bd27dadc20d30e5d85ba; WinForms 6580cce9b1b661defdabcf7bc6781cd47a8c05b5. Full supplemental notices are included even where some listed components are not used by this app.
- System.Drawing.Common 6.0.0 and Microsoft.Win32.SystemEvents 6.0.0: QRCoder's Windows target resolves these transitive packages. Their MIT licenses and third-party notices are retained conservatively even where the desktop framework supplies the runtime assembly.
- Microsoft.NET.ILLink.Tasks is build tooling; it is not distributed as application runtime code.

The versioned copies in ThirdParty are unchanged upstream texts. licenses-manifest.json records their SHA-256 hashes. Changes in dependency versions require a fresh license inventory; packaging stops for unknown versions or missing/modified notice files.

Microsoft's general guidance (https://github.com/dotnet/core/blob/main/license-information.md) describes Windows product distributions under the .NET Library License, whereas these exact runtime packages label themselves MIT. We preserve the exact packages' license evidence and notices rather than replacing those texts with a generic agreement or asserting that MIT resolves all possible distribution terms. Interpretation of any conflict between general guidance and package-specific terms remains unresolved. No claim of complete legal compliance is made. See also https://github.com/dotnet/runtime/blob/main/docs/project/licensing-assets.md.

## Name check

A preliminary web search for QR King / QRKing on 7 November 2026 found other QR products using the same or similar name:

- QR King website and self-described QR service: https://www.qrking.in/blog/how-to-scan-qr-code-from-image.html
- QR King directory listing: https://alternativeto.net/software/qr-king-app/
- Older Android product listing: https://www.appbrain.com/app/qr-king-qr-and-barcode-scann/com.SpiderMonkey.QRKing

These are name-use leads, not evidence establishing current ownership, geographic coverage, registered status, or infringement. Search-engine queries restricted to EUIPO and the Slovak office did not yield a conclusive registration result. The attempted EUIPO search entry page returned HTTP 403; no complete TMview/Slovak register, similar-mark, or goods/services-class examination was completed. Therefore the name is NOT cleared. Do not interpret lack of a search hit as availability. A naming decision remains with the maintainer; no rename was made.

QR Code is a registered trademark of DENSO WAVE INCORPORATED. This acknowledgment follows its FAQ: https://www.qrcode.com/en/faq.html?lang=en. It does not establish rights to the separate QR King name or imply DENSO endorsement.

## Scope

The project remains non-commercial. Commercialization review (point 7 of the agreed checklist) was explicitly excluded. This record neither determines statutory exemptions nor provides a liability or compliance guarantee.
