## Summary

What this changes and why. Assume the reviewer has the diff — explain the reasoning and the blast radius, not the mechanics.

## Changes

- Change 1
- Change 2

## Type of Change

- [ ] Bug fix (non-breaking change that fixes an issue)
- [ ] New feature (non-breaking change that adds functionality)
- [ ] Breaking change (fix or feature that would change existing behaviour)
- [ ] Documentation update
- [ ] Refactoring (no functional changes)

## Testing

- [ ] Unit tests added or updated
- [ ] `dotnet build TaskbarFolders.sln -c Release` — clean
- [ ] `dotnet test TaskbarFolders.sln -c Release` — green
- [ ] `dotnet format TaskbarFolders.sln --verify-no-changes` — clean
- [ ] Ran the application and exercised the change (the test suite is headless and does not open a window)
- [ ] Checked `%APPDATA%\TaskbarFolders\logs\` afterwards for warnings with no visible counterpart

State what you actually verified, and say plainly what you could not. "Tests pass" and "I used the feature" are different claims.

## Checklist

- [ ] Follows the project's conventions (see [CONTRIBUTING.md](../CONTRIBUTING.md))
- [ ] Self-reviewed the diff
- [ ] Documentation updated for any behaviour change
- [ ] `CHANGELOG.md` entry added under `[Unreleased]`
- [ ] An [ADR](../docs/adr/README.md) added, if this decides something that constrains future work
- [ ] No new warnings — the build treats them as errors

## Release-affecting changes only

- [ ] Version bumped in all five places (see [release-process.md](../docs/release-process.md))
- [ ] Installer or portable smoke test performed, or explicitly noted as not performed
