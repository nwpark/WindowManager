using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

    // seperate thread to control window positioning
    private Thread windowPositionerThread;
    private Dispatcher dispatcher;

    private delegate void TestDelegate();

    public WindowPositioner()
    {
      dispatcher = Dispatcher.CurrentDispatcher;
      f = new Form();
      f.BackColor = Color.White;
      f.FormBorderStyle = FormBorderStyle.None;
      f.Bounds = new Rectangle(10, 10, 950, 1000);
      f.TopMost = true;
      f.ShowInTaskbar = false;
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

    public void ShowWindow()
    {
      f.Location = new Point(10, 10);
      f.Show();
      //dispatcher.Invoke(new TestDelegate(f.Show));
    }

    public void HideWindow()
    {
      dispatcher.Invoke(new TestDelegate(f.Hide));
    }

    private static Form f;
    public void ControlForegroundWindow()
    {
      IntPtr foregroundWindow = GetForegroundWindow();
      StringBuilder foregroundWindowName = new StringBuilder();
      //GetWindowText(foregroundWindow, foregroundWindowName, 5120);
      //Console.WriteLine(foregroundWindowName.ToString());

      //System.Drawing.Point cursorPos = System.Windows.Forms.Control.MousePosition;
      //Console.WriteLine(cursorPos.ToString());

      //ShowWindow();
      dispatcher.Invoke(new TestDelegate(f.Show));

      Console.WriteLine(Control.ModifierKeys);

      while (Control.ModifierKeys == Keys.Shift)
      {
        Console.WriteLine(Control.ModifierKeys);
        Thread.Sleep(100);
      }

      HideWindow();
      Console.WriteLine("Exiting");
    }

  }
}
