using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace EXTERNAL
{
    public class Aimem
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, IntPtr nSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        public static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        public struct PatternData
        {
            public byte[] pattern { get; set; }
            public byte[] mask { get; set; }
        }

        public struct MemoryPage
        {
            public IntPtr Start;
            public int Size;
            public MemoryPage(IntPtr start, int size)
            {
                Start = start;
                Size = size;
            }
        }

        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public UIntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        public int processId;
        public IntPtr _processHandle;
        private bool _enableCheck = true;

        public bool SetProcess(string[] processNames)
        {
            processId = 0;
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (processNames.Any(name => name.Equals(process.ProcessName, StringComparison.OrdinalIgnoreCase)))
                {
                    processId = process.Id;
                    break;
                }
            }
            if (processId <= 0) return false;

            _processHandle = OpenProcess(ProcessAccessFlags.AllAccess, false, processId);
            return _processHandle != IntPtr.Zero;
        }

        public void CheckProcess()
        {
            if (!_enableCheck || processId <= 0) return;
            try
            {
                foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
                {
                    IntPtr hThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                    if (hThread != IntPtr.Zero)
                    {
                        while (ResumeThread(hThread) > 0) ;
                        CloseHandle(hThread);
                    }
                }
            }
            catch { }
        }

        public async Task<IEnumerable<long>> AoBScan(string pattern)
        {
            PatternData patternData = GetPatternDataFromPattern(pattern);
            List<long> addressRet = new List<long>();

            await Task.Run(() =>
            {
                List<MemoryPage> pages = new List<MemoryPage>();
                IntPtr address = IntPtr.Zero;
                int mbiSize = Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION));

                while (VirtualQueryEx(_processHandle, address, out MEMORY_BASIC_INFORMATION mbi, (uint)mbiSize) == mbiSize)
                {
                    if (mbi.State == 0x1000 && (mbi.Protect == 0x04 || mbi.Protect == 0x20 || mbi.Protect == 0x40))
                    {
                        pages.Add(new MemoryPage(mbi.BaseAddress, (int)mbi.RegionSize.ToUInt64()));
                    }
                    address = (IntPtr)((long)mbi.BaseAddress + (long)mbi.RegionSize.ToUInt64());
                }

                Parallel.ForEach(pages, page =>
                {
                    byte[] buffer = new byte[page.Size];
                    if (ReadProcessMemory(_processHandle, page.Start, buffer, (IntPtr)page.Size, out _))
                    {
                        int index = -patternData.pattern.Length;
                        do
                        {
                            index = FindPattern(buffer, patternData.pattern, patternData.mask, index + patternData.pattern.Length);
                            if (index >= 0)
                            {
                                lock (addressRet) { addressRet.Add((long)page.Start + index); }
                            }
                        } while (index != -1);
                    }
                });
            });

            return addressRet.OrderBy(a => a);
        }

        private int FindPattern(byte[] body, byte[] pattern, byte[] masks, int start = 0)
        {
            if (body.Length == 0 || start > body.Length - pattern.Length) return -1;
            for (int i = start; i <= body.Length - pattern.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if ((body[i + j] & masks[j]) != (pattern[j] & masks[j]))
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return i;
            }
            return -1;
        }

        private PatternData GetPatternDataFromPattern(string pattern)
        {
            string[] parts = pattern.Split(' ');
            return new PatternData
            {
                pattern = parts.Select(s => s == "??" ? (byte)0 : byte.Parse(s, NumberStyles.HexNumber)).ToArray(),
                mask = parts.Select(s => s == "??" ? (byte)0 : (byte)0xFF).ToArray()
            };
        }

                public bool WriteInt(long address, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteProcessMemory(_processHandle, (IntPtr)address, bytes, (IntPtr)bytes.Length, out _);
        }

        public bool IsProcessAlive()
        {
            if (processId <= 0) return false;
            try
            {
                var proc = Process.GetProcessById(processId);
                return !proc.HasExited;
            }
            catch { return false; }
        }

        public void Release()
        {
            if (_processHandle != IntPtr.Zero)
            {
                CloseHandle(_processHandle);
                _processHandle = IntPtr.Zero;
                processId = 0;
            }
        }
public int ReadInt(long addressToRead)
        {
            byte[] buffer = new byte[4];
            if (ReadProcessMemory(_processHandle, (IntPtr)addressToRead, buffer, (IntPtr)buffer.Length, out _))
            {
                return BitConverter.ToInt32(buffer, 0);
            }
            return 0;
        }

        public float ReadFloat(long addressToRead)
        {
            byte[] buffer = new byte[4];
            if (ReadProcessMemory(_processHandle, (IntPtr)addressToRead, buffer, (IntPtr)buffer.Length, out _))
            {
                return BitConverter.ToSingle(buffer, 0);
            }
            return 0f;
        }

        public bool AobReplace(long address, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteProcessMemory(_processHandle, (IntPtr)address, bytes, (IntPtr)bytes.Length, out _);
        }

        public bool AobReplace(long address, string bytePattern)
        {
            try
            {
                byte[] array = StringToByteArray(bytePattern);
                return WriteProcessMemory(_processHandle, (IntPtr)address, array, (IntPtr)array.Length, out _);
            }
            catch { return false; }
        }

        public bool WriteMemory(long address, string type, string value)
        {
            if (type.ToLower() == "int")
            {
                byte[] bytes = BitConverter.GetBytes(int.Parse(value));
                return WriteProcessMemory(_processHandle, (IntPtr)address, bytes, (IntPtr)bytes.Length, out _);
            }
            return false;
        }

        private byte[] StringToByteArray(string hex)
        {
            return hex.Split(' ').Select(s => byte.Parse(s, NumberStyles.HexNumber)).ToArray();
        }

        public void InjectDll(string dllPath)
        {
            Process[] procs = Process.GetProcessesByName("HD-Player");
            if (procs.Length == 0) return;

            IntPtr hProc = OpenProcess(ProcessAccessFlags.AllAccess, false, procs[0].Id);
            if (hProc == IntPtr.Zero) return;

            byte[] pathBytes = System.Text.Encoding.ASCII.GetBytes(dllPath + "\0");
            IntPtr alloc = VirtualAllocEx(hProc, IntPtr.Zero, (uint)pathBytes.Length, 0x1000, 0x40);

            WriteProcessMemory(hProc, alloc, pathBytes, (IntPtr)pathBytes.Length, out _);
            IntPtr loadLib = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            IntPtr hThread = CreateRemoteThread(hProc, IntPtr.Zero, 0, loadLib, alloc, 0, IntPtr.Zero);

            WaitForSingleObject(hThread, 0xFFFFFFFF);
            CloseHandle(hThread);
            CloseHandle(hProc);
        }
    }

    [Flags]
    public enum ProcessAccessFlags : uint
    {
        AllAccess = 0x001F0FFF
    }

    [Flags]
    public enum ThreadAccess : int
    {
        SUSPEND_RESUME = 0x0002
    }
}