using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceDLL.Manager;
using CoreDLL.foundation;
using System.Reflection;

namespace ResourceDLL.worker
{

    public enum eRWorkerTask 
    { 
        INITIALIZE, 
        PROCESS_GUI_REQUEST, 
        PROCESS_REQUEST_DATA , 
        UNINITIALIZE, 
        ERROR , 
        PROCESS_STATUS_UPDATE ,
        PROCESS_INTERFACE_EVENT 
    }
   
    [AttributeUsage(AttributeTargets.Method)]
    public class eRWorkerTaskHandlerAttribute : System.Attribute
    {
        private eRWorkerTask m_TaskAssigned;

        public eRWorkerTask getTask()
        {
            return m_TaskAssigned;
        }

        public eRWorkerTaskHandlerAttribute(eRWorkerTask Task)
        {
            m_TaskAssigned = Task;
        }

        public static Boolean isHandler(MethodInfo method)
        {
            foreach (object attribute in method.GetCustomAttributes(true))
            {
                if (attribute is eRWorkerTaskHandlerAttribute)
                    return true;
            }

            return false;
        }

        public static eRWorkerTask getTask(MethodInfo method)
        {
            foreach (object attribute in method.GetCustomAttributes(true))
            {
                if (attribute is eRWorkerTaskHandlerAttribute)
                    return (attribute as eRWorkerTaskHandlerAttribute).getTask();
            }

            throw new Exception("Unexpected method, cannot read TaskHandler!");
        }
    }

    public interface iResourceXMLWorker {

        String          getXML();
        XMLBlueprint getXMLStructure();
    }
}
