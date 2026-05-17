using System;
using System.Runtime.InteropServices;

namespace TaskbarFolders.Core.Interop;

/// <summary>
/// Marshalled equivalent of the Win32 <c>SHFILEINFOW</c> struct
/// passed by reference to <see cref="NativeMethods.SHGetFileInfo"/>.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct SHFILEINFO
{
    public IntPtr hIcon;
    public int iIcon;
    public uint dwAttributes;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string szDisplayName;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
    public string szTypeName;
}
