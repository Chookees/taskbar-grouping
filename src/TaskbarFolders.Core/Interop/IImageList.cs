using System;
using System.Runtime.InteropServices;

namespace TaskbarFolders.Core.Interop;

/// <summary>
/// Partial declaration of the Win32 <c>IImageList</c> COM interface.
/// Methods are declared in vtable order up to and including <c>GetIcon</c> — the only
/// method this project calls. Later vtable entries are intentionally omitted; calling
/// them would crash the host because their slots are not declared here.
/// </summary>
[ComImport]
[Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IImageList
{
    [PreserveSig]
    int Add(IntPtr hbmImage, IntPtr hbmMask, out int pi);

    [PreserveSig]
    int ReplaceIcon(int i, IntPtr hicon, out int pi);

    [PreserveSig]
    int SetOverlayImage(int iImage, int iOverlay);

    [PreserveSig]
    int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);

    [PreserveSig]
    int AddMasked(IntPtr hbmImage, uint crMask, out int pi);

    [PreserveSig]
    int Draw(IntPtr pimldp);

    [PreserveSig]
    int Remove(int i);

    [PreserveSig]
    int GetIcon(int i, uint flags, out IntPtr picon);
}
