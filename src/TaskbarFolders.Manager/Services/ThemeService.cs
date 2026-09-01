using System;
using System.Runtime.Versioning;
using System.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using TaskbarFolders.Shared.Models;

namespace TaskbarFolders.Manager.Services;

/// <summary>
/// Default <see cref="IThemeService"/>. Swaps the active <see cref="ResourceDictionary"/>
/// in <see cref="Application.Resources"/> and re-applies on
/// <see cref="SystemEvents.UserPreferenceChanged"/> when the user selected
/// <see cref="ThemePreference.System"/>.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class ThemeService : IThemeService, IDisposable
{
    private const string LightDictionaryUri = "/TaskbarFolders.Manager;component/Themes/Light.xaml";
    private const string DarkDictionaryUri = "/TaskbarFolders.Manager;component/Themes/Dark.xaml";

    private readonly ISystemThemeProbe _probe;
    private readonly ILogger<ThemeService>? _logger;
    private ResourceDictionary? _currentDictionary;
    private bool _systemListenerWired;

    // EffectiveTheme only ever resolves to Light or Dark, so System doubles as a "nothing
    // applied yet" sentinel: the first apply always counts as a change.
    private ThemePreference _lastRaisedTheme = ThemePreference.System;
    private bool _disposed;

    /// <summary>Initializes a new instance.</summary>
    public ThemeService(ISystemThemeProbe probe, ILogger<ThemeService>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(probe);

        _probe = probe;
        _logger = logger;
    }

    /// <inheritdoc/>
    public event EventHandler? ThemeChanged;

    /// <inheritdoc/>
    public ThemePreference Preference { get; private set; } = ThemePreference.System;

    /// <inheritdoc/>
    public ThemePreference EffectiveTheme => Preference == ThemePreference.System
        ? (_probe.IsLightMode ? ThemePreference.Light : ThemePreference.Dark)
        : Preference;

    /// <inheritdoc/>
    public void SetPreference(ThemePreference preference)
    {
        Preference = preference;
        ApplyCurrent();

        if (preference == ThemePreference.System)
        {
            WireSystemListener();
        }
        else
        {
            UnwireSystemListener();
        }

        _logger?.LogInformation("Theme preference set to {Preference} (effective: {Effective}).", Preference, EffectiveTheme);
    }

    private void ApplyCurrent()
    {
        // Raised even when Application.Current is null (unit tests) so subscribers can be
        // exercised headlessly; the dictionary swap below is what needs a live Application.
        var effective = EffectiveTheme;
        var changed = effective != _lastRaisedTheme;
        _lastRaisedTheme = effective;

        try
        {
            ApplyDictionary(effective);
        }
        finally
        {
            if (changed)
            {
                ThemeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void ApplyDictionary(ThemePreference effective)
    {
        // Application.Current is null during unit tests — be defensive.
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        var uri = effective == ThemePreference.Dark
            ? new Uri(DarkDictionaryUri, UriKind.Relative)
            : new Uri(LightDictionaryUri, UriKind.Relative);

        var merged = app.Resources.MergedDictionaries;

        // SystemEvents.UserPreferenceChanged fires for many categories beyond app theme
        // (accent colour, regional, mouse settings). Skip the dictionary swap when the
        // resolved URI hasn't actually changed AND our cached instance is still merged —
        // a third-party theme swap that removed our dictionary would otherwise leave the
        // window unstyled if we short-circuited on URI alone.
        if (_currentDictionary is { Source: { } currentSource }
            && currentSource == uri
            && merged.Contains(_currentDictionary))
        {
            return;
        }

        var newDict = new ResourceDictionary { Source = uri };

        if (_currentDictionary is not null)
        {
            merged.Remove(_currentDictionary);
        }

        merged.Insert(0, newDict);
        _currentDictionary = newDict;
    }

    private void WireSystemListener()
    {
        if (_systemListenerWired)
        {
            return;
        }
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        _systemListenerWired = true;
    }

    private void UnwireSystemListener()
    {
        if (!_systemListenerWired)
        {
            return;
        }
        SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
        _systemListenerWired = false;
    }

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category != UserPreferenceCategory.General)
        {
            return;
        }

        // Must run on the UI thread because Application.Resources are dispatcher-affine.
        // BeginInvoke is fire-and-forget — a theme repaint has no return value and the
        // synchronous Invoke could deadlock if the UI thread is blocked on anything that
        // ultimately waits on the SystemEvents thread.
        var app = Application.Current;
        app?.Dispatcher.BeginInvoke(new Action(ApplyCurrent));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        UnwireSystemListener();
        _disposed = true;
    }
}
