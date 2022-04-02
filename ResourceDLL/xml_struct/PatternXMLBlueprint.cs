using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceDLL.Manager;
using System.Xml;
using ResourceDLL.core;

namespace ResourceDLL.xml_struct.patternXML
{
    class PatternXMLBluePrint : XMLBlueprint
    {
        public PatternsRoot m_PRoot = new PatternsRoot();



        public override void ProcessBluePrint(ResourceManager clsResManager)
        {
            foreach (PatternNode node in m_PRoot.patterns)
            {
                PatternObjStruct new_pattern = new PatternObjStruct(node.getPatternName(),
                                                                     node.getRegEx(),
                                                                     node.getPriority(),
                                                                     clsResManager.getProcedure(node.getProcedureName()));

                foreach (ParseDataNode ParseDataNode in node.ParseTableNode.lstDataNode)
                {
                    ParseDataObj ParseData = null;
                    if((ParseData = clsResManager.getParseData(ParseDataNode.getParseDataID())) == null)
                        throw new XmlException("Invalid ParseData!,ParseData['" + ParseDataNode.getParseDataID()+ "'] does not exit on ParseTable.");
                    else
                        new_pattern.ParseDataTable.Add(ParseData);
                }

                clsResManager.PatternList.Add(new_pattern);
            }

            clsResManager.PatternList.OrderBy(o => o.PatternPriority);
        }

    }


    class PatternsRoot : RootableXMLNode
    {

        public List<PatternNode> patterns = new List<PatternNode>();


        public override String getName() { return "patterns"; }

        public override void processXMLContent(String XMLNodeBody)
        {
        }

    }


    class PatternNode : RootableXMLNode, iParameterizeXMLNode 
    {
        public List<RegexNode> lstRegexNodes = new List<RegexNode>();
        public ParseTableNode ParseTableNode = new ParseTableNode();
        public ProcedureNode ProcedureNode = new ProcedureNode();
        public override String getName() { return "pattern"; }


        private int m_nPriority = 1;
        private String m_strPatternName = null;
        private String m_strFinalRegexVal = null;

        public int      getPriority()       { return m_nPriority;       }
        public String   getPatternName()    { return m_strPatternName;  }
        public String   getRegEx()          { return m_strFinalRegexVal;}
        public String   getProcedureName()      {
            return ProcedureNode.getProcName();
        }

        public override void processXMLContent(String XMLContent)
        {
            String strFinalRegexVal = "";

            foreach (RegexNode regex in lstRegexNodes)
                strFinalRegexVal += regex.getRegexVal();

            try
            {
                new System.Text.RegularExpressions.Regex(strFinalRegexVal);
                m_strFinalRegexVal = strFinalRegexVal;
            }
            catch (Exception e) { 
                throw new Exception("Invalid Regex Value found!,exception=" + e.Message); 
            }

        }

        public void processXMLNodeParameters(params XMLParamStruct[] parameters)
        {
            Boolean bFoundName = false;
            if (parameters.Length > 2)
                throw new XmlException("Invalid Pattern-node syntax!, unknown parameter count.");
            else
            {
                foreach(XMLParamStruct param in parameters)
                    if(param.m_strParam.Equals("name")) {
                        bFoundName = true;
                        m_strPatternName = param.m_strVal;
                    } else if(param.m_strParam.Equals("priority"))
                        m_nPriority =  Convert.ToInt32(param.m_strVal);
                    else
                        throw new XmlException("Invalid Pattern-node syntax!, unknown parameter[" + param.m_strParam 
                                              +"] found.");
            }


            if (!bFoundName)
                throw new XmlException("Invalid Pattern-node syntax!, name-parameter not found.");
        }
    }


    class ParseTableNode : RootableXMLNode
    {

        public List<ParseDataNode> lstDataNode = new List<ParseDataNode>();
        public override String getName() { return "parse-table"; }

        public override void processXMLContent(String XMLContent)
        {
        }
    }

    class ParseDataNode : XMLNode, iParameterizeXMLNode
    {
        private String m_strParseDataID = null;
        public override String getName() { return "parse-data"; }

        public void processXMLNodeParameters(params XMLParamStruct[] parameters)
        {
            Boolean bFoundName = false;
            if (parameters.Length != 1)
                throw new XmlException("Invalid parse-data syntax!, unknown parameter count.");
            else
                if (parameters[0].m_strParam.Equals("name"))
                {
                    m_strParseDataID = parameters[0].m_strVal;
                    bFoundName = true;
                }

            

            if(!bFoundName)
                throw new XmlException("Invalid parse-data syntax!, name-parameter not found.");
        }

        public String getParseDataID() { return m_strParseDataID; }
    }


    class RegexNode : XMLNode , iParameterizeXMLNode
    {
        public override String getName() { return "regex"; }

        public String strRegexVal = null;


        public void processXMLNodeParameters(params XMLParamStruct[] parameters)
        {
            Boolean bFoundValue = false;

            if (parameters.Length > 1 || parameters.Length < 1)
                throw new XmlException("Invalid Regex syntax!, unknown parameter count.");
            else
                if (parameters[0].m_strParam.Equals("value"))
                    bFoundValue = true;

            if(!bFoundValue)
                throw new XmlException("Invalid Regex syntax!, value-parameter not found.");


            strRegexVal = parameters[0].m_strVal;

        }



        public String getRegexVal() { return strRegexVal; }
    }

    class ProcedureNode : XMLNode, iParameterizeXMLNode
    {
        public override String getName() { return "procedure"; }

        private String m_strProcName = null;


        public void processXMLNodeParameters(params XMLParamStruct[] parameters)
        {
            Boolean bFoundValue = false;

            if (parameters.Length > 1 || parameters.Length < 1)
                throw new XmlException("Invalid procedure-node syntax!, unknown parameter count.");
            else
                if (parameters[0].m_strParam.Equals("name"))
                    bFoundValue = true;

            if (!bFoundValue)
                throw new XmlException("Invalid procedure-node syntax!, name-parameter not found.");


            if (!ResourceManager.prtyclsLWInstance.isProcedureExist(parameters[0].m_strVal))
                throw new XmlException("Invalid procedure-node syntax!, Procedure[" + parameters[0].m_strVal 
                                     + "] is non-existence.");

            m_strProcName = parameters[0].m_strVal;
        }


        public String getProcName() { return m_strProcName; }
    }
}
