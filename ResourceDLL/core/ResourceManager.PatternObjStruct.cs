using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResourceDLL.core
{
    public class PatternObjStruct
    {
        private string      m_strPatternID;
        private string      m_strPatternRegEx;
        private int         m_nPriorityLevel;
        private ProcedureWrapper  m_ptrProcedure;
        private List<ParseDataObj> m_lstPtrParseDataObj;

        public PatternObjStruct(string strPatternID, 
                                string strPatternRegEx, 
                                int nPriorityLevel, 
                                ProcedureWrapper ptrProcedure)
        {
            m_nPriorityLevel = nPriorityLevel;
            m_strPatternID = strPatternID;
            m_strPatternRegEx = strPatternRegEx;
            m_ptrProcedure = ptrProcedure;
            m_lstPtrParseDataObj = new List<ParseDataObj>();
        }


        public String PatternID             { get { return m_strPatternID;      } }
        public String PatternRegEx          { get { return m_strPatternRegEx;   } }
        public int PatternPriority          { get { return m_nPriorityLevel;    } }
        public ProcedureWrapper PatternProcedure { get { return m_ptrProcedure;      } }
        public List<ParseDataObj> ParseDataTable { get { return m_lstPtrParseDataObj; } }

    }
}
