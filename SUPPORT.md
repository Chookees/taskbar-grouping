# Support

## Before opening an issue

1. Check the [Troubleshooting guide](docs/troubleshooting.md). It covers the common symptoms — a tile that does nothing, a pin that produces no tile, a popup in the wrong place — and lists the current known limitations, several of which look like bugs but are not.
2. Read your logs. `%APPDATA%\TaskbarFolders\logs\` holds one file per day; the launcher records why it gave up, including its exit code.
3. Check the [CHANGELOG](CHANGELOG.md) and the [latest release](https://github.com/eXORR6077/taskbar-grouping/releases) — the problem may already be fixed.

## Reporting a bug

Use the [bug report template](https://github.com/eXORR6077/taskbar-grouping/issues/new/choose) and include:

- your Windows edition, version and build number (`winver`),
- the TaskbarFolders version,
- your display scaling and monitor arrangement — a surprising number of issues are specific to one or the other,
- and the relevant excerpt from `manager-*.log` or `launcher-*.log`.

The logs are the single most useful thing you can attach. A screenshot rarely identifies a cause; a log line usually does.

## Requesting a feature

Use the [feature request template](https://github.com/eXORR6077/taskbar-grouping/issues/new/choose). Describing the problem you are trying to solve is more useful than describing the solution you have in mind — there may be a better one, or one that already exists.

## Security vulnerabilities

Do **not** open a public issue. See [SECURITY.md](SECURITY.md).

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for the branching model, commit conventions and the checks that have to pass, and the [Developer Guide](docs/developer-guide.md) for building and debugging.

## Expectations

This is a solo-maintained project. Issues are read, but responses are best-effort and there is no support commitment attached to any release.
