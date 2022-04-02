using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceDLL.worker;
using System.Windows.Forms;
using ResourceDLL.core;
using CoreDLL.foundation;

namespace ResourceDLL
{
    public class SequentialTB
    {
        public static TabPage generatePage()
        {
            ResourceRequest request = new ResourceRequest();
            //request.STBEventHandler_OnLoadLogs(@"C:\WORKBENCH\Authorized Project\Coles\Tasks\Issues\Open\SSCOI-20257\SV2023NW111-130426-145411");
            
            TaskStruct<eRWorkerTask> task = new TaskStruct<eRWorkerTask>(eRWorkerTask.PROCESS_GUI_REQUEST, 
                                                                         ParseEngineController.Instance,
                                                                         new ObjDataStruct(request));
            ParseEngineController.Instance.queueTask(task);
            return request.getTabPage();
        }
    }
}
