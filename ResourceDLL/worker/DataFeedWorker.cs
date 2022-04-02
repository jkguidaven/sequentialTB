using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceDLL.worker;
using CoreDLL.foundation;
using ResourceDLL.core;
using System.Text.RegularExpressions;
using System.IO;
using CoreDLL.utilities;

namespace ResourceDLL.worker
{
    class DataFeedWorker : ResourceWorker 
    {
        private static readonly String POSM_TAG = @"POSM:\s*";
        private static readonly String FLM_TAG  = @"FLM:\s*";
        private static readonly String DATEREGX = @"\d{2}/\d{2}\s+\d{2}:\d{2}:\d{2}";


        public DataFeedWorker(ParseEngineController clsPEController)
            : base(clsPEController)
        {

        }

        [eRWorkerTaskHandler(eRWorkerTask.PROCESS_REQUEST_DATA)]
        public void ProcessRequestData_Handler(TaskStruct<eRWorkerTask> TSTask)
        {
            ResourceRequest request = ObjDataStruct.get<ResourceRequest>(TSTask.m_lstODSData);
            trace("Starting RawDataFeeder.",
                  "loading posm-log=" + request.getPOSMLogFilePath() + ";",
                  "loading flm-log=" + request.getFLMLogFilePath() + ";");


            TaskStruct<eRWorkerTask> TStaskUpdate = new TaskStruct<eRWorkerTask>(PROCESS_STATUS_UPDATE_TASK, this);
            TStaskUpdate.m_lstODSData.Add(new ObjDataStruct(request));
            TStaskUpdate.m_lstODSData.Add(new ObjDataStruct("Reading Raw Log Data"));
            m_clsTWLink.queueTask(TStaskUpdate);

            request.LogData = StartReadingLogs(request.getFLMLogFilePath(), 
                                        request.getPOSMLogFilePath(),
                                        request.getLogTimeStampStart(), request.getLogTimeStampEnd(),
                                        request.IncludeBackUp());


            m_clsTWLink.queueTask(new TaskStruct<eRWorkerTask>(PROCESS_REQUEST_DATA_TASK,
                                                               this,
                                                               new ObjDataStruct(request)));
        }


        private List<RequestLogData> StartReadingLogs(String strFLMFilePath, String strPOSMFilePath,
                                             String strStartingTimeStamp,String strEndingTimeStamp,Boolean includeBackUp)
        {
            trace("+StartReadingLogs");
            List<RequestLogData> LogData = new List<RequestLogData>();


            List<String> FLMUnfilteredLogs  = new List<String>(),
                         POSMUnfilteredLogs = new List<String>();

            if (includeBackUp)
            {
                if (File.Exists(strFLMFilePath + ".BAK")) FLMUnfilteredLogs.AddRange(File.ReadAllLines(strFLMFilePath + ".BAK").ToList());
                if (File.Exists(strPOSMFilePath + ".BAK")) POSMUnfilteredLogs.AddRange(File.ReadAllLines(strPOSMFilePath + ".BAK").ToList());
            }

            FLMUnfilteredLogs.AddRange(File.ReadAllLines(strFLMFilePath).ToList());
            POSMUnfilteredLogs.AddRange(File.ReadAllLines(strPOSMFilePath).ToList());

            int index=0,FLMCount=0,POSMCount=0;
            String stackFLMLine = "",stackPOSMLine="";
            Boolean bFLMReading = true;
            Boolean bPOSMReading = true;

            while ((bFLMReading || bPOSMReading) && prtyAliveState)
            {

                if (index < FLMUnfilteredLogs.Count)
                {

                        if (Regex.Match(FLMUnfilteredLogs[index], FLM_TAG + DATEREGX).Success)
                        {
                            String timestap = Regex.Match(stackFLMLine, DATEREGX).Value;
                            if (stackFLMLine.Trim() != "" && TimeStampOnRange(timestap,strStartingTimeStamp,strEndingTimeStamp))
                            {
                                LogData.Add(new RequestLogData(stackFLMLine));
                                FLMCount++;
                            }
                            stackFLMLine = FLMUnfilteredLogs[index];
                        }
                        else
                            stackFLMLine += FLMUnfilteredLogs[index] + "\n";
                }
                else
                    bFLMReading = false;

                if (index < POSMUnfilteredLogs.Count)
                {
                    if (Regex.Match(POSMUnfilteredLogs[index], POSM_TAG + DATEREGX).Success)
                    {

                       String timestap = Regex.Match(stackPOSMLine, DATEREGX).Value;
                       if (stackPOSMLine.Trim() != "" && TimeStampOnRange(timestap, strStartingTimeStamp, strEndingTimeStamp))
                       {
                           LogData.Add(new RequestLogData(stackPOSMLine));
                           POSMCount++;
                       }
                        stackPOSMLine = POSMUnfilteredLogs[index];

                    }
                    else
                        stackPOSMLine += POSMUnfilteredLogs[index] + "\n";
                }
                else
                    bPOSMReading = false;


                index++;
            }
            LogData = LogData.OrderBy(o => StringHelper.DSToL(Regex.Match(o.value, DATEREGX).Value)).ToList();
            trace("FLM total log count: " + FLMCount);
            trace("POSM total log count: " + POSMCount);
            trace("Total logs count: " + LogData.Count);
            trace("-StartReadingLogs");


            return LogData;
        }

        private static Boolean TimeStampOnRange(String LogTimeStamp, String StartTimeStamp, String EndTimeStamp)
        {
            Boolean bPassStart = StartTimeStamp == null;
            Boolean bPassEnd   = EndTimeStamp == null;


            if (StartTimeStamp != null)
            {
                bPassStart = StringHelper.DSToL(LogTimeStamp) >= StringHelper.DSToL(StartTimeStamp);
            }


            if (EndTimeStamp != null)
            {
                bPassEnd   = StringHelper.DSToL(LogTimeStamp) <= StringHelper.DSToL(EndTimeStamp);
            }

            return bPassStart && bPassEnd;
        }

        


    }
}
