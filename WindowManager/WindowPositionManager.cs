using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;

namespace WindowManager
{
  class WindowPositionManager
  {
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern bool MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private WindowHelper windowHelper;

    private WindowPositioner layout1Positioner;
    private WindowPositioner layout2Positioner;

    private Dictionary<IntPtr, Rectangle> windowResetPositions;

    public WindowPositionManager()
    {
      windowHelper = new WindowHelper();

      int correctionOffset = 7;
      int correctionWidth = correctionOffset * 2;

      HashSet<Rectangle> layout1 = new HashSet<Rectangle>();
      layout1.Add(new Rectangle(0 - correctionOffset, 0, 1147 + correctionWidth, 1410));
      layout1.Add(new Rectangle(1147 - correctionOffset, 0, 1146 + correctionWidth, 1410));
      layout1.Add(new Rectangle(2293 - correctionOffset, 0, 1147 + correctionWidth, 1410));
      layout1Positioner = new WindowPositioner(layout1, Keys.Shift);

      HashSet<Rectangle> layout2 = new HashSet<Rectangle>();
      layout2.Add(new Rectangle(0, 0, 960, 505));
      layout2.Add(new Rectangle(960, 0, 960, 505));
      layout2.Add(new Rectangle(0, 505, 960, 505));
      layout2.Add(new Rectangle(960, 505, 960, 505));
      layout2Positioner = new WindowPositioner(layout2, Keys.Control);

      windowResetPositions = new Dictionary<IntPtr, Rectangle>();
    } // WindowPositionManager

    public void CheckWindowMovement()
    {
      if(WindowIsMoving())
      {
        IntPtr foregroundWindow = GetForegroundWindow();
        if (windowResetPositions.ContainsKey(foregroundWindow))
          ResetWindowPos(foregroundWindow);

        Rectangle foregroundWindowPos = windowHelper.GetWindowRectangle(foregroundWindow);

        Boolean windowMoved = false;

        while (Control.MouseButtons == MouseButtons.Left)
        {
          switch(Control.ModifierKeys)
          {
            case Keys.Shift:
              windowMoved = layout1Positioner.ControlForegroundWindow();
              break;
            case Keys.Control:
              windowMoved = layout2Positioner.ControlForegroundWindow();
              break;
          }

          Thread.Sleep(100);
        }

        if (windowMoved)
          windowResetPositions.Add(foregroundWindow, foregroundWindowPos);
      }
    } // CheckWindowMovement

    private Boolean WindowIsMoving()
    {
      IntPtr foregroundWindow = GetForegroundWindow();
      Rectangle initWindowPos = windowHelper.GetWindowRectangle(foregroundWindow);

      Point initMousePos = Control.MousePosition;

      while (Control.MouseButtons == MouseButtons.Left)
      {
        if (MouseMoved(initMousePos))
        {
          Rectangle windowPos = windowHelper.GetWindowRectangle(foregroundWindow);

          return !windowPos.Equals(initWindowPos) && windowPos.Width == initWindowPos.Width 
                                                  && windowPos.Height == initWindowPos.Height;
        }

        Thread.Sleep(100);
      }

      return false;
    } // WindowIsMoving

    public void ResetWindowPos(IntPtr foregroundWindow)
    {
      Rectangle windowRect = windowHelper.GetWindowRectangle(foregroundWindow);
      Rectangle windowResetPos = windowResetPositions[foregroundWindow];

      double relativeMouseX = (Control.MousePosition.X - windowRect.Left) / (double)windowRect.Width;
      int newWindowX = Control.MousePosition.X - (int)(relativeMouseX * windowResetPos.Width);

      windowHelper.DropForegroundWindow();
      MoveWindow(foregroundWindow,
                 newWindowX, windowRect.Top,
                 windowResetPos.Width, windowResetPos.Height, true);
      SetForegroundWindow(foregroundWindow);

      windowHelper.AttachWindowToMouse(foregroundWindow);
      
      windowResetPositions.Remove(foregroundWindow);
    }

    private Boolean MouseMoved(Point initMousePos)
    {
      Point difference = Point.Subtract(initMousePos, new Size(Control.MousePosition));
      return Math.Abs(difference.X) > 20 || Math.Abs(difference.Y) > 20;
    }

  } // class WindowPositionManager
} // WindowManager
