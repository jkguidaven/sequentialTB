using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphicDLL.common;
using System.Windows.Forms;
using GraphicDLL.XPanels;
using System.Drawing.Drawing2D;
using System.Drawing;
using CoreDLL.utilities;

namespace GraphicDLL.XControls
{
    class XControl_TimestampSlice : XControl
    {


        public class TimeSlice
        {
            public int m_posY;
            public long m_lTStamp;


            public TimeSlice(int posY, long lTStamp)
            {
                m_posY = posY;
                m_lTStamp = lTStamp;
            }
        }


        public List<TimeSlice> lstTimeSlice = null;
        public XControl_LifeLineBody POSMLLEventBody   = null;
        public XControl_LifeLineBody POSLLEventBody = null;
        public XControl_LifeLineBody FLMLLEventBody = null;
        public XControl_LifeLineBody FLLLEventBody = null;

        public ToolTip tooltip = new ToolTip();

        public XControl_TimestampSlice(iPageRequest request, Control holder)
            : base(request,holder)
        {
        }

        protected override void XControl_OnInitialize(params object[] data)
        {
            lstTimeSlice      = new List<TimeSlice>();
            POSMLLEventBody   = new XControl_LifeLineBody(m_IPRequest,this);
            POSLLEventBody    = new XControl_LifeLineBody(m_IPRequest,this);
            FLMLLEventBody    = new XControl_LifeLineBody(m_IPRequest,this);
            FLLLEventBody     = new XControl_LifeLineBody(m_IPRequest,this);



            this.Controls.Add(POSLLEventBody);
            this.Controls.Add(POSMLLEventBody);
            this.Controls.Add(FLMLLEventBody);
            this.Controls.Add(FLLLEventBody);

            this.BackColor = Color.Transparent;
        }

        protected override void XControl_OnResize(object sender, EventArgs e)
        {
            Width = Parent.Width;
        }


        protected override void XControl_OnPaint(object sender, PaintEventArgs e)
        {
            using (Pen p = new Pen(SharedResource.BORDER_COLOR), 
                       p2 = new Pen(Color.White),
                       p3 = new Pen(SharedResource.TIMESTAMP_SLICE_LINE_COLOR),
                       p4 = new Pen(SharedResource.TIMESTAMP_SLICE_LINE_COLOR2))
            using (Brush b = new SolidBrush(SharedResource.ALT_BACKGROUND_COLOR2))
            {

                //e.Graphics.FillRectangle(b, 0, 0, SharedResource.EVENT_GRID_WIDTH, this.Height);

                e.Graphics.DrawLine(/*p*/p3, SharedResource.EVENT_GRID_WIDTH, 0,
                                       SharedResource.EVENT_GRID_WIDTH, this.Height);

                //e.Graphics.DrawLine(p2, 0, 0, 0, this.Height);
                //e.Graphics.DrawLine(p3, 0, 0, SharedResource.EVENT_GRID_WIDTH - 1, 0);


                int prev_y = SharedResource.EVENT_START_YPOINT;
                e.Graphics.DrawLine(/*p4*/p3, 0, prev_y, SharedResource.EVENT_GRID_WIDTH, prev_y);
                e.Graphics.DrawLine(p2, 0, prev_y + 1, SharedResource.EVENT_GRID_WIDTH - 1, prev_y + 1);
                e.Graphics.DrawLine(p3, SharedResource.EVENT_GRID_WIDTH + 1, prev_y, Width, prev_y);

                foreach (TimeSlice slice in lstTimeSlice)
                {

                    e.Graphics.DrawLine(p3, /* SharedResource.EVENT_GRID_WIDTH + 1 */ 0, slice.m_posY, Width, slice.m_posY);
                    //e.Graphics.DrawLine(p4, 0, slice.m_posY, SharedResource.EVENT_GRID_WIDTH, slice.m_posY);
                    e.Graphics.DrawLine(p2, 0, slice.m_posY+1, SharedResource.EVENT_GRID_WIDTH - 1, slice.m_posY+1);


                    e.Graphics.DrawString(StringHelper.LToDS(slice.m_lTStamp),
                                          SharedResource.EVENT_TEXT_FONT,
                                          new SolidBrush(SharedResource.TIMESTAMP_SLICE_LINE_COLOR2), 
                                          new Point(SharedResource.EVENT_GRID_WIDTH + 5, prev_y + 5));

                    prev_y = slice.m_posY;
                }


                PaintLinkLines(e.Graphics, POSLLEventBody);
                PaintLinkLines(e.Graphics, POSMLLEventBody);
                PaintLinkLines(e.Graphics, FLMLLEventBody);
                PaintLinkLines(e.Graphics, FLLLEventBody);
            }



        }

        private void PaintLinkLines(Graphics g,XControl_LifeLineBody Lifeline)
        {
            using (Pen p = new Pen(Color.Black))
            {
                foreach (clsObjLLEvent LLEvent in Lifeline.getLLEvents())
                {
                    if (LLEvent is clsObjLLEventLink)
                    {

                        if (LLEvent.getLinkEvent() != null)
                        {
                            int frmX = Lifeline.Location.X;
                            int desX = getLifeLineBody(LLEvent.getLinkEvent()).Location.X;
                            int pointY = LLEvent.getPosY() + (int)(SharedResource.EVENT_OBJ_HEIGHT * 1.5);
                            int pointX = 0;
                            g.DrawLine(p, frmX + (SharedResource.EVENT_OBJ_WIDTH / 2),
                                                   pointY,
                                                   desX + (SharedResource.EVENT_OBJ_WIDTH / 2),
                                                   pointY);

                            Boolean bGoLeft = (frmX > desX);


                            using (Brush b = new SolidBrush(Color.Black))
                            {
                                if (bGoLeft)
                                {
                                    pointX = desX + SharedResource.EVENT_OBJ_WIDTH;

                                    Point[] polygonPoints = new Point[]{
                                                 new Point(pointX, pointY),
                                                 new Point(pointX + 5, pointY + 5),
                                                 new Point(pointX + 5, pointY - 5) 
                                };

                                    g.FillPolygon(b, polygonPoints);

                                }
                                else
                                {
                                    pointX = desX;

                                    Point[] polygonPoints = new Point[]{
                                                 new Point(pointX, pointY),
                                                 new Point(pointX - 5, pointY + 5),
                                                 new Point(pointX - 5, pointY - 5) 
                                };

                                    g.FillPolygon(b, polygonPoints);
                                }
                            }
                           
                        }
                        else
                        {
                        }
                    }
                }
            }
        }

        private XControl_LifeLineBody getLifeLineBody(clsObjLLEvent LLEvent)
        {
            if (POSLLEventBody.getLLEvents().Contains(LLEvent))
                return POSLLEventBody;
            else if (POSMLLEventBody.getLLEvents().Contains(LLEvent))
                return POSMLLEventBody;
            else if (FLMLLEventBody.getLLEvents().Contains(LLEvent))
                return FLMLLEventBody;
            else 
                return FLLLEventBody;
        }


        public clsObjLLEvent[] popsLLEventsByTimeLine(long lTimestamp, Queue<clsObjLLEvent> QueueLLEvent)
        {
            List<clsObjLLEvent> tempDatalst = new List<clsObjLLEvent>();

            while (QueueLLEvent.Count > 0)
            {
                if (QueueLLEvent.Peek().getLongTimeStamp() == lTimestamp)
                    tempDatalst.Add(QueueLLEvent.Dequeue());
                else if (QueueLLEvent.Peek().getLongTimeStamp() > lTimestamp)
                    break;
            }

            return tempDatalst.ToArray();
        }


        public void addTimeSlice(int y, long longTStamp)
        {
            lstTimeSlice.Add(new TimeSlice(y, longTStamp));
            POSMLLEventBody.setCurrentPosY(y);
            POSLLEventBody.setCurrentPosY(y);
            FLMLLEventBody.setCurrentPosY(y);
            FLLLEventBody.setCurrentPosY(y);

            Height = y + SharedResource.ALLOWANCE_TIMESTAMP_BOT;
        }

        public void addEventToPOSM(params clsObjLLEvent[] arrLLEvent)
        {
            foreach (clsObjLLEvent LLEvent in arrLLEvent)
                POSMLLEventBody.Add(LLEvent);
        }

        public void addEventToPOS(params clsObjLLEvent[] arrLLEvent)
        {
            foreach (clsObjLLEvent LLEvent in arrLLEvent)
                POSLLEventBody.Add(LLEvent);
        }

        public void addEventToFLM(params clsObjLLEvent[] arrLLEvent)
        {
            foreach (clsObjLLEvent LLEvent in arrLLEvent)
                FLMLLEventBody.Add(LLEvent);
        }

        public void addEventToFL(params clsObjLLEvent[] arrLLEvent)
        {
            foreach (clsObjLLEvent LLEvent in arrLLEvent)
                FLLLEventBody.Add(LLEvent);
        }
    }
}
