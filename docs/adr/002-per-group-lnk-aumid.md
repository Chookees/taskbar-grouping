# ADR-002: Per-group shortcut with a distinct AppUserModelID

## Status

Accepted. Supersedes the "standalone executable per group" premise in [ADR-001](001-wpf-over-winui.md); that record's WPF decision is unaffected.

## Context

Each group has to appear on the taskbar as its own tile, with its own icon, that opens that group's popup when clicked.

Windows groups taskbar buttons by **AppUserModelID**. Two processes sharing an AUMID share a tile; two identities get two tiles. So the question is not "how do we get a second executable onto the taskbar" but "how do we give one executable several shell identities".

Two designs were prototyped during the M5 spike.

## Decision

Ship **one** `TaskbarFolders.Launcher.exe` and give each group its own `.lnk` in `%APPDATA%\TaskbarFolders\shortcuts\` that

- targets that shared executable,
- passes `--group-id "<id>"`,
- points its icon at the group's generated `.ico`,
- and carries `PKEY_AppUserModel_ID` = `TaskbarFolders.Group.<id>`, written through `IPropertyStore`.

The launcher reads the group from `--group-id`, falling back to the AUMID Windows assigned the process when it is started without arguments.

A matching shortcut is also written to the Start menu and announced with `SHChangeNotify`, because `TaskbarManager.RequestPinCurrentAppAsync` will not reliably persist a pin for an AUMID the shell's AppsFolder index has not seen.

## Alternatives considered

**A copy of the launcher executable per group, with the icon written into the PE.** Copy `TaskbarFolders.Launcher.exe` per group, stamp the composite icon into its resources with `BeginUpdateResource`/`UpdateResource`, embed the group id, and pin the copy directly.

Rejected. The result is an unsigned executable, freshly written at runtime, in a per-user directory, with modified resources — a combination that reads as packer behaviour to heuristic scanners. The spike produced Defender false positives. Shipping a tool whose normal operation manufactures executables that antivirus software may quarantine is not a defensible trade for an icon.

It is also more expensive in every other dimension: an executable per group instead of a shortcut per group, every group needing a rewrite when the launcher is updated, and no way to fix a launcher bug without regenerating every group's binary.

**A single always-running host process with one tile.** One tile, one process, groups selected inside the popup. Rejected because it defeats the purpose — the point is several tiles sitting on the taskbar, each opening straight into its group.

## Consequences

- Updating the launcher updates every group at once. There is nothing per-group to regenerate on upgrade.
- The pinned artefact is a shortcut, so the user-facing manual fallback is the familiar right-click → *Pin to taskbar* on a `.lnk`.
- Group ids must be valid inside an AUMID. They are constrained to `^[A-Za-z0-9._-]{1,96}$`, which keeps the full AUMID inside its 128-character limit and simultaneously blocks path traversal in the derived file names.
- The Start menu anchor is a hard dependency of pinning, not a nicety. Deleting it breaks one-click pinning for that group until the Manager heals it on next startup.
- The launcher must tolerate being started with no arguments, because Windows launches a pinned tile through the shell identity rather than the command line.
- Each group costs three small files (config, icon, shortcut) plus one Start menu entry, instead of one large executable.
