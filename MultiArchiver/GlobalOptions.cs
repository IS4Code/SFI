using System.Diagnostics;

namespace IS4.MultiArchiver
{
    public static class GlobalOptions
    {
        static bool? suppressErrors;

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
