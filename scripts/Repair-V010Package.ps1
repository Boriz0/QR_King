param([string]$OriginalZip = 'artifacts/QR_King-0.1.0-win-x64.zip')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$original = [IO.Path]::GetFullPath($OriginalZip)
# Digest verified against the original GitHub release asset, ID 717070913.
$expected = '4c6a4c2fe611e582ce83ebd05ec3d6d5a805384f7151ef282ea11d8ed06b8563'
if ((Get-FileHash -LiteralPath $original -Algorithm SHA256).Hash.ToLowerInvariant() -ne $expected) { throw 'This is not the audited original v0.1.0 ZIP. Stop and review the source artifact.' }
$stage = Join-Path $root ('artifacts/repack-v0.1.0-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $stage | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::OpenRead($original)
try {
    foreach ($name in @('QR_King.exe', 'LICENSE', 'THIRD-PARTY-NOTICES.txt')) {
        $entry = $zip.GetEntry($name)
        if ($null -eq $entry) { throw "Original ZIP is missing $name" }
        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, (Join-Path $stage $name))
    }
} finally { $zip.Dispose() }
$exeHash = (Get-FileHash (Join-Path $stage 'QR_King.exe') -Algorithm SHA256).Hash.ToLowerInvariant()
# Audited original source: tag v0.1.0 / 93f875e4cb624d3a5ae55163e6a32279e132b1d2.
# Original release uses QRCoder 1.6.0 and self-contained .NET/Desktop 8.0.33; no ZXing.
$legacyDeps = @{ libraries = @{
 'QR_King/0.1.0' = @{}; 'QRCoder/1.6.0' = @{};
 'System.Drawing.Common/6.0.0' = @{}; 'Microsoft.Win32.SystemEvents/6.0.0' = @{};
 'runtimepack.Microsoft.NETCore.App.Runtime.win-x64/8.0.33' = @{};
 'runtimepack.Microsoft.WindowsDesktop.App.Runtime.win-x64/8.0.33' = @{}
} }
$depsFile = Join-Path $root 'artifacts/v0.1.0-reviewed-dependencies.json'
$legacyDeps | ConvertTo-Json -Depth 5 | Set-Content $depsFile -Encoding UTF8
@"
# v0.1.0 packaging correction — 7 November 2026

This package retains the original executable byte-for-byte and adds required runtime licenses/notices, their index, and privacy documentation. Application behaviour and the v0.1.0 source tag are unchanged. Newer privacy controls described for v0.2.0 do not exist in this executable.

Original published ZIP SHA-256: $expected
Executable SHA-256 (unchanged): $exeHash
Original source revision: 93f875e4cb624d3a5ae55163e6a32279e132b1d2

Keep the complete extracted folder together. QR Code is a registered trademark of DENSO WAVE INCORPORATED.
"@ | Set-Content (Join-Path $stage 'PACKAGING-CORRECTION.md') -Encoding UTF8
$output = Join-Path $root 'artifacts/QR_King-0.1.0-win-x64-licenses-fixed.zip'
& (Join-Path $PSScriptRoot 'New-ReleaseArchive.ps1') -PublishDirectory $stage -DependencyFile $depsFile -OutputPath $output
$corrected = [IO.Compression.ZipFile]::OpenRead($output)
try {
    $stream = $corrected.GetEntry('QR_King.exe').Open()
    try { $sha = [Security.Cryptography.SHA256]::Create(); $actual = [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-', '').ToLowerInvariant(); $sha.Dispose() } finally { $stream.Dispose() }
    if ($actual -ne $exeHash) { throw 'Executable changed during repackaging.' }
} finally { $corrected.Dispose() }
Write-Output "Verified unchanged v0.1.0 executable: $exeHash"
