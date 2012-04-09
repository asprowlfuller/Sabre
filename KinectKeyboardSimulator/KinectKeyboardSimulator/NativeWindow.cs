using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Media;

namespace KinectKeyboardSimulator
{
    public class NativeWindow
    {
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public IntPtr Handle { get; set; }

        public string Name { get; set; }

        public ImageSource Icon { get; set; }


        public void BringToFront()
        {
            SetForegroundWindow(Handle);
        }
    }
}
