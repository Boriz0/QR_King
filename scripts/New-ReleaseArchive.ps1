param(
    [Parameter(Mandatory=$true)][string]$PublishDirectory,
    [Parameter(Mandatory=$true)][string]$DependencyFile,
    [Parameter(Mandatory=$true)][string]$OutputPath
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
& (Join-Path $PSScriptRoot 'Add-Licenses.ps1') -PublishDirectory $PublishDirectory -DependencyFile $DependencyFile
$root = Split-Path $PSScriptRoot -Parent
Copy-Item (Join-Path $root 'PRIVACY.md') (Join-Path $PublishDirectory 'PRIVACY.md') -Force
foreach ($name in @('QR_King.exe', 'LICENSE', 'THIRD-PARTY-NOTICES.txt', 'PRIVACY.md', 'licenses/INDEX.md', 'licenses/inventory.json')) {
    if (-not (Test-Path (Join-Path $PublishDirectory $name) -PathType Leaf)) { throw "Cannot package: missing $name" }
}
$outputFull = [IO.Path]::GetFullPath($OutputPath)
$publishFull = [IO.Path]::GetFullPath($PublishDirectory).TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
if ($outputFull.StartsWith($publishFull, [StringComparison]::OrdinalIgnoreCase)) { throw 'Place the archive outside its input folder.' }
New-Item -ItemType Directory -Force ([IO.Path]::GetDirectoryName($outputFull)) | Out-Null
Compress-Archive -Path (Join-Path $publishFull '*') -DestinationPath $outputFull -Force
$hash = (Get-FileHash -LiteralPath $outputFull -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash  $([IO.Path]::GetFileName($outputFull))" | Set-Content ($outputFull + '.sha256') -Encoding ascii
Write-Output "Created $outputFull"
