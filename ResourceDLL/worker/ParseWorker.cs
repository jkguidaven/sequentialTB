using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreDLL.foundation;
using ResourceDLL.Manager;
using ResourceDLL.xml_struct;
using ResourceDLL.xml_struct.ParseTableXML;
using ResourceDLL.core;
using System.Text.RegularExpressions;

namespace ResourceDLL.worker
{
    class ParseWorker : ResourceWorker, iResourceXMLWorker
    {
        public ParseWorker(ParseEngineController clsPEController)
            : base(clsPEController)
        {
        }

        public XMLBlueprint getXMLStructure()
        {
            return new ParseTableXMLBlueprint();
        }

        public String getXML()
        {
            return GET_CONFIG("Parse-table", "File");
        }

        [eRWorkerTaskHandler(eRWorkerTask.INITIALIZE)]
        public void Initialize_Handler(TaskStruct<eRWorkerTask> TStask)
        {
            trace("Initializing Parse-table database.");
            trace("Loading XML[" + getXML() + "] database file.");

            try
            {
                TaskStruct<eRWorkerTask> TStaskUpdate = new TaskStruct<eRWorkerTask>(PROCESS_STATUS_UPDATE_TASK, this);
                TStaskUpdate.m_lstODSData.Add(new ObjDataStruct(ObjDataStruct.peek<ResourceRequest>(TStask.m_lstODSData)));
                TStaskUpdate.m_lstODSData.Add(new ObjDataStruct("Loading XML[" + getXML() + "] database file"));
                m_clsTWLink.queueTask(TStaskUpdate);
            }
            catch (Exception) { }

            if (m_ResManager.LoadingXML(this))
            {

                trace("successfully loaded XML[" + getXML() + "].");
                m_clsTWLink.queueTask(new TaskStruct<eRWorkerTask>(INITIALIZE_TASK, this, TStask.m_lstODSData.ToArray()));
            }
            else
            {
                TaskStruct<eRWorkerTask> new_TStask = new TaskStruct<eRWorkerTask>(ERROR_TASK,
                                                                                     this,
                                                                                     TStask.m_lstODSData.ToArray());
                new_TStask.m_lstODSData.Add(
                    new ObjDataStruct("Error loading XML[" + getXML() + "] database file! aborting initialization, " +
                    "see log[" + GET_CONFIG("Trace", "LogFile") + "] file for Error description."));

                m_clsTWLink.queueTask(new_TStask);
            }
        }

        [eRWorkerTaskHandler(eRWorkerTask.PROCESS_REQUEST_DATA)]
        public void ProcessRequestData_Handler(TaskStruct<eRWorkerTask> TSTask)
        {
            ResourceRequest request = ObjDataStruct.get<ResourceRequest>(TSTask.m_lstODSData);

            TaskStruct<eRWorkerTask> TStaskUpdate = new TaskStruct<eRWorkerTask>(PROCESS_STATUS_UPDATE_TASK, this);
            TStaskUpdate.m_lstODSData.Add(new ObjDataStruct(request));
            TStaskUpdate.m_lstODSData.Add(new ObjDataStruct("Parsing Processed Log Data"));
            m_clsTWLink.queueTask(TStaskUpdate);

            trace("Parsing Data[" + request + "].");


            foreach (RequestLogData data in request.LogData)
            {
                String parseValue = "";
                foreach(ParseDataObj DataParser in data.LogPattern.ParseDataTable){
                    try
                    {

                        Match RegExMatch = Regex.Match(data.value, DataParser.getParseDataStartRegex());
                        if (!DataParser.getParseDataEndRegex().Equals(DataParser.getParseDataStartRegex()))
                        {
                            if (RegExMatch.Success)
                            {
                                int StartIndex = RegExMatch.Index;
                                parseValue = data.value.Substring(StartIndex + RegExMatch.Length);
                                if ((RegExMatch = Regex.Match(parseValue, DataParser.getParseDataEndRegex())).Success)
                                {
                                    int EndIndex = RegExMatch.Index;
                                    parseValue = parseValue.Substring(0, EndIndex);
                                    //trace(L3, "Attribute-Add:name=" + DataParser.getParseDataID() + ";value=" + parseValue);
                                    data.addAttribute(DataParser.getParseDataID(), parseValue);
                                }
                            }
                        }
                        else
                        {
                            if (RegExMatch.Success)
                            {
                                //trace(L3, "Attribute-Add:name=" + DataParser.getParseDataID() + ";value=" + RegExMatch.Value);
                                data.addAttribute(DataParser.getParseDataID(), RegExMatch.Value);
                            }
                        }
                    }
                    catch (Exception) { }
                }
            }

            trace("Successfully Parse Data!");
            m_clsTWLink.queueTask(new TaskStruct<eRWorkerTask>(PROCESS_REQUEST_DATA_TASK,
                                                   this,
                                                   new ObjDataStruct(request)));
        }
    }
}
