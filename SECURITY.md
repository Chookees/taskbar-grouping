# Security Policy

## Supported versions

| Version | Supported |
|---|---|
| 0.4.x | Yes |
| < 0.4 | No |

This project is pre-1.0 and solo-maintained. Only the latest release line receives fixes; older versions are not patched. Upgrade to the current release before reporting a problem.

## Reporting a vulnerability

**Do not open a public issue.**

Report privately to `<SECURITY-CONTACT>`.

> **Maintainer:** replace the placeholder above with a real address before this repository is made public. GitHub's private vulnerability reporting is an alternative, but it is not available for this repository on its current plan and visibility — enable it first if you would rather use it, and then point this section at it instead.

Please include:

- what the issue is and what an attacker could achieve with it,
- the affected version,
- steps to reproduce, or a proof of concept,
- and any relevant excerpt from `%APPDATA%\TaskbarFolders\logs\`.

## What to expect

Acknowledgement as soon as the report is seen, an assessment once it has been reproduced, and a fix in a patch release when one is warranted. This is a single-maintainer project with no on-call rotation — there is no guaranteed response time, and a fix may take days rather than hours. Please allow a reasonable window before disclosing publicly.

## Scope

The TaskbarFolders application and everything in this repository.

Worth knowing when assessing impact:

- Both executables run as the invoking user. Neither requests elevation; only the installer does, because it writes to Program Files.
- All application data lives under the user's own `%APPDATA%\TaskbarFolders\`. Group ids are validated against `^[A-Za-z0-9._-]{1,96}$` before any path is derived from them, which is what keeps a crafted configuration from escaping that directory.
- The application launches whatever executables a user has added to a group, using the shell. It is a launcher by design; that a group can be configured to start an arbitrary program is intended behaviour, not a vulnerability. A way to make it start something the user did not configure would be.
- Autostart is a per-user `HKCU\…\Run` entry. Nothing is written to `HKLM` after installation.

## Not in scope

- Vulnerabilities in Windows itself or in the .NET runtime — report those to their vendors.
- Issues that require an attacker to already have write access to the user's `%APPDATA%` or their account.

## Code scanning

CodeQL runs against this repository on a schedule, but results are not uploaded to GitHub code scanning: that requires Advanced Security, which is unavailable for a private repository on the current plan. The workflow instead fails on any finding and keeps the results as a build artifact. There is therefore no public code-scanning alert feed for this project.
