# Code signing policy

**Status: preparation only; provider approval and signing integration pending.**

OpenCredential currently publishes unsigned evaluation builds. This policy does not claim that SignPath Foundation has accepted, sponsored or signed the project. SHA256 checksums and Git tags are not Authenticode signatures and do not establish a trusted Windows publisher.

## Scope and responsibilities

- Project: [pedropablobm/OpenCredential](https://github.com/pedropablobm/OpenCredential), an explicitly identified fork of pGina. Upstream copyright/license notices remain unchanged.
- Authorized signing approver: **Pedro Pablo Bermudez Medina**, [@pedropablobm](https://github.com/pedropablobm), as authorized by the repository owner.
- Initial author/reviewer responsibility for build and signing configuration: @pedropablobm. Contributions from others require review by a trusted maintainer; this is not a claim that independent review or branch rules are already enforced.
- All signing participants must use MFA for GitHub and the signing provider. An approval is required for every signing request, including requests submitted by CI. No unattended release-signing policy is authorized.
- `.github/CODEOWNERS` identifies review ownership. The owner must separately configure branch protections and any protected signing environment; CODEOWNERS alone does not enforce approval.

## Artifact origin and approval

1. Build the exact reviewed commit on GitHub-hosted Windows runners. No local/self-hosted binary, external upload or pull-request artifact may be promoted to a trusted signed release merely because a checksum matches.
2. Restore pinned public dependencies, build x64/Win32/core plugins, and run the native and Desktop regression tests. Evaluate integration/security checks relevant to the change; CI compilation is not a substitute for a Windows sign-in test.
3. Upload the unsigned artifact through GitHub Actions. Build metadata and hashes are supplementary evidence, not a replacement for the signing provider's independent origin verification.
4. The approver reviews the exact commit, build/run identity, dependency and installer changes, tests, intended version and artifact contents before manually approving a signing request.
5. Restrict any future signing job to reviewed release refs in this repository, a protected environment and a least-privilege submission identity. Never execute untrusted PR code with signing credentials, use `pull_request_target` to run submitted source, or accept arbitrary artifact URLs as signing inputs.
6. Verify Authenticode signatures, the expected publisher, SHA256 digest algorithm and trusted timestamp after signing. Regenerate checksums from the signed files. Publish signed assets only with explicit release approval; do not silently replace existing assets.

## Private keys and third-party components

Private keys must remain with the approved hardware-backed signing provider. Do not put keys, PFX files, tokens, passwords or credentials in Git, artifacts, job logs or chat. Credentials needed later must be entered directly into protected GitHub/environment secrets by the owner, not supplied to the assistant.

Sign only the project's own reviewed binaries and installer under its approved policy. Do not re-sign Microsoft Visual C++ redistributables or upstream libraries with the project's certificate. Preserve their original signatures, licenses and notices. A future inner-binary-signing stage must distinguish these files explicitly before packaging and signing the outer installer.

## Proposed SignPath Foundation path

Apply at <https://signpath.org/apply> and review <https://signpath.org/terms>. Acceptance is discretionary and includes reputation, repository control, licensing, fork/upstream and build-origin requirements. The service is free only for accepted projects. Under that program the certificate publisher is SignPath Foundation, not a personal certificate issued to Pedro or OpenCredential.

After written approval and actual integration, add the required acknowledgment to the home/download pages: “Free code signing provided by [SignPath.io](https://about.signpath.io), certificate by [SignPath Foundation](https://signpath.org)”. Do not present that acknowledgment as a current sponsorship before acceptance.

Before activating signing, review the BSD-3-Clause license declaration (GitHub currently detects NOASSERTION), third-party/system-library packaging eligibility, and ProductName/ProductVersion consistency across the own binaries selected for signing. Some core plugins currently have component-specific product names/versions. No license or binary metadata is silently changed by this policy.

## Privacy and limitations

See [Privacy policy](PRIVACY.md). Signing does not certify functionality, security, all supported Windows versions or third-party plugin compatibility. It does not guarantee immediate Microsoft SmartScreen reputation. If signing credentials, build integrity or releases are suspected of compromise, stop signing/publication and investigate; request provider revocation where appropriate, with explicit owner approval for destructive actions.

Operational preparation and remaining steps: [docs/signing/README.md](docs/signing/README.md).
