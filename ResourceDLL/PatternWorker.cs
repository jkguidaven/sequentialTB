using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceDLL.worker;
using ResourceDLL.Manager;
using ResourceDLL.xml_struct;
using CoreDLL.foundation;

namespace ParseEngine
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



        public void INITIALIZE(TaskStruct<eRWorkerTask> TStask)
        {
            trace("Initializing Pattern database.");
            trace("Loading XML[" + getXML() + "] database file.");
            if (m_ResManager.LoadingXML(this))
            {
                trace("successfully loaded XML[" + getXML() + "].");
                m_clsTWLink.queueTask(new TaskStruct<eRWorkerTask>(INITIALIZE_TASK, this));
            }
            else
            {
                m_clsTWLink.queueTask(new TaskStruct<eRWorkerTask>(ERROR_TASK,
                    this,
                    "Error loading XML[" + getXML() + "] database file! aborting initialization, " +
                    "see log[" + GET_CONFIG("Trace", "LogFile") + "] file for Error description."));
            }
        }

    }
}
