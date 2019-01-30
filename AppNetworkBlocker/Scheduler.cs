using System;
using System.Collections.Generic;

namespace AppNetworkBlocker
{
    class Scheduler
    {
        /// <summary>
        /// Current UNIX time
        /// </summary>
        private static long now = 0;
        /// <summary>
        /// List of functions and the last 
        /// UNIX time when they were called
        /// </summary>
        private static Dictionary<string, long> functions = new Dictionary<string, long>();

        /// <summary>
        /// Update current time to Now
        /// </summary>
        public static void Refresh() {
            now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); 
        }

        /// <summary>
        /// Checks if the given function can be called
        /// again, because the wait time has elapsed
        /// </summary>
        /// <param name="function">Function name</param>
        /// <param name="delay">Time to wait before the function can be executed again</param>
        /// <returns></returns>
        public static bool isReady(string function, int delay) {
            if (!functions.ContainsKey(function)) {
                functions.Add(function, now);
                return true;
            }
            if (functions[function] + delay < now)
            {
                functions[function] = now;
                return true;
            }
            return false;
        }
    }
}
