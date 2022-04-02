using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace GraphicDLL.common
{
    public delegate void STBHandlerDelegate(iPageRequest request,int EventCode);

    public interface iPageRequest
    {
        TabPage getTabPage();
        String getPOSMLogFilePath();
        String getFLMLogFilePath();
        Queue<clsObjLLEvent> getPOSMLLEventList();
        Queue<clsObjLLEvent> getPOSLLEventList();
        Queue<clsObjLLEvent> getFLMLLEventList();
        Queue<clsObjLLEvent> getFLLLEventList();

        Boolean STBEventHandler_OnLoadLogs(String strDiagPath, Boolean bIncludeBackUp);
        STBHandlerDelegate getSTBEventHandler();
    }
}
