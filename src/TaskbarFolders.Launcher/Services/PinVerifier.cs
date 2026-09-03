using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using TaskbarFolders.Core.Shortcuts;

namespace TaskbarFolders.Launcher.Services;

/// <summary>
/// Answers "did a tile for this group actually appear on the taskbar?" by looking for a
/// pinned shortcut carrying the group's AppUserModelID.
/// </summary>
/// <remarks>
/// <para>
/// <c>TaskbarManager.RequestPinCurrentAppAsync</c> returning <see langword="true"/> is not
/// proof that anything was pinned — it has been observed to report success while persisting
/// nothing (see the v0.4.2 CHANGELOG entry). Believing it produced a "Pinned" notification
/// with no tile behind it, which is indistinguishable from the app being broken.
/// </para>
/// <para>
/// Matching is by AUMID rather than by file name on purpose. Windows copies the shortcut it
/// resolved into its pinned-items folder under a name of its own choosing — a hand-pinned
/// group keeps the <c>&lt;id&gt;.lnk</c> name from our shortcuts folder, while a programmatic
/// pin resolves through the Start-menu anchor and arrives under the group's display name.
/// The AUMID is the only stable identity across both routes.
/// </para>
/// <para>
/// The pinned-items folder is a shell implementation detail, so every failure to inspect it is
/// treated as "cannot tell" rather than "not pinned". A diagnostic must never invent a failure.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed class PinVerifier
{
    private readonly IShortcutReader _reader;
    private readonly ILogger<PinVerifier>? _logger;

    /// <summary>Initializes a new instance.</summary>
    /// <param name="reader">Reads the AppUserModelID off a shortcut.</param>
    /// <param name="logger">Optional logger.</param>
    public PinVerifier(IShortcutReader reader, ILogger<PinVerifier>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(reader);

        _reader = reader;
        _logger = logger;
    }

    /// <summary>
    /// Directory Windows keeps pinned taskbar shortcuts in, for the current user.
    /// </summary>
    public static string PinnedItemsDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Microsoft",
        "Internet Explorer",
        "Quick Launch",
        "User Pinned",
        "TaskBar");

    /// <summary>
    /// Looks for a pinned shortcut stamped with <paramref name="aumid"/>.
    /// </summary>
    /// <param name="aumid">The group's AppUserModelID.</param>
    /// <returns>
    /// <see langword="true"/> when a matching pinned shortcut was found, <see langword="false"/>
    /// when the folder was readable and contained none, and <see langword="null"/> when the
    /// folder could not be inspected at all — the caller must not treat that as a failed pin.
    /// </returns>
    public bool? IsPinned(string aumid)
    {
        if (string.IsNullOrWhiteSpace(aumid))
        {
            return null;
        }

        var directory = PinnedItemsDirectory;

        try
        {
            if (!Directory.Exists(directory))
            {
                _logger?.LogInformation("Pin verification skipped: {Directory} does not exist.", directory);
                return null;
            }

            var shortcuts = Directory.EnumerateFiles(directory, "*.lnk").ToList();
            var match = shortcuts.FirstOrDefault(
                path => string.Equals(_reader.TryReadAumid(path), aumid, StringComparison.OrdinalIgnoreCase));

            if (match is not null)
            {
                _logger?.LogInformation(
                    "Pin verified: {File} carries {Aumid}.", Path.GetFileName(match), aumid);
                return true;
            }

            _logger?.LogWarning(
                "Pin not verified: none of the {Count} pinned shortcut(s) in {Directory} carries {Aumid}.",
                shortcuts.Count, directory, aumid);
            return false;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            _logger?.LogWarning(ex, "Pin verification could not read {Directory}; treating as inconclusive.", directory);
            return null;
        }
    }
}
