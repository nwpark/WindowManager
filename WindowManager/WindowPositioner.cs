﻿using System;
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

    private delegate void ShowWindowDelegate(Rectangle windowPos);
    private delegate void HideWindowDelegate();

    private Dispatcher dispatcher;
    private Form previewWindow;
    private HashSet<Rectangle> windowPositions;
    private static Dictionary<IntPtr, Rect> windowResetPositions
      = new Dictionary<IntPtr, Rect>();

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

    public void ControlForegroundWindow(Keys givenModifier)
    {
      Rectangle currentPreviewPos;
      Point mousePos;
      IntPtr foregroundWindow;
      Rect foregroundWindowPos;
      Rectangle windowDraggableArea;

      while (Control.ModifierKeys == givenModifier)
      {
        if (Control.MouseButtons == MouseButtons.Left)
        {
          currentPreviewPos = new Rectangle(0, 0, 0, 0);
          mousePos = Control.MousePosition;
          foregroundWindow = GetForegroundWindow();
          foregroundWindowPos = new Rect();
          GetWindowRect(foregroundWindow, ref foregroundWindowPos);
          windowDraggableArea = GetDraggableArea(foregroundWindow);

          // check if foreground window is actually being moved
          if (windowDraggableArea.Contains(mousePos))
            while (Control.MouseButtons == MouseButtons.Left
                          && Control.ModifierKeys == givenModifier)
            {
              mousePos = Control.MousePosition;

              if (!currentPreviewPos.Contains(mousePos))
              {
                currentPreviewPos = new Rectangle(0, 0, 0, 0);

                if (previewWindow.Visible)
                  dispatcher.Invoke(new HideWindowDelegate(HidePreview));

                foreach (Rectangle windowPos in windowPositions)
                  if (windowPos.Contains(mousePos))
                  {
                    currentPreviewPos = windowPos;
                    dispatcher.Invoke(new ShowWindowDelegate(ShowPreview),
                                      new object[] { currentPreviewPos });
                    break;
                  }
              }
              Thread.Sleep(100);
            } // while

          if (previewWindow.Visible)
            dispatcher.Invoke(new HideWindowDelegate(HidePreview));

          if (!currentPreviewPos.IsEmpty && Control.ModifierKeys == givenModifier)
          {
            windowResetPositions.Add(foregroundWindow, foregroundWindowPos);
            MoveWindow(foregroundWindow, currentPreviewPos.X, currentPreviewPos.Y,
                       currentPreviewPos.Width, currentPreviewPos.Height, true);
          }
        } // if (Control.MouseButtons == MouseButtons.Left)

        Thread.Sleep(100);
      } // while (Control.ModifierKeys == givenModifier)
    } // ControlForegroundWindow

    public static void CheckForReset()
    {
      IntPtr foregroundWindow = GetForegroundWindow();
      Rectangle windowDraggableArea = GetDraggableArea(foregroundWindow);
      if (windowResetPositions.ContainsKey(foregroundWindow)
                && windowDraggableArea.Contains(Control.MousePosition))
      {
        Rect foregroundWindowPos = new Rect();
        GetWindowRect(foregroundWindow, ref foregroundWindowPos);

        Rect windowResetPos = windowResetPositions[foregroundWindow];
        double relativeMouseX = (double)(Control.MousePosition.X - windowDraggableArea.Left)
                                  / windowDraggableArea.Width;
        int newWindowWidth = windowResetPos.Right - windowResetPos.Left;
        int newWindowHeight = windowResetPos.Bottom - windowResetPos.Top;
        int newWindowX = Control.MousePosition.X - (int)(relativeMouseX * newWindowWidth);

        MoveWindow(foregroundWindow, newWindowX, foregroundWindowPos.Top,
                   newWindowWidth, newWindowHeight, true);

        windowResetPositions.Remove(foregroundWindow);
      }
    }

    private static Rectangle GetDraggableArea(IntPtr window)
    {
      Rect windowPos = new Rect();
      GetWindowRect(window, ref windowPos);
      
      return new Rectangle(windowPos.Left, windowPos.Top,
                           windowPos.Right - windowPos.Left, 30);
    }

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

    private struct Rect
    {
      public int Left { get; set; }
      public int Top { get; set; }
      public int Right { get; set; }
      public int Bottom { get; set; }
    }

  }
}
