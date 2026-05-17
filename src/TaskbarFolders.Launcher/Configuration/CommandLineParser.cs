using System;

namespace TaskbarFolders.Launcher.Configuration;

/// <summary>
/// Parses launcher command-line arguments into strongly typed options.
/// </summary>
public static class CommandLineParser
{
    /// <summary>
    /// Name of the argument that carries the target group identifier.
    /// </summary>
    public const string GroupIdArg = "--group-id";

    /// <summary>
    /// Attempts to extract the group identifier from the supplied argument vector.
    /// </summary>
    /// <param name="args">Raw arguments as received from <see cref="System.Windows.StartupEventArgs.Args"/>.</param>
    /// <returns>The group identifier or <see langword="null"/> if absent or empty.</returns>
    public static string? TryParseGroupId(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], GroupIdArg, StringComparison.OrdinalIgnoreCase))
            {
                var value = args[i + 1];
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }

        return null;
    }
}
