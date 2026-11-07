param(
    [Parameter(Mandatory=$true)][string]$PublishDirectory,
    [Parameter(Mandatory=$true)][string]$DependencyFile
)
$ErrorActionPreference = 'Stop'
# MSBuild can inherit PowerShell 7's module path when launching Windows PowerShell.
# Prefer this process's native modules; do not change any persisted system setting.
if ($PSVersionTable.PSEdition -eq 'Desktop') { $env:PSModulePath = "$PSHOME\Modules;" + [Environment]::GetEnvironmentVariable('PSModulePath', 'Machine') }
Set-StrictMode -Version Latest
$root = Split-Path $PSScriptRoot -Parent
$manifest = Get-Content (Join-Path $root 'ThirdParty/licenses-manifest.json') -Raw | ConvertFrom-Json
$deps = Get-Content -LiteralPath $DependencyFile -Raw | ConvertFrom-Json
if (-not (Test-Path (Join-Path $PublishDirectory 'QR_King.exe'))) { throw 'Published executable is missing.' }
$components = @('QR_King')
foreach ($library in $deps.libraries.PSObject.Properties) {
    if ($library.Name -like 'QR_King/*' -or $library.Name -like 'Microsoft.NET.ILLink.Tasks/*') { continue }
    if (-not $manifest.components.PSObject.Properties[$library.Name]) { throw "License inventory has no reviewed entry for $($library.Name). Update ThirdParty/licenses-manifest.json before packaging." }
    $components += $library.Name
}
if (-not ($components -match '^runtimepack.Microsoft.NETCore.App.Runtime.win-x64/')) { throw 'Self-contained .NET runtime dependency is missing.' }
if (-not ($components -match '^runtimepack.Microsoft.WindowsDesktop.App.Runtime.win-x64/')) { throw 'Desktop runtime dependency is missing.' }
$files = @()
foreach ($component in $components) { $files += @($manifest.components.PSObject.Properties[$component].Value) }
$files = @($files | Sort-Object -Unique)
# Validate the complete inventory before writing any distributable notices.
foreach ($relative in $files) {
    $source = Join-Path $root $relative
    if (-not (Test-Path -LiteralPath $source -PathType Leaf)) { throw "Required license or notice missing: $relative" }
    $expected = $manifest.sha256.PSObject.Properties[$relative].Value
    if ((Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash.ToLowerInvariant() -ne $expected) { throw "License/notice checksum mismatch: $relative" }
}
$licenseDirectory = Join-Path $PublishDirectory 'licenses'
New-Item -ItemType Directory -Force $licenseDirectory | Out-Null
$index = @('# Bundled software licenses', '', 'QR King source is MIT licensed. Other components retain their own licenses and copyrights. Keep this folder with the application.', '')
foreach ($component in $components) {
    $names = @($manifest.components.PSObject.Properties[$component].Value | ForEach-Object { [IO.Path]::GetFileName($_) })
    $index += "- ${component}: $($names -join ', ')"
}
$inventory = @()
foreach ($relative in $files) {
    $name = [IO.Path]::GetFileName($relative)
    Copy-Item -LiteralPath (Join-Path $root $relative) -Destination (Join-Path $licenseDirectory $name) -Force
    $inventory += [ordered]@{ file = "licenses/$name"; sha256 = $manifest.sha256.PSObject.Properties[$relative].Value; source = $relative }
}
$index += @('', 'Runtime package versions are read from the build dependency manifest. Exact-source provenance and unresolved interpretation questions are documented in LICENSE-REVIEW.md. This index is not a replacement license or a legal clearance.', '', 'QR Code is a registered trademark of DENSO WAVE INCORPORATED.')
$index | Set-Content (Join-Path $licenseDirectory 'INDEX.md') -Encoding UTF8
$inventory | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $licenseDirectory 'inventory.json') -Encoding UTF8
Copy-Item (Join-Path $root 'docs/LICENSE-REVIEW.md') (Join-Path $licenseDirectory 'LICENSE-REVIEW.md') -Force
Write-Output "Validated license bundle: $($components.Count) components, $($files.Count) license/notice files."
