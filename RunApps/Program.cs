using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CoreDLL.foundation;
using ResourceDLL.worker;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SequentialTB;

namespace RunApps
{
    static class Program
    {

        public static ContextMenuStrip mnu = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ParseEngineController controller = ParseEngineController.Instance;

            //controller.queueTask(new TaskStruct<eRWorkerTask>(eRWorkerTask.INITIALIZE, controller));


            Application.ApplicationExit += new EventHandler(SequentialTBInstance.Instance.OnApplicationExit);

            #if DEBUG
            if (!IsProcessOpen("baretail"))
            {
            Process secondProc = new Process();
            secondProc.StartInfo.FileName = "SequentialTBTrace.log";
            secondProc.Start();
            }
            #endif




            Form1 formApp = new Form1();
            formApp.Activated += new EventHandler(SequentialTBInstance.Instance.OnActivate);
            formApp.Deactivate += new EventHandler(SequentialTBInstance.Instance.OnActivate);
            formApp.LostFocus += new EventHandler(SequentialTBInstance.Instance.OnFocus);
            formApp.KeyDown += new KeyEventHandler(SequentialTBInstance.Instance.OnKeyPress);
            formApp.KeyPreview = true;
            TabPage new_page = SequentialTBInstance.Instance.generatePage();

            //TabPage new_page2 = SequentialTB.generatePage();

            formApp.tabControl1.Controls.Add(new_page);
            //formApp.tabControl1.Controls.Add(new_page2);
            formApp.tabControl1.SelectedTab = new_page;

           // formApp.tabControl1.MouseClick += new MouseEventHandler(Delete);
            Application.Run(formApp);


        }



        public static void Delete(object sender, MouseEventArgs e)
        {
            foreach (Control control in (sender as TabControl).Controls)
            {
                if (control is TabPage)
                {
                    (sender as TabControl).Controls.Remove(control);

                    control.Dispose();
                    break;
                }
            }
        }

        public static bool IsProcessOpen(string name)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains(name))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
