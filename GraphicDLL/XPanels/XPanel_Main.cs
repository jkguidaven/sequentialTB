using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using GraphicDLL.common;

namespace GraphicDLL.XPanels
{
    class XPanel_Main : XPanel
    {



        private XPanel_FilterPane       m_filterPanel       = null;
        private XPanel_EventPane        m_EventPanel        = null;
        private XPanel_ActivityPane     m_ActivityPanel     = null;
       
        public XPanel_Main(iPageRequest request) : base(request,request.getTabPage()) 
        {
            m_EventPanel.loadLogDataToLifeLine(request);
        }

        public XPanel_FilterPane getFilterPanel() { return m_filterPanel; }
        public XPanel_EventPane getEventPanel() { return m_EventPanel; }
        public XPanel_ActivityPane getActivityPanel() { return m_ActivityPanel; }

        protected override void XPanel_OnPreInitialize()
        {
            m_filterPanel = new XPanel_FilterPane(m_IPRequest,this);
            m_EventPanel = new XPanel_EventPane(m_IPRequest,this);
            m_ActivityPanel = new XPanel_ActivityPane(m_IPRequest,this);
            AnchorTo(AnchorStyle.RIGHT, AnchorStyle.RIGHT);
            AnchorTo(AnchorStyle.BOTTOM, AnchorStyle.BOTTOM);
            margin(5);
        }

        protected override void XPanel_OnPostInitialize()
        {
        }


        protected override void XPanel_OnPaint(object sender, PaintEventArgs e)
        {
            using (Pen p = new Pen(SharedResource.BORDER_COLOR))
            {
                p.DashStyle = DashStyle.Solid;
                using (SolidBrush b = new SolidBrush(SharedResource.BACKGROUND_COLOR)) e.Graphics.FillRectangle(b, 0, 0, Width - 1, Height - 1);
                e.Graphics.DrawRectangle(p, new Rectangle(0, 0, Width - 1, Height - 1));
            }
        }
    }
}
