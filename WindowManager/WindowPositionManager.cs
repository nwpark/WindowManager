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
    static extern bool GetWindowRect(IntPtr hwnd, ref Rect lpRect);

    private static WindowPositioner layout1Positioner;
    private static WindowPositioner layout2Positioner;

    public WindowPositionManager()
    {
      int correctionOffset = 7;
      int correctionWidth = correctionOffset * 2;

      HashSet<Rectangle> layout1 = new HashSet<Rectangle>();
      layout1.Add(new Rectangle(0 - correctionOffset, 0, 1147 + correctionWidth, 1410));
      layout1.Add(new Rectangle(1147 - correctionOffset, 0, 1146 + correctionWidth, 1410));
      layout1.Add(new Rectangle(2293 - correctionOffset, 0, 1147 + correctionWidth, 1410));
      layout1Positioner = new WindowPositioner(layout1);

      HashSet<Rectangle> layout2 = new HashSet<Rectangle>();
      layout2.Add(new Rectangle(0, 0, 960, 505));
      layout2.Add(new Rectangle(960, 0, 960, 505));
      layout2.Add(new Rectangle(0, 505, 960, 505));
      layout2.Add(new Rectangle(960, 505, 960, 505));
      layout2Positioner = new WindowPositioner(layout2);
    } // WindowPositionManager

    public void CheckWindowMovement()
    {
      if(WindowIsMoving())
      {
        // TODO: check for window reset
        
        while(Control.MouseButtons == MouseButtons.Left)
        {
          switch(Control.ModifierKeys)
          {
            case Keys.Shift:
              Console.WriteLine("shift");
              layout1Positioner.ControlForegroundWindow(Keys.Shift);
              break;
            case Keys.Control:
              Console.WriteLine("Control");
              layout2Positioner.ControlForegroundWindow(Keys.Control);
              break;
          }

          Thread.Sleep(100);
        }
      }
    } // CheckWindowMovement

    private static Boolean WindowIsMoving()
    {
      IntPtr foregroundWindow = GetForegroundWindow();
      Rect initWindowPos = new Rect();
      GetWindowRect(foregroundWindow, ref initWindowPos);

      Point initMousePos = Control.MousePosition;

      while (Control.MouseButtons == MouseButtons.Left)
      {
        if (MouseMoved(initMousePos))
        {
          Rect windowPos = new Rect();
          GetWindowRect(foregroundWindow, ref windowPos);

          return !windowPos.Equals(initWindowPos);
        }

        Thread.Sleep(100);
      }

      return false;
    } // WindowIsMoving

    private static Boolean MouseMoved(Point initMousePos)
    {
      Point difference = Point.Subtract(initMousePos, new Size(Control.MousePosition));
      return Math.Abs(difference.X) > 20 || Math.Abs(difference.Y) > 20;
    } // MouseMoved

    private struct Rect
    {
      public int Left { get; set; }
      public int Top { get; set; }
      public int Right { get; set; }
      public int Bottom { get; set; }
    } // Rect

  } // class WindowPositionManager
} // WindowManager
