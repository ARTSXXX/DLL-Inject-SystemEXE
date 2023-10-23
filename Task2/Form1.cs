using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Task2
{
    public partial class Form1 : Form
    {
        #region Fields

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc;
        private static IntPtr _hookID = IntPtr.Zero;
        private static TextBox textBox1;
        private IntPtr notepadHwnd;

        #endregion

        public Form1()
        {
            InitializeComponent();
            textBox1 = new TextBox();
            textBox1.Multiline = true;
            textBox1.Dock = DockStyle.Fill;
            Controls.Add(textBox1);
        }

        #region Main Part Handler Keyboards
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            notepadHwnd = FindWindow("Notepad", null);

            if (notepadHwnd != IntPtr.Zero)
            {
                _proc = HookCallback;
                _hookID = SetHook(_proc);
            }
            else
            {
                MessageBox.Show("Окно блокнота не найдено.");
                Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                char keyChar = GetCharFromKey(vkCode);
                textBox1.AppendText(keyChar.ToString());
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static char GetCharFromKey(int vkCode)
        {
            return (char)vkCode;
        }

        #endregion

        #region Dll-Include

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        #endregion
    }
}
