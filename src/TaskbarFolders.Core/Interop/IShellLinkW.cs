using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TaskbarFolders.Core.Interop;

/// <summary>
/// Full Unicode <c>IShellLinkW</c> COM interface declaration. The vtable layout matters
/// for COM interop — every method up to and including the ones we call must appear in
/// source order. Pinvoke.net and the official Windows SDK <c>shobjidl_core.h</c> agree
/// on this ordering.
/// </summary>
[ComImport]
[Guid("000214F9-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IShellLinkW
{
    void GetPath(
        [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
        int cchMaxPath,
        IntPtr pfd,
        uint fFlags);

    void GetIDList(out IntPtr ppidl);

    void SetIDList(IntPtr pidl);

    void GetDescription(
        [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName,
        int cchMaxName);

    void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

    void GetWorkingDirectory(
        [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir,
        int cchMaxPath);

    void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

    void GetArguments(
        [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs,
        int cchMaxPath);

    void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

    void GetHotkey(out short pwHotkey);

    void SetHotkey(short wHotkey);

    void GetShowCmd(out int piShowCmd);

    void SetShowCmd(int iShowCmd);

    void GetIconLocation(
        [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
        int cchIconPath,
        out int piIcon);

    void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

    void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);

    void Resolve(IntPtr hwnd, uint fFlags);

    void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
}

/// <summary>
/// Subset of <c>IPersistFile</c> needed to call <c>Load</c>. Method order matches
/// the underlying interface: <c>IPersist::GetClassID</c> then the IPersistFile methods.
/// </summary>
[ComImport]
[Guid("0000010B-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPersistFile
{
    void GetClassID(out Guid pClassID);

    [PreserveSig]
    int IsDirty();

    void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);

    void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);

    void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

    void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
}

/// <summary>
/// CoClass that exposes <see cref="IShellLinkW"/> and <see cref="IPersistFile"/>.
/// Instantiate with <c>new ShellLink()</c> then cast to whichever interface is needed.
/// </summary>
[ComImport]
[Guid("00021401-0000-0000-C000-000000000046")]
internal class ShellLink
{
}
