using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreDLL.foundation;
using System.Threading;
using ResourceDLL.core;

namespace ResourceDLL.worker
{
    public partial class ParseEngineController : ResourceWorker
    {
        #region MACRO
            enum ePEControllerState { STARTUP, INIT, RUNNING, UINIT, ERROR }
            private readonly ePEControllerState STARTUP = ePEControllerState.STARTUP;
            private readonly ePEControllerState INIT = ePEControllerState.INIT;
            private readonly ePEControllerState RUNNING = ePEControllerState.RUNNING;
            private readonly ePEControllerState UINIT = ePEControllerState.UINIT;
            private readonly ePEControllerState ERROR = ePEControllerState.ERROR;

            private Boolean UNINITIALIZE_SYNC
            {
                get 
                { 
                    return (m_clsGRWorker == null && m_clsPRWorker == null && 
                              m_clsPTWorker == null && m_clsPCWorker == null && 
                              m_clsDFWorker == null); 
                }
            }
        #endregion

        #region VARIABLE
            private static ParseEngineController s_clsPECInstance = null;
            private readonly Object              m_objStateLock   = new Object();
            private ePEControllerState           m_PECState;
            private GraphicWorker                m_clsGRWorker;
            private ParseWorker                  m_clsPRWorker;
            private PatternWorker                m_clsPTWorker;
            private ProcedureWorker              m_clsPCWorker;
            private DataFeedWorker               m_clsDFWorker;
        #endregion

        #region CONSTRUCTOR
            public static ParseEngineController Instance
            {
                get
                {
                    if (s_clsPECInstance == null)
                    {
                        s_clsPECInstance = new ParseEngineController();
                    }
                    return s_clsPECInstance;

                }
            }

            private ParseEngineController()
                : base (null)
            {
                this.m_clsGRWorker = new GraphicWorker(this);
                this.m_clsPRWorker = new ParseWorker(this);
                this.m_clsPTWorker = new PatternWorker(this);
                this.m_clsPCWorker = new ProcedureWorker(this);
                this.m_clsDFWorker = new DataFeedWorker(this);
            }

        #endregion

        #region METHOD
            private ePEControllerState prtyState
            {
                get { lock (m_objStateLock) { return m_PECState; } }
                set { lock (m_objStateLock) { m_PECState = value;} }
            }

            [eRWorkerTaskHandler(eRWorkerTask.INITIALIZE)]
            public void Initialize_Handler(TaskStruct<eRWorkerTask> TStask)
            {
                if (prtyState == STARTUP || prtyState == INIT)
                {
                    if(prtyState != INIT) prtyState = INIT;
                    if (TStask.m_clsTWSourceTask.Equals(this))
                    {
                        trace("SequentialTB initializing!", "loading XML Configuration Files..");
                        m_clsPCWorker.queueTask(new TaskStruct<eRWorkerTask>(INITIALIZE_TASK,
                                                this, 
                                                TStask.m_lstODSData.ToArray()));
                    }
                    else if (TStask.m_clsTWSourceTask.Equals(m_clsPCWorker))
                    {
                        m_clsPRWorker.queueTask(new TaskStruct<eRWorkerTask>(INITIALIZE_TASK,
                                                this,
                                                TStask.m_lstODSData.ToArray()));

                    }
                    else if (TStask.m_clsTWSourceTask.Equals(m_clsPRWorker))
                    {
                        m_clsPTWorker.queueTask(new TaskStruct<eRWorkerTask>(INITIALIZE_TASK,
                                                this,
                                                TStask.m_lstODSData.ToArray()));

                    }
                    else if (TStask.m_clsTWSourceTask.Equals(m_clsPTWorker))
                    {
                        trace("Successfully loaded XML Configuration files.", "SequentialTB application ready!");
                        prtyState = RUNNING;
                    }
                }
            }

            [eRWorkerTaskHandler(eRWorkerTask.PROCESS_GUI_REQUEST)]
            public void GUIRequest_Handler(TaskStruct<eRWorkerTask> TSTask)
            {
                if (prtyState != ERROR && prtyState != UINIT)
                {
                    if (prtyState == RUNNING)
                    {
                        ResourceRequest request = ObjDataStruct.peek<ResourceRequest>(TSTask.m_lstODSData);

                        if (request.getPOSMLogFilePath() == null && request.getFLMLogFilePath() == null)
                        {
                            trace("processing new SequentialTB User Interface.");
                            m_clsGRWorker.queueTask(TSTask);
                        }
                        else
                        {
                            ProcessRequestData_Handler(new TaskStruct<eRWorkerTask>(PROCESS_REQUEST_DATA_TASK,
                                                                                  this, new ObjDataStruct(request)));
                        }

                        System.GC.Collect();
                    }
                    else
                    {
                        if (prtyState == STARTUP)
                            queueTask(new TaskStruct<eRWorkerTask>(eRWorkerTask.INITIALIZE, this, TSTask.m_lstODSData.ToArray()));

                        Thread.Sleep(200); // give 200 milisecond allowance to requeue
                        this.queueTask(TSTask);
                    }
                }
            }

            [eRWorkerTaskHandler(eRWorkerTask.PROCESS_REQUEST_DATA)]
            public void ProcessRequestData_Handler(TaskStruct<eRWorkerTask> TSTask)
            {
                if (prtyState == RUNNING)
                {
                    if (TSTask.m_clsTWSourceTask == this)
                    {
                        trace("Forwarding Raw Data Request to DataFeederWorker.");
                        m_clsDFWorker.queueTask(TSTask);
                    }
                    else if (TSTask.m_clsTWSourceTask == m_clsDFWorker)
                    {
                        trace("Forwarding Raw Data Logs to PatternWorker.");
                        m_clsPTWorker.queueTask(TSTask);
                    }
                    else if (TSTask.m_clsTWSourceTask == m_clsPTWorker)
                    {

                        trace("Forwading Filtered DataLogs to ParseWorker.");
                        m_clsPRWorker.queueTask(TSTask);
                    }
                    else if (TSTask.m_clsTWSourceTask == m_clsPRWorker)
                    {
                        trace("Forwading DataLogs to ProcedureWorker.");
                        m_clsPCWorker.queueTask(TSTask);
                    }
                    else
                    {
                        trace("Successfully Process LogData, forwarding data to GraphicEngine for Display.");
                        ResourceRequest request = ObjDataStruct.get<ResourceRequest>(TSTask.m_lstODSData);
                        trace("processing new SequentialTB User Interface.");
                        m_clsGRWorker.queueTask(new TaskStruct<eRWorkerTask>(eRWorkerTask.PROCESS_GUI_REQUEST,
                                                                         this,
                                                                         new ObjDataStruct(request)));
                    }
                }
            }

            [eRWorkerTaskHandler(eRWorkerTask.PROCESS_STATUS_UPDATE)]
            public void StatusUpdate_Handler(TaskStruct<eRWorkerTask> TSTask)
            {
                trace("Forwarding Status Update data to GraphicWorker!");
                m_clsGRWorker.queueTask(TSTask);
            }

            [eRWorkerTaskHandler(eRWorkerTask.ERROR)]
            public void Error_Handler(TaskStruct<eRWorkerTask> TSTask)
            {
                trace("processing new Error Exception.");
                m_clsGRWorker.queueTask(TSTask);
                if(TSTask.m_clsTWSourceTask != this) prtyState = ERROR;
            }

            [eRWorkerTaskHandler(eRWorkerTask.UNINITIALIZE)]
            public override void Uninitialize_Handler(TaskStruct<eRWorkerTask> TSTask)
            {
                if(prtyState != UINIT)
                {
                    trace("SequentialTB Uninitializing!");
                    TaskStruct<eRWorkerTask> TSNewTask = new TaskStruct<eRWorkerTask>(UNINITIALIZE_TASK, this);
                    m_clsGRWorker.Uninitialize_Handler(TSNewTask);
                    m_clsPRWorker.Uninitialize_Handler(TSNewTask);
                    m_clsPCWorker.Uninitialize_Handler(TSNewTask);
                    m_clsPTWorker.Uninitialize_Handler(TSNewTask);
                    m_clsDFWorker.Uninitialize_Handler(TSNewTask);

                    prtyState = UINIT;
                }
                else
                {
                    if      (TSTask.m_clsTWSourceTask.Equals(m_clsGRWorker)) m_clsGRWorker = null;
                    else if (TSTask.m_clsTWSourceTask.Equals(m_clsPRWorker)) m_clsPRWorker = null;
                    else if (TSTask.m_clsTWSourceTask.Equals(m_clsPCWorker)) m_clsPCWorker = null;
                    else if (TSTask.m_clsTWSourceTask.Equals(m_clsPTWorker)) m_clsPTWorker = null;
                    else if (TSTask.m_clsTWSourceTask.Equals(m_clsDFWorker)) m_clsDFWorker = null;

                    if (UNINITIALIZE_SYNC)
                    { 
                        prtyAliveState = false;
                        KillTrace();
                    }
                }
            }

            [eRWorkerTaskHandler(eRWorkerTask.PROCESS_INTERFACE_EVENT)]
            public void InterfaceEvent_Handler(TaskStruct<eRWorkerTask> TSTask)
            {
                if (prtyState == RUNNING)
                {
                    trace("Event captured! processing Event ID[forwarding to GraphicWorker]");
                    m_clsGRWorker.queueTask(TSTask);
                }
            }

        #endregion

    }
}
