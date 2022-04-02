using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceDLL.worker;
using System.Reflection;
using System.Xml;
using System.Collections;
using System.IO;
using ResourceDLL.core;

namespace ResourceDLL.Manager
{
    public partial class ResourceManager : clsResourceTrace
    {

        #region VARIABLE
            private static          ResourceManager   s_clsRMInstance     = null;

            private static          List<ProcedureWrapper> s_lstProcObjData           = null;
            private static          List<PatternObjStruct> s_lstPatternObjStruct = null;
            private static          List<ParseDataObj> s_lstParseDataObjStruct   = null;


            private static readonly Object s_objStateLock           = new Object();
            private static readonly Object s_objProcListLock        = new Object();
            private static readonly Object s_objPatternListLock     = new Object();
            private static readonly Object s_objParseDataListLock   = new Object();
        #endregion
        

        #region PROPERTIES
            public List<ProcedureWrapper> ProcedureList
            { 
                get 
                {
                  lock(s_objProcListLock) 
                      return s_lstProcObjData;
                }
                set
                {
                    lock (s_objProcListLock)
                        s_lstProcObjData = value;
                }
            }

            public List<PatternObjStruct> PatternList
            {
                get
                {
                    lock (s_objPatternListLock)
                        return s_lstPatternObjStruct;
                }
                set
                {
                    lock (s_objPatternListLock)
                        s_lstPatternObjStruct = value;
                }
            }

            public List<ParseDataObj> ParseTable
            {
                get
                {
                    lock (s_objParseDataListLock)
                        return s_lstParseDataObjStruct;
                }
                set
                {
                    lock (s_objParseDataListLock)
                        s_lstParseDataObjStruct = value;
                }
            }
        #endregion



        #region CONSTRUCTOR
        public static ResourceManager prtyclsLWInstance
        {
            get
            {
                lock (s_objStateLock)
                {
                    if (s_clsRMInstance == null)
                        s_clsRMInstance = new ResourceManager();
                }
                return s_clsRMInstance;
            }
        }

        private ResourceManager() : base(L2)
        { 
           s_lstProcObjData =  new List<ProcedureWrapper>();
           s_lstPatternObjStruct = new List<PatternObjStruct>();
           s_lstParseDataObjStruct = new List<ParseDataObj>();
        }


        public Boolean isProcedureExist(String strProcName) {
            foreach (ProcedureWrapper procedure in ProcedureList)
            {
                if (procedure.getProcedureID().Equals(strProcName))
                    return true;
            }

            return false;
        }

        public ProcedureWrapper getProcedure(String strProcName)
        {
            foreach (ProcedureWrapper procedure in ProcedureList)
            {
                if (procedure.getProcedureID().Equals(strProcName))
                    return procedure;
            }

            return null;
        }

        public ParseDataObj getParseData(String ParseDataID)
        {
            foreach(ParseDataObj ParseData in ParseTable)
                if(ParseData.getParseDataID().Equals(ParseDataID))
                    return ParseData;


            return null;
        }
        #endregion

    }
}
