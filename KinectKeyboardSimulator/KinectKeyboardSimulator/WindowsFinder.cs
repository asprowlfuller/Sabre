using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KinectKeyboardSimulator
{
    public static class WindowsFinder
    {
        /// <summary>
        /// Win32 API Imports
        /// </summary>
        [DllImport("user32.dll")]
        private static extern int GetWindowText(int hWnd, StringBuilder title, int size);
        [DllImport("user32.dll")]
        private static extern int EnumWindows(EnumWindowsProc enumWindowsProc, int lParam);
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(int hWnd);
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(int hWnd, out int lpdwProcessId);

        delegate bool EnumWindowsProc(int hWnd, int lParam);

        static List<NativeWindow> nativeWindows;

        public static IEnumerable<NativeWindow> NativeWindows
        {
            get
            {
                nativeWindows = new List<NativeWindow>();
                EnumWindows(EvalWindow, 0);
                return nativeWindows;
            }
        }

        private static bool EvalWindow(int hWnd, int lParam)
        {
            if (!IsWindowVisible(hWnd))
                return true;

            StringBuilder title = new StringBuilder(256);

            GetWindowText(hWnd, title, 256);

            string name = title.ToString();

            if (string.IsNullOrEmpty(name))
                return true;

            int pid;
            GetWindowThreadProcessId(hWnd, out pid);

            Process process = Process.GetProcessById(pid);

            ImageSource wpfBitmap;
            try
            {
                Icon icon = Icon.ExtractAssociatedIcon(process.MainModule.FileName);

                Bitmap bitmap = icon.ToBitmap();
                IntPtr hBitmap = bitmap.GetHbitmap();
                wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                if (!DeleteObject(hBitmap))
                {
                    throw new Win32Exception();
                } 
            }
            catch
            {
                return true;
            }

            nativeWindows.Add(new NativeWindow { Name = name, Handle = (IntPtr)hWnd, Icon = wpfBitmap});

            return true;
        }
    }
}
