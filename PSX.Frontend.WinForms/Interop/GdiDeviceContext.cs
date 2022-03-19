using System;

namespace PSX.Frontend.WinForms.Interop;

internal readonly ref struct GdiDeviceContext
{
    private readonly IntPtr _handle;
    private readonly IntPtr _hdc;

    public GdiDeviceContext(IntPtr handle)
    {
        _handle = handle;
        _hdc    = NativeMethods.GetDC(handle);
    }

    public void Dispose()
    {
        NativeMethods.ReleaseDC(_handle, _hdc);
    }

    public static implicit operator IntPtr(GdiDeviceContext dc)
    {
        return dc._hdc;
    }
}