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

    private delegate void ShowWindowDelegate(Rectangle windowPos);
    private delegate void HideWindowDelegate();

    private Dispatcher dispatcher;
    private Form previewWindow;
    private HashSet<Rectangle> windowPositions;
    private static Dictionary<IntPtr, Rectangle> windowResetPositions
      = new Dictionary<IntPtr, Rectangle>();

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

    public Boolean ControlForegroundWindow(Keys givenModifier)
    {
      Rectangle currentPreviewPos = new Rectangle(0, 0, 0, 0);
      IntPtr foregroundWindow = GetForegroundWindow();

      while (Control.MouseButtons == MouseButtons.Left
                    && Control.ModifierKeys == givenModifier) {

        if (!currentPreviewPos.Contains(Control.MousePosition)) {
          currentPreviewPos = new Rectangle(0, 0, 0, 0);

          if (previewWindow.Visible)
            dispatcher.Invoke(new HideWindowDelegate(HidePreview));

          foreach (Rectangle previewPos in windowPositions)
            if (previewPos.Contains(Control.MousePosition)) {
              dispatcher.Invoke(new ShowWindowDelegate(ShowPreview),
                                new object[] { previewPos });
              currentPreviewPos = previewPos;
              break;
            }
        }
        Thread.Sleep(100);
      }

      if (previewWindow.Visible)
        dispatcher.Invoke(new HideWindowDelegate(HidePreview));

      if (!currentPreviewPos.IsEmpty && Control.ModifierKeys == givenModifier)
      {
        MoveWindow(foregroundWindow, currentPreviewPos.X, currentPreviewPos.Y,
                    currentPreviewPos.Width, currentPreviewPos.Height, true);

        return true;
      }

      return false;
    } // ControlForegroundWindow

    private void ShowPreview(Rectangle windowPos)
    {
      MoveWindow(previewWindow.Handle, windowPos.X + 10, windowPos.Y + 10,
                 windowPos.Width - 20, windowPos.Height - 20, false);
      previewWindow.Show();
    }

    private void HidePreview()
    {
      previewWindow.Hide();
    }

  }
}
