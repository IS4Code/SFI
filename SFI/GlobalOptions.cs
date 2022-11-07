using System.Diagnostics;

namespace IS4.SFI
{
    /// <summary>
    /// Global properties affecting the application.
    /// </summary>
    public static class GlobalOptions
    {
        static bool? suppressErrors;

        /// <summary>
        /// Whether to ignore recoverable exceptions; by default the exceptions
        /// are not ignored only when <see cref="Debugger.IsAttached"/> is true.
        /// </summary>
        public static bool SuppressNonCriticalExceptions {
            get {
                return suppressErrors ?? !Debugger.IsAttached;
            }
            set {
                suppressErrors = value;
            }
        }
    }
}
