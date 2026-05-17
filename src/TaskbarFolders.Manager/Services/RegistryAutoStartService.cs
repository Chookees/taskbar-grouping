using System;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace TaskbarFolders.Manager.Services;

/// <summary>
/// Default <see cref="IAutoStartService"/> backed by
/// <c>HKCU\Software\Microsoft\Windows\CurrentVersion\Run</c>. Per-user, no elevation required.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class RegistryAutoStartService : IAutoStartService
{
    /// <summary>Sub-key under <see cref="Registry.CurrentUser"/> containing per-user run entries.</summary>
    public const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>Value name used by the Manager run entry.</summary>
    public const string ValueName = "TaskbarFolders";

    /// <inheritdoc/>
    public bool IsEnabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
            return key?.GetValue(ValueName) is not null;
        }
    }

    /// <inheritdoc/>
    public void Enable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true)
            ?? Registry.CurrentUser.CreateSubKey(RunKey, writable: true);

        var exePath = Environment.ProcessPath
            ?? throw new InvalidOperationException("Could not determine the current process path.");

        // Embedded quote would break the Run-key parser. Symlink targets on NTFS can
        // contain double-quotes, so reject explicitly rather than corrupt the registry.
        if (exePath.Contains('"', StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Process path '{exePath}' contains a double-quote, which cannot be safely written to the Run key.");
        }

        // Quote the value so Windows handles paths with spaces correctly.
        key.SetValue(ValueName, $"\"{exePath}\"");
    }

    /// <inheritdoc/>
    public void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
        key?.DeleteValue(ValueName, throwOnMissingValue: false);
    }
}
