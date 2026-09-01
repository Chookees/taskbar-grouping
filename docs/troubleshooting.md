# Troubleshooting

Start with the logs. Almost everything below is diagnosable from them.

## Reading the logs

```
%APPDATA%\TaskbarFolders\logs\
```

One file per day per process: `manager-<date>.log` for the Manager, `launcher-<date>.log` for anything started by clicking a tile or pinning. Files are kept for 14 days. Each line is

```
<ISO-8601 UTC timestamp> [<Level>] <Category>: <message>
```

with an exception, if there was one, on the following line. Only `Information` and above are written.

When reporting a problem, attach the relevant `launcher-*.log` and `manager-*.log` — they are far more useful than a screenshot.

### Launcher exit codes

The launcher records why it gave up before it exits.

| Mode | Code | Meaning |
|---|---|---|
| Popup | 1 | No group id — neither `--group-id` nor a usable AppUserModelID |
| Popup | 3 | Startup threw; the exception is in the log |
| Popup | 4 | Unhandled exception after startup |
| Pin | 0 | Pinned |
| Pin | 1 | You declined the Windows dialog |
| Pin | 2 | Pinning unavailable on this edition, policy or Windows version |
| Pin | 3 | Pinning failed unexpectedly |

## Clicking a pinned tile does nothing

A brief busy cursor and then nothing means the launcher started and exited.

1. Open today's `launcher-*.log`.
2. Look for the most recent shutdown line and its exit code.

- **Exit code 1** — the shortcut lost its AppUserModelID, or was created by hand without one. Open the Manager, select the group, and click **Pin to taskbar** again, or re-pin from **Show shortcut…**; both regenerate the shortcut.
- **Exit code 3** — startup failed and the exception is logged. If the group's JSON has been hand-edited, check that it is still valid JSON.
- **No new lines at all** — the launcher never ran. The pinned shortcut may point at a launcher that has moved: this happens if a portable install was reorganised so that `Manager\` and `Launcher\` are no longer siblings. Restore the layout and re-pin.

If the popup opens but is invisible, check the *Enable popup animations* setting — turning animations off removes the animation path entirely and is a useful way to isolate that.

## "Pin to taskbar" reports that pinning is unavailable

Exit code 2. Programmatic pinning needs Windows 10 version 2004 or newer, and some editions and managed environments block it outright.

Use the manual path: the Manager opens the shortcut folder for you, or click **Show shortcut…**. Right-click the `.lnk` → **Show more options** (Windows 11 22H2 and later) → **Pin to taskbar**.

## Pinning appears to succeed but no tile appears

Windows only persists a programmatic pin when the AppUserModelID is already known to the Start menu index. TaskbarFolders writes a Start menu entry for every group and notifies the shell, but a slow or busy indexer can still lose the race.

1. Close and reopen the Manager. On startup it re-checks and repairs every group's Start menu entry.
2. Try **Pin to taskbar** again.
3. Check that `%APPDATA%\Microsoft\Windows\Start Menu\Programs\TaskbarFolders\` contains an entry for the group. The pin-time log lines in `launcher-*.log` list exactly what that folder contained when the attempt was made.

If it still fails, pin manually via **Show shortcut…** — the result is identical.

## Nothing is generated when I add an app

No icon, no shortcut, and **Show shortcut…** opens nothing.

- **The group has no apps.** An empty group deliberately produces no icon and no shortcut. Add at least one.
- **Every icon extraction failed.** If none of the apps yielded an icon, generation stops rather than producing an iconless tile. Check the paths still exist.
- **The launcher binary was not found.** The Manager needs to know where `TaskbarFolders.Launcher.exe` is in order to write a shortcut targeting it. It logs every path it probed at error level — search `manager-*.log` for "Launcher binary not found". In a portable install this means `Manager\` and `Launcher\` are no longer siblings.

## The popup opens in the wrong place

Placement is DPI-sensitive. On mixed-DPI or scaled setups it should still land next to the tile you clicked; if it does not, note the display scaling percentage, the monitor arrangement, and which monitor the taskbar is on, and include them in the report. Placement at scaling other than 100 % was corrected in v0.4.3 — if you are on an older build, upgrade first.

## The popup closes immediately, or will not close

The popup closes when it loses focus, after a successful launch, and via a three-second fallback if Windows never gave it focus at all. A failed launch keeps it open and shows an error strip naming the app.

There are no keyboard shortcuts — Escape does not close it. Click elsewhere.

## My settings keep reverting

**Affects v0.4.4 and earlier.** Opening Settings showed the built-in defaults rather than your saved values, and clicking **Save** wrote those defaults over `settings.json`, including switching autostart off. Fixed after v0.4.4.

On an affected build, avoid clicking Save unless you intend to set every value in the dialog, or edit `%APPDATA%\TaskbarFolders\settings.json` directly with the Manager closed.

## The group icon looks wrong or out of date

- Only the **first four** apps contribute to the composite icon. Reorder is not available in the UI, so change which apps are in the group, or edit the order in the group's JSON with the Manager closed.
- Extracted source icons are cached under `icons\cache\`. Deleting that folder is safe — icons are re-extracted on the next change.
- Windows caches taskbar icons independently. If the file changed but the tile did not, unpin and re-pin.

## Known limitations

Current, deliberate or not-yet-implemented — not bugs to report:

- **The popup has no keyboard support.** No Escape to dismiss, no focused item on open.
- **Theming is partial.** Window surfaces follow the theme; standard controls keep their default Windows appearance, and the Settings dialog is not themed at all.
- **No renaming, reordering, custom icons, or launch arguments** from the interface. `arguments` and the per-group `columns` value (1–6, default 3) exist in the JSON only; a per-group `theme` field is serialised but ignored.
- **Deleting a group does not unpin its tile.** Unpin it yourself.
- **Renaming a group by editing its JSON leaves the old Start menu entry behind.** There is no reconciler yet.
- **Renaming a group's JSON file changes its identity**, orphaning its icon, shortcut and any existing pin.
- **x64 only.** There is no ARM64 build.
- **No single-instance guard.** Nothing stops two Managers running at once; the second may overwrite the first's changes.
- **Uninstalling leaves `%APPDATA%\TaskbarFolders` and the Start menu entries in place.** See the [user guide](user-guide.md#uninstalling) for removing them.

## Still stuck

Open an issue with the [bug report template](https://github.com/eXORR6077/taskbar-grouping/issues/new/choose). Include your Windows version and build, the TaskbarFolders version, your display scaling, your monitor arrangement, and the relevant log excerpt. [SUPPORT.md](../SUPPORT.md) covers where else to ask.
