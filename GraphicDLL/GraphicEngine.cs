using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CoreDLL.logworker;
using GraphicDLL.XPanels;
using ResourceDLL.core;
using GraphicDLL.common;
using System.Threading;

namespace GraphicDLL
{
    public abstract class GraphicEngine : clsResourceTrace
    {
        private GraphicEngine() : base(TraceLevel.L2) { }

        private static List<Thread> m_lstRegisteredRunningThread = new List<Thread>();

        public static void ADD_GRAPHICAL_USER_INTERFACE(iPageRequest IPRequest)
        {
            trace("+INITIALIZING_GRAPHICAL_USER_INTERFACE: " + IPRequest.getTabPage());

            IPRequest.getTabPage().Invoke(new MethodInvoker(delegate
            {
                if (IPRequest.getPOSMLogFilePath() == null)
                    new XPanel_LoadLogPane(IPRequest);
                else
                {
                    new XPanel_Main(IPRequest);
                    foreach (Control ptrControl in IPRequest.getTabPage().Controls)
                    {
                        if (ptrControl is XPanel_LoadLogPane)
                        {
                            IPRequest.getTabPage().Controls.Remove(ptrControl);
                            (ptrControl as XPanel_LoadLogPane).StopLoading();
                            break;
                        }
                    }
                }
                XPanel.removeLoadingPane(IPRequest.getTabPage());


            }));
            trace("-INITIALIZING_GRAPHICAL_USER_INTERFACE: " + IPRequest.getTabPage());
        }

        public static void DISPLAY_STATUS_UPDATE(iPageRequest IPRequest,String StatusText)
        {

            trace("+DISPLAY_STATUS_UPDATE: " + IPRequest.getTabPage());
            IPRequest.getTabPage().Invoke(new MethodInvoker(delegate
            {
                foreach (Control ptrControl in IPRequest.getTabPage().Controls)
                {
                    if (ptrControl is XPanel_LoadingPane)
                    {
                        (ptrControl as XPanel_LoadingPane).setDisplayableText(StatusText);
                        ptrControl.Refresh();
                        break;
                    }
                    else if (ptrControl is XPanel_LoadLogPane)
                    {
                        (ptrControl as XPanel_LoadLogPane).setDisplayableText(StatusText);
                        ptrControl.Refresh();
                        break;
                    }
                }
            }));
            trace("-DISPLAY_STATUS_UPDATE: " + IPRequest.getTabPage());
        }


        public static void PROCESS_EVENT_ID(iPageRequest IPRequest, int EventID)
        {
            IPRequest.getTabPage().Invoke(new MethodInvoker(delegate
            {
                foreach (Control ptrControl in IPRequest.getTabPage().Controls)
                {
                    if (ptrControl is XPanel_Main)
                    {
                        XPanel_Main main_panel = ptrControl as XPanel_Main;
                        switch (EventID)
                        {
                            case (int)clsGUIEventTypes.HotKeyEventID.KEY_UP_ID:
                                main_panel.getEventPanel().getTimestampPanel().moveView(ArrowDirection.Up);
                                break;
                            case (int)clsGUIEventTypes.HotKeyEventID.KEY_DOWN_ID:
                                main_panel.getEventPanel().getTimestampPanel().moveView(ArrowDirection.Down);
                                break;
                        }
                        break;
                    }
                }
            }));
        }

        public static void DESTROY_REGISTERED_THREAD()
        {
            foreach (Thread thread in m_lstRegisteredRunningThread)
                thread.Abort();

            m_lstRegisteredRunningThread.Clear();
        }

        public static void RegisterThread(Thread thread)
        {
            m_lstRegisteredRunningThread.Add(thread);
        }

        public static void UnRegisterThread(Thread thread)
        {
            m_lstRegisteredRunningThread.Remove(thread);
        }


    }
}
