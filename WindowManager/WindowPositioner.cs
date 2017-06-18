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
    static extern bool MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

    private delegate void ShowWindowDelegate(Rectangle windowPos);
    private delegate void HideWindowDelegate();

    private Keys modifier;

    private Dispatcher dispatcher;
    private Form previewWindow;
    private HashSet<Rectangle> windowPositions;
    private static Dictionary<IntPtr, Rectangle> windowResetPositions
      = new Dictionary<IntPtr, Rectangle>();

    public WindowPositioner(HashSet<Rectangle> windowPositions, Keys givenModifier)
    {
      this.windowPositions = windowPositions;
      this.modifier = givenModifier;

      dispatcher = Dispatcher.CurrentDispatcher;

      InitPreviewWindow();
    }

    public Boolean ControlForegroundWindow()
    {
      Rectangle currentPreviewPos = new Rectangle(0, 0, 0, 0);
      IntPtr foregroundWindow = WindowModifier.GetForegroundWindow();

      while (Control.MouseButtons == MouseButtons.Left
                    && Control.ModifierKeys == modifier) {

        if (!currentPreviewPos.Contains(Control.MousePosition)) {
          currentPreviewPos = new Rectangle(0, 0, 0, 0);

          if (previewWindow.Visible)
            HidePreview();

          foreach (Rectangle previewPos in windowPositions)
            if (previewPos.Contains(Control.MousePosition)) {
              dispatcher.Invoke(new ShowWindowDelegate(ShowPreview),
                                new object[] { previewPos });
              currentPreviewPos = previewPos;
              break;
            }
        }
        Thread.Sleep(100);
Console.WriteLine(Control.MouseButtons + ", " + Control.ModifierKeys + ", " + modifier + ", " + DateTime.Now);
      }

      if (previewWindow.Visible)
        HidePreview();

      if (!currentPreviewPos.IsEmpty && Control.ModifierKeys == modifier)
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
      dispatcher.Invoke(new HideWindowDelegate(previewWindow.Hide));
    }

    private void InitPreviewWindow()
    {
      previewWindow = new Form();
      previewWindow.BackColor = Color.LightBlue;
      previewWindow.FormBorderStyle = FormBorderStyle.None;
      previewWindow.TopMost = true;
      previewWindow.ShowInTaskbar = false;
      previewWindow.Opacity = 0.2;
    }

  }
}
