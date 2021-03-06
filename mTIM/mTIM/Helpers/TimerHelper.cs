using System;
using System.Diagnostics;
using System.Threading;

namespace mTIM.Helpers
{
    public class TimerHelper : IDisposable
    {
        private const int dueTime = 1000;
        private TimerCallback _callback;
        public static TimerHelper _instance;
        public static TimerHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TimerHelper();
                }

                return _instance;
            }
        }

        Timer timer;
        public void Create(TimerCallback callback)
        {
            _callback = callback;
        }

        public void StartTimer()
        {
            //if (timer == null)
            //{
            //    // Create a timer that invokes CheckStatus after one second, 
            //    Debug.WriteLine("{0:h:mm:ss.fff} Creating timer.\n", DateTime.Now);
            //    timer = new Timer(_callback, this, 0, dueTime);
            //}
        }

        public void StopTimer()
        {
            timer?.Dispose();
            timer = null;
        }

        public void Dispose() {
            timer?.Dispose();
            timer = null;
            Debug.WriteLine("\nDestroying timer.");
        }
    }
}
