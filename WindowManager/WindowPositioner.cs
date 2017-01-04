using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;

namespace WindowManager
{
  class WindowPositioner
  {
    // used for moving other windows:

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern IntPtr FindWindow(String lpClassName, String lpWindowName);

    [DllImport("user32.dll")]
    static extern IntPtr GetWindowText(IntPtr hwnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    static extern bool MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    static extern bool GetWindowRect(IntPtr hwnd, ref Rect lpRect);

    private struct Rect
    {
      public int Left { get; set; }
      public int Top { get; set; }
      public int Right { get; set; }
      public int Bottom { get; set; }
    }

    // seperate thread to control window positioning
    private Thread windowPositionerThread;
    private Dispatcher dispatcher;

    private delegate void ShowWindowDelegate(Rectangle windowPos);
    private delegate void HideWindowDelegate();

    private Keys expectedModifier;
    private Form previewWindow;
    private HashSet<Rectangle> windowPositions;

    public WindowPositioner(HashSet<Rectangle> windowPositions)
    {
      this.windowPositions = windowPositions;
      dispatcher = Dispatcher.CurrentDispatcher;

      previewWindow = new Form();
      previewWindow.BackColor = Color.LightBlue;
      previewWindow.FormBorderStyle = FormBorderStyle.None;
      previewWindow.TopMost = true;
      previewWindow.ShowInTaskbar = false;
      previewWindow.Opacity = 0.2;
    }

    public bool IsActive()
    {
      return windowPositionerThread != null && windowPositionerThread.IsAlive;
    }

    public void SetActive(Keys givenModifier)
    {
      if(!IsActive())
      {
        expectedModifier = givenModifier;
        windowPositionerThread = new Thread(new ThreadStart(this.ControlForegroundWindow));
        windowPositionerThread.Start();
      }
    }

    private void ShowPreview(Rectangle windowPos)
    {
      MoveWindow(previewWindow.Handle, windowPos.X+10, windowPos.Y+10,
                 windowPos.Width-20, windowPos.Height-20, false);
      previewWindow.Show();
    }

    private void HidePreview()
    {
      previewWindow.Hide();
    }

    private void ControlForegroundWindow()
    {
      Rectangle currentWindowPos;
      Point mousePos;
      IntPtr foregroundWindow;
      Rect foregroundWindowPos;
      Rectangle windowDraggableArea;

      while (Control.ModifierKeys == expectedModifier)
      {
        if (Control.MouseButtons == MouseButtons.Left)
        {
          currentWindowPos = new Rectangle(0, 0, 0, 0);
          mousePos = Control.MousePosition;
          foregroundWindow = GetForegroundWindow();
          foregroundWindowPos = new Rect();
          GetWindowRect(foregroundWindow, ref foregroundWindowPos);
          windowDraggableArea
            = new Rectangle(foregroundWindowPos.Left, foregroundWindowPos.Top,
                            foregroundWindowPos.Right - foregroundWindowPos.Left, 30);

          // check if foreground window is actually being moved
          if (windowDraggableArea.Contains(mousePos))
            while (Control.MouseButtons == MouseButtons.Left
                          && Control.ModifierKeys == expectedModifier)
            {
              mousePos = Control.MousePosition;

              if (!currentWindowPos.Contains(mousePos))
              {
                currentWindowPos = new Rectangle(0, 0, 0, 0);

                if (previewWindow.Visible)
                  dispatcher.Invoke(new HideWindowDelegate(HidePreview));

                foreach (Rectangle windowPos in windowPositions)
                  if (windowPos.Contains(mousePos))
                  {
                    currentWindowPos = windowPos;
                    dispatcher.Invoke(new ShowWindowDelegate(ShowPreview),
                                      new object[] { currentWindowPos });
                    break;
                  }
              }
              Thread.Sleep(100);
            }

          if (previewWindow.Visible)
            dispatcher.Invoke(new HideWindowDelegate(HidePreview));

          if (!currentWindowPos.IsEmpty && Control.ModifierKeys == expectedModifier)
            MoveWindow(foregroundWindow, currentWindowPos.X, currentWindowPos.Y,
                       currentWindowPos.Width, currentWindowPos.Height, true);
        }
        Thread.Sleep(100);
      }
    } // ControlForegroundWindow

  }
}
