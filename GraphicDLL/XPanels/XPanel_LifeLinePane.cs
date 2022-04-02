using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using GraphicDLL.common;
using GraphicDLL.XControls;

namespace GraphicDLL.XPanels
{
    class XPanel_LifeLinePane : XPanel
    {

        private XControl_LifeLine POSLifeLine   = null;
        private XControl_LifeLine POSMLifeLine  = null;
        private XControl_LifeLine FLMLifeLine   = null;
        private XControl_LifeLine FLLifeLine    = null;
        public  XControl_LifeLine MovingLifeline = null;

        public XPanel_LifeLinePane(iPageRequest request, XPanel_EventPane holder) : base(request, holder) { }

        protected override void XPanel_OnPreInitialize()
        {

        }

        protected override void XPanel_OnPostInitialize()
        {
            int ALLOWANCE = 0;
            int dev = ((Parent.Width > 500 ? Parent.Width : SystemInformation.VirtualScreen.Width) - ALLOWANCE) / 5;
            
            POSLifeLine = new XControl_LifeLine(m_IPRequest,"POS", ALLOWANCE + (dev), this);
            POSMLifeLine = new XControl_LifeLine(m_IPRequest, "POSM", ALLOWANCE + (dev * 2), this);
            FLMLifeLine = new XControl_LifeLine(m_IPRequest, "FLM", ALLOWANCE + (dev * 3), this);
            FLLifeLine = new XControl_LifeLine(m_IPRequest, "FL", ALLOWANCE + (dev * 4), this);
            MovingLifeline = new XControl_LifeLine(m_IPRequest, " ", 550, this);

            MovingLifeline.Visible = false;

            AnchorTo(AnchorStyle.RIGHT, AnchorStyle.RIGHT);
            this.BackColor = SharedResource.ALT_BACKGROUND_COLOR2;
            this.Height = 80;
        }


        public XControl_LifeLine getLifeline(String LLName)
        {
            switch (LLName)
            {
                case "POS": return POSLifeLine;
                case "POSM": return POSMLifeLine;
                case "FLM": return FLMLifeLine;
                case "FL": return FLLifeLine;
                case "movable": return MovingLifeline;
            }

            return null;
        }

        protected override void XPanel_OnPaint(object sender, PaintEventArgs e)
        {
            List<XControl_LifeLine> ListOfLineLine = new List<XControl_LifeLine>();
            XControl_LifeLine Top = null;

            foreach (Control obj in this.Controls)
            {
                if (obj is XControl_LifeLine)
                {
                    if(XControl_LifeLine.TopLifeLine != obj) // always add Top lifeline at last to Overlap 
                        ListOfLineLine.Add(obj as XControl_LifeLine);
                    else
                        Top = obj as XControl_LifeLine;
                }
            }

            if (Top != null) ListOfLineLine.Add(Top); // adding Top lifeline

            using (Pen p = new Pen(SharedResource.HEADER_BOT_COLOR))
            {
                p.DashStyle = DashStyle.Solid;
                e.Graphics.DrawRectangle(p, new Rectangle(-1, 0, this.Width + 1, this.Height - 1));
            }

            foreach (XControl_LifeLine lifeline in ListOfLineLine)
            {
                if ((lifeline == MovingLifeline && MovingLifeline.Visible) || lifeline != MovingLifeline)
                {
                    int StringHeight = (int)e.Graphics.MeasureString(lifeline.getName(), SharedResource.LIFELINE_HEAD_FONT).Height;
                    int StringWidth = (int)e.Graphics.MeasureString(lifeline.getName(), SharedResource.LIFELINE_HEAD_FONT).Width;
                    
                    using (Pen pen2 = new Pen(SharedResource.HEADER_BOT_COLOR))
                    using (Pen pen = new Pen(SharedResource.LIFELINE_HEAD_STROKE_COLOR))
                    {
                    
                        pen2.DashStyle = DashStyle.Dash; pen2.Color = SharedResource.LIFELINE_STROKE_COLOR;
                        
                        if (lifeline == MovingLifeline)
                        {
                            pen2.DashStyle = DashStyle.Solid;
                            pen2.Width = 5;
                            pen2.Color = Color.FromArgb(70, pen2.Color);
                        }

                        e.Graphics.DrawLine(pen2, lifeline.getPositionX(), SharedResource.LIFELINE_TOP_GAP + lifeline.Height,
                                                  lifeline.getPositionX(), this.Height - 1);

                        Rectangle border = new Rectangle(lifeline.Location.X, lifeline.Location.Y, lifeline.Width - 1, lifeline.Height - 1);
                        e.Graphics.FillRectangle(new SolidBrush(lifeline.CurrBackColor), border);
                        e.Graphics.DrawRectangle(pen, border);

                        SolidBrush Stringbrush = new SolidBrush(SharedResource.LIFELINE_HEAD_COLOR_SHADOW);
                        e.Graphics.DrawString(lifeline.getName(), SharedResource.LIFELINE_HEAD_FONT, Stringbrush,
                                              new Point(lifeline.Location.X + lifeline.Width / 2 - (StringWidth / 2) + 1, lifeline.Location.Y + lifeline.Height / 2 - (StringHeight / 2)));
                        Stringbrush.Color = SharedResource.LIFELINE_HEAD_STROKE_COLOR;
                        e.Graphics.DrawString(lifeline.getName(), SharedResource.LIFELINE_HEAD_FONT, Stringbrush,
                                                               new Point(lifeline.Location.X + lifeline.Width / 2 - (StringWidth / 2), lifeline.Location.Y + lifeline.Height / 2 - (StringHeight / 2)));

                    }
                }
            }

            m_ctrlContainer.Invalidate();
        }

    }
}
