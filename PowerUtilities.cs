using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace DontSleep
{
    public class PowerUtilities
    {
        [Flags]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint SetThreadExecutionState(EXECUTION_STATE esFlags);

        private static readonly AutoResetEvent _event = new AutoResetEvent(false);

        public static void PreventPowerSave()
        {
            (new TaskFactory()).StartNew(() =>
            {
                SetThreadExecutionState(
                    EXECUTION_STATE.ES_CONTINUOUS
                    | EXECUTION_STATE.ES_DISPLAY_REQUIRED
                    | EXECUTION_STATE.ES_SYSTEM_REQUIRED);
                _event.WaitOne();
            },
                TaskCreationOptions.LongRunning);
        }

        public static void Shutdown()
        {
            _event.Set();
        }

        /// <summary>
        /// Alternative way to prevent power save
        /// </summary>
        public static void PreventPowerSaveViaTimer()
        {
            using var timer = new Timer((s) =>
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED |
                                        EXECUTION_STATE.ES_CONTINUOUS |
                                        EXECUTION_STATE.ES_SYSTEM_REQUIRED);
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            // execute after timer stop?
            //SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }

        public static void ToggleGoodbye(bool state)
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true);
            var newValue = Convert.ToInt32(state);
            var keyValue = key?.GetValue("EnableGoodbye") ?? newValue;
            if ((int)keyValue != newValue)
            {
                key.SetValue("EnableGoodbye", newValue);
            }
        }
    }
}