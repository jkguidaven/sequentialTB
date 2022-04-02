using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResourceDLL.core
{
    public class ParseDataObj
    {
        private String m_strParseDataID = null;
        private String m_strParseDataStartRegex = null;
        private String m_strParseDataEndRegex = null;
        public ParseDataObj(String strParseDataID, String strParseDataStartRegex,String strParseDataEndRegex)
        {
            m_strParseDataID = strParseDataID;
            m_strParseDataStartRegex = strParseDataStartRegex;
            m_strParseDataEndRegex = strParseDataEndRegex;
        }


        public String getParseDataID()    { return m_strParseDataID;  }
        public String getParseDataStartRegex() { return m_strParseDataStartRegex; }
        public String getParseDataEndRegex() { return m_strParseDataEndRegex; }
    }
}
