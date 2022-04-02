using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreDLL.foundation;
using CoreDLL.logworker;
using System.Reflection;
using ResourceDLL.Manager;


namespace ResourceDLL.worker
{
    public class ResourceWorker : TaskWorker<eRWorkerTask>
    {
        #region MACRO
            protected static readonly TraceLevel L1 = TraceLevel.L1;
            protected static readonly TraceLevel L2 = TraceLevel.L2;
            protected static readonly TraceLevel L3 = TraceLevel.L3;

            protected static readonly eRWorkerTask INITIALIZE_TASK   = eRWorkerTask.INITIALIZE;
            protected static readonly eRWorkerTask UNINITIALIZE_TASK = eRWorkerTask.UNINITIALIZE;
            protected static readonly eRWorkerTask PROCESS_GUI_REQUEST_TASK = eRWorkerTask.PROCESS_GUI_REQUEST;
            protected static readonly eRWorkerTask PROCESS_REQUEST_DATA_TASK = eRWorkerTask.PROCESS_REQUEST_DATA;
            protected static readonly eRWorkerTask PROCESS_STATUS_UPDATE_TASK = eRWorkerTask.PROCESS_STATUS_UPDATE;
            protected static readonly eRWorkerTask PROCESS_INTERFACE_EVENT_TASK = eRWorkerTask.PROCESS_INTERFACE_EVENT;
            protected static readonly eRWorkerTask ERROR_TASK = eRWorkerTask.ERROR;
        #endregion

        #region VARIABLE
            private   static LogWorker              m_LWLogger      = null;
            protected ResourceManager               m_ResManager    = null;
        #endregion 

        #region CONSTRUCTOR
            public ResourceWorker(ResourceWorker clsTWLink)
                : base(clsTWLink)
            {
                m_LWLogger      = LogWorker.prtyclsLWInstance;
                m_ResManager    = ResourceManager.prtyclsLWInstance;
            }
        #endregion

        #region TRACE_METHOD
            public static void trace(TraceLevel eTLevel, params String[] arrstrTraces) {
                m_LWLogger.trace(eTLevel, arrstrTraces);
            }

            public static void trace(params String[] arrstrTraces)
            {
                m_LWLogger.trace(L1, arrstrTraces); 
            }

            public static void trace(Object str)
            {
                trace(str.ToString());
            }
        #endregion
        #region METHOD
            protected void KillTrace(){
                m_LWLogger.queueTask(new TaskStruct<eLogTask>(eLogTask.UNINITIALIZE, m_LWLogger));
            }



            protected sealed override void processTask(TaskStruct<eRWorkerTask> task)
            {
                foreach (MethodInfo method in this.GetType().GetMethods())
                {
                    if(eRWorkerTaskHandlerAttribute.isHandler(method) &&  eRWorkerTaskHandlerAttribute.getTask(method) == task.m_eType)
                    {
                        trace(L2,this.GetType().Name + ": invoking handler[" + method.Name + 
                               "] for task type=" + task.m_eType.ToString() + 
                               ((task.m_clsTWSourceTask == this) ? "" :  " from " + task.m_clsTWSourceTask));
                        method.Invoke(this, new Object[] { task });
                        return;
                    }
                }

                trace(L2,this.GetType().Name + ": no handler found for task type=" + task.m_eType.ToString());
            }

            [eRWorkerTaskHandlerAttribute(eRWorkerTask.UNINITIALIZE)]
            public virtual void Uninitialize_Handler(TaskStruct<eRWorkerTask> TSTask)
            {
                trace(this.GetType() + ": uninitialize()");
                TaskStruct<eRWorkerTask> TSNewTask = new TaskStruct<eRWorkerTask>(UNINITIALIZE_TASK,
                           this);
                this.m_clsTWLink.queueTask(TSNewTask);
                prtyAliveState = false;
            }
        #endregion
    }
}
