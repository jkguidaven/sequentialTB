using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceDLL.Manager;
using CoreDLL.foundation;
using ResourceDLL.xml_struct;
using ResourceDLL.xml_struct.ProcedureXML;
using ResourceDLL.core;
using System.Reflection;


namespace ResourceDLL.worker
{
    class ProcedureWorker : ResourceWorker , iResourceXMLWorker
    {
        public ProcedureWorker(ParseEngineController clsPEController)
            : base(clsPEController)
        {
        }

        public XMLBlueprint getXMLStructure()
        {
            return new ProcedureXMLBlueprint();
        }

        public String getXML()
        {
            return GET_CONFIG("Procedure", "File");
        }



        [eRWorkerTaskHandler(eRWorkerTask.INITIALIZE)]
        public void Initialize_Handler(TaskStruct<eRWorkerTask> TStask)
        {
            trace("Initializing Procedure database.");
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
                m_clsTWLink.queueTask(new TaskStruct<eRWorkerTask>(INITIALIZE_TASK, this , TStask.m_lstODSData.ToArray() ));
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
            TStaskUpdate.m_lstODSData.Add(new ObjDataStruct("Executing Log Data Procedures"));
            m_clsTWLink.queueTask(TStaskUpdate);

            foreach (RequestLogData log in request.LogData)
            {
                ProcedureWrapper procedure = getProcedure(log);
                Queue<ProcObj> ProcedureQueue = procedure.getActionList();
                Execute(log, ProcedureQueue , request);
                //trace("executing procedure=" + procedure.getProcedureID());
            }

            request.LogData = null; // clean LogData
            m_clsTWLink.queueTask(new TaskStruct<eRWorkerTask>(PROCESS_REQUEST_DATA_TASK,
                                       this,
                                       new ObjDataStruct(request)));
        }


        private ProcedureWrapper getProcedure(RequestLogData log)
        {
            return log.LogPattern.PatternProcedure;
        }


        private void Execute(RequestLogData log, Queue<ProcObj> ProcedureQueue, ResourceFunctionObj FunctionWrapper)
        {
            while(ProcedureQueue.Count > 0)
            {
                ProcObj action = ProcedureQueue.Dequeue();

                if (action is ProcObjCondition)
                {
                    while (action is ProcObjCondition)
                    {
                        ProcObjCondition ConditionAction = (action as ProcObjCondition);
                        if (ConditionAction.getProcedureType() != ProcObjType.ELSE)
                        {
                            if (ExpressionHelper.TestExpression(ConditionAction.getLexModelExpression(), log, FunctionWrapper))
                            {
                                Execute(log, ConditionAction.getActionList(), FunctionWrapper);


                                if (ProcedureQueue.Count > 0)
                                {
                                    while (ProcedureQueue.Count > 0 && ProcedureQueue.Peek() is ProcObjCondition)
                                    {

                                        if ((ProcedureQueue.Peek() as ProcObjCondition).getProcedureType() == ProcObjType.ELSE)
                                        {
                                            ProcedureQueue.Dequeue();
                                            break;
                                        }
                                        else
                                        {
                                            if ((ProcedureQueue.Peek() as ProcObjCondition).getProcedureType() != ProcObjType.IF)
                                                ProcedureQueue.Dequeue();
                                            else
                                                break;
                                        }
                                    }
                                }

                                break;
                            }

                            if (ProcedureQueue.Count > 0 && ProcedureQueue.Peek() is ProcObjCondition)
                                action = ProcedureQueue.Dequeue();
                            else
                                break;
                        }
                        else
                        {
                            Execute(log, ConditionAction.getActionList(), FunctionWrapper);
                            break;
                        }
                    }
                }
                else
                {
                    MethodInfo refMethod = (action as ProcObjAction).getMethodReference();
                    FunctionWrapper.InvokeFunction(refMethod, log, (action as ProcObjAction).getParameterString());
                }
            }
        }
    }
}
