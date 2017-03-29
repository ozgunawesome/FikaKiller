using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;

namespace FikaKiller
{
    class Program
    {
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImportAttribute("User32", EntryPoint = "GetLastInputInfo", SetLastError = true)]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImportAttribute("User32", EntryPoint = "LockWorkStation", SetLastError = true)]
        private static extern bool LockWorkStation();

        private static uint GetLastInputTime()
        {
            LASTINPUTINFO info = new LASTINPUTINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            GetLastInputInfo(ref info);
            return info.dwTime;
        }

        private static void DeviceChangeEvent(object sender, EventArrivedEventArgs e)
        {
            var firstInput = GetLastInputTime();
            Thread.Sleep(2000);
            if (GetLastInputTime() <= firstInput)
            {
                LockWorkStation();
            }
        }
        static void Main(string[] args)
        {
            var watcher = new ManagementEventWatcher();
            watcher.EventArrived += new EventArrivedEventHandler(DeviceChangeEvent);
            watcher.Query = new WqlEventQuery(@"select * from __InstanceDeletionEvent within 1
                                                    where TargetInstance ISA 'Win32_PnPEntity'
                                                    and TargetInstance.Description='Apple iPhone'");
            watcher.Start();
            Thread.Sleep(999999999);
        }
    }
}
