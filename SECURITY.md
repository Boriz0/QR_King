# Security policy

## Supported versions

Security fixes target the latest stable release. Older releases are not maintained separately; please update before reporting a problem. Reports affecting the current main branch are also welcome.

The standalone Windows download includes its .NET runtime. Updating a separately installed .NET runtime does not update an existing QR King executable; install a newer QR King release when one is available.

## Report a vulnerability privately

Use [GitHub private vulnerability reporting](https://github.com/Boriz0/QR_King/security/advisories/new). Reports are shared with the repository's maintainers, not posted as public issues.

Please include:

- The QR King version, Windows version, and whether you used a release ZIP or built from source.
- A description of the impact and steps to reproduce it.
- A minimal sample using synthetic data, relevant logs, and suggested mitigation if known.

Do not include real clipboard contents, passwords, personal information, or private QR codes. Do not post exploit details in public issues or pull requests before coordinated disclosure.

If GitHub's private reporting form is unavailable, open a public issue asking only for a private reporting channel. Do not include vulnerability details in that issue.

This is a volunteer, non-commercial project maintained by Boris Vacula. Reports are handled as availability permits; there is no guaranteed response time or paid bounty program. Please allow time to investigate and coordinate disclosure with the maintainer.

## Build integrity

[Windows CI](https://github.com/Boriz0/QR_King/actions/workflows/windows-build.yml) builds and tests pull requests, main-branch changes, and release tags. Trusted main/tag builds receive GitHub artifact attestations. See [release verification](docs/RELEASING.md) for verification commands and limitations.

Current executables are not Authenticode signed. Build provenance identifies the workflow and source associated with an artifact; it is not an Authenticode signature or proof that the application has no vulnerabilities. Releases through v0.2.2 were built locally and do not have these new CI attestations.
