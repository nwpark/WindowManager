using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Threading;

namespace WindowManager
{
  /// Interaction logic for MainWindow.xaml
  public partial class MainWindow : Window
  {
    // used for hotkeys without focus:

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelPeripheralProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    private delegate IntPtr LowLevelPeripheralProc(int nCode, IntPtr wParam, IntPtr lParam);

    // vars
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WH_MOUSE_LL = 14;
    private static LowLevelPeripheralProc _mouseProc = MouseHookCallback;
    private static IntPtr _keyboardHookID = IntPtr.Zero;
    private static IntPtr _mouseHookID = IntPtr.Zero;

    private static WindowPositionManager windowPositionManager;

    public MainWindow()
    {
      InitializeComponent();

      windowPositionManager = new WindowPositionManager();

      // set hook to listen for mouse button press
      _mouseHookID = SetHook(WH_MOUSE_LL, _mouseProc);
      
      //UnhookWindowsHookEx(_mouseHookID);
    }

    private void button_Click(object sender, RoutedEventArgs e)
    {
      Console.WriteLine("button was clicked");
    }

    private static IntPtr SetHook(int peripheral, LowLevelPeripheralProc proc)
    {
      using (Process curProcess = Process.GetCurrentProcess())
      using (ProcessModule curModule = curProcess.MainModule)
      {
        return SetWindowsHookEx(peripheral, proc,
                                GetModuleHandle(curModule.ModuleName), 0);
      }
    }

    private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
      if (nCode >= 0 && MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
      {
        Thread wpmThread
          = new Thread(new ThreadStart(windowPositionManager.CheckWindowMovement));

        wpmThread.Start();
      }
      return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
    }

    private enum MouseMessages
    {
      WM_LBUTTONDOWN = 0x0201,
      WM_LBUTTONUP = 0x0202,
      WM_MOUSEMOVE = 0x0200,
      WM_MOUSEWHEEL = 0x020A,
      WM_RBUTTONDOWN = 0x0204,
      WM_RBUTTONUP = 0x0205
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
      public int x;
      public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
      public POINT pt;
      public uint mouseData;
      public uint flags;
      public uint time;
      public IntPtr dwExtraInfo;
    }
  }
}
