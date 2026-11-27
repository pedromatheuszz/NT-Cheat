using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web.UI;
using static Scarlet.FastMemory;

namespace Scarlet
{
    public class FastMemory
    {
        //ALL CREDITS >>>> FINISHU (THE BEST DEVELOPER)
        #region Imports
        [DllImport("kernel32.dll")]
        static extern int VirtualQueryEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            out MEMORY_BASIC_INFORMATION lpBuffer,
            uint dwLength
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int dwSize,
            IntPtr lpNumberOfBytesRead
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int dwSize,
            IntPtr lpNumberOfBytesWritten
        );

        [DllImport("kernel32.dll")]
        private static extern void GetSystemInfo(
            out SYSTEM_INFO lpSystemInfo
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
            int dwDesiredAccess,
            bool bInheritHandle,
            int dwProcessId
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(
            IntPtr hObject
        );
        #endregion
        #region Global
        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }
        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        private IntPtr pHandle;

        [Flags]
        public enum MemoryState : uint
        {
            MEM_COMMIT = 0x1000,
            MEM_FREE = 0x10000,
            MEM_RESERVE = 0x2000
        }

        [Flags]
        public enum MemoryProtection : uint
        {
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400
        }

        public struct SYSTEM_INFO
        {
            public ushort processorArchitecture;
            private ushort reserved;
            public uint pageSize;
            public UIntPtr minimumApplicationAddress;
            public UIntPtr maximumApplicationAddress;
            public IntPtr activeProcessorMask;
            public uint numberOfProcessors;
            public uint processorType;
            public uint allocationGranularity;
            public ushort processorLevel;
            public ushort processorRevision;
        }

        #endregion
        #region Functions
        public bool OpenProcess(string procName)
        {
            if (procName.Contains(".exe"))
                procName = procName.Replace(".exe", "");
            Int32 procId = Process.GetProcessesByName(procName).FirstOrDefault().Id;

            pHandle = OpenProcess(PROCESS_ALL_ACCESS, false, procId);

            if (pHandle != IntPtr.Zero)
            {
                return true;
            }
            return false;
        }

        public bool OpenProcess(Int32 procId)
        {
            pHandle = OpenProcess(PROCESS_ALL_ACCESS, false, procId);

            if (pHandle != IntPtr.Zero)
            {
                return true;
            }
            return false;
        }

        public T ReadMemory<T>(IntPtr Address)
        {
            object obj = null;

            if (typeof(T) == typeof(int))
            {
                byte[] bytes = new byte[4];
                ReadProcessMemory(pHandle, Address, bytes, sizeof(int), IntPtr.Zero);
                obj = BitConverter.ToInt32(bytes, 0);
            }
            else if (typeof(T) == typeof(float))
            {
                byte[] bytes = new byte[4];
                ReadProcessMemory(pHandle, Address, bytes, sizeof(float), IntPtr.Zero);
                obj = BitConverter.ToInt32(bytes, 0);
            }
            else if (typeof(T) == typeof(double))
            {
                byte[] bytes = new byte[8];
                ReadProcessMemory(pHandle, Address, bytes, sizeof(double), IntPtr.Zero);
                obj = BitConverter.ToInt32(bytes, 0);
            }
            else
            {
                throw new NotSupportedException($"Tipo {typeof(T)} não é suportado.");
            }

            T result = (T)Convert.ChangeType(obj, typeof(T));

            return result;
        }
        public Task<IEnumerable<long>> PatternScan(long start, long end, string aob, bool readable = true, bool writable = true)
        {
            return Task.Run<IEnumerable<long>>(() =>
            {
                string[] aoblist = aob.Split(' ');
                byte[] patternBytes = new byte[aoblist.Length];
                byte[] maskBytes = new byte[aoblist.Length];

                for (int i = 0; i < aoblist.Length; i++)
                {
                    string token = aoblist[i];

                    if (token == "??" || token == "?")
                    {
                        patternBytes[i] = 0x00;
                        maskBytes[i] = 0x00;
                    }
                    else if (token.Length == 2 && token.Contains("?"))
                    {
                        if (token[0] == '?')
                        {
                            patternBytes[i] = (byte)(Convert.ToByte(token[1].ToString(), 16));
                            maskBytes[i] = 0x0F;
                        }
                        else if (token[1] == '?')
                        {
                            patternBytes[i] = (byte)(Convert.ToByte(token[0].ToString(), 16) << 4);
                            maskBytes[i] = 0xF0;
                        }
                    }
                    else
                    {
                        patternBytes[i] = Convert.ToByte(token, 16);
                        maskBytes[i] = 0xFF;
                    }
                }

                Debug.WriteLine("[ SCARLET ] : Starting AOB SCAN");
                Stopwatch sw = Stopwatch.StartNew();

                List<MEMORY_BASIC_INFORMATION> regions = new List<MEMORY_BASIC_INFORMATION>();
                IntPtr currentAddress = new IntPtr(start);

                while ((ulong)currentAddress.ToInt64() < (ulong)end)
                {
                    if (VirtualQueryEx(pHandle, currentAddress, out MEMORY_BASIC_INFORMATION mbi, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))) == 0)
                        break;

                    bool canScan = mbi.State == (uint)MemoryState.MEM_COMMIT;

                    if (readable && writable)
                        canScan &= mbi.Protect == (uint)MemoryProtection.PAGE_READWRITE;
                    else if (readable)
                        canScan &= mbi.Protect == (uint)MemoryProtection.PAGE_READONLY || mbi.Protect == (uint)MemoryProtection.PAGE_EXECUTE_READ;
                    else
                        canScan &= mbi.Protect == (uint)MemoryProtection.PAGE_READWRITE;

                    if (canScan)
                        regions.Add(mbi);

                    currentAddress = new IntPtr(mbi.BaseAddress.ToInt64() + mbi.RegionSize.ToInt64());
                }

                ConcurrentBag<long> result = new ConcurrentBag<long>();

                Parallel.ForEach(regions, mbi =>
                {
                    long regionSize = mbi.RegionSize.ToInt64();
                    if (regionSize > int.MaxValue)
                        regionSize = int.MaxValue;

                    byte[] memoryBytes = new byte[regionSize];
                    if (!ReadProcessMemory(pHandle, mbi.BaseAddress, memoryBytes, memoryBytes.Length, IntPtr.Zero))
                    {
                        Debug.WriteLine("[ SCARLET ] : Falha ao ler memória em " + mbi.BaseAddress);
                        currentAddress = new IntPtr(mbi.BaseAddress.ToInt64() + mbi.RegionSize.ToInt64());
                        return;
                    }

                    for (int i = 0; i <= memoryBytes.Length - patternBytes.Length; i++)
                    {
                        bool match = true;

                        for (int j = 0; j < patternBytes.Length; j++)
                        {
                            if ((memoryBytes[i + j] & maskBytes[j]) != (patternBytes[j] & maskBytes[j]))
                            {
                                match = false;
                                break;
                            }
                        }

                        if (match)
                        {
                            long foundAddress = mbi.BaseAddress.ToInt64() + i;
                            result.Add(foundAddress);
                        }
                    }
                });

                sw.Stop();
                Debug.WriteLine($"[ SCARLET ] : Found {result.Count} Address(es) in {sw.Elapsed.TotalSeconds:F2}");
                return result.ToArray();
            });
        }
        /*
        public Task<IEnumerable<long>> PatternScan(long start, long end, string aob, bool readable = true, bool writable = true)
        {
            return Task.Run<IEnumerable<long>>(() =>
            {
                string[] aoblist = aob.Split(' ');
                byte[] patternBytes = new byte[aoblist.Length];
                byte[] maskBytes = new byte[aoblist.Length];

                for (int i = 0; i < aoblist.Length; i++)
                {
                    string token = aoblist[i];
                    if (token == "??" || token == "?")
                    {
                        patternBytes[i] = 0x00;
                        maskBytes[i] = 0x00;
                    }
                    else if (token.Length == 2 && token.Contains("?"))
                    {
                        if (token[0] == '?')
                        {
                            patternBytes[i] = (byte)Convert.ToByte(token[1].ToString(), 16);
                            maskBytes[i] = 0x0F;
                        }
                        else if (token[1] == '?')
                        {
                            patternBytes[i] = (byte)(Convert.ToByte(token[0].ToString(), 16) << 4);
                            maskBytes[i] = 0xF0;
                        }
                    }
                    else
                    {
                        patternBytes[i] = Convert.ToByte(token, 16);
                        maskBytes[i] = 0xFF;
                    }
                }

                Debug.WriteLine("[ SCARLET ] : Starting AOB SCAN");
                Stopwatch sw = Stopwatch.StartNew();

                List<MEMORY_BASIC_INFORMATION> regions = new List<MEMORY_BASIC_INFORMATION>();
                IntPtr currentAddress = new IntPtr(start);

                while ((ulong)currentAddress.ToInt64() < (ulong)end)
                {
                    if (VirtualQueryEx(pHandle, currentAddress, out MEMORY_BASIC_INFORMATION mbi, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))) == 0)
                        break;

                    bool canScan = mbi.State == (uint)MemoryState.MEM_COMMIT;

                    if (readable && writable)
                        canScan &= mbi.Protect == (uint)MemoryProtection.PAGE_EXECUTE_READWRITE;
                    else if (readable)
                        canScan &= mbi.Protect == (uint)MemoryProtection.PAGE_EXECUTE_READ;
                    else
                        canScan &= mbi.Protect == (uint)MemoryProtection.PAGE_READWRITE;

                    if (canScan)
                        regions.Add(mbi);

                    currentAddress = new IntPtr(mbi.BaseAddress.ToInt64() + mbi.RegionSize.ToInt64());
                }

                ConcurrentBag<long> foundAddresses = new ConcurrentBag<long>();

                Parallel.ForEach(regions, mbi =>
                {
                    try
                    {
                        long regionSize = mbi.RegionSize.ToInt64();
                        if (regionSize > int.MaxValue)
                            regionSize = int.MaxValue;

                        byte[] buffer = new byte[regionSize];

                        bool readSuccess = ReadProcessMemory(pHandle, mbi.BaseAddress, buffer, buffer.Length, IntPtr.Zero);

                        if (!readSuccess)
                        {
                            Debug.WriteLine($"[ SCARLET ] : Falha ao ler memória em {mbi.BaseAddress}");
                            return;
                        }

                        for (int i = 0; i <= buffer.Length - patternBytes.Length; i++)
                        {
                            bool match = true;
                            for (int j = 0; j < patternBytes.Length; j++)
                            {
                                if ((buffer[i + j] & maskBytes[j]) != (patternBytes[j] & maskBytes[j]))
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if (match)
                                foundAddresses.Add(mbi.BaseAddress.ToInt64() + i);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[ SCARLET ] : Exceção ao processar região: {ex.Message}");
                    }
                });

                sw.Stop();
                Debug.WriteLine($"[ SCARLET ] : Found {foundAddresses.Count} Address(es) in {sw.Elapsed.TotalSeconds:F2}");
                return foundAddresses.ToArray();
            });
        }
        */

        public async Task<IEnumerable<long>> AobScan(string search, bool readable = true, bool writable = true)
        {
            SYSTEM_INFO system_INFO = new SYSTEM_INFO();
            GetSystemInfo(out system_INFO);
            return await PatternScan(0L, long.MaxValue, search, readable, writable);
            //return await PatternScan((long)system_INFO.minimumApplicationAddress, (long)system_INFO.maximumApplicationAddress, search, readable, writable);
        }

        public bool WriteMemory(string Address, string type, string replace)
        {
            try
            {
                IntPtr lpBaseAddress = (IntPtr)Convert.ToInt64(Address, 16);
                byte[] bytes = new byte[4];
                int size = 4;

                switch (type.ToLower())
                {
                    case "int":
                        bytes = BitConverter.GetBytes(Convert.ToInt32(replace, 16));
                        size = 4;
                        break;
                    case "double":
                        bytes = BitConverter.GetBytes(Convert.ToDouble(replace));
                        size = 8;
                        break;
                    case "float":
                        bytes = BitConverter.GetBytes(Convert.ToSingle(replace));
                        size = 4;
                        break;
                    case "byte":
                        bytes = new byte[]
                            {
                            Convert.ToByte(replace, 16)
                            };
                        size = 1;
                        break;
                    case "bytes":
                        bytes = TransformToArrayOfBytes(replace, out size);
                        break;
                    default:
                        throw new Exception("Tipo de dado desconhecido.");
                }
                return WriteProcessMemory(pHandle, lpBaseAddress, bytes, size, IntPtr.Zero);
            }
            catch (FormatException ex)
            {
                Debug.WriteLine($"Erro de formatação no valor '{replace}' para tipo '{type}': {ex.Message}");
                throw;
            }
        }

        private byte[] TransformToArrayOfBytes(string array)
        {
            string[] array2 = { };
            if (array.Contains(" "))
            {
                array2 = array.Split(' ');
            }
            else
            {
                array2 = new string[] { array };
            }
            byte[] result = new byte[array.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(array2[i], 16);
            }
            return result;
        }

        private byte[] TransformToArrayOfBytes(string array, out int size)
        {
            string[] array2 = new string[0];
            if (array.Contains(" "))
            {
                array2 = array.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                array2 = new string[] { array };
            }
            byte[] result = new byte[array2.Length];
            for (int i = 0; i < array2.Length; i++)
            {
                result[i] = Convert.ToByte(array2[i], 16);
            }
            size = array2.Length;
            return result;
        }

        public void CloseProcess()
        {
            CloseHandle(pHandle);
        }
        #endregion
    }
}
