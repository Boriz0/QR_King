# Building, verifying, and releasing QR King

## Automated Windows checks

The Windows build workflow runs on pull requests targeting main, pushes to main, version tags, and manual dispatch. It installs the SDK specified in global.json on a hosted Windows runner, then:

1. Restores dependencies with NuGet auditing of direct and transitive dependencies. Known vulnerabilities and audit-source failures fail the restore.
2. Runs the WPF smoke-test executable. A nonzero exit fails CI; logs and rendered light/dark views are retained as artifacts, and passing results appear in the run summary.
3. Exercises license-packaging success, unknown dependencies, altered notices, and missing notices.
4. Publishes the self-contained application with the triggering commit as its build revision.
5. Validates the exact runtime license inventory, creates the ZIP and checksum, and records build metadata.
6. For trusted main/tag builds, attests the ZIP and build metadata in a separate job that does not check out or execute repository code.

Actions are pinned to immutable commits. Pull requests receive read-only repository permissions and cannot run the attestation or release jobs. The workflow does not use pull_request_target or private signing keys.

The smoke tests are an executable harness, not a dotnet test suite; their PASS lines and process exit status are the test results. Download windows-test-results from the run for logs and rendered views. These tests do not replace interactive testing on a clean Windows machine.

## Prepare a release

1. Update the project version, About-version smoke assertion, README download link, and release notes. Update the SDK/runtime pins and exact license inventory when servicing dependencies.
2. Merge to main and require a successful Windows build. Review the generated package and test results.
3. Create and push a matching tag, for example v0.2.3 for project version 0.2.3. Never reuse or move a published tag.
4. The tag workflow rebuilds, tests, attests, verifies provenance, and creates a **draft** GitHub release with the exact ZIP, checksum, build metadata, and attestation bundle.
5. Review the draft notes and assets, perform the interactive checks, then publish it. Do not rebuild or alter the ZIP after attestation.

The workflow fails instead of replacing an existing release. If a tag run fails after creating a draft, inspect that draft and the run before taking recovery action. Artifact retention is 30 days; published release assets and their attached verification bundles provide the durable download. Existing v0.2.2 assets are not replaced or retroactively attested.

Repository owners should protect main with required Windows CI checks and restrict version-tag creation to trusted maintainers. These repository rules are separate settings; adding a workflow does not enforce them.

## Verify a download

Install a current [GitHub CLI](https://cli.github.com/) and download the ZIP and .sha256 file from the release. In PowerShell:

    $archive = 'QR_King-0.2.3-win-x64.zip'
    $expected = ((Get-Content "$archive.sha256" -Raw).Trim() -split '\s+')[0]
    $actual = (Get-FileHash $archive -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actual -ne $expected) { throw 'Checksum mismatch' }
    gh attestation verify $archive --repo Boriz0/QR_King --signer-workflow Boriz0/QR_King/.github/workflows/windows-build.yml
    if ($LASTEXITCODE -ne 0) { throw 'Provenance verification failed' }

Review the verified source commit/ref and compare them with the intended release tag. A checksum detects changed bytes but is not independently authenticated. The attestation binds the artifact digest to its GitHub workflow identity and source; it does not establish that the code is safe or that builds are byte-for-byte reproducible.

See [GitHub's attestation documentation](https://docs.github.com/en/actions/how-tos/secure-your-work/use-artifact-attestations/use-artifact-attestations).

## Authenticode signing roadmap

Authenticode is not enabled yet. It requires a trusted publisher identity and a code-signing certificate/service controlled by the maintainer.

When that is available:

1. Configure hardware-backed keys or a managed signing service. Prefer short-lived OIDC authentication; do not commit certificates, private keys, passwords, or tokens.
2. Isolate signing to reviewed release tags in a protected GitHub environment. Require approval and restrict the signing identity to this repository/workflow.
3. Sign the final published EXE with SHA-256 and an RFC 3161 SHA-256 timestamp before ZIP creation.
4. Verify the signature and expected publisher, then package, checksum, and attest the signed bytes. Never sign or modify files after their ZIP has been attested.
5. Make signing failure fail the release. Update build metadata and documentation only after a valid signature is verified.

Microsoft documents the commands and timestamp requirements in [SignTool](https://learn.microsoft.com/en-us/windows/win32/seccrypto/signtool). Code signing and GitHub provenance serve different purposes; retain both once signing is available.
