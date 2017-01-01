using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Threading;

namespace WindowManager
{
  /// Interaction logic for MainWindow.xaml
  public partial class MainWindow : Window
  {
    // used for hotkeys without focus:

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    // vars
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;
    private static int LEFT = 37, RIGHT = 39;

    private static WindowPositioner windowPositioner;
    

    public MainWindow()
    {
      InitializeComponent();

      windowPositioner = new WindowPositioner();

      // set this process as windows hook to listen for shift key press
      _hookID = SetHook(_proc);

      //UnhookWindowsHookEx(_hookID);
    }

    private void button_Click(object sender, RoutedEventArgs e)
    {
      Console.WriteLine("button was clicked");
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
      using (Process curProcess = Process.GetCurrentProcess())
      using (ProcessModule curModule = curProcess.MainModule)
      {
        return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                                GetModuleHandle(curModule.ModuleName), 0);
      }
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
      if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
      {
        int vkCode = Marshal.ReadInt32(lParam);
        Console.WriteLine(((Keys)vkCode).ToString() + ", code: " + vkCode.ToString());

        //if(System.Windows.Forms.Control.ModifierKeys == Keys.Shift)
          windowPositioner.SetActive();
      }

      return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }
  }
}
