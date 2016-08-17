/*
 * Source
 * http://stackoverflow.com/questions/4963135/wpf-inactivity-and-activity
 * User Timothy Khouri
 */

using System;
using System.Runtime.InteropServices;

namespace PasswordSafe
{
    public static class IdleTimeDetector
    {
        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref Lastinputinfo plii);

        public static IdleTimeInfo GetIdleTimeInfo()
        {
            int systemUptime = Environment.TickCount;
            int idleTicks = 0;

            Lastinputinfo lastInputInfo = new Lastinputinfo();
            lastInputInfo.CbSize = (uint) Marshal.SizeOf(lastInputInfo);
            lastInputInfo.DwTime = 0;

            if (GetLastInputInfo(ref lastInputInfo))
            {
                int lastInputTicks = (int) lastInputInfo.DwTime;

                idleTicks = systemUptime - lastInputTicks;
            }

            return new IdleTimeInfo
            {
                LastInputTime = DateTime.Now.AddMilliseconds(-1 * idleTicks),
                IdleTime = new TimeSpan(0, 0, 0, 0, idleTicks),
                SystemUptimeMilliseconds = systemUptime
            };
        }
    }

    public class IdleTimeInfo
    {
        public DateTime LastInputTime { get; internal set; }

        public TimeSpan IdleTime { get; internal set; }

        public int SystemUptimeMilliseconds { get; internal set; }
    }

    internal struct Lastinputinfo
    {
        public uint CbSize;
        public uint DwTime;
    }
}