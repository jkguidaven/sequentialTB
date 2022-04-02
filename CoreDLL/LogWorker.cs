using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreDLL.foundation;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace CoreDLL.logworker
{
    public sealed class LogWorker : TaskWorker<eLogTask>
    {
        #region VARIABLE
            private static readonly String      s_strVersion        = AssemblyName.GetAssemblyName("CoreDLL.dll").Version.ToString();
            private static          String      s_strLogFile        = null;
            private static          LogWorker   s_clsLWInstance     = null;
            private static          TraceLevel  s_eTLcurrTraceLevel = 0;
            private static          int         s_nStackLevel       = 0;

            private static Boolean s_bInitialize = false;
            private static readonly Object      s_objStateLock      = new Object();
        #endregion

        #region CONSTRUCTOR
            public static LogWorker prtyclsLWInstance 
            { 
                get {
                        lock (s_objStateLock)
                        {
                            if (s_clsLWInstance == null)
                                s_clsLWInstance = new LogWorker();
                        }
                        return s_clsLWInstance;
                    }  
            }



            private LogWorker()
                : base(null)
            {
                #if DEBUG
                    s_eTLcurrTraceLevel = TraceLevel.L3;
                    s_nStackLevel = 3;
                #else
                    s_eTLcurrTraceLevel = TraceLevel.L1;
                    s_nStackLevel = 1;
                #endif

                 s_strLogFile = GET_CONFIG("Trace", "LogFile");
                 if (!File.Exists(s_strLogFile))
                 {
                     using (var c_clsFile = File.Create(s_strLogFile))
                         c_clsFile.Close();
                 }

                TaskStruct<eLogTask> TSTask = new TaskStruct<eLogTask>(eLogTask.INITIALIZE,this);
                this.queueTask(TSTask);
            }

        #endregion


        #region METHOD
            protected override void processTask(TaskStruct<eLogTask> TSTask)
            {
                    using (StreamWriter c_clsSWriter = File.AppendText(s_strLogFile))
                    {
                        switch (TSTask.m_eType)
                        {
                            case eLogTask.INITIALIZE:
                                if (!s_bInitialize)
                                {
                                    c_clsSWriter.WriteLine("TRACE:" + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fffffff tt") + " - Tracer ver." + s_strVersion + " -");
                                    c_clsSWriter.WriteLine("TRACE:" + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fffffff tt") + " - { begin Capture } -");

                                    FileInfo f = new FileInfo(s_strLogFile);
                                    int f_size_limit = Int32.Parse(GET_CONFIG("Trace", "max-size"));

                                     if (f.Length > f_size_limit)
                                     {
                                         
                                         trace("log file reach size limit.");
                                         trace("Attempting to backup " + s_strLogFile + ".");

                                         try
                                         {
                                             String BackupFile = s_strLogFile + GET_CONFIG("Trace", "backup-format");
                                             if (File.Exists(BackupFile))
                                                 File.Delete(BackupFile);

                                             File.Copy(s_strLogFile, BackupFile);
                                             trace("Successfully backup file to " + s_strLogFile);
                                             trace("removing " + s_strLogFile + ".");
                                             c_clsSWriter.Close();
                                             File.Delete(s_strLogFile);


                                             using (var c_clsFile = File.Create(s_strLogFile))
                                                 c_clsFile.Close();

                                             using (StreamWriter c_clsSWriterBackup = File.AppendText(BackupFile))
                                             {
                                                 c_clsSWriterBackup.WriteLine(("TRACE:" + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fffffff tt") + " - End of TraceFile."));
                                                 c_clsSWriterBackup.Close();
                                             }
                                         }
                                         catch (Exception ex)
                                         {
                                             trace("Unsuccessful backup file to " + s_strLogFile + ";exception-desc:" + ex.Message);
                                         }



                                         using (StreamWriter new_c_clsSWriter = File.AppendText(s_strLogFile))
                                         {
                                             new_c_clsSWriter.WriteLine("TRACE:" + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fffffff tt") + " - Tracer ver." + s_strVersion + " -");
                                             new_c_clsSWriter.WriteLine("TRACE:" + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fffffff tt") + " - { begin Capture } -");
                                             new_c_clsSWriter.Close();
                                         }
                                     }

                                }
                                break;
                            case eLogTask.TRACE_JOB1:
                                    PrintJobStruct PJSJob = ObjDataStruct.get<PrintJobStruct>(TSTask.m_lstODSData);
                                    try
                                    {
                                        StackFrame clsSFrame = PJSJob.m_clsSFrame;
                                        String strFuncName = clsSFrame.GetMethod().Name;
                                        String strFileName = clsSFrame.GetFileName();
                                        strFileName = strFileName.Substring(strFileName.LastIndexOf("\\") + 1);
                                        int nLineNo = clsSFrame.GetFileLineNumber();

                                        String strTimestamp = "TRACE:" + PJSJob.m_clsDTime.ToString("MM/dd/yyyy hh:mm:ss.fffffff tt") + " - " + strFileName + ":" + strFuncName + "@" + nLineNo;
                                        String strFinal = strTimestamp + ((strFileName + ":" + strFuncName + "@" + nLineNo).Length < 36 ? " - " : " - ") + PJSJob.m_arrstrPrintables[0].ToString();

                                        c_clsSWriter.WriteLine(strFinal);

                                    }
                                    catch (Exception ex) { Console.WriteLine(ex.StackTrace); }
                                    c_clsSWriter.Close();
                                break;

                            case eLogTask.UNINITIALIZE:
                                c_clsSWriter.WriteLine("TRACE:" + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fffffff tt") + " - LogWorker::Uninitialize();");
                                c_clsSWriter.WriteLine("TRACE:" + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fffffff tt") + " - {  End Capture  } -");
                                prtyAliveState = false;
                                c_clsSWriter.Close();
                                break;
                        }

                    }
            }


            public void trace(TraceLevel eTLevel, int nStackDeepLevel, params String[] arrstrTraces)
            {
                if (s_eTLcurrTraceLevel >= eTLevel && arrstrTraces.Length > 0 && prtyAliveState)
                {
                    for (int i = 0; i < arrstrTraces.Count(); i++)
                    {
                        PrintJobStruct PJSJob = new PrintJobStruct(new StackFrame(nStackDeepLevel, true), DateTime.Now, arrstrTraces[i]);
                        queueTask(new TaskStruct<eLogTask>(eLogTask.TRACE_JOB1, this, new ObjDataStruct(PJSJob)));
                    }
                }
            }

            public void trace(TraceLevel eTLevel, params String[] arrstrTraces) { trace(eTLevel, s_nStackLevel, arrstrTraces); }
            public void trace(params String[] arrstrTraces) { trace(TraceLevel.L1, s_nStackLevel, arrstrTraces); }
        #endregion
    }
}
