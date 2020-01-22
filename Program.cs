using System;
using System.Runtime.InteropServices;

namespace net_core_pinvoke
{
  class Program
  {
    // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdesktopwindow
    // http://pinvoke.net/default.aspx/user32/GetDesktopWindow.html
    [DllImport("user32")]
    private static extern IntPtr GetDesktopWindow();

    // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-
    // http://pinvoke.net/default.aspx/user32/GetDC.html
    [DllImport("user32")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getsystemmetrics
    // http://pinvoke.net/default.aspx/user32/GetSystemMetrics.html
    [DllImport("user32")]
    private static extern int GetSystemMetrics(int mIndex);

    // https://docs.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-createcompatibledc
    // http://pinvoke.net/default.aspx/gdi32/CreateCompatibleDC.html
    [DllImport("gdi32")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    // https://docs.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-createcompatiblebitmap
    // http://pinvoke.net/default.aspx/gdi32/CreateCompatibleBitmap.html
    [DllImport("gdi32")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int cx, int cy);

    // https://docs.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-selectobject
    // http://pinvoke.net/default.aspx/gdi32/SelectObject.html
    [DllImport("gdi32")]
    private static extern IntPtr SelectObject(IntPtr hDc, IntPtr h);

    // https://docs.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-bitblt
    // http://pinvoke.net/default.aspx/gdi32/BitBlt.html
    [DllImport("gdi32")]
    private static extern IntPtr BitBlt(IntPtr hdc, int x, int y, int cx, int cy, IntPtr hdcSrc, int x1, int y1, uint dwRop);

    // https://docs.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-getdibits
    // http://pinvoke.net/default.aspx/gdi32/GetDIBits.html
    [DllImport("gdi32")]
    private static extern IntPtr GetDIBits();

    static void Main(string[] args)
    {
      var desktopPointer = GetDesktopWindow();
      if (desktopPointer == IntPtr.Zero)
      {
        throw new Exception("Failed to obtain a pointer to the desktop window");
      }

      var desktopDeviceContextPointer = GetDC(desktopPointer);
      if (desktopDeviceContextPointer == IntPtr.Zero)
      {
        throw new Exception("Failed to obtain a pointer to the desktop device context");
      }

      var desktopWidth = GetSystemMetrics(0 /* SM_CXSCREEN */);
      var desktopHeight = GetSystemMetrics(1 /* SM_CYSCREEN */);
      Console.WriteLine($"The desktop window dimensions are {desktopWidth}x{desktopHeight}");

      var bitmapDeviceContextPointer = CreateCompatibleDC(desktopDeviceContextPointer);
      if (bitmapDeviceContextPointer == IntPtr.Zero)
      {
        throw new Exception("Failed to obtain a pointer to the bitmap device context");
      }

      var bitmapPointer = CreateCompatibleBitmap(bitmapDeviceContextPointer, desktopWidth, desktopHeight);
      if (bitmapPointer == IntPtr.Zero)
      {
        throw new Exception("Failed to obtain a pointer to the bitmap");
      }

      var selectObjectResult = SelectObject(bitmapDeviceContextPointer, bitmapPointer);
      if (selectObjectResult == IntPtr.Zero || selectObjectResult == (IntPtr)(0xFFFFFFFF) /* HGDI_ERROR */)
      {
        throw new Exception("Failed to select the bitmap device context for the bitmap");
      }

      var bitblockTransferResult = BitBlt(bitmapDeviceContextPointer, 0, 0, desktopWidth, desktopHeight, desktopDeviceContextPointer, 0, 0, 0x00CC0020 /* SRCCOPY */ | 0x40000000 /* CAPTUREBLT */);
      if (bitblockTransferResult == IntPtr.Zero)
      {
        throw new Exception("Failed to transfer the bit-block");
      }

      // TODO: Finalize this
      // https://docs.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-getdibits#remarks
      // If the lpvBits parameter is a valid pointer, the first six members of the BITMAPINFOHEADER structure must be initialized to specify the size and format of the DIB
      // https://github.com/alexchandel/screenshot-rs/blob/master/src/lib.rs#L462
      var rgb = new byte[desktopWidth * desktopHeight * 4]; // TODO: 3 or 4?
      GetDIBits(bitmapDeviceContextPointer, bitmapPointer, 0, (uint)desktopHeight, rgb, IntPtr.Zero, );

      Console.WriteLine("Hello World!");
    }
  }
}
