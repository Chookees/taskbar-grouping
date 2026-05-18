namespace TaskbarFolders.Core.Shortcuts;

/// <summary>
/// Notifies the Windows Shell that a file system entry has changed so dependent indexes
/// (notably the AppsFolder index consulted by <c>TaskbarManager.RequestPinCurrentAppAsync</c>)
/// pick the entry up immediately instead of racing the background indexer.
/// </summary>
/// <remarks>
/// Wraps the unmanaged <c>SHChangeNotify</c> Shell API so the call site stays testable
/// without executing P/Invoke in unit tests. The default implementation is wired up via DI;
/// tests substitute a recording stub.
/// </remarks>
public interface IShellChangeNotifier
{
    /// <summary>
    /// Tells the Shell that a new file was created at <paramref name="path"/>. Uses
    /// <c>SHCNE_CREATE</c> with <c>SHCNF_FLUSH</c> so the call blocks until pending shell
    /// notifications have been delivered.
    /// </summary>
    /// <param name="path">Absolute path of the newly created file.</param>
    void NotifyCreate(string path);
}
