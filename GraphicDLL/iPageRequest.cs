using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace GraphicDLL
{
    public interface iPageRequest
    {
        Boolean STBEventHandler_OnLoadLogs(String strDiagPath);
        TabPage getTabPage();
        String getPOSMLogFilePath();
        String getFLMLogFilePath();
    }
}
