using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphicDLL.common
{


    public enum LLType { POSM, POS, FLM, FL, UNIDENTIFIED }


    public enum LLEventType { NORMAL, ERROR, LINK , END  }


    public class clsObjLLEvent
    {
        private LLEventType m_LLEtype;
        private String      m_strContextValue;
        private long        m_lTimestamp;
        private int m_nPosX;
        private int m_nPosY;
        private String m_strIdentifier = null;

        protected clsObjLLEvent m_clsOLLinkEvent;
        protected Boolean m_bFollowLinkAlignment;


        public clsObjLLEvent(long lTimestamp,LLEventType LLEtype = LLEventType.NORMAL)
        {
            m_LLEtype = LLEtype;
            m_strContextValue = null;
            m_lTimestamp = lTimestamp;
        }

        public void setContext(string strContext)
        {
            m_strContextValue = strContext;
        }



        public void setLinkEvent(clsObjLLEvent clsOLLinkEvent)
        {
            m_clsOLLinkEvent = clsOLLinkEvent;
        }

        public void setIdentifier(string strIdentifier)
        {
            m_strIdentifier = strIdentifier;
        }

        public String getContext() { return m_strContextValue; }
        public String getIdentifier() { return m_strIdentifier; }
        public long getLongTimeStamp() { return m_lTimestamp; }

        public LLEventType getLLEventType() { return m_LLEtype; }


        public void setPosY(int new_y) { m_nPosY = new_y; }



        public void setToFollow(Boolean bFollowLinkAlignment)
        {
            m_bFollowLinkAlignment = bFollowLinkAlignment;
        }

        public Boolean AlignToLink() { return m_bFollowLinkAlignment; }


        public override String ToString()
        {
            if (m_clsOLLinkEvent == null)
                return getLLEventType().ToString();
            else
                return getLLEventType().ToString() + " - " + m_clsOLLinkEvent.getLLEventType().ToString();
        }


        public int getPosY()
        {
            if (m_bFollowLinkAlignment)
                return m_clsOLLinkEvent != null ? m_clsOLLinkEvent.getPosY() : m_nPosY;
            else
                return m_nPosY;
        }

        public clsObjLLEvent getLinkEvent() { return m_clsOLLinkEvent; }
    }


    public class clsObjLLEventLink : clsObjLLEvent
    {
        private String m_strLinkText;
        public clsObjLLEventLink(long lTimestamp, clsObjLLEvent clsOLLinkEvent) : this(lTimestamp, clsOLLinkEvent, false) { }
        public clsObjLLEventLink(long lTimestamp, clsObjLLEvent clsOLLinkEvent, Boolean bFollowLinkAlignment)
            : base(lTimestamp, LLEventType.LINK)
        {
            m_strLinkText = "";
            m_clsOLLinkEvent = clsOLLinkEvent;
            m_bFollowLinkAlignment = bFollowLinkAlignment;
        }

        public void setLinkText(String strLinkText)
        {
            m_strLinkText = strLinkText;
        }

        public String getLinkText() { return m_strLinkText; }






    }
}
