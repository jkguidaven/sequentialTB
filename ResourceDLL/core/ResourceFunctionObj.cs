using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CoreDLL.utilities;
using System.Text.RegularExpressions;
using GraphicDLL.common;

namespace ResourceDLL.core
{
    public abstract class ResourceFunctionObj : clsResourceTrace
    {
        protected Queue<clsObjLLEvent> m_lstPOSMLLEvent = new Queue<clsObjLLEvent>();
        protected Queue<clsObjLLEvent> m_lstPOSLLEvent  = new Queue<clsObjLLEvent>();
        protected Queue<clsObjLLEvent> m_lstFLMLLEvent  = new Queue<clsObjLLEvent>();
        protected Queue<clsObjLLEvent> m_lstFLLLEvent   = new Queue<clsObjLLEvent>();

        protected Boolean m_bLinkNextMessageToPOS = false;
        protected Boolean m_bLinkNextMessageToPOSM = false;
        protected Boolean m_bLinkNextMessageToFLM = false;
        protected Boolean m_bStartOfFLMMessage  = false;

        protected String m_strLastParamValue = "";

        public Queue<clsObjLLEvent> getPOSMLLEventList() { return m_lstPOSMLLEvent; }
        public Queue<clsObjLLEvent> getPOSLLEventList()  { return m_lstPOSLLEvent;  }
        public Queue<clsObjLLEvent> getFLMLLEventList()  { return m_lstFLMLLEvent;  }
        public Queue<clsObjLLEvent> getFLLLEventList()   { return m_lstFLLLEvent;   }

        private List<clsObjLLEvent> m_lstPOSMPendingLinkEvent = new List<clsObjLLEvent>();
        private List<clsObjLLEvent> m_lstPOSPendingLinkEvent  = new List<clsObjLLEvent>();
        private List<clsObjLLEvent> m_lstFLMPendingLinkEvent  = new List<clsObjLLEvent>();
        private List<clsObjLLEvent> m_lstFLPendingLinkEvent   = new List<clsObjLLEvent>();


        protected ResourceFunctionObj() : base(L1) { }
   
        protected RequestLogData m_currRequestLogData;
        public static Boolean       isValidFunctionSyntax(String strVal) 
        {
            return getFunctionMethod(strVal) != null;
        }

        public static MethodInfo getFunctionMethod(String strVal)
        {
            Queue<String> lexes = new Queue<string>(StringHelper.SplitExpression(strVal));
            String functionName = lexes.Dequeue(); // dequeue function name

            if (lexes.Dequeue() != "(") // return false if second lex not open parenthesis
                return null;

            List<Type> parameters = new List<Type>();

            while (lexes.Count > 0)
            {
                String lastDequeue = null;
                if (StringHelper.isStringLiteral((lastDequeue = lexes.Dequeue())))
                {
                    parameters.Add("".GetType());
                }
                else if (StringHelper.isStringBoolean(lastDequeue))
                {
                    parameters.Add(new Boolean().GetType());
                }
                else if (StringHelper.isStringInteger(lastDequeue))
                {
                    parameters.Add(new int().GetType());
                }
                else if (isFunctionExist(lastDequeue))
                {
                    String StrFunction = lastDequeue;
                    int PairCount = 0;
                    while (lexes.Count > 0)
                    {
                        String lastPop = lexes.Dequeue();
                        StrFunction += lastPop;

                        if (lastPop == "(")
                        {
                            PairCount++;
                        }

                        if (lastPop == ")")
                        {
                            PairCount--;
                            if (PairCount == 0)
                                break;
                        }
                    }
                    MethodInfo ptrFunction = null;
                    if ((ptrFunction = getFunctionMethod(StrFunction)) == null)
                        return null;
                    else
                    {
                        parameters.Add(ptrFunction.ReturnType);
                    }
                }

                if(lexes.Count > 0) lastDequeue = lexes.Dequeue();
                if (lastDequeue != "," && lastDequeue != ")")
                    return null;
            }

            foreach (MethodInfo PtrMethod in ResouceXmlFunctionAttribute.getRegisteredFunction())
            {
                if (PtrMethod.Name.Equals(functionName) && PtrMethod.GetParameters().Length == parameters.Count)
                {
                    Boolean bExactParamType = true;
                    for (int i = 0; i < PtrMethod.GetParameters().Length; i++)
                    {

                        if (parameters[i] != PtrMethod.GetParameters()[i].ParameterType)
                            bExactParamType = false;
                    }

                    if (bExactParamType)
                        return PtrMethod;
                }
            }

            return null;
        }

        public static Boolean isFunctionExist(String strVal)
        {
            foreach (MethodInfo PtrMethod in ResouceXmlFunctionAttribute.getRegisteredFunction())
            {
                if (PtrMethod.Name.Equals(strVal))
                    return true;
            }
            return false;
        }


        public static String extractParamStr(String strVal)
        {
            int indexStart = strVal.IndexOf('(')+1;

            if (strVal[indexStart] != ')')
            {
                return strVal.Substring(indexStart, strVal.Length - indexStart-1);
            }
            else
                return "";
        }

        public Object InvokeFunction(MethodInfo method,RequestLogData log,String parameters)
        {
            m_currRequestLogData = log;
            String[] ParamStr = StringHelper.splitParameter(parameters);
            List<Object> objParams = new List<Object>();

            for (int i = 0; i < ParamStr.Length && ParamStr.Length!=0; i++)
            {
                if (ParamStr[i].Trim().Equals("")) 
                    continue;

                if (StringHelper.isStringLiteral(ParamStr[i]))
                {
                    String parameter = ParamStr[i] as String;
                    objParams.Add(parameter.Substring(1,parameter.Length-2));
                }
                else if(StringHelper.isStringBoolean(ParamStr[i]))
                {
                    
                    String parameter = ParamStr[i] as String;
                    objParams.Add(parameter.Equals("true"));
                }
                else if (StringHelper.isStringInteger(ParamStr[i]))
                {
                    int parameter = Int32.Parse((ParamStr[i] as String));
                    objParams.Add(parameter);
                }
                else
                {
                    MethodInfo mi = getFunctionMethod(ParamStr[i] as String);
                    objParams.Add(InvokeFunction(mi, log, extractParamStr(ParamStr[i] as String)));
                }
            }
            return method.Invoke(this,objParams.ToArray());
        }


        [ResouceXmlFunctionAttribute]
        public String getLogDataLine() { return m_currRequestLogData.value; }

        [ResouceXmlFunctionAttribute]
        public String getPatternName() { return m_currRequestLogData.LogPattern.PatternID; }

        [ResouceXmlFunctionAttribute]
        public Boolean Contains(String strSource, String strVal) { return strSource.Contains(strVal); }

        [ResouceXmlFunctionAttribute]
        public Boolean isLinkToPos() {
            return m_bLinkNextMessageToPOS; 
        }

        [ResouceXmlFunctionAttribute]
        public Boolean isLinkToPOSM()
        {
            return m_bLinkNextMessageToPOSM;
        }


        [ResouceXmlFunctionAttribute]
        public Boolean isLinkToFLM()
        {
            return m_bLinkNextMessageToFLM;
        }

        [ResouceXmlFunctionAttribute]
        public Boolean isStartOfMessage()
        {
            return m_bStartOfFLMMessage;
        }

        [ResouceXmlFunctionAttribute]
        public String getPatternMatch(String strSource)
        {
            return Regex.Match(strSource, m_currRequestLogData.LogPattern.PatternRegEx).Value;
        }

        [ResouceXmlFunctionAttribute]
        public String getLastParam()
        {
            return m_strLastParamValue;
        }

        [ResouceXmlFunctionAttribute]
        public String extractRawDataValueFromDump(String Dump)
        {
            try
            {
                int IndexOfFirstBR = Dump.IndexOf('\n');
                Dump = Dump.Substring(IndexOfFirstBR + 1);
            }
            catch (Exception)
            {
                return "";
            }

            return Dump;
        }


        [ResouceXmlFunctionAttribute]
        public String getAttribute(String Attribute)
        {
            foreach (AttributeStruct currAttribute in m_currRequestLogData.getAttributes())
            {
                if (currAttribute.Name.Equals(Attribute))
                {
                    return currAttribute.Value;
                }
            }

            return "";
        }

        [ResourceXmlAction("ADD_NORMAL_EVENT")]
        public void ADDNORMAL_handler(String Lifeline)
        {
            long lTimeStamp = StringHelper.DSToL(StringHelper.getTimeStamp(m_currRequestLogData.value));

            clsObjLLEvent LLEvent = new clsObjLLEvent(lTimeStamp);
            String strContext = "";
            foreach (AttributeStruct currAttribute in m_currRequestLogData.getAttributes())
                strContext += currAttribute.Name + " = " + currAttribute.Value + "\n";

            LLEvent.setContext(strContext);

            switch (Lifeline)
            {
                case "POSM":
                    m_lstPOSMLLEvent.Enqueue(LLEvent);
                    break;
                case "POS":
                    m_lstPOSLLEvent.Enqueue(LLEvent);
                    break;
                case "FLM":
                    m_lstFLMLLEvent.Enqueue(LLEvent);
                    break;
                case "FL":
                    m_lstFLLLEvent.Enqueue(LLEvent);
                    break;
                default:
                    trace(L2, "Enable to execute current Procedure Action[ADD_NORMAL_EVENT],Lifeline[" +
                              Lifeline + "] was not found!", "skipping current action[ADD_NORMAL_EVENT]");
                    break;
            }
        }

        [ResourceXmlAction("ADD_ERROR_EVENT")]
        public void ADDERROR_handler(String Lifeline, String ErrorMsg)
        {
        }

        [ResourceXmlAction("SET_LAST_PARAM")]
        public void SETLASTPARAM_handler(String value)
        {
            m_strLastParamValue = value;
        }

        [ResourceXmlAction("END_MESSAGE_EVENT")]
        public void ENDMESSAGE_handler(String Lifeline)
        {
            long lTimeStamp = StringHelper.DSToL(StringHelper.getTimeStamp(m_currRequestLogData.value));
            clsObjLLEvent LLEvent = new clsObjLLEvent(lTimeStamp, LLEventType.END);

            switch (Lifeline)
            {
                case "POSM":
                    m_lstPOSMLLEvent.Enqueue(LLEvent);
                    break;
                case "POS":
                    m_lstPOSLLEvent.Enqueue(LLEvent);
                    break;
                case "FLM":
                    m_lstFLMLLEvent.Enqueue(LLEvent);
                    break;
                case "FL":
                    m_lstFLLLEvent.Enqueue(LLEvent);
                    break;
                default:
                    trace(L2, "Enable to execute current Procedure Action[END_MESSAGE_EVENT],Lifeline[" +
                               Lifeline + "] was not found!", "skipping current action[END_MESSAGE_EVENT]");
                    break;
            }
        }

        [ResourceXmlAction("DUMP_DATA")]
        public void DUMPDATA_Handler(String DumpPage, String DumpData)
        {
            trace(L3, "Dumping data to Page[" + DumpPage + "]:\n" + DumpData);
        }



        [ResourceXmlAction("ADD_LINK_EVENT")]
        public void ADDLINK_handler(String Lifeline, String DestinationLifeLine, String Identifier, Boolean AlignToLink)
        {
            long lTimeStamp = StringHelper.DSToL(StringHelper.getTimeStamp(m_currRequestLogData.value));
            Queue<clsObjLLEvent> pendingLinkEvent = null;
            List<clsObjLLEvent> ptrList = null;
            switch (DestinationLifeLine)
            {
                case "POSM":
                    pendingLinkEvent = new Queue<clsObjLLEvent>(m_lstPOSMPendingLinkEvent.ToArray());
                    ptrList = m_lstPOSMPendingLinkEvent;
                    break;
                case "POS":
                    pendingLinkEvent = new Queue<clsObjLLEvent>(m_lstPOSPendingLinkEvent.ToArray());
                    ptrList = m_lstPOSPendingLinkEvent;
                    break;
                case "FLM":
                    pendingLinkEvent = new Queue<clsObjLLEvent>(m_lstFLMPendingLinkEvent.ToArray());
                    ptrList = m_lstFLMPendingLinkEvent;
                    break;
                case "FL":
                    pendingLinkEvent = new Queue<clsObjLLEvent>(m_lstFLPendingLinkEvent.ToArray());
                    ptrList = m_lstFLPendingLinkEvent;
                    break;
                default:
                    trace(L2, "Enable to execute current Procedure Action[ADD_LINK_EVENT],Lifeline[" +
                               Lifeline + "] was not found!", "skipping current action[ADD_LINK_EVENT]");
                    break;
            }

            clsObjLLEventLink LLEvent = new clsObjLLEventLink(lTimeStamp, null);
            LLEvent.setIdentifier(Identifier);
            LLEvent.setToFollow(AlignToLink);
            String strContext = "";
            switch (Lifeline)
            {
                case "POSM":
                    foreach (AttributeStruct currAttribute in m_currRequestLogData.getAttributes())
                            strContext += currAttribute.Name + " = " + currAttribute.Value + "\n";

                    LLEvent.setContext(strContext);
                    m_lstPOSMLLEvent.Enqueue(LLEvent);
                    break;
                case "POS":
                    m_lstPOSLLEvent.Enqueue(LLEvent);
                    break;
                case "FLM":
                    foreach (AttributeStruct currAttribute in m_currRequestLogData.getAttributes())
                            strContext += currAttribute.Name + " = " + currAttribute.Value + "\n";

                    LLEvent.setContext(strContext);
                    m_lstFLMLLEvent.Enqueue(LLEvent);
                    break;
                case "FL":
                    m_lstFLLLEvent.Enqueue(LLEvent);
                    break;
                default:
                    trace(L2, "Enable to execute current Procedure Action[END_MESSAGE_EVENT],Lifeline[" +
                               Lifeline + "] was not found!", "skipping current action[END_MESSAGE_EVENT]");
                    break;
            }


            while (pendingLinkEvent.Count > 0)
            {
                clsObjLLEvent currLLEvent = pendingLinkEvent.Dequeue();
                if (currLLEvent.getLongTimeStamp() == lTimeStamp)
                {
                    if (currLLEvent.getIdentifier().Equals(Identifier) && !(currLLEvent is clsObjLLEventLink))
                    {
                        ptrList.Remove(currLLEvent);
                        currLLEvent.setLinkEvent(LLEvent);
                        LLEvent.setLinkEvent(currLLEvent);
                        currLLEvent.setToFollow(!AlignToLink);
                        return;
                    }
                }
            }

            ptrList.Add(LLEvent);
        }

        [ResourceXmlAction("ADD_LINK_RECEIVER_EVENT")]
        public void ADDLINKRECEIVER_handler(String Lifeline, String Identifier)
        {
            long lTimeStamp = StringHelper.DSToL(StringHelper.getTimeStamp(m_currRequestLogData.value));
            Queue<clsObjLLEvent> pendingLinkEvent = null;
            List<clsObjLLEvent> ptrList = null;
            switch (Lifeline)
            {
                case "POSM":
                    pendingLinkEvent = new Queue<clsObjLLEvent>(m_lstPOSMPendingLinkEvent.ToArray());
                    ptrList = m_lstPOSMPendingLinkEvent;
                    break;
                case "POS":
                    pendingLinkEvent = new Queue<clsObjLLEvent>(m_lstPOSPendingLinkEvent.ToArray());
                    ptrList = m_lstPOSPendingLinkEvent;
                    break;
                case "FLM":
                    pendingLinkEvent = new Queue<clsObjLLEvent>(m_lstFLMPendingLinkEvent.ToArray());
                    ptrList = m_lstFLMPendingLinkEvent;
                    break;
                case "FL":
                    pendingLinkEvent = new Queue<clsObjLLEvent>(m_lstFLPendingLinkEvent.ToArray());
                    ptrList = m_lstFLPendingLinkEvent;
                    break;
                default:
                    trace(L2, "Enable to execute current Procedure Action[ADD_LINK_EVENT],Lifeline[" +
                               Lifeline + "] was not found!", "skipping current action[ADD_LINK_EVENT]");
                    break;
            }

            clsObjLLEvent LLEvent = new clsObjLLEvent(lTimeStamp);
            LLEvent.setIdentifier(Identifier);

            String strContext = "";
            switch (Lifeline)
            {
                case "POSM":
                    foreach (AttributeStruct currAttribute in m_currRequestLogData.getAttributes())
                        strContext += currAttribute.Name + " = " + currAttribute.Value + "\n";

                    LLEvent.setContext(strContext);
                    m_lstPOSMLLEvent.Enqueue(LLEvent);
                    break;
                case "POS":
                    m_lstPOSLLEvent.Enqueue(LLEvent);
                    break;
                case "FLM":
                    foreach (AttributeStruct currAttribute in m_currRequestLogData.getAttributes())
                        strContext += currAttribute.Name + " = " + currAttribute.Value + "\n";

                    LLEvent.setContext(strContext);
                    m_lstFLMLLEvent.Enqueue(LLEvent);
                    break;
                case "FL":
                    m_lstFLLLEvent.Enqueue(LLEvent);
                    break;
                default:
                    trace(L2, "Enable to execute current Procedure Action[END_MESSAGE_EVENT],Lifeline[" +
                               Lifeline + "] was not found!", "skipping current action[END_MESSAGE_EVENT]");
                    break;
            }

            while (pendingLinkEvent.Count > 0)
            {
                clsObjLLEvent currLLEvent = pendingLinkEvent.Dequeue();
                if (currLLEvent.getLongTimeStamp() == lTimeStamp)
                {
                    if (currLLEvent.getIdentifier().Equals(Identifier) && (currLLEvent is clsObjLLEventLink))
                    {
                        ptrList.Remove(currLLEvent);
                        currLLEvent.setLinkEvent(LLEvent);
                        LLEvent.setLinkEvent(currLLEvent);

                        LLEvent.setToFollow(!currLLEvent.AlignToLink());
                        //(currLLEvent as clsObjLLEventLink).setLinkText(getAttribute("Message"));
                        //(currLLEvent as clsObjLLEventLink).setContext(getAttribute("Message"));
                        return;
                    }
                }
            }


            ptrList.Add(LLEvent);
        }

        [ResourceXmlAction("LINK_NEXT_MESSAGE_TO_POS")]
        public void LINKPOSMESSAGE_handler(Boolean enable)
        {
            m_bLinkNextMessageToPOS = enable;
        }

        [ResourceXmlAction("LINK_NEXT_MESSAGE_TO_POSM")]
        public void LINKPOSMMESSAGE_handler(Boolean enable)
        {
            m_bLinkNextMessageToPOSM = enable;
        }

        [ResourceXmlAction("LINK_NEXT_MESSAGE_TO_FLM")]
        public void LINKFLMMESSAGE_handler(Boolean enable)
        {
            m_bLinkNextMessageToFLM = enable;
        }

        [ResourceXmlAction("MARK_NEXT_OBJ_AS_START")]
        public void LINKFLMNEWMESSAGE_handler(Boolean value)
        {
            m_bStartOfFLMMessage = value;
        }
    }



}
