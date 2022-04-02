using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceDLL.Manager;
using System.Xml;
using ResourceDLL.core;

namespace ResourceDLL.xml_struct.ParseTableXML
{
    class ParseTableXMLBlueprint : XMLBlueprint
    {
        public ParseRoot m_PRoot = new ParseRoot();



        public override void ProcessBluePrint(ResourceManager clsResManager)
        {
            foreach(ParseDataNode node in m_PRoot.lstParseData)
            {
                foreach(ParseDataObj ParseData in clsResManager.ParseTable)
                    if(node.getParseDataID().Equals(ParseData.getParseDataID()))
                      throw new XmlException("Found duplicate ParseData[" + node.getParseDataID() + "]");

                clsResManager.ParseTable.Add(new ParseDataObj(node.getParseDataID(),
                                            node.getParseDataStartRegex(),node.getParseDataEndRegex()));
            }
        }
    }


    class ParseRoot : RootableXMLNode
    {

        public List<ParseDataNode> lstParseData = new List<ParseDataNode>();
        public override String getName() { return "parse-table"; }


        public override void processXMLContent(String XMLContent)
        {
        }
    }


    class ParseDataNode : XMLNode, iParameterizeXMLNode
    {
        private String m_strParseDataID = null;
        private String m_strParseDataStartRegex = null;
        private String m_strParseDataEndRegex = null;
        public override String getName() { return "parse-data"; }

        public void processXMLNodeParameters(params XMLParamStruct[] parameters)
        {
            Boolean bFoundName = false;
            Boolean bFoundStartRegex = false;
            Boolean bFoundEndRegex = false;
            Boolean bFoundRegex = false;
            
            
            if (parameters.Length < 2 && parameters.Length > 3)
                throw new XmlException("Invalid parse-data-node syntax!, unknown parameter count.");

            for(int i=0;i<parameters.Length;i++){
                if (parameters[i].m_strParam.Equals("name"))
                {
                    m_strParseDataID = parameters[i].m_strVal;
                    bFoundName = true;
                }
                else if (parameters[i].m_strParam.Equals("start-regex"))
                {
                    try
                    {
                        new System.Text.RegularExpressions.Regex(parameters[i].m_strVal);
                        m_strParseDataStartRegex = parameters[i].m_strVal;
                    }
                    catch (Exception e)
                    {
                        throw new XmlException("Invalid Regex Value found!,exception=" + e.Message);
                    }
                    bFoundStartRegex = true;

                    if (bFoundEndRegex && m_strParseDataEndRegex.Equals(m_strParseDataStartRegex))
                        throw new XmlException("Invalid Regex Value found!Start and End Regex is equal!");
                }
                else if (parameters[i].m_strParam.Equals("end-regex"))
                {
                    try
                    {
                        new System.Text.RegularExpressions.Regex(parameters[i].m_strVal);
                        m_strParseDataEndRegex = parameters[i].m_strVal;
                    }
                    catch (Exception e)
                    {
                        throw new XmlException("Invalid Regex Value found!,exception=" + e.Message);
                    }
                    bFoundEndRegex = true;
                    if (bFoundStartRegex && m_strParseDataStartRegex.Equals(m_strParseDataEndRegex))
                        throw new XmlException("Invalid Regex Value found!Start and End Regex is equal!");
                }
                else if (parameters[i].m_strParam.Equals("regex"))
                {
                    try
                    {
                        new System.Text.RegularExpressions.Regex(parameters[i].m_strVal);
                        m_strParseDataStartRegex = m_strParseDataEndRegex = parameters[i].m_strVal;
                    }
                    catch (Exception e)
                    {
                        throw new XmlException("Invalid Regex Value found!,exception=" + e.Message);
                    }
                    bFoundRegex = true;
                }
            }

            if(!bFoundName || (bFoundRegex && (bFoundEndRegex || bFoundStartRegex)))
                throw new XmlException("Invalid parse-data-node syntax!, missing name or regex value or Incorrect regex combination.");
        }

        public String getParseDataID() { return m_strParseDataID; }
        public String getParseDataStartRegex() { return m_strParseDataStartRegex; }
        public String getParseDataEndRegex() { return m_strParseDataEndRegex; }
    }


}
