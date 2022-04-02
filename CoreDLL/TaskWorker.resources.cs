using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreDLL.foundation
{
    public struct ObjDataStruct
    {
        #region VARIABLE
            public Type     m_clsTClass;
            public Object   m_objData;
        #endregion

        #region CONSTRUCTOR
            public ObjDataStruct(Object objData)
            {
                this.m_clsTClass    = objData.GetType();
                this.m_objData      = objData;
            }
        #endregion CONSTRUCTOR


        #region METHOD
            public static T get<T>(List<ObjDataStruct> lstODSData)
            {

                foreach (ObjDataStruct Objdata in lstODSData)
                {
                    if (Objdata.m_objData is T)
                    {
                       lstODSData.Remove(Objdata);
                       return (T)Objdata.m_objData;
                    }
                }

                throw new Exception("Enable to get data from cObjData,Type=" + typeof(T));
            }

            public static T peek<T>(List<ObjDataStruct> lstODSData)
            {

                foreach (ObjDataStruct Objdata in lstODSData)
                {
                    if (Objdata.m_objData is T)
                    {
                        return (T)Objdata.m_objData;
                    }
                }

                throw new Exception("Enable to get data from cObjData,Type=" + typeof(T));
            }
        #endregion
    }

    public struct TaskStruct<T>
    {
        public T                    m_eType;
        public TaskWorker<T>        m_clsTWSourceTask;
        public List<ObjDataStruct>  m_lstODSData;

        public TaskStruct(T eType)
            : this(eType, null)
        {
        }

        public TaskStruct(T eType, 
                          TaskWorker<T> clsTWSourceTask,
                          params ObjDataStruct[] arrOjbData)
        {
            this.m_eType            = eType;
            this.m_clsTWSourceTask  = clsTWSourceTask;
            this.m_lstODSData = new List<ObjDataStruct>(arrOjbData);

        }
    }

}
