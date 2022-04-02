using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreDLL.utilities;
using System.Reflection;
using ResourceDLL.core;

namespace ResourceDLL.xml_struct
{
    public enum LexemType { GT, GTE, LT, LTE, EE, NE, INT, BOOL, STRING, OR, AND , FUNC}

    public class Lexem : clsResourceTrace
    {
        private LexemType m_Type;
        private String    m_strVal;


        public Lexem(LexemType type, String strVal) : base(L4) { m_strVal = strVal; m_Type = type; }
        public Lexem(LexemType type) :base(L3)  {   m_strVal = "";m_Type = type; }
        public Lexem(String strVal) : base(L3)  { m_strVal = strVal;  m_Type   = getType(m_strVal); }

        public LexemType getType()
        {
            return m_Type;
        }

        public String getVal() { return m_strVal; }

        public static LexemType getType(String strVal)
        {
            int n=0;
            if (StringHelper.isStringLiteral(strVal))
                return LexemType.STRING;
            else if (StringHelper.isStringBoolean(strVal))
                return LexemType.BOOL;
            else if (strVal.Equals("=="))
                return LexemType.EE;
            else if (strVal.Equals("!="))
                return LexemType.NE;
            else if (strVal.Equals("<="))
                return LexemType.LTE;
            else if (strVal.Equals(">="))
                return LexemType.GTE;
            else if (strVal.Equals("<"))
                return LexemType.LT;
            else if (strVal.Equals(">"))
                return LexemType.GT;
            else if (strVal.Equals("||"))
                return LexemType.OR;
            else if (strVal.Equals("&&"))
                return LexemType.AND;
            else if (Int32.TryParse(strVal,out n))
                return LexemType.INT;
            else
            {
                if (ResourceFunctionObj.isFunctionExist(strVal))
                    return LexemType.FUNC;
            }

            throw new Exception("Unknown Lexem Type!!!");
        }

        protected static LexemType getFunctionReturnLexemType(MethodInfo methodInfo)
        {

            if (methodInfo.ReturnType.IsEquivalentTo("".GetType()))
                return LexemType.STRING;
            else if (methodInfo.ReturnType.IsEquivalentTo(true.GetType()))
                return LexemType.BOOL;
            else if (methodInfo.ReturnType.IsEquivalentTo(new int().GetType()))
                return LexemType.INT;
            else
                throw new Exception("Unknown Lexem Type!!!");


            throw new Exception("Enable to find function!");
        }
    }

    public class LexemGroup : Lexem
    {
        public List<Lexem>  m_lstLexem = new List<Lexem>();
        public LexemTree    m_LTree    = null;
        public LexemGroup(String strVal)
            : this(new Queue<String>(StringHelper.SplitExpression(strVal)))
        {
        }


        public LexemGroup(Queue<String> lexes)
            : base(LexemType.BOOL)
        {
            trace("Reading Strings of lexes.");
            String DisplayLex = "Lexes = { ";
            foreach (String lex in lexes)
                DisplayLex += "\"" + lex + "\" , ";
            DisplayLex += "}";
            trace(DisplayLex);
            ReadLexes(ref lexes);
        }

        public Queue<LexemType> getLexemTypes()
        {
            Queue<LexemType> LexemTypes = new Queue<LexemType>();

            foreach(Lexem lexem in m_lstLexem)
            {
                LexemTypes.Enqueue(lexem.getType());
            }

            return LexemTypes;
        }

        public void ReadLexes(ref Queue<String> lexes)
        {
            trace("+ReadLexes");
            while (lexes.Count > 0)
            {
                String ptrLex = lexes.Dequeue();
                if (ptrLex == "(")
                {
                    m_lstLexem.Add(new LexemGroup(lexes));
                }
                else if (ptrLex == ")")
                    return;
                else
                {
                    LexemType type = getType(ptrLex);

                    if (type == LexemType.FUNC)
                    {
                        String funcName = ptrLex;
                        trace("function found at expression. reading syntax.");
                        int PairCount = 0;
                        while (lexes.Count > 0)
                        {
                            String lastPop = lexes.Dequeue();
                            ptrLex += lastPop;

                            if (lastPop == "(")
                            {
                                PairCount++;
                            }

                            if (lastPop == ")")
                            {
                                PairCount--;
                                if(PairCount==0)
                                break;
                            }
                        }
                        trace("function read: " + ptrLex);
                        if (!ResourceFunctionObj.isValidFunctionSyntax(ptrLex))
                        {
                            trace("Invalid Function Syntax found!");
                            trace("-ReadLexes");
                                throw new Exception("Invalid Function Syntax found!");
                        }

                        type = getFunctionReturnLexemType(ResourceFunctionObj.getFunctionMethod(ptrLex));
                    }
                    m_lstLexem.Add(new Lexem(type,ptrLex));
                }
            }

            m_LTree = generateLexemTree(m_lstLexem);
            trace("-ReadLexes");
        }

        public LexemTree getLexemTree()
        {
            return m_LTree.clone(); 
        }
        private static LexemTree generateLexemTree(List<Lexem> lstLexem)
        {
            trace("+generateLexemTree");
            Queue<Lexem> LexesQueue = new Queue<Lexem>(lstLexem);
            LexemTree TopTree = null;
            while (LexesQueue.Count > 0)
            {
                Lexem CurrLexem = LexesQueue.Dequeue();

                if (CurrLexem.getType() == LexemType.OR || CurrLexem.getType() == LexemType.AND)
                {
                    LexemTree newTree = new LexemTree(CurrLexem.getType());
                    if (TopTree == null)
                        TopTree = newTree;
                    else
                    {
                        newTree.LeftTree = TopTree;
                        TopTree = newTree;
                    }
                }
                else if (CurrLexem.getType() == LexemType.EE ||
                        CurrLexem.getType() == LexemType.NE ||
                        CurrLexem.getType() == LexemType.GT ||
                        CurrLexem.getType() == LexemType.GTE ||
                        CurrLexem.getType() == LexemType.LT ||
                        CurrLexem.getType() == LexemType.LTE)
                {

                    LexemTree newTree = new LexemTree(CurrLexem.getType());

                    if (TopTree == null)
                        TopTree = newTree;
                    else 
                        TopTree.RightTree = newTree;
                }
            }

            LexesQueue = new Queue<Lexem>(lstLexem.Where(CurrLexem => CurrLexem.getType() != LexemType.EE &&
                                                                        CurrLexem.getType() != LexemType.NE &&
                                                                        CurrLexem.getType() != LexemType.GT &&
                                                                        CurrLexem.getType() != LexemType.GTE &&
                                                                        CurrLexem.getType() != LexemType.LT &&
                                                                        CurrLexem.getType() != LexemType.LTE &&
                                                                        CurrLexem.getType() != LexemType.OR &&
                                                                        CurrLexem.getType() != LexemType.AND));
            while (LexesQueue.Count > 0)
            {
                Lexem CurrLexem = LexesQueue.Dequeue();

                if (CurrLexem is LexemGroup)
                {
                    if(TopTree == null) TopTree = new LexemTree(null);
                    TopTree.add( generateLexemTree((CurrLexem as LexemGroup).m_lstLexem));
                }
                else
                {
                    Boolean bIsFunction = false;

                    try
                    {
                        bIsFunction = ResourceFunctionObj.isValidFunctionSyntax(CurrLexem.getVal());
                    }
                    catch (Exception) { }

                    if (!bIsFunction)
                    {
                        if (CurrLexem.getType() == LexemType.STRING)
                            TopTree.add(CurrLexem.getVal().Substring(1,CurrLexem.getVal().Length-2));
                        else if (CurrLexem.getType() == LexemType.INT)
                            TopTree.add(Int32.Parse(CurrLexem.getVal()));
                        else
                        {
                            if (TopTree == null) TopTree = new LexemTree(null);
                            TopTree.add(CurrLexem.getVal().Equals("true"));
                        }
                    }
                    else
                    {
                        if (TopTree == null) TopTree = new LexemTree(null);
                        TopTree.add(new MethodObj(ResourceFunctionObj.getFunctionMethod(CurrLexem.getVal()),
                                                  ResourceFunctionObj.extractParamStr(CurrLexem.getVal())));
                    }
                }
            }

            trace("-generateLexemTree");
            return TopTree;
        }



        public Boolean getResult(ResourceFunctionObj functionWrapper, RequestLogData log)
        {
            return getResult(getLexemTree(), functionWrapper, log);
        }

        public static Boolean getResult(LexemTree CurrTree, ResourceFunctionObj functionWrapper, RequestLogData log)
        {

            if (CurrTree.Value != null)
            {
                if (CurrTree.LeftVal == null)
                    CurrTree.LeftVal = getResult(CurrTree.LeftTree, functionWrapper, log);
                else if (CurrTree.LeftVal is LexemTree)
                    CurrTree.LeftVal = getResult(CurrTree.LeftVal as LexemTree, functionWrapper, log);
                else if (CurrTree.LeftVal is MethodObj)
                {
                    MethodObj MethodWrapper = (MethodObj)CurrTree.LeftVal;
                    CurrTree.LeftVal = functionWrapper.InvokeFunction(MethodWrapper.method, log,MethodWrapper.parameters);
                }
                if (CurrTree.RightVal == null)
                    CurrTree.RightVal = getResult(CurrTree.RightTree, functionWrapper, log);
                else if (CurrTree.RightVal is LexemTree)
                    CurrTree.RightVal = getResult(CurrTree.RightVal as LexemTree, functionWrapper, log);
                else if (CurrTree.RightVal is MethodObj)
                {
                    MethodObj MethodWrapper = (MethodObj)CurrTree.RightVal;
                    CurrTree.RightVal = functionWrapper.InvokeFunction(MethodWrapper.method, log, MethodWrapper.parameters);
                }

                switch ((LexemType)CurrTree.Value)
                {
                    case LexemType.AND: return (Boolean)CurrTree.LeftVal && (Boolean)CurrTree.RightVal;
                    case LexemType.OR:
                        return (Boolean)CurrTree.LeftVal || (Boolean)CurrTree.RightVal;
                    case LexemType.EE:
                        if (CurrTree.LeftVal == null)
                            return CurrTree.LeftVal == CurrTree.RightVal;
                        else
                            return CurrTree.LeftVal.Equals(CurrTree.RightVal);
                    case LexemType.NE: 
                        if (CurrTree.LeftVal == null)
                            return CurrTree.LeftVal != CurrTree.RightVal;
                        else
                            return !(CurrTree.LeftVal.Equals(CurrTree.RightVal));
                    case LexemType.GT: return (int)CurrTree.LeftVal > (int)CurrTree.RightVal;
                    case LexemType.GTE: return (int)CurrTree.LeftVal >= (int)CurrTree.RightVal;
                    case LexemType.LT: return (int)CurrTree.LeftVal < (int)CurrTree.RightVal;
                    case LexemType.LTE: return (int)CurrTree.LeftVal <= (int)CurrTree.RightVal;
                }
            }

            if(CurrTree.LeftVal is Boolean)
                return (Boolean)CurrTree.LeftVal;
            else if (CurrTree.LeftVal is MethodObj)
            {

                MethodObj MethodWrapper = (MethodObj)CurrTree.LeftVal;
                return (Boolean)functionWrapper.InvokeFunction(MethodWrapper.method, log, MethodWrapper.parameters);
            }
            else
                return getResult(CurrTree.LeftVal as LexemTree, functionWrapper, log);
        }

    }

    public class LexemTree
    {
        public Object Value = null;
        public LexemTree LeftTree = null;
        public Object LeftVal = null;
        public LexemTree RightTree = null;
        public Object RightVal = null;

        public LexemTree(Object value) { this.Value = value; }


        public LexemTree clone()
        {
            LexemTree cloneTree = new LexemTree(this.Value);
            cloneTree.LeftVal = this.LeftVal;
            cloneTree.RightVal = this.RightVal;

            if (LeftTree != null)
                cloneTree.LeftTree = LeftTree.clone();

            if (RightTree != null)
                cloneTree.RightTree = RightTree.clone();

            return cloneTree;
        }

        public Boolean add(Object value)
        {
            if (LeftTree != null)
            {
                if (LeftTree.add(value))
                    return true;

                if (RightTree == null)
                    RightVal = value;
                else
                    return RightTree.add(value);
            }
            else
            {
                if (LeftVal == null)
                {
                    LeftVal = value;
                    return true;
                }
                else if (RightVal == null)
                {
                    if (RightTree == null)
                    {
                        RightVal = value;
                        return true;
                    }
                    else
                        return RightTree.add(value);
                }
            }

            return false;
        }
    }



    public class ExpressionHelper : FiniteAutomata<LexemType>
    {

        private static ExpressionHelper m_clsInstance = null;
        private static readonly Object InstanceLock = new Object();
        public static ExpressionHelper Instance
        {
            get {
                lock (InstanceLock)
                {
                    if (m_clsInstance == null) m_clsInstance = new ExpressionHelper();
                    return m_clsInstance;
                }
            }
        }

        private static AutomataState<LexemType> q0 = new AutomataState<LexemType>();
        private static AutomataState<LexemType> q1 = new AutomataState<LexemType>();
        private static AutomataState<LexemType> q2 = new AutomataState<LexemType>();
        private static AutomataState<LexemType> q3 = new AutomataState<LexemType>();
        private static AutomataState<LexemType> q4 = new AutomataState<LexemType>();
        private static AutomataState<LexemType> q5 = new AutomataState<LexemType>();
        private static AutomataState<LexemType> q6 = new AutomataState<LexemType>();
        private static AutomataState<LexemType> q7 = new AutomataState<LexemType>();
        private static AutomataState<LexemType> q8 = new AutomataState<LexemType>();

        private ExpressionHelper() : base(q0, q1, q5, q8) { }

        protected override void ConstructTuple()
        {
            q0.addTransition(new ASTransition<LexemType>(q1, LexemType.BOOL));
            q0.addTransition(new ASTransition<LexemType>(q3, LexemType.INT));
            q0.addTransition(new ASTransition<LexemType>(q6, LexemType.STRING));


            q1.addTransition(new ASTransition<LexemType>(q0, LexemType.AND, LexemType.OR));
            q1.addTransition(new ASTransition<LexemType>(q2, LexemType.EE, LexemType.NE));

            q2.addTransition(new ASTransition<LexemType>(q1, LexemType.BOOL));


            q3.addTransition(new ASTransition<LexemType>(q4, LexemType.GT ,
                                                             LexemType.GTE,
                                                             LexemType.LT,
                                                             LexemType.LTE,
                                                             LexemType.EE, 
                                                             LexemType.NE));

            q4.addTransition(new ASTransition<LexemType>(q5, LexemType.INT));
            q5.addTransition(new ASTransition<LexemType>(q0, LexemType.AND, LexemType.OR));

            q6.addTransition(new ASTransition<LexemType>(q7, 
                                                 LexemType.EE,
                                                 LexemType.NE));

            q7.addTransition(new ASTransition<LexemType>(q8, LexemType.STRING));
            q8.addTransition(new ASTransition<LexemType>(q0, LexemType.AND, LexemType.OR));
        }

        public Boolean isValidExpression(String Expression,ref LexemGroup LexModel)
        {
            try
            {
                LexModel = new LexemGroup(Expression);
                
            }
            catch (Exception) { return false; }


            return TraverseLexemGroup(LexModel);
        }


        public Boolean TraverseLexemGroup(LexemGroup group)
        {
            foreach (Lexem lexem in group.m_lstLexem)
            {
                if (lexem is LexemGroup)
                {
                    if (!TraverseLexemGroup(lexem as LexemGroup))
                        return false;
                }
            }

            return isMappable(group.getLexemTypes());
        }


        public static Boolean TestExpression(LexemGroup LexModelExpression, RequestLogData log, ResourceFunctionObj FunctionWrapper)
         {

             return LexModelExpression.getResult(FunctionWrapper, log);
         }
    }
}
