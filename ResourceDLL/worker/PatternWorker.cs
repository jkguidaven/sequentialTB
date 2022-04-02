using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceDLL.worker;
using ResourceDLL.Manager;
using ResourceDLL.xml_struct;
using CoreDLL.foundation;
using ResourceDLL.core;
using System.Text.RegularExpressions;
using ResourceDLL.xml_struct.patternXML;

namespace ResourceDLL.worker
{
    class PatternWorker : ResourceWorker, iResourceXMLWorker
    {
        public PatternWorker(ParseEngineController clsPEController)
            : base(clsPEController)
        {

        }

        public XMLBlueprint getXMLStructure()
        {
            return new PatternXMLBluePrint();
        }

        public String getXML()
        {
            return GET_CONFIG("Pattern", "File");
        }


        [eRWorkerTaskHandler(eRWorkerTask.INITIALIZE)]
        public void Initialize_Handler(TaskStruct<eRWorkerTask> TStask)
        {
            trace("Initializing Pattern database.");
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
                m_clsTWLink.queueTask(new TaskStruct<eRWorkerTask>(INITIALIZE_TASK, this));
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
            trace("PatternWorker: extracting relevant Log Data.");
            ResourceRequest request = ObjDataStruct.get<ResourceRequest>(TSTask.m_lstODSData);

            TaskStruct<eRWorkerTask> TStaskUpdate = new TaskStruct<eRWorkerTask>(PROCESS_STATUS_UPDATE_TASK, this);
            TStaskUpdate.m_lstODSData.Add(new ObjDataStruct(request));
            TStaskUpdate.m_lstODSData.Add(new ObjDataStruct("Filtering Raw Log Data"));
            m_clsTWLink.queueTask(TStaskUpdate);

            int nRemoveCount = 0;
            List<RequestLogData> newLogData = new List<RequestLogData>();
            foreach (RequestLogData LogData in request.LogData)
            {
                PatternObjStruct refPatternObj = null;
                if (!isRelevantLog(LogData.value, ref refPatternObj))
                    nRemoveCount++;
                else
                {
                    LogData.setPattern(refPatternObj);
                    newLogData.Add(LogData);
                }
            }

            request.LogData = newLogData;
            trace("Remove count: " + nRemoveCount);
            trace("Filtered Log Data count: " + request.LogData.Count);


            m_clsTWLink.queueTask(new TaskStruct<eRWorkerTask>(PROCESS_REQUEST_DATA_TASK,
                                                               this,
                                                               new ObjDataStruct(request)));

        }

        private Boolean isRelevantLog(String strLogDataLine,ref PatternObjStruct refPatternObj)
        {

            foreach (PatternObjStruct Pattern in m_ResManager.PatternList)
            {
                if (Regex.Match(strLogDataLine, Pattern.PatternRegEx).Success)
                {
                    refPatternObj = Pattern;
                    return true;
                }
            }

            return false;
        }
    }
}
