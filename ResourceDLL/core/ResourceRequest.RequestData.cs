using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResourceDLL.core
{
    public class RequestLogData
    {
        private String           m_strLogLineData = null;
        private PatternObjStruct m_posLogPattern;
        private List<AttributeStruct> m_lstAttribute = null;

        public RequestLogData(String strLogLineData)
        {
            m_strLogLineData = strLogLineData;
            m_lstAttribute = new List<AttributeStruct>();

        }

        public void setPattern(PatternObjStruct LogPattern)
        {
            m_posLogPattern = LogPattern;
        }

        public PatternObjStruct LogPattern { get { return m_posLogPattern; } }
        public String value { get { return m_strLogLineData; } set { m_strLogLineData = value; } }

        public void addAttribute(String Name, String Value)
        {
            m_lstAttribute.Add(new AttributeStruct(Name, Value));
        }


        public AttributeStruct[] getAttributes()
        {
            return m_lstAttribute.ToArray();
        }
    }

    public struct AttributeStruct
    {
        public String Name;
        public String Value;
        public AttributeStruct(String Name, String Value)
        {
            this.Name = Name;
            this.Value = Value;
        }
    }
}
