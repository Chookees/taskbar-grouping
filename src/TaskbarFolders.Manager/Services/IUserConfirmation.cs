namespace TaskbarFolders.Manager.Services;

/// <summary>
/// Asks the user to confirm a destructive action or surfaces a one-button information dialog.
/// Abstracted from <see cref="System.Windows.MessageBox"/> so view models stay testable.
/// </summary>
public interface IUserConfirmation
{
    /// <summary>Shows a confirmation prompt.</summary>
    /// <param name="caption">Window title.</param>
    /// <param name="message">Body text.</param>
    /// <returns><see langword="true"/> if the user confirmed; otherwise <see langword="false"/>.</returns>
    bool Confirm(string caption, string message);

    /// <summary>Shows an information-only dialog with a single OK button. Returns when dismissed.</summary>
    /// <param name="caption">Window title.</param>
    /// <param name="message">Body text.</param>
    void Notify(string caption, string message);
}
