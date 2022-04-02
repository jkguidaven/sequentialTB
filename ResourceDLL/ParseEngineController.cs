using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceDLL.worker;
using CoreDLL.foundation;

namespace ParseEngine
{
    public partial class ParseEngineController : ResourceWorker
    {
        #region MACRO
            enum             ePEControllerState { INIT , RUNNING, UINIT }
            private readonly ePEControllerState INIT = ePEControllerState.INIT;
            private readonly ePEControllerState RUNNING = ePEControllerState.RUNNING;
            private readonly ePEControllerState UINIT = ePEControllerState.UINIT;

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
            private static ParseEngineController s_clsPECInstance = new ParseEngineController();
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

            public void INITIALIZE(TaskStruct<eRWorkerTask> TStask)
            {
                if (prtyState == INIT)
                {
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
                        trace("Successfully loaded XML Configuration files.","SequentialTB application ready!");
                        prtyState = RUNNING;
                    }
                }
            }

            public override void UNINITIALIZE(TaskStruct<eRWorkerTask> TSTask)
            {
                

                if(prtyState != UINIT)
                {
                    trace("SequentialTB Uninitializing!");
                    TaskStruct<eRWorkerTask> TSNewTask = new TaskStruct<eRWorkerTask>(UNINITIALIZE_TASK, this);
                    m_clsGRWorker.queueTask(TSNewTask);
                    m_clsPRWorker.queueTask(TSNewTask);
                    m_clsPCWorker.queueTask(TSNewTask);
                    m_clsPTWorker.queueTask(TSNewTask);
                    m_clsDFWorker.queueTask(TSNewTask);

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

            public void OnApplicationExit(object sender, EventArgs e)
            {
                TaskStruct<eRWorkerTask> TSNewTask = new TaskStruct<eRWorkerTask>(UNINITIALIZE_TASK, this);
                this.queueTask(TSNewTask);
            }
        #endregion

    }
}
