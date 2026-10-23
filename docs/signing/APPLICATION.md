# SignPath Foundation — application draft

**Not submitted.** The owner must review this text, complete identity/account checks directly, and accept any terms personally at <https://signpath.org/apply>. No provider acceptance or certificate is currently available.

## Project description (suggested text)

OpenCredential is an open-source Windows Credential Provider and Windows service with managed authentication, authorization, local-account gateway and session-logging plugins. It supports MySQL/MariaDB and PostgreSQL, LDAP and local Windows accounts, with optional machine-protected offline authentication and deferred logging.

The project is an unofficial fork of pGina (<https://github.com/pgina/pgina>), not an upstream-endorsed product. Our changes include modern database integration, .NET Framework 4.8, offline protections, PostgreSQL support, installer preservation and fixes for RDP credential handling. The code affects Windows sign-in and may create/synchronize local accounts and groups when the administrator enables the corresponding gateway. It installs a Windows service and credential provider and includes an uninstaller. It is not a kernel driver.

We request evaluation for free Authenticode signing of reviewed project binaries and installers. We do not request a certificate for signing arbitrary external files or re-signing Microsoft redistributables/third-party libraries. We understand acceptance is discretionary and that the Foundation would be the certificate publisher under its program.

## Public references

- Repository: <https://github.com/pedropablobm/OpenCredential>
- Existing prerelease: <https://github.com/pedropablobm/OpenCredential/releases/tag/v1.0.1.0-rc.1> (unsigned).
- License and attribution: repository `LICENSE`, `NOTICE` and `Installer/OpenCredential-License.txt`; BSD-3-Clause text with fork attribution. GitHub's last observed license detection was `NOASSERTION`; provider review is still required.
- Proposed approval policy: `CODE_SIGNING.md`.
- Privacy draft: `PRIVACY.md`.
- Proposed build workflow: `.github/workflows/desktop-unsigned.yml` (GitHub-hosted Windows, exact commit, pinned actions, manual release/signing approval required in the future).
- Authorized signing approver: Pedro Pablo Bermudez Medina, GitHub `@pedropablobm`.

These newly prepared files must be published with owner approval before they can be offered as public links. Do not claim the hosted workflow is passing until an actual run has completed.

## Confirm before submitting

- [ ] Owner reviewed the signing/privacy policies and approved publishing them.
- [ ] CI configuration is published and a successful hosted build/run URL is available.
- [ ] Owner confirms MFA for every signing participant; do not infer MFA from an authenticated GitHub CLI session.
- [ ] Verify all bundled dependency licenses and provider eligibility, including Microsoft system redistributables and legacy/fork components.
- [ ] Provider reviewed fork/upstream and license-detection concerns; do not change the license unilaterally.
- [ ] Confirm which own binaries may be signed and resolve their component-specific product/version metadata before activation.
- [ ] Configure effective manual approval and branch/environment controls; CODEOWNERS alone is not enforcement.

No tokens, passwords, private keys or private identity documents belong in this file, public issues, artifacts or chat. The provider's live form may request different fields; this is supporting text, not an exact reproduction of that form.
