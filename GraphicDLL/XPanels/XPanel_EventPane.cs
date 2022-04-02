using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GraphicDLL.common;
using System.Drawing.Drawing2D;
using GraphicDLL.XControls;

namespace GraphicDLL.XPanels
{
    class XPanel_EventPane : XPanel
    {

        public XPanel_EventPane(iPageRequest request,XPanel_Main holder) : base(request,holder) { }

        private XPanel_LifeLinePane  LifeLinePanel  = null;
        private XPanel_TimestampPane timeStampPanel = null;

        protected override void XPanel_OnPreInitialize()
        {
            LifeLinePanel  = new XPanel_LifeLinePane(m_IPRequest,this);
            timeStampPanel = new XPanel_TimestampPane(m_IPRequest,this);



            margin(0, 1, 1, 0);
            this.BackColor = SharedResource.BACKGROUND_COLOR;
        }

        protected override void XPanel_OnPostInitialize()
        {
            XPanel_Main holder = (XPanel_Main)m_ctrlContainer;
            AnchorTo(AnchorStyle.LEFT, holder.getFilterPanel(), AnchorStyle.RIGHT);
            AnchorTo(AnchorStyle.BOTTOM, holder.getActivityPanel(), AnchorStyle.TOP);
            AnchorTo(AnchorStyle.RIGHT, AnchorStyle.RIGHT);
        }

        protected override void XPanel_OnPaint(object sender, PaintEventArgs e)
        {
            using (Pen p = new Pen(SharedResource.LIFELINE_STROKE_COLOR))
            {
                p.DashStyle = DashStyle.Dash;
                e.Graphics.DrawLine(p,  LifeLinePanel.getLifeline("POS").getPositionX(),    0,
                                        LifeLinePanel.getLifeline("POS").getPositionX(),    this.Height);
                e.Graphics.DrawLine(p,  LifeLinePanel.getLifeline("POSM").getPositionX(),   0,
                                        LifeLinePanel.getLifeline("POSM").getPositionX(),   this.Height);
                e.Graphics.DrawLine(p,  LifeLinePanel.getLifeline("FLM").getPositionX(),    0,
                                        LifeLinePanel.getLifeline("FLM").getPositionX(),    this.Height);
                e.Graphics.DrawLine(p,  LifeLinePanel.getLifeline("FL").getPositionX(),     0,
                                        LifeLinePanel.getLifeline("FL").getPositionX(),     this.Height);

                if (LifeLinePanel.getLifeline("movable").Visible == true)
                {
                    p.DashStyle = DashStyle.Solid;
                    p.Width = 5;
                    p.Color = Color.FromArgb(80, p.Color);
                    e.Graphics.DrawLine(p, LifeLinePanel.getLifeline("movable").getPositionX(), 0,
                            LifeLinePanel.getLifeline("movable").getPositionX(), this.Height);

                }

                timeStampPanel.XControl_TSSlice.POSMLLEventBody.Location = new Point(LifeLinePanel.getLifeline("POSM").getPositionX() - (timeStampPanel.XControl_TSSlice.POSMLLEventBody.Width / 2),
                                                                                      SharedResource.EVENT_START_YPOINT);

                timeStampPanel.XControl_TSSlice.POSLLEventBody.Location = new Point(LifeLinePanel.getLifeline("POS").getPositionX() - (timeStampPanel.XControl_TSSlice.POSLLEventBody.Width / 2),
                                                                      SharedResource.EVENT_START_YPOINT);

                timeStampPanel.XControl_TSSlice.FLMLLEventBody.Location = new Point(LifeLinePanel.getLifeline("FLM").getPositionX() - (timeStampPanel.XControl_TSSlice.FLMLLEventBody.Width / 2),
                                                                      SharedResource.EVENT_START_YPOINT);

                timeStampPanel.XControl_TSSlice.FLLLEventBody.Location = new Point(LifeLinePanel.getLifeline("FL").getPositionX() - (timeStampPanel.XControl_TSSlice.FLLLEventBody.Width / 2),
                                                                      SharedResource.EVENT_START_YPOINT);

            }
        }


        public void refreshTimeStamp()
        {
            timeStampPanel.XControl_TSSlice.Refresh();
        }

        public XPanel_LifeLinePane getLifeLinePanel() { return LifeLinePanel; }
        public XPanel_TimestampPane getTimestampPanel() { return timeStampPanel; }


        public void loadLogDataToLifeLine(iPageRequest request)
        {

            int point_y = SharedResource.EVENT_START_YPOINT;
            while (request.getPOSLLEventList().Count  > 0 ||
                   request.getPOSMLLEventList().Count > 0 ||
                   request.getFLMLLEventList().Count  > 0 ||
                   request.getFLLLEventList().Count   > 0)
            {
                clsObjLLEvent[] cur_arrPOSLLEvent = new clsObjLLEvent[0];
                clsObjLLEvent[] cur_arrPOSMLLEvent = new clsObjLLEvent[0];
                clsObjLLEvent[] cur_arrFLMLLEvent = new clsObjLLEvent[0];
                clsObjLLEvent[] cur_arrFLLLEvent = new clsObjLLEvent[0];

                long LowestTimeStamp    = getLowestTimeStamp(request.getPOSLLEventList(),
                                                             request.getPOSMLLEventList(),
                                                             request.getFLMLLEventList(),
                                                             request.getFLLLEventList());

                if (request.getPOSLLEventList().Count > 0 &&
                    LowestTimeStamp == request.getPOSLLEventList().Peek().getLongTimeStamp())
                        cur_arrPOSLLEvent = popEventsByTimeStamp(request.getPOSLLEventList());


                if (request.getPOSMLLEventList().Count > 0 &&
                    LowestTimeStamp == request.getPOSMLLEventList().Peek().getLongTimeStamp())
                       cur_arrPOSMLLEvent = popEventsByTimeStamp(request.getPOSMLLEventList());


                if (request.getFLMLLEventList().Count > 0 &&
                    LowestTimeStamp == request.getFLMLLEventList().Peek().getLongTimeStamp())
                        cur_arrFLMLLEvent = popEventsByTimeStamp(request.getFLMLLEventList());

                if (request.getFLLLEventList().Count > 0 &&
                    LowestTimeStamp == request.getFLLLEventList().Peek().getLongTimeStamp())
                        cur_arrFLLLEvent = popEventsByTimeStamp(request.getFLLLEventList());



                List<clsObjLLEvent> POSMListEvent = new List<clsObjLLEvent>(cur_arrPOSMLLEvent);
                List<clsObjLLEvent> FLMListEvent = new List<clsObjLLEvent>(cur_arrFLMLLEvent);


                AlignLinks(ref POSMListEvent, ref FLMListEvent,false);
                AlignLinks(ref FLMListEvent, ref POSMListEvent,true);


                cur_arrPOSMLLEvent = POSMListEvent.ToArray();
                cur_arrFLMLLEvent  = FLMListEvent.ToArray();

                int HighestCount = getHighestCount(cur_arrPOSLLEvent.Length,
                                    cur_arrPOSMLLEvent.Length,
                                    cur_arrFLMLLEvent.Length,
                                    cur_arrFLLLEvent.Length);

                AdjustEndEvents(ref FLMListEvent,POSMListEvent, HighestCount);
                AdjustEndEvents(ref POSMListEvent,FLMListEvent, HighestCount);
                
                // recount Highest after adjustment

                cur_arrPOSMLLEvent = POSMListEvent.ToArray();
                cur_arrFLMLLEvent = FLMListEvent.ToArray();
                HighestCount = getHighestCount(cur_arrPOSLLEvent.Length,
                                    cur_arrPOSMLLEvent.Length,
                                    cur_arrFLMLLEvent.Length,
                                    cur_arrFLLLEvent.Length);

                point_y += HighestCount * SharedResource.EVENT_OBJ_HEIGHT;

                timeStampPanel.XControl_TSSlice.addEventToPOSM(cur_arrPOSMLLEvent);
                timeStampPanel.XControl_TSSlice.addEventToFLM(cur_arrFLMLLEvent);
                timeStampPanel.XControl_TSSlice.addEventToFL(cur_arrFLLLEvent);
                timeStampPanel.XControl_TSSlice.addEventToPOS(cur_arrPOSLLEvent);
                timeStampPanel.XControl_TSSlice.addTimeSlice(point_y, LowestTimeStamp);
            }
        }

        private static void AdjustEndEvents(ref List<clsObjLLEvent> ListEvent,List<clsObjLLEvent> ExceptionListEvent, int HighestCount)
        {
            if (ListEvent.Count > 0 && ListEvent.Count < HighestCount && ListEvent[ListEvent.Count-1].getLLEventType() != LLEventType.END)
            {
                int InsertIndex = ListEvent.Count-1;
                while (ListEvent[InsertIndex].getLLEventType() != LLEventType.END && InsertIndex > 0)
                {
                    if (ListEvent[InsertIndex].getLLEventType() == LLEventType.LINK || (ListEvent[InsertIndex].getLinkEvent() != null))
                    {
                        if(ExceptionListEvent.Contains(ListEvent[InsertIndex].getLinkEvent()))
                            break;
                    }
                    
                    InsertIndex--;
                }

                long curr_lTimestamp = ListEvent[InsertIndex].getLongTimeStamp();
                LLEventType prevType = ListEvent[InsertIndex].getLLEventType();
                while (ListEvent.Count != HighestCount)
                {
                    if(prevType == LLEventType.END)
                    ListEvent.Insert(InsertIndex+1,
                        new clsObjLLEvent(curr_lTimestamp, LLEventType.END));
                    else
                        ListEvent.Insert(InsertIndex + 1,
                            new clsObjLLEvent(curr_lTimestamp, LLEventType.NORMAL));

                }
            }
        }

        private static void AlignLinks(ref List<clsObjLLEvent> LeftListEvent,ref List<clsObjLLEvent> RightListEvent,Boolean AdjustRight)
        {
                for (int i=0;i<RightListEvent.Count;i++)
                {
                    clsObjLLEvent RightEvent = RightListEvent[i];
                    
                    if (RightEvent is clsObjLLEventLink && RightEvent.getLinkEvent() != null && LeftListEvent.Contains(RightEvent.getLinkEvent()))
                    {
                        while (RightListEvent.IndexOf(RightEvent) != LeftListEvent.IndexOf(RightEvent.getLinkEvent()))
                        {
                            if (RightListEvent.IndexOf(RightEvent) > LeftListEvent.IndexOf(RightEvent.getLinkEvent()))
                            {
                                LLEventType prevType = LLEventType.END;

                                if(LeftListEvent.IndexOf(RightEvent.getLinkEvent())>0)
                                    prevType = LeftListEvent[LeftListEvent.IndexOf(RightEvent.getLinkEvent()) - 1].getLLEventType();

                                LeftListEvent.Insert(LeftListEvent.IndexOf(RightEvent.getLinkEvent()),
                                                   new clsObjLLEvent(RightEvent.getLongTimeStamp(), prevType));
                            }
                            else
                            {
                                LLEventType prevType = LLEventType.END;

                                if (RightListEvent.IndexOf(RightEvent) > 0)
                                    prevType = RightListEvent[RightListEvent.IndexOf(RightEvent) - 1].getLLEventType();

                                RightListEvent.Insert(RightListEvent.IndexOf(RightEvent),
                                new clsObjLLEvent(RightEvent.getLongTimeStamp(), prevType));
                            }
                        }

                        if(AdjustRight)
                            AlignLinks(ref RightListEvent, ref LeftListEvent, false); 

                        i = RightListEvent.IndexOf(RightEvent);
                    }
                }
       }
        


        private static int getHighestCount(int n,params int[] Othern)
        {
            int highest = n;

            foreach (int index in Othern)
                if (highest < index) highest = index;

            return highest;
        }

        private static long getLowestTimeStamp(Queue<clsObjLLEvent> Queue,
                                               params Queue<clsObjLLEvent>[] Queues)
        {
            long lowestLong=-1;

            if (Queue.Count > 0)
                lowestLong = Queue.Peek().getLongTimeStamp();

            foreach (Queue<clsObjLLEvent> otherQueue in Queues)
            {
                if (otherQueue.Count > 0)
                {
                    if (lowestLong > otherQueue.Peek().getLongTimeStamp() || lowestLong == -1)
                        lowestLong = otherQueue.Peek().getLongTimeStamp();
                }
            }
            return lowestLong;
        }

        private static clsObjLLEvent[] popEventsByTimeStamp(Queue<clsObjLLEvent> Queue)
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
    }
}
