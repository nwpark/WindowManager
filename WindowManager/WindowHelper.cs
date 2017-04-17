using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace WindowManager
{
  class WindowHelper
  {
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hwnd, ref Rect lpRect);

    [DllImport("user32.dll")]
    public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

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

    public void AttachWindowToMouse(IntPtr givenWindow)
    {
      new Thread(() => {
        Rectangle windowPosition = GetWindowRectangle(givenWindow);
        Point initMousePos = Control.MousePosition;

        while (Control.MouseButtons == MouseButtons.Left)
        {
          int dx = Control.MousePosition.X - initMousePos.X;
          int dy = Control.MousePosition.Y - initMousePos.Y;
          MoveWindow(givenWindow, windowPosition.X + dx, windowPosition.Y + dy,
                     windowPosition.Width, windowPosition.Height, true);

          Thread.Sleep(1);
        }
      }).Start();
    }

    public void DropForegroundWindow()
    {
      Form dummyWindow = new Form();
      SetForegroundWindow(dummyWindow.Handle);
    }

    public struct Rect
    {
      public int Left { get; set; }
      public int Top { get; set; }
      public int Right { get; set; }
      public int Bottom { get; set; }
    }

  }
}
