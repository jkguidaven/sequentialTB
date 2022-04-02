using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CoreDLL.system;

namespace CoreDLL.foundation
{
    public abstract class TaskWorker<T> : SysConfig
    {
        #region VARIABLE
            protected        TaskWorker<T>        m_clsTWLink           = null;

            private readonly Object               m_objStateLock        = new Object();
            private readonly Object               m_objQueueLock        = new Object();
            private          Boolean              m_bAlive              = false;
            private          Queue<TaskStruct<T>> m_clsQTaskQueue       = null;
            private          Thread               m_thrdWork            = null;

        #endregion

        #region CONSTRUCTOR
            protected   TaskWorker(TaskWorker<T> clsTWlink)
                : this(clsTWlink, ThreadPriority.Normal) { }

            protected TaskWorker(TaskWorker<T> clsTWlink,
                                     ThreadPriority eTPriority) 
            {
                this.m_clsTWLink = clsTWlink;
                this.m_clsQTaskQueue  = new Queue<TaskStruct<T>>();


                this.m_thrdWork = new Thread(new ThreadStart(run));
                this.m_thrdWork.Name = this.GetType().ToString() + "Thread";
                this.m_thrdWork.Priority = eTPriority;

                this.prtyAliveState = true;
                this.m_thrdWork.Start();
            }
        #endregion


        #region METHOD
            private Queue<TaskStruct<T>> prtyThreadQueue 
            { 
                get { lock (m_objQueueLock) { return m_clsQTaskQueue;  } } 
                set { lock (m_objQueueLock) { m_clsQTaskQueue = value; } } 
                                              
            }

            protected Boolean prtyAliveState
            {
                get { lock (m_objStateLock) { return m_bAlive; } }
                set { lock (m_objStateLock) { m_bAlive = value; } }
            }
            
            protected abstract void processTask(TaskStruct<T> task);

            private void run()
            {
                while (prtyAliveState || prtyThreadQueue.Count > 0)
                {
                    if (prtyThreadQueue.Count > 0)
                        processTask(prtyThreadQueue.Dequeue());

                    Thread.Sleep(1);
                }
            }

            private void enQueue(Object obj) { prtyThreadQueue.Enqueue((TaskStruct<T>)obj); }
            public void queueTask(TaskStruct<T> task)
            {
                Thread _process = new Thread(new ParameterizedThreadStart(enQueue));
                _process.Name = this.GetType().ToString() + "::ProcessThread[" + task.m_eType.ToString() + "]";
                _process.Start(task);
            }
        #endregion

    }
}
