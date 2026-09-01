# ADR-001: WPF over WinUI 3

## Status

Accepted.

One premise below did not survive contact with implementation. The context and
rationale originally assumed each group would ship as its own executable; the
M5 spike rejected that approach, and grouping is now done with a shared
launcher plus per-group shortcuts. Those two points are corrected in place and
recorded in [ADR-002](002-per-group-lnk-aumid.md). The decision to build on WPF
rather than WinUI 3 is unaffected and still holds.

## Context

TaskbarFolders requires deep integration with the Windows taskbar, including:
- Custom window positioning near the taskbar
- Transparent, borderless popup windows with blur effects
- Icon manipulation via Win32 Shell APIs (P/Invoke)
- Multi-monitor and DPI awareness
- Giving every group its own taskbar identity and icon

We evaluated WPF and WinUI 3 as the UI framework.

## Decision

We chose **WPF** (Windows Presentation Foundation) over WinUI 3.

## Rationale

### WPF Advantages

1. **Win32 Interop**: WPF has mature, well-documented Win32 interop via `HwndSource` and P/Invoke. Taskbar positioning, icon extraction, and window manipulation are straightforward.

2. **Transparent Windows**: `AllowsTransparency="True"` with `WindowStyle="None"` works reliably for custom popup shapes with rounded corners and blur effects.

3. **No MSIX Requirement**: WPF apps can be distributed as plain .exe files. This matters because the taskbar integration is built on ordinary shortcuts and AppUserModelIDs rather than a packaged identity, which an MSIX-only story would have forced us into.

4. **Mature Ecosystem**: Extensive community resources, StackOverflow answers, and battle-tested patterns for MVVM, DI, and custom controls.

5. **Self-Contained Deployment**: `dotnet publish` with `--self-contained` produces a single .exe without requiring WinUI runtime installation.

### WinUI 3 Limitations

1. **MSIX Packaging**: While unpackaged WinUI 3 apps are possible, many features require MSIX packaging, which conflicts with our standalone .exe requirement.

2. **Win32 Interop Maturity**: WinUI 3's interop story is still evolving. Custom window chrome, transparent windows, and taskbar integration are more complex.

3. **Runtime Dependency**: WinUI 3 requires the Windows App SDK runtime, adding an installation dependency.

4. **Smaller Ecosystem**: Fewer community resources and proven patterns compared to WPF.

## Consequences

- We commit to .NET 8+ with WPF for all UI components
- We accept that WPF is Windows-only (acceptable for a Windows taskbar tool)
- We gain reliable Win32 interop for all taskbar-related functionality
- We can distribute as standalone .exe files without packaging constraints
