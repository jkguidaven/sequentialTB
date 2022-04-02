using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreDLL.foundation;
using ResourceDLL.core;
using GraphicDLL;
using System.Windows.Forms;

namespace ResourceDLL.worker
{
    class GraphicWorker : ResourceWorker 
    {
        public GraphicWorker(ParseEngineController clsPEController)
            : base(clsPEController)
        {
        }

        [eRWorkerTaskHandler(eRWorkerTask.PROCESS_GUI_REQUEST)]
        public void GUIRequest_Handler(TaskStruct<eRWorkerTask> TSTask)
        {
            ResourceRequest request = ObjDataStruct.get<ResourceRequest>(TSTask.m_lstODSData);


            GraphicEngine.ADD_GRAPHICAL_USER_INTERFACE(request);
            
        }
        [eRWorkerTaskHandler(eRWorkerTask.ERROR)]
        public void Error_Handler(TaskStruct<eRWorkerTask> TSTask)
        {
            String Data = ObjDataStruct.get<String>(TSTask.m_lstODSData);

            MessageBox.Show(Data,"SequentialTB Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        [eRWorkerTaskHandler(eRWorkerTask.PROCESS_STATUS_UPDATE)]
        public void StatusUpdate_Handler(TaskStruct<eRWorkerTask> TSTask)
        {
            trace("Processing Status Update, reflecting to GUI!");
            try
            {
                String StatusUpdateText = ObjDataStruct.get<String>(TSTask.m_lstODSData);
                GraphicEngine.DISPLAY_STATUS_UPDATE(ObjDataStruct.get<ResourceRequest>(TSTask.m_lstODSData), StatusUpdateText);
            }
            catch (Exception) { }
        }

        public override void Uninitialize_Handler(TaskStruct<eRWorkerTask> TSTask)
        {
            GraphicEngine.DESTROY_REGISTERED_THREAD();
            base.Uninitialize_Handler(TSTask);
        }


        [eRWorkerTaskHandler(eRWorkerTask.PROCESS_INTERFACE_EVENT)]
        public void InterfaceEvent_Handler(TaskStruct<eRWorkerTask> TSTask)
        {
            GraphicEngine.PROCESS_EVENT_ID(ObjDataStruct.get<ResourceRequest>(TSTask.m_lstODSData), 
                                           ObjDataStruct.get<int>(TSTask.m_lstODSData));
        }
    }
}
