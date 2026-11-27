using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NT_Cheat
{
    internal static class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool IsDebuggerPresent();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Anti-debug básico
            if (IsDebuggerPresent())
            {
                Environment.Exit(0);
            }

            bool remoteDebugger = false;
            CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref remoteDebugger);
            if (remoteDebugger)
            {
                Environment.Exit(0);
            }

            // Verifica ferramentas de análise comuns
            string[] blacklist = { "dnspy", "x64dbg", "x32dbg", "ollydbg", "ida", "ida64",
                                   "cheatengine", "cheat engine", "wireshark", "fiddler",
                                   "httpdebuggerpro", "charles", "procmon", "processhacker" };
            var running = Process.GetProcesses().Select(p => p.ProcessName.ToLowerInvariant());
            foreach (string tool in blacklist)
            {
                if (running.Any(p => p.Contains(tool)))
                {
                    Environment.Exit(0);
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
