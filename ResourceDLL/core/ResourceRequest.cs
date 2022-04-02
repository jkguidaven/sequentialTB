using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GraphicDLL.XPanels;
using GraphicDLL;
using System.IO;
using System.Threading;
using ResourceDLL.worker;
using CoreDLL.foundation;
using CoreDLL.utilities;
using GraphicDLL.common;
using ResourceDLL.hook;

namespace ResourceDLL.core
{
    public class ResourceRequest : ResourceFunctionObj , iPageRequest
    {

        private static int              s_nRequestCount     = 1;
        private TabPage                 m_TBPage            = null;
        private String                  m_strPOSMLFile      = null;
        private String                  m_strFLMFile        = null;
        private String                  m_strStartTimeStamp = null;
        private String                  m_strEndTimeStamp   = null;
        private List<RequestLogData>    m_strLogData        = null;
        private Boolean                 m_bIncludeBackUp    = false;





        public ResourceRequest()
        {
            m_TBPage = new TabPage(s_nRequestCount > 1? 
                "SequentialTB("+s_nRequestCount+")" : 
                "SequentialTB");
            new XPanel_LoadingPane(this,m_TBPage, true);
            s_nRequestCount++;

        }


        public TabPage  getTabPage()                {   return m_TBPage;            }
        public String   getPOSMLogFilePath()        {   return m_strPOSMLFile;      }
        public String   getFLMLogFilePath()         {   return m_strFLMFile;        }
        public String   getLogTimeStampStart()      {   return m_strStartTimeStamp; }
        public String   getLogTimeStampEnd()        {   return m_strEndTimeStamp;   }

        public List<RequestLogData> LogData
        {
            get { return m_strLogData; }
            set { m_strLogData = value; }
        }


        public Boolean IncludeBackUp() { return m_bIncludeBackUp; }


        public Boolean STBEventHandler_OnLoadLogs(String strDiagPath,Boolean bIncludeBackUp)
        {
            this.m_bIncludeBackUp = bIncludeBackUp;
            if (File.Exists(strDiagPath + "\\posm.log") && File.Exists(strDiagPath + "\\flm.log"))
            {
                m_strPOSMLFile = strDiagPath + "\\posm.log";
                m_strFLMFile = strDiagPath + "\\flm.log";
                TaskStruct<eRWorkerTask> task = new TaskStruct<eRWorkerTask>(eRWorkerTask.PROCESS_REQUEST_DATA, 
                                                                         ParseEngineController.Instance,
                                                                         new ObjDataStruct(this));
                ParseEngineController.Instance.queueTask(task);

                return true;
            }
            else
            {
                TaskStruct<eRWorkerTask> task = new TaskStruct<eRWorkerTask>(eRWorkerTask.ERROR,
                                                         ParseEngineController.Instance,
                                                         new ObjDataStruct("Could not find posm.log/flm.log on path.Logs Not Found!"));
                ParseEngineController.Instance.queueTask(task);
                return false; // return false if Fails
            }
        }

        public void STBEventHandler_OnInterfaceEvent(int EventCode)
        {
            SequentialTBHook.Instance.InvokeEventHandler(this, EventCode);
        }

        public void EventHandler_OnGlobalKeyPress(int KeyID)
        {
            trace("+EventHandler_OnGlobalKeyPress");
            ParseEngineController.Instance.queueTask(new TaskStruct<eRWorkerTask>(
                                                        eRWorkerTask.PROCESS_INTERFACE_EVENT,
                                                        ParseEngineController.Instance,
                                                        new ObjDataStruct(KeyID),
                                                        new ObjDataStruct(this)));
            trace("-EventHandler_OnGlobalKeyPress");
        }


        public STBHandlerDelegate getSTBEventHandler()
        {
            return  SequentialTBHook.Instance.InvokeEventHandler;
        }
    }
}
