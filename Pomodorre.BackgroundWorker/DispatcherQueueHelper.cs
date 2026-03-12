using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

namespace Pomodorre.BackgroundWorker
{
    public static class DispatcherQueueHelper
    {
        [System.Runtime.InteropServices.DllImport("CoreMessaging.dll")]
        private static extern int CreateDispatcherQueueController(DispatcherQueueOptions options, out IntPtr dispatcherQueueController);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct DispatcherQueueOptions { public int dwSize; public int threadType; public int apartmentType; }

        public static object CreateDispatcherQueue()
        {
            DispatcherQueueOptions options = new()
            {
                dwSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(DispatcherQueueOptions)),
                threadType = 2, // DQTYPE_THREAD_CURRENT
                apartmentType = 2 // DQTAT_COM_STA
            };
            CreateDispatcherQueueController(options, out _);
            return DispatcherQueue.GetForCurrentThread();
        }
    }
}
