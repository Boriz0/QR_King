$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$fixture = Join-Path $root ('artifacts/license-test-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force (Join-Path $fixture 'scripts'), (Join-Path $fixture 'publish'), (Join-Path $fixture 'docs') | Out-Null
Copy-Item (Join-Path $root 'ThirdParty') $fixture -Recurse
Copy-Item (Join-Path $root 'LICENSE') $fixture
Copy-Item (Join-Path $root 'docs/LICENSE-REVIEW.md') (Join-Path $fixture 'docs')
Copy-Item (Join-Path $PSScriptRoot 'Add-Licenses.ps1') (Join-Path $fixture 'scripts')
[IO.File]::WriteAllBytes((Join-Path $fixture 'publish/QR_King.exe'), [byte[]]@(0))
$deps = @{ libraries = @{
 'QR_King/0.2.2' = @{};
 'runtimepack.Microsoft.NETCore.App.Runtime.win-x64/10.0.14' = @{};
 'runtimepack.Microsoft.WindowsDesktop.App.Runtime.win-x64/10.0.14' = @{}
} }
$depsPath = Join-Path $fixture 'dependencies.json'
$deps | ConvertTo-Json -Depth 5 | Set-Content $depsPath
$script = Join-Path $fixture 'scripts/Add-Licenses.ps1'
& $script -PublishDirectory (Join-Path $fixture 'publish') -DependencyFile $depsPath
function Expect-Failure([string]$pattern) {
    try { & $script -PublishDirectory (Join-Path $fixture 'publish') -DependencyFile $depsPath }
    catch { if ($_.Exception.Message -match $pattern) { return }; throw }
    throw "Expected packaging to fail: $pattern"
}
$deps.libraries['Unknown.Dependency/1.0.0'] = @{}
$deps | ConvertTo-Json -Depth 5 | Set-Content $depsPath
Expect-Failure 'no reviewed entry'
$deps.libraries.Remove('Unknown.Dependency/1.0.0')
$deps | ConvertTo-Json -Depth 5 | Set-Content $depsPath
$notice = Join-Path $fixture 'ThirdParty/runtime-10.0.14/NETCore-NOTICES.txt'
[IO.File]::WriteAllText($notice, 'tampered')
Expect-Failure 'checksum mismatch'
Remove-Item -LiteralPath $notice
Expect-Failure 'Required license or notice missing'
Write-Output 'PASS: license bundle validation, unknown dependency rejection, tampered notice rejection, missing notice rejection.'
