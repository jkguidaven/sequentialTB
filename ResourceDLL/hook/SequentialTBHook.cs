using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceDLL.worker;
using System.Windows.Forms;
using ResourceDLL.core;
using CoreDLL.foundation;
using System.Runtime.InteropServices;
using GraphicDLL.common;

namespace ResourceDLL.hook
{
    
    public delegate void SequentialTBEventHandler(object sender,SequentialTBEventArgs e);
    public class SequentialTBHook : NativeWindow , IDisposable
    {

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private static Boolean OnExit = false;
        private static int WM_HOTKEY = 0x0312;

        protected static ResourceRequest m_currRequest = null;


        public event SequentialTBEventHandler OnSequentialTBEvent;

        protected static SequentialTBHook m_clsInstance = null;


        protected SequentialTBHook() { }


        public static SequentialTBHook Instance
        {
            get
            {
                if (m_clsInstance == null)
                {
                    m_clsInstance = new SequentialTBHook();
                    m_clsInstance.CreateHandle(new CreateParams());
                    m_clsInstance.registerAllHotkeys();
                }

                return m_clsInstance;
            }
        }

        public struct KeyModifier
        {
            public int HotKeyID;
            public ModifierKeys KeyModifierID;
            public Keys KeyEventID;

            public enum ModifierKeys : uint
            {
                Alt = 1,
                Control = 2,
                Shift = 4,
                Win = 8
            }

            public KeyModifier(int HotKeyID, ModifierKeys KeyModifierID, Keys KeyEventID)
            {
                this.KeyEventID = KeyEventID;
                this.KeyModifierID = KeyModifierID;
                this.HotKeyID = HotKeyID;
            }
        }

        public static readonly KeyModifier[] keyModifiers = new KeyModifier[]{
            new KeyModifier((int)clsGUIEventTypes.HotKeyEventID.GOTO_ID,KeyModifier.ModifierKeys.Control,Keys.G),
            new KeyModifier((int)clsGUIEventTypes.HotKeyEventID.FIND_ID,KeyModifier.ModifierKeys.Control,Keys.F)
        };




        protected override void WndProc(ref Message m)
        {

            base.WndProc(ref m);

            if (m.Msg == WM_HOTKEY)
            {
               
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);                  
                int id = m.WParam.ToInt32();                                        

                if (m_currRequest != null)
                    m_currRequest.EventHandler_OnGlobalKeyPress(id);
            }



        }


        public void Dispose()
        {
            unregisterAllHotkeys();
        }

        public void registerAllHotkeys()
        {
            if (!OnExit)
            {
                foreach (KeyModifier modifier in keyModifiers)
                {
                    try
                    {
                        RegisterHotKey(this.Handle,
                                            modifier.HotKeyID,
                                           (int)modifier.KeyModifierID,
                                           (int)modifier.KeyEventID);
                    }
                    catch (Exception) { }
                }
            }
        }

        public void unregisterAllHotkeys()
        {
            foreach (KeyModifier key in keyModifiers)
            {
                UnregisterHotKey(this.Handle, key.HotKeyID);
            }
        }



        [DllImport("user32.dll")]
        static extern int GetForegroundWindow();

        public void OnActivate(object sender, EventArgs e)
        {
            int handle = GetForegroundWindow();

            if ((sender as Form).Handle == (IntPtr)handle)
                registerAllHotkeys();
            else
                unregisterAllHotkeys();
        }


        public void OnFocus(object sender, EventArgs e)
        {
            registerAllHotkeys();
        }


        public void OnApplicationExit(object sender, EventArgs e)
        {
            TaskStruct<eRWorkerTask> TSNewTask = new TaskStruct<eRWorkerTask>(eRWorkerTask.UNINITIALIZE, ParseEngineController.Instance);
            ParseEngineController.Instance.queueTask(TSNewTask);
            this.Dispose();
            OnExit = true;
        }


        public void OnKeyPress(object sender, KeyEventArgs e)
        {

            if (m_currRequest != null)
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        m_currRequest.EventHandler_OnGlobalKeyPress((int)clsGUIEventTypes.HotKeyEventID.KEY_UP_ID);
                        break;
                    case Keys.Down:
                        m_currRequest.EventHandler_OnGlobalKeyPress((int)clsGUIEventTypes.HotKeyEventID.KEY_DOWN_ID);
                        break;
                }
            }
        }

        public void InvokeEventHandler(iPageRequest request, int EventCode)
        {
            SequentialTBEventHandler handler = OnSequentialTBEvent;

            if (handler != null)
                handler(request, new SequentialTBEventArgs(EventCode));
        }

        public TabPage generatePage()
        {
            ResourceRequest request = new ResourceRequest();
            #if DEBUG

            //request.STBEventHandler_OnLoadLogs(@"C:\WORKBENCH\Authorized Project\MASTERS\Tasks\Issues\Open\SSCOI-21461\7585hisssc007-130619-113551", false);
            request.STBEventHandler_OnLoadLogs(@"C:\WORKBENCH\Authorized Project\Coles\Tasks\Issues\Open\SSCOI-20257\SV2023NW111-130426-145411", false);
            #endif

            TaskStruct<eRWorkerTask> task = new TaskStruct<eRWorkerTask>(eRWorkerTask.PROCESS_GUI_REQUEST,
                                                                         ParseEngineController.Instance,
                                                                         new ObjDataStruct(request));


            ParseEngineController.Instance.queueTask(task);
            m_currRequest = request;
            return request.getTabPage();
        }
    }
}
