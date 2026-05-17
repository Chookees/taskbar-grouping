namespace TaskbarFolders.Manager.Services;

/// <summary>
/// Asks the user to confirm a destructive action. Abstracted from
/// <see cref="System.Windows.MessageBox"/> so view models stay testable.
/// </summary>
public interface IUserConfirmation
{
    /// <summary>Shows a confirmation prompt.</summary>
    /// <param name="caption">Window title.</param>
    /// <param name="message">Body text.</param>
    /// <returns><see langword="true"/> if the user confirmed; otherwise <see langword="false"/>.</returns>
    bool Confirm(string caption, string message);
}
