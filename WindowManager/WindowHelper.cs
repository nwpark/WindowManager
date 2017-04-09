using System;
using System.Runtime.InteropServices;
using System.Drawing;

namespace WindowManager
{
  class WindowHelper
  {
    [DllImport("user32.dll")]
    static extern bool GetWindowRect(IntPtr hwnd, ref Rect lpRect);

    public Rectangle GetWindowRectangle(IntPtr givenWindow)
    {
      Rect windowRect = new Rect();
      GetWindowRect(givenWindow, ref windowRect);

      int windowWidth = windowRect.Right - windowRect.Left;
      int windowHeight = windowRect.Bottom - windowRect.Top;

      Rectangle windowRectangle
        = new Rectangle(windowRect.Left, windowRect.Top, windowWidth, windowHeight);

      return windowRectangle;
    }

    private struct Rect
    {
      public int Left { get; set; }
      public int Top { get; set; }
      public int Right { get; set; }
      public int Bottom { get; set; }
    }
  }
}
