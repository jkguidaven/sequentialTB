using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CoreDLL.logworker
{

    public enum eLogTask { INITIALIZE, UNINITIALIZE, TRACE_JOB1 }


    public enum TraceLevel { NO_TRACE = -1, L1 = 0, L2 = 1, L3 = 2 , L4 = 3 }

    public struct PrintJobStruct
    {
        public String[]     m_arrstrPrintables;
        public StackFrame   m_clsSFrame;
        public DateTime     m_clsDTime;

        public PrintJobStruct(StackFrame clsSFrame, DateTime clsDTime, params String[] m_arrstrPrintables)
        {
            this.m_arrstrPrintables = m_arrstrPrintables;
            this.m_clsSFrame = clsSFrame;
            this.m_clsDTime = clsDTime;
        }
    }
}
