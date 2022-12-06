using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace FolderDeepSearchV2.Core {
    public static class Opener {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern bool ShellExecuteEx(ref ShellExecuteInfo lpExecInfo);

        [Serializable]
        private struct ShellExecuteInfo {
            public int Size;
            public uint Mask;
            public IntPtr hwnd;
            public string Verb;
            public string File;
            public string Parameters;
            public string Directory;
            public uint Show;
            public IntPtr InstApp;
            public IntPtr IDList;
            public string Class;
            public IntPtr hkeyClass;
            public uint HotKey;
            public IntPtr Icon;
            public IntPtr Monitor;
        }

        private const uint SW_NORMAL = 1;

        public static void OpenFile(string path) {
            if (File.Exists(path)) {
                try {
                    Process.Start(path);
                }
                catch {
                    ShellExecuteInfo sei = new ShellExecuteInfo();
                    sei.Size = Marshal.SizeOf(sei);
                    sei.Verb = "openas";
                    sei.File = path;
                    sei.Show = SW_NORMAL;
                    ShellExecuteEx(ref sei);
                }
            }
            else {
                Process.Start(path);
            }
        }
    }
}