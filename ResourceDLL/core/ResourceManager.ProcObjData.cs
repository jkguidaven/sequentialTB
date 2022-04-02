using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceDLL.xml_struct;
using System.Reflection;

namespace ResourceDLL.core
{
    public class ProcedureWrapper
    {
        private String m_strProcedureID = null;
        private List<ProcObj> m_lstProcObject = null;

        public ProcedureWrapper(String strProcedureID)
        {
            m_strProcedureID = strProcedureID;
            m_lstProcObject = new List<ProcObj>();
        }

        public Boolean isEqual(ProcedureWrapper strProcedureID)
        {
            return strProcedureID.m_strProcedureID.Equals(this.m_strProcedureID);
        }

        public override string ToString()
        {
            return m_strProcedureID;
        }

        public void add(ProcObj ProcObject)
        {
            m_lstProcObject.Add(ProcObject);
        }

        public String getProcedureID() { return m_strProcedureID; }

        public Queue<ProcObj> getActionList() { return new Queue<ProcObj>(m_lstProcObject); }
    }

    public enum ProcObjType { 
                              IF,               
                              ELSEIF,       
                              ELSE, 
                              ACTION, 
                            }

    public abstract class ProcObj
    {
  
        protected ProcObjType m_nProcObjType;

        protected ProcObj(ProcObjType ProcObjType)
        {
            m_nProcObjType = ProcObjType;
        }

        public ProcObjType getProcedureType()
        {
            return m_nProcObjType;
        }
    }

    public class ProcObjCondition : ProcObj
    {
        private List<ProcObj> m_lstSubProcObj = null;
        private LexemGroup m_LexModelExpression = null;
        public ProcObjCondition(ProcObjType Type)
            : base(Type)
        {
            if (Type != ProcObjType.IF &&
                Type != ProcObjType.ELSEIF &&
                Type != ProcObjType.ELSE )
                    throw new Exception("Invalid Procedure Assignment.");
            m_lstSubProcObj = new List<ProcObj>();
        }

        public void add(ProcObj ProcObject)
        {
            m_lstSubProcObj.Add(ProcObject);
        }

        public void setExpression(LexemGroup LexModelExpression)
        {
            m_LexModelExpression = LexModelExpression;
        }

        public LexemGroup getLexModelExpression()
        {
            return m_LexModelExpression;
        }


        public Queue<ProcObj> getActionList() { return new Queue<ProcObj>(m_lstSubProcObj); }
    }

    public class ProcObjAction : ProcObj
    {
        private MethodInfo refMethod;
        private String strParameters;
        public ProcObjAction(ProcObjType Type,MethodInfo method,String parameters)
            : base(Type)
        {
            if (Type != ProcObjType.ACTION)
                throw new Exception("Invalid Procedure Assignment.");


            refMethod = method;
            strParameters = parameters;
        }


        public MethodInfo getMethodReference()
        {
            return refMethod;
        }

        public String getParameterString()
        {
            return strParameters;
        }
    }


}
