using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphicDLL.common;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace GraphicDLL.XControls
{
    class XControl_LifeLineBody : XControl
    {

        private List<clsObjLLEvent> LLEventList = null;
        private clsObjLLEvent Highlighted_LLEvent = null;
        private clsObjLLEvent Focus_LLEvent = null;
        private ContextMenu cm=null;

        private int m_nCurrPosY = SharedResource.EVENT_START_YPOINT;

        public XControl_LifeLineBody(iPageRequest request,XControl_TimestampSlice holder)
            : base(request,holder)
        {
        }

        protected override void XControl_OnInitialize(params object[] data)
        {
            cm = new ContextMenu();
            MenuItem menuItem1 = new MenuItem("Goto log line");
            MenuItem menuItem2 = new MenuItem("Goto xml line");
            menuItem1.Click += new EventHandler(XControl_OnMouseGotoLogLine);
            cm.MenuItems.Add(menuItem1);
            cm.MenuItems.Add(menuItem2);
            LLEventList = new List<clsObjLLEvent>();
            this.BackColor = Color.Transparent;
            this.MouseMove += new MouseEventHandler(XControl_OnMouseMove);
            this.MouseLeave += new EventHandler(XControl_OnMouseLeave);
            this.MouseClick += new MouseEventHandler(XControl_OnMouseClick);
            XControl_OnResize(null, null);
        }


        protected void XControl_OnMouseGotoLogLine(object sender, EventArgs e)
        {
            STBHandlerDelegate handler = m_IPRequest.getSTBEventHandler();
            handler.Invoke(m_IPRequest, 10);
        }

        protected void XControl_OnMouseClick(object sender, MouseEventArgs e)
        {

            if (Focus_LLEvent != Highlighted_LLEvent)
            {
                Focus_LLEvent = Highlighted_LLEvent;
                this.Refresh();
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Right && Focus_LLEvent  != null && Focus_LLEvent.getLLEventType() != LLEventType.END)
            {
                int mousePosY = (e.Y < 0 ? (32768 + e.Y) + 32768 : e.Y);
                cm.Show(this, new Point(e.X, mousePosY));
            }
        }

        protected void XControl_OnMouseLeave(object sender, EventArgs e)
        {
            Highlighted_LLEvent = null;
            this.Refresh();
        }

        protected override void XControl_OnResize(object sender, EventArgs e)
        {
            this.Width = SharedResource.EVENT_OBJ_WIDTH;
            this.Height = m_ctrlContainer.Height;
        }

        protected void XControl_OnMouseMove(object sender, MouseEventArgs e)
        {
            int mousePosY = (e.Y < 0 ? (32768 + e.Y) + 32768 : e.Y);

            foreach (clsObjLLEvent LLEvent in LLEventList)
            {
                if (mousePosY >= LLEvent.getPosY() && mousePosY <= LLEvent.getPosY() + SharedResource.EVENT_OBJ_HEIGHT)
                {
                    if (Highlighted_LLEvent != LLEvent)
                    {
                        Highlighted_LLEvent = LLEvent;
                        this.Refresh();

                        (Parent as XControl_TimestampSlice).tooltip.SetToolTip(this, Highlighted_LLEvent.getContext());
                    }
                    break;

                }
            }
        }

        protected override void XControl_OnPaint(object sender, PaintEventArgs e)
        {
                Queue<clsObjLLEvent> QueueLLEvent = new Queue<clsObjLLEvent>(LLEventList.ToArray());
                int prev_y = SharedResource.EVENT_START_YPOINT;

                using (Pen p = new Pen(Color.Black), p2 = new Pen(Color.Black), p3 = new Pen(Color.Black))
                {
                    p3.DashStyle = DashStyle.Dash;
                    //p2.Width = 3;
                    Boolean IsNotEnd = false;
                    foreach (XControl_TimestampSlice.TimeSlice slice in (m_ctrlContainer as XControl_TimestampSlice).lstTimeSlice)
                    {
                        int index = 0;
                        if (QueueLLEvent.Count > 0)
                        {
                            if (QueueLLEvent.Peek().getLongTimeStamp() == slice.m_lTStamp)
                            {
                                clsObjLLEvent[] LLEvents = popEventsByTimeStamp(ref QueueLLEvent);
                                foreach (clsObjLLEvent LLEvent in LLEvents)
                                {
                                    if (LLEvent.getLLEventType() != LLEventType.END)
                                    {
                                        IsNotEnd = true;
                                        if (LLEvent.getLLEventType() != LLEventType.LINK)
                                        {
                                            if (LLEvent.getLinkEvent() == null)
                                            {
                                                if (LLEvent == Focus_LLEvent)
                                                    e.Graphics.FillRectangle(new SolidBrush(SharedResource.LIFELINE_HEAD_COLOR_ALT2),
                                                                            0, prev_y + ((index - 1) * SharedResource.EVENT_OBJ_HEIGHT),
                                                                            this.Width - 1, SharedResource.EVENT_OBJ_HEIGHT);
                                                else
                                                    e.Graphics.FillRectangle(new SolidBrush(Highlighted_LLEvent == LLEvent ? SharedResource.LIFELINE_HEAD_COLOR_ALT : SharedResource.LIFELINE_HEAD_COLOR),
                                                                            0, prev_y + ((index - 1) * SharedResource.EVENT_OBJ_HEIGHT),
                                                                            this.Width - 1, SharedResource.EVENT_OBJ_HEIGHT);
                                                
                                                e.Graphics.DrawRectangle(LLEvent != Focus_LLEvent ? p3 : p2, 0, prev_y + ((index - 1) * SharedResource.EVENT_OBJ_HEIGHT),
                                                                           this.Width - 1, SharedResource.EVENT_OBJ_HEIGHT);

                                                if (index == 0 || (index > 0 && LLEvents[index - 1].getLLEventType() == LLEventType.END))
                                                    e.Graphics.DrawLine(p, 0, prev_y + ((index-1) * SharedResource.EVENT_OBJ_HEIGHT),
                                                                        this.Width - 1, prev_y + ((index-1) * SharedResource.EVENT_OBJ_HEIGHT));

                                                //e.Graphics.DrawLine(useDash ? p3 : p, 0, prev_y + ((index - 1) * SharedResource.EVENT_OBJ_HEIGHT),
                                                //                    this.Width - 1, prev_y + ((index - 1) * SharedResource.EVENT_OBJ_HEIGHT));


                                                e.Graphics.DrawLine(p, 0, prev_y + ((index - 1) * SharedResource.EVENT_OBJ_HEIGHT),
                                                                       0, prev_y + ((index) * SharedResource.EVENT_OBJ_HEIGHT));
                                                e.Graphics.DrawLine(p, this.Width-1, prev_y + ((index - 1) * SharedResource.EVENT_OBJ_HEIGHT),
                                                                       this.Width-1, prev_y + ((index) * SharedResource.EVENT_OBJ_HEIGHT));
                                            }
                                            else
                                            {
                                                if (LLEvent == Focus_LLEvent)
                                                    e.Graphics.FillRectangle(new SolidBrush(SharedResource.LIFELINE_HEAD_COLOR_ALT2),
                                                                             0, LLEvent.getPosY(),
                                                                             this.Width - 1, SharedResource.EVENT_OBJ_HEIGHT);
                                                else
                                                    e.Graphics.FillRectangle(new SolidBrush(Highlighted_LLEvent == LLEvent ? SharedResource.LIFELINE_HEAD_COLOR_ALT : SharedResource.LIFELINE_HEAD_COLOR),
                                                                             0, LLEvent.getPosY(),
                                                                             this.Width - 1, SharedResource.EVENT_OBJ_HEIGHT);

                                                e.Graphics.DrawRectangle(LLEvent != Focus_LLEvent ? p : p2, 0, LLEvent.getPosY(),
                                                                        this.Width - 1, SharedResource.EVENT_OBJ_HEIGHT);
                                            }
                                        }
                                        else
                                        {
                                            clsObjLLEventLink LinkEvent = LLEvent as clsObjLLEventLink;

                                            if (LinkEvent != null)
                                            {
                                                e.Graphics.FillRectangle(new SolidBrush(Highlighted_LLEvent == LLEvent ? SharedResource.LIFELINE_HEAD_COLOR_ALT : SharedResource.LIFELINE_HEAD_COLOR),
                                                    0, (LinkEvent.getLinkEvent() == null ? prev_y + ((index - 1) * SharedResource.EVENT_OBJ_HEIGHT) : LinkEvent.getPosY()),
                                                                        this.Width - 1, SharedResource.EVENT_OBJ_HEIGHT);
                                                e.Graphics.DrawRectangle(p, 0, (LinkEvent.getLinkEvent() == null ? prev_y + ((index - 1) * SharedResource.EVENT_OBJ_HEIGHT) : LinkEvent.getPosY()),
                                                                        this.Width - 1, SharedResource.EVENT_OBJ_HEIGHT);
                                            }
                                        }
                                    }
                                    else if (IsNotEnd)
                                    {
                                        e.Graphics.DrawLine(p, 0, prev_y + ((index-1) * SharedResource.EVENT_OBJ_HEIGHT),
                                                                       this.Width - 1, prev_y + ((index-1) * SharedResource.EVENT_OBJ_HEIGHT));

                                        IsNotEnd = false;
                                    }


                                    //LLEvent.setPosY(prev_y + ((index - 1) * SharedResource.EVENT_OBJ_HEIGHT));
                                    index++;
                                }
                            }
                            prev_y = slice.m_posY;
                        }
                        else
                            break;
                    }
                }
        }


        public void Add(clsObjLLEvent LLEvent)
        {
            LLEvent.setPosY(m_nCurrPosY - SharedResource.EVENT_OBJ_HEIGHT);
            LLEventList.Add(LLEvent);
            m_nCurrPosY += SharedResource.EVENT_OBJ_HEIGHT;
        }

        public void setCurrentPosY(int nCurrPosY)
        {
            m_nCurrPosY = nCurrPosY;
        }

        private static clsObjLLEvent[] popEventsByTimeStamp(ref Queue<clsObjLLEvent> Queue)
        {
            List<clsObjLLEvent> lstEvent = new List<clsObjLLEvent>();

            long CurrlTimestamp = Queue.Peek().getLongTimeStamp();

            while (Queue.Count > 0)
            {
                if (Queue.Peek().getLongTimeStamp() == CurrlTimestamp)
                    lstEvent.Add(Queue.Dequeue());
                else
                    break;
            }
            return lstEvent.ToArray();
        }

        public clsObjLLEvent[] getLLEvents()
        {
            return LLEventList.ToArray();
        }
    }
}
