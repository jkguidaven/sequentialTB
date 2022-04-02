using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceDLL.worker;
using CoreDLL.foundation;

namespace ParseEngine
{
    class ParseWorker : ResourceWorker 
    {
        public ParseWorker(ParseEngineController clsPEController)
            : base(clsPEController)
        {
        }


        public void INITIALIZE(TaskStruct<eRWorkerTask> TStask)
        {
            m_clsTWLink.queueTask(new TaskStruct<eRWorkerTask>(INITIALIZE_TASK, this,TStask.m_lstODSData.ToArray()));
        }
    }
}
