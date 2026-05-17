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
    private bool _disposed;

    /// <summary>Initializes a new instance.</summary>
    public ThemeService(ISystemThemeProbe probe, ILogger<ThemeService>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(probe);

        _probe = probe;
        _logger = logger;
    }

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
        // Application.Current is null during unit tests — be defensive.
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        var uri = EffectiveTheme == ThemePreference.Dark
            ? new Uri(DarkDictionaryUri, UriKind.Relative)
            : new Uri(LightDictionaryUri, UriKind.Relative);

        var newDict = new ResourceDictionary { Source = uri };
        var merged = app.Resources.MergedDictionaries;

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
        var app = Application.Current;
        app?.Dispatcher.Invoke(ApplyCurrent);
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
