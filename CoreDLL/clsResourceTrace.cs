using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreDLL.logworker;

namespace ResourceDLL.core
{
    public abstract class clsResourceTrace
    {
        #region MACRO
        protected static readonly TraceLevel L1 = TraceLevel.L1;
        protected static readonly TraceLevel L2 = TraceLevel.L2;
        protected static readonly TraceLevel L3 = TraceLevel.L3;
        protected static readonly TraceLevel L4 = TraceLevel.L4;
        private   static          TraceLevel DEFAULT_LEVEL = L1;
        #endregion

        #region TRACE_METHOD
        private static LogWorker m_LWLogger = LogWorker.prtyclsLWInstance;
        public  static void trace(TraceLevel eTLevel, params String[] arrstrTraces)
        {
            m_LWLogger.trace(eTLevel, arrstrTraces);
        }

        public static void trace(params String[] arrstrTraces)
        {
            m_LWLogger.trace(DEFAULT_LEVEL, arrstrTraces);
        }

        public static void trace(Object str)
        {
            trace(str.ToString());
        }
        #endregion

        #region CONSTRUCTOR
        public clsResourceTrace(TraceLevel Level)
        {
            DEFAULT_LEVEL = Level;
        }
        #endregion
    }
}
