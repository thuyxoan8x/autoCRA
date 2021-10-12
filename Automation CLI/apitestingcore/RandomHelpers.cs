using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace EPiServer.Automation.APITestingCore
{
    /// <summary>
    /// Helper for random data
    /// </summary>
    public class RandomHelpers
    {
        /// <summary>
        /// Create a random string with a given text
        /// </summary>
        /// <param name="start">The start of a random string</param>
        /// <returns></returns>
        public static string RandomString(string start)
        {
            return start + new Random().Next(10, 100000) + Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// Create a string from current timestamp
        /// </summary>
        /// <param name="format">String format for DateTime</param>
        /// <returns></returns>
        public static string GetCurrentTimestamp(string format = "yyyyMMddHHmmssffff")
        {
            return DateTime.Now.ToString(format);
        }

        /// <summary>
        /// Get utc time to the same format when getting data from API
        /// </summary>
        /// <returns></returns>
        public static string GetNowUTCTime()
        {
            var now = DateTime.UtcNow.ToString("O");
            return Regex.Replace(now, "0+?Z", "Z");
        }
    }
}
