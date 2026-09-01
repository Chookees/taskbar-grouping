# ADR-003: Device pixels at the Win32 boundary, one conversion in placement

## Status

Accepted (v0.4.3).

## Context

Placing the popup means mixing two coordinate systems. Win32 gives physical **device pixels**: `SHAppBarMessage(ABM_GETTASKBARPOS)` for the taskbar rectangle and edge, `GetCursorPos` for the click position, `GetMonitorInfo` for monitor and work-area bounds. WPF positions windows in **device-independent pixels** — device pixels divided by the monitor's scale factor.

Up to v0.4.2 the two were mixed. Raw device-pixel rectangles and the raw cursor position were assigned straight into WPF coordinates. At 100 % scaling the systems coincide and everything looked correct; at 150 % the popup landed roughly a third of the screen away from the tile that opened it, and a click near the right edge could place it off-screen entirely.

The trap is that this class of bug is invisible on the developer's monitor if that monitor runs at 100 %, and the mixed values are individually plausible everywhere — nothing in a `double` says which unit it is in.

## Decision

**Everything crossing the Win32 boundary is in device pixels.** That includes taskbar and monitor rectangles, the cursor position, and the seeded cursor anchor.

**Conversion to DIPs happens exactly once**, inside placement calculation, using the effective DPI of the monitor under the cursor obtained from `GetDpiForMonitor`.

Two ordering rules follow and are equally binding:

- Per-monitor-V2 DPI awareness is enabled **before** the cursor position is captured, so the captured coordinates are genuinely physical rather than virtualised by the system.
- Values are not pre-converted before being handed to the anchor, and DIP values are never passed into placement's Win32 parameters.

## Alternatives considered

**Convert at each Win32 call site.** Rejected: it multiplies the number of places that need to know the monitor's DPI, and every new call site is another chance to convert twice or not at all.

**Work entirely in device pixels and convert only when assigning `Left`/`Top`.** Nearly equivalent, and rejected only because placement already needs the monitor DPI for clamping against the work area; concentrating the conversion there keeps one function responsible for the whole unit story.

**Rely on WPF's DPI handling.** Not applicable — the geometry never passes through a WPF visual, so nothing in the framework knows to scale it.

## Consequences

- A single function owns the unit conversion. Reviewing placement for unit errors means reviewing one function.
- Any new Win32 geometry source feeds in unconverted. Helpfully converting it first is the specific mistake this record exists to prevent, and it reintroduces the pre-v0.4.3 drift.
- The contract must be stated wherever the boundary is crossed — the anchor abstraction carries device pixels, the placement result carries DIPs, and both say so in their documentation.
- Placement is covered by tests at 150 % scaling, including the off-screen right-edge case, so a regression fails the build rather than waiting for a bug report from a user on a scaled display.
