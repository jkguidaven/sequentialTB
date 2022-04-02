using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceDLL.Manager;
using System.Xml;
using ResourceDLL.core;
using System.Reflection;
using CoreDLL.utilities;

namespace ResourceDLL.xml_struct.ProcedureXML
{
    class ProcedureXMLBlueprint : XMLBlueprint 
    {
        public ProceduresXMLNode clsXProcNode = new ProceduresXMLNode();

        public override void ProcessBluePrint(ResourceManager clsResManager)
        {
            foreach (ProcXMLNode ProcXMLNode in clsXProcNode.lstProcXMLNode)
                clsResManager.ProcedureList.Add(ProcXMLNode.getProcedureWrapper());
        }

    }

    class ProceduresXMLNode : RootableXMLNode
    {
        public List<ProcXMLNode> lstProcXMLNode = new List<ProcXMLNode>();
        public override String getName() { return "Procedures"; }

        public override void processXMLContent(String strXMLContent)
        {
            foreach (ProcXMLNode procXMLNode in lstProcXMLNode)
            {
                foreach (ProcXMLNode ComprprocXMLNode in lstProcXMLNode)
                {
                    if (!procXMLNode.Equals(ComprprocXMLNode) && procXMLNode.getProcedure().Equals(ComprprocXMLNode.getProcedure()))
                    {
                        throw new XmlException("Found duplicate Procedure '" + procXMLNode.getProcedure() + "'!!");
                    }
                }
            }
        }
    }

    class ProcXMLNode : RootableXMLNode , iParameterizeXMLNode
    {
        public override String getName() { return "Procedure"; }

        public ActionListXMLNode ProcActionListXMLNode = new ActionListXMLNode();

        private String m_strProcName = "";
        private int    m_nPriorityNo = 1;
        private ProcedureWrapper ProcWrapper = null;

        public String getProcedure() { return m_strProcName; }
        public int getProcedurePriority() { return m_nPriorityNo; }

        public ProcedureWrapper getProcedureWrapper() { return ProcWrapper; }

        public override void processXMLContent(String strXMLContent)
        {
            List<ProcObj> lstProcObj = ExtractXMLNodeToProcObjType(ProcActionListXMLNode.getActionList());

            ProcWrapper = new ProcedureWrapper(m_strProcName);
            foreach (ProcObj ProcObj in lstProcObj)
                ProcWrapper.add(ProcObj);
        }

        public List<ProcObj> ExtractXMLNodeToProcObjType(Queue<XMLNode> ActionList)
        {
            List<ProcObj> lstProcObj = new List<ProcObj>();

            foreach (XMLNode node in ActionList)
            {
                if (node is clsXMLActionNodeHolder)
                {
                    ProcObjCondition ProcCondition = new ProcObjCondition((node as clsXMLActionNodeHolder).getProcedureType());

                    if (node is IFXMLNode) ProcCondition.setExpression((node as IFXMLNode).getExpression());

                    List<ProcObj> Sub_lstProcObj = ExtractXMLNodeToProcObjType((node as clsXMLActionNodeHolder).getActionList());
                    foreach (ProcObj sub_ProcObj in Sub_lstProcObj)
                        ProcCondition.add(sub_ProcObj);

                    lstProcObj.Add(ProcCondition);
                }
                else
                {
                    ProcObjAction Action = new ProcObjAction((node as ActionXMLNode).getProcedureType(),
                                                             (node as ActionXMLNode).getMethodInfo(),
                                                             (node as ActionXMLNode).getParameterString());
                    lstProcObj.Add(Action);
                }
                
            }

            return lstProcObj;
        }

        public void processXMLNodeParameters(params XMLParamStruct[] arrXPSParam)
        {
            Boolean bfoundNameParameter = false;

            foreach (XMLParamStruct XMLParam in arrXPSParam)
            {
                if (XMLParam.m_strParam.Equals("name"))
                {
                    m_strProcName = XMLParam.m_strVal;
                    bfoundNameParameter = true;
                }
                else if (XMLParam.m_strParam.Equals("priority"))
                {
                    if (!int.TryParse(XMLParam.m_strVal.Trim(),out m_nPriorityNo)){
                        throw new XmlException("incorrect Procedure attribute value,attribute 'priority' is not a valid integer!");
                    }
                }

            }

            if (!bfoundNameParameter)
                throw new XmlException("missing Procedure attribute,attribute 'name' not found!");
        }
    }


    abstract class clsXMLActionNodeHolder : RootableXMLNode
    {
        public List<ActionXMLNode> actionlist = new List<ActionXMLNode>();
        public List<IFXMLNode> iflist = new List<IFXMLNode>();
        public List<ELSEIFXMLNode> elseiflist = new List<ELSEIFXMLNode>();
        public List<ELSEXMLNode> elselist = new List<ELSEXMLNode>();

        protected Queue<XMLNode> innerActionList = new Queue<XMLNode>();
        public void add(XMLNode actionNode) { innerActionList.Enqueue(actionNode); }
        public Queue<XMLNode> getActionList() { return innerActionList; }
        public abstract ProcObjType getProcedureType();
    }

    class ActionListXMLNode : clsXMLActionNodeHolder 
    {
        public override String getName() { return "Action-list"; }

        public override void processXMLContent(String strXMLContent)
        {

        }

        public override ProcObjType getProcedureType()
        {
            return ProcObjType.IF;
        }
    }

    class IFXMLNode : clsXMLActionNodeHolder, iParameterizeXMLNode 
    {
        protected LexemGroup m_LexModelExpression = null;

        public override String getName() { return "if"; }

        public void processXMLNodeParameters(params XMLParamStruct[] arrXPSParam)
        {
            Boolean bFoundExpression = false;

            foreach(XMLParamStruct parameter in arrXPSParam)
            {
                if (parameter.m_strParam == "expression")
                {
                    bFoundExpression = true;
                    trace("reading expression");
                    LexemGroup LexModel = null;
                    if (!ExpressionHelper.Instance.isValidExpression(parameter.m_strVal, ref LexModel))
                        throw new XmlException("Invalid '" + getName() + "' expression found.");


                    m_LexModelExpression = LexModel;
                }
            }

            if (!bFoundExpression)
                throw new XmlException("Missing expression in '" + getName() + "' node.");
        }

        public override void processXMLContent(String strXMLContent)
        {

            clsXMLActionNodeHolder AlistNode = XMLNodeParent as clsXMLActionNodeHolder;
            AlistNode.add(this);

        }

        public LexemGroup getExpression() { return m_LexModelExpression; }

        public override ProcObjType getProcedureType()
        {
            return ProcObjType.IF;
        }
    }

    class ELSEIFXMLNode : IFXMLNode
    {
        public override String getName() { return "else-if"; }

        public override void processXMLContent(String strXMLContent)
        {
            clsXMLActionNodeHolder AlistNode = XMLNodeParent as clsXMLActionNodeHolder;

            if (AlistNode.getActionList().Count > 0)
            {
                int minusIndex = 1;
                XMLNode lastNode = AlistNode.getActionList().ToArray()[AlistNode.getActionList().Count - minusIndex];
                Boolean bFoundIF = false;

                while (!bFoundIF)
                {
                    if (!(lastNode is IFXMLNode || lastNode is ELSEIFXMLNode))
                    {
                        bFoundIF = (lastNode is IFXMLNode);
                        throw new XmlException("cannot add 'else-if' node without Node 'if'");
                    }
                    else
                    {
                        bFoundIF = (lastNode is IFXMLNode);
                    }

                    if(!bFoundIF){
                        minusIndex++;
                        if (AlistNode.getActionList().Count - minusIndex < 0)
                        {
                            throw new XmlException("cannot add 'else-if' node without Node 'if'");
                        }
                        else
                        {
                            lastNode = AlistNode.getActionList().ToArray()[AlistNode.getActionList().Count - minusIndex];
                        }
                    }
                }
            }
            else
            {
                throw new XmlException("cannot add 'else-if' node without Node 'if'");
            }

            AlistNode.add(this);
        }

        public override ProcObjType getProcedureType()
        {
            return ProcObjType.ELSEIF;
        }
    }

    class ELSEXMLNode : clsXMLActionNodeHolder 
    {
        public override String getName() { return "else"; }



        public override void processXMLContent(String strXMLContent)
        {
            clsXMLActionNodeHolder AlistNode = XMLNodeParent as clsXMLActionNodeHolder;

            if (AlistNode.getActionList().Count > 0)
            {
                XMLNode lastNode = AlistNode.getActionList().ToArray()[AlistNode.getActionList().Count - 1];
                if (!(lastNode is IFXMLNode || lastNode is ELSEIFXMLNode))
                {
                    throw new XmlException("cannot add 'else' node without Node 'if' or 'else-if'");
                }
            }
            else
            {
                throw new XmlException("cannot add 'else' node without Node 'if' or 'else-if'");
            }

            AlistNode.add(this);
        }

        public override ProcObjType getProcedureType()
        {
            return ProcObjType.ELSE;
        }
    }



    class ActionXMLNode : RootableXMLNode, iParameterizeXMLNode
    {
        public  override String getName() { return "Action"; }
        private String strProcType = "";
        private String strProcParam = "";
        private MethodInfo ptrMethod = null;
        public ProcObjType getProcedureType()
        {
            return ProcObjType.ACTION;
        }

        public MethodInfo getMethodInfo()
        {
            return ptrMethod;
        }

        public String getParameterString()
        {
            return strProcParam;
        }

        public void processXMLNodeParameters(params XMLParamStruct[] arrXPSParam)
        {
            Boolean bFoundType = false;
            Boolean bFoundParameter = false;
            ResourceXmlActionAttribute ptrAction = null;
            if (arrXPSParam.Length > 0)
            {
                foreach (XMLParamStruct XMLParam in arrXPSParam)
                {
                    if (XMLParam.m_strParam.Equals("type"))
                    {
                        strProcType = XMLParam.m_strVal;
                        foreach (ResourceXmlActionAttribute Action in ResourceXmlActionAttribute.getRegisteredAction())
                        {
                            if (Action.getActionName().Equals(strProcType))
                            {
                                bFoundType = true;
                                ptrAction = Action;
                                break;
                            }
                        }
                    }
                    else if (XMLParam.m_strParam.Equals("parameter"))
                    {
                        strProcParam = XMLParam.m_strVal;
                        bFoundParameter = true; 
                    }
                }
            }

            if (!bFoundType)
                throw new XmlException("Action Exception: enable to determine Action-Type!");

            Boolean bMatchAction = false;
            if (bFoundParameter)
            {
                foreach (MethodInfo method in ResourceXmlActionAttribute.getMethodRegisteredWithAction(ptrAction))
                {
                    String ParamString = "";
                    String[] strParameters = listParameters(strProcParam);

                    Boolean bAllParamMatch = true;
                    foreach (ParameterInfo param in method.GetParameters())
                    {
                        Boolean bParamMatchFound = false;
                        foreach (String strParam in strParameters)
                        {
                            if (strParam.Substring(0, strParam.IndexOf('=')).Equals(param.Name))
                            {
                                if (StringHelper.isStringLiteral(strParam.Substring(strParam.IndexOf('=') + 1)))
                                {
                                    if (param.ParameterType.Equals(typeof(String)))
                                    {
                                        if (ParamString != "") ParamString += ",";
                                        ParamString+=strParam.Substring(strParam.IndexOf('=') + 1);
                                        bParamMatchFound = true;
                                        break;
                                    }
                                }
                                else if (StringHelper.isStringBoolean(strParam.Substring(strParam.IndexOf('=') + 1)))
                                {
                                    if (param.ParameterType.Equals(typeof(Boolean)))
                                    {
                                        if (ParamString != "") ParamString += ",";
                                        ParamString += strParam.Substring(strParam.IndexOf('=') + 1);
                                        bParamMatchFound = true;
                                        break;
                                    }
                                }
                                else if (StringHelper.isStringInteger(strParam.Substring(strParam.IndexOf('=') + 1)))
                                {
                                    if (param.ParameterType.Equals(typeof(int)))
                                    {
                                        if (ParamString != "") ParamString += ",";
                                        ParamString += strParam.Substring(strParam.IndexOf('=') + 1);
                                        bParamMatchFound = true;
                                        break;
                                    }
                                }
                                else if(ResourceFunctionObj.isValidFunctionSyntax(strParam.Substring(strParam.IndexOf('=') + 1)))
                                {
                                    MethodInfo subFunction = ResourceFunctionObj.getFunctionMethod(strParam.Substring(strParam.IndexOf('=') + 1));

                                    if (param.ParameterType.Equals(subFunction.ReturnType))
                                    {
                                        if (ParamString != "") ParamString += ",";
                                        ParamString += strParam.Substring(strParam.IndexOf('=') + 1);
                                        bParamMatchFound = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (!bParamMatchFound)
                        {
                            bAllParamMatch = false;
                            break;
                        }
                    }

                    if (bAllParamMatch)
                    {
                        ptrMethod = method;
                        strProcParam = ParamString;
                        bMatchAction = true;
                        break;
                    }
                }
            }
            else
            {
                foreach (MethodInfo method in ResourceXmlActionAttribute.getMethodRegisteredWithAction(ptrAction))
                {
                    if (method.GetParameters().Length == 0)
                    {
                        ptrMethod = method;
                        strProcParam = "";
                        bMatchAction = true;
                        break;
                    }
                }
            }
            
            if(!bMatchAction)
                throw new XmlException("Action Exception: enable to determine Action-Type!,no identical parameter found.");

            clsXMLActionNodeHolder AlistNode = XMLNodeParent as clsXMLActionNodeHolder;
            AlistNode.add(this);
        }

        private String[] listParameters(String parameter)
        {
            List<String> Params = new List<String>();
            Boolean bWithInString = false;

            String buff = "";
            foreach (char ch in parameter)
            {
                if (ch == '\'')
                {
                    bWithInString = !bWithInString;
                    buff += ch;
                }
                else if (ch == ';' && !bWithInString)
                {
                    if (isValidParam(buff))
                    {
                        Params.Add(buff);
                        buff = "";
                    }
                    else
                        throw new XmlException("Action Exception:Invalid Parameter found!");
                }
                else
                    buff += ch;

            }

            if (buff != "") {
                if (isValidParam(buff))
                    Params.Add(buff);
                else
                    throw new XmlException("Action Exception:Invalid Parameter found!");
            }

            return Params.ToArray();
        }

        private Boolean isValidParam(String param)
        {
            try
            {
                String parameterName = param.Substring(0, param.IndexOf('='));
                String parameterVal = param.Substring(param.IndexOf('=')+1);
                return true;
            }
            catch (Exception) { }
           

            return false;
        }

        public override void processXMLContent(String strXMLContent)
        {

        }
    }

}
