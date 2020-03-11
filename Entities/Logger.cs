using log4net;

namespace Entities
{
    /// <summary>
    /// This class contains a logger instance used to access the log4net API
    /// </summary>
    public class Logger
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// ILog type interface
        /// </summary>
        public static ILog Log => log;
    }
}
