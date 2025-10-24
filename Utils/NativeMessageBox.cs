using System;
using System.Runtime.InteropServices;

namespace ConsoleBaseTool {
    public class NativeMessageBox {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        public enum MessageBoxButtons {
            OK = 0x00000000, OKCancel = 0x00000001, YesNo = 0x00000004, YesNoCancel = 0x00000003
        }

        public enum MessageBoxIcon {
            None = 0x00000000, Information = 0x00000040, Warning = 0x00000030, Error = 0x00000010, Question = 0x00000020
        }

        public enum MessageBoxResult {
            OK = 1, Cancel = 2, Yes = 6, No = 7
        }

        public static MessageBoxResult Show(string text, string caption = "", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None) {
            uint type = (uint)buttons | (uint)icon;
            int result = MessageBox(IntPtr.Zero, text, caption, type);
            return (MessageBoxResult)result;
        }
    }
}