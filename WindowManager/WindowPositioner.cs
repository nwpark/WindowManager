using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
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

    // seperate thread to control window positioning
    private Thread windowPositionerThread;
    private Dispatcher dispatcher;

    private delegate void ShowWindowDelegate(Rectangle windowPos);
    private delegate void HideWindowDelegate();
    private delegate bool MousePressedDelegate();

    private Form f;
    private HashSet<Rectangle> windowPositions;

    public WindowPositioner(HashSet<Rectangle> windowPositions)
    {
      this.windowPositions = windowPositions;
      dispatcher = Dispatcher.CurrentDispatcher;

      f = new Form();
      f.BackColor = Color.LightBlue;
      f.FormBorderStyle = FormBorderStyle.None;
      f.Bounds = new Rectangle(10, 10, 950, 1000);
      f.TopMost = true;
      f.ShowInTaskbar = false;
      f.Opacity = 0.3;
      f.StartPosition = FormStartPosition.Manual;
      f.Location = new Point(10, 10);
    }

    public bool IsActive()
    {
      return windowPositionerThread != null && windowPositionerThread.IsAlive;
    }

    public void SetActive()
    {
      if(!IsActive())
      {
        windowPositionerThread = new Thread(new ThreadStart(this.ControlForegroundWindow));
        windowPositionerThread.Start();
      }
    }

    public void ShowWindow(Rectangle windowPos)
    {
      MoveWindow(f.Handle, windowPos.X+10, windowPos.Y+10,
                 windowPos.Width-20, windowPos.Height-20, false);
      f.Show();
    }

    public void HideWindow()
    {
      f.Hide();
    }

    private bool MousePressed()
    {
      return Mouse.LeftButton == MouseButtonState.Pressed;
    }

    public void ControlForegroundWindow()
    {
      IntPtr foregroundWindow = GetForegroundWindow();
      
      Rectangle currentWindowPos = new Rectangle(0, 0, 0, 0);
      Point mousePos = Control.MousePosition;

      while (Control.ModifierKeys == Keys.Shift)
      {
        while(Control.MouseButtons == MouseButtons.Left
                       && Control.ModifierKeys == Keys.Shift)
        {
          mousePos = Control.MousePosition;

          if (!currentWindowPos.Contains(mousePos))
          {
            foreach (Rectangle windowPos in windowPositions)
            {
              if (windowPos.Contains(mousePos))
              {
                currentWindowPos = windowPos;
                dispatcher.Invoke(new ShowWindowDelegate(ShowWindow),
                                  new object[] { currentWindowPos });
                break;
              }

              if(f.Visible)
                dispatcher.Invoke(new HideWindowDelegate(HideWindow));
            }
          }

          Thread.Sleep(100);
        }

        if (f.Visible)
          dispatcher.Invoke(new HideWindowDelegate(HideWindow));

        Thread.Sleep(100);
      }

      dispatcher.Invoke(new HideWindowDelegate(HideWindow));
      Console.WriteLine("Exiting");
    }

  }
}
