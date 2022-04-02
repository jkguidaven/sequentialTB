using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceDLL.worker;
using ResourceDLL.Manager;
using CoreDLL.foundation;
using ResourceDLL.xml_struct;

namespace ParseEngine
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



        public void INITIALIZE(TaskStruct<eRWorkerTask> TStask)
        {
            trace("Initializing Procedure database.");
            trace("Loading XML[" + getXML() + "] database file.");
            if (m_ResManager.LoadingXML(this))
            {
                trace("successfully loaded XML[" + getXML() + "].");
                m_clsTWLink.queueTask(new TaskStruct<eRWorkerTask>(INITIALIZE_TASK, this , TStask.m_lstODSData.ToArray() ));
            }
            else
            {
                m_clsTWLink.queueTask(new TaskStruct<eRWorkerTask>(ERROR_TASK,
                    this,
                    "Error loading XML[" + getXML() + "] database file! aborting initialization, " +
                    "see log[" + GET_CONFIG("Trace","LogFile") + "] file for Error description.",
                    TStask.m_lstODSData.ToArray()));
            }
        }
    }
}
