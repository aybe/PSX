using System;
using System.Runtime.InteropServices;

namespace ProjectPSX.WinForms.Interop;

internal class GdiBitmap : IDisposable
{
    private readonly IntPtr _oldObject;

    public readonly IntPtr BitmapData;
    public readonly IntPtr BitmapHandle;
    public readonly int    BytesPerPixel = 4;

    public readonly IntPtr DeviceContext;
    public readonly int    Height;

    public readonly int Width;

    private bool _disposed;

    public GdiBitmap(int width, int height)
    {
        Width  = width;
        Height = height;

        DeviceContext = NativeMethods.CreateCompatibleDC(IntPtr.Zero);

        var bitmapHeader = new BitmapInfoHeader
        {
            biSize        = (uint)Marshal.SizeOf<BitmapInfoHeader>(),
            biWidth       = width,
            biHeight      = -height, // negative, top-down bitmap
            biPlanes      = 1,
            biBitCount    = (ushort)(8 * BytesPerPixel),
            biCompression = BitmapCompression.BI_RGB
        };

        var bitmapInfo = new BitmapInfo
        {
            bmiHeader = bitmapHeader
        };

        BitmapHandle = NativeMethods.CreateDIBSection(DeviceContext, in bitmapInfo, ColorUsage.DIB_RGB_COLORS,
            out BitmapData, IntPtr.Zero, 0);

        _oldObject = NativeMethods.SelectObject(DeviceContext, BitmapHandle);
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public unsafe void DrawPixel(int x, int y, int color)
    {
        var pixel = (int*)BitmapData;
        pixel  += x + y * Width;
        *pixel =  color;
    }

    public unsafe int GetPixel(int x, int y)
    {
        var pixel = (int*)BitmapData;
        pixel += x + y * Width;
        return *pixel;
    }

    ~GdiBitmap()
    {
        Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            if (_oldObject != IntPtr.Zero)
            {
                NativeMethods.SelectObject(DeviceContext, _oldObject);
            }

            if (BitmapHandle != IntPtr.Zero)
            {
                NativeMethods.DeleteObject(BitmapHandle);
            }

            if (DeviceContext != IntPtr.Zero)
            {
                NativeMethods.DeleteDC(DeviceContext);
            }

            _disposed = true;
        }
    }
}