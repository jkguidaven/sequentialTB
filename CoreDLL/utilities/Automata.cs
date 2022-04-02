using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreDLL.utilities
{

    public struct ASTransition<T>
    {
        private AutomataState<T> m_clsAState;
        private List<T>          m_lstInput;

        public ASTransition(AutomataState<T> clsAState, T Input, params T[] moreInput)
        {
            m_clsAState = clsAState;
            m_lstInput  = new List<T>(moreInput);
            m_lstInput.Add(Input);
        }

        public Boolean isMappable(T Input)
        {
            foreach (T possibleInput in m_lstInput)
            {
                if (Input.Equals(possibleInput))
                    return true;
            }

            return false;
        }

        public AutomataState<T> getState() { 
            return m_clsAState; 
        }
    }

    public class AutomataState<T>
    {
        private List<ASTransition<T>> m_lstMapDirection;

        public AutomataState()
        {
            m_lstMapDirection = new List<ASTransition<T>>();
        }

        public void addTransition(ASTransition<T> structASTransition)
        {
            m_lstMapDirection.Add(structASTransition);
        }

        public List<ASTransition<T>> getTransitionPath() { return m_lstMapDirection; }
    }

    public abstract class FiniteAutomata<T>
    {
        private AutomataState<T>         m_clsASEntry;
        private List<AutomataState<T>>   m_lstASExit;

        protected FiniteAutomata(AutomataState<T> clsASEntry,
                                 AutomataState<T> clsASExit, 
                                 params AutomataState<T>[] MoreASExit)
        {
            m_clsASEntry = clsASEntry;

            m_lstASExit = new List<AutomataState<T>>(MoreASExit);
            m_lstASExit.Add(clsASExit);

            ConstructTuple();
        }

        protected abstract void ConstructTuple();

        protected Boolean isMappable(Queue<T> DataQueue )
        {
            return isMappable(m_clsASEntry, ref DataQueue);
        }

        private Boolean isMappable(AutomataState<T> clsASState, ref Queue<T> DataQueue)
        {
            if (DataQueue.Count > 0)
            {
                T objData = DataQueue.Dequeue();

                foreach (ASTransition<T> transitionPath in clsASState.getTransitionPath())
                {
                    if (transitionPath.isMappable(objData))
                    {
                            return DataQueue.Count > 0 ?
                              isMappable(transitionPath.getState(), ref DataQueue) :
                              m_lstASExit.Contains(transitionPath.getState());
                    }
                }

            }
            return false;
        }

    }
}
