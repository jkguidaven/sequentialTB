using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using CoreDLL.logworker;
using GraphicDLL.XPanels;

namespace GraphicDLL.common
{
    public abstract class XPanel : UserControl
    {
        #region MACRO
        protected static readonly TraceLevel L1 = TraceLevel.L1;
        protected static readonly TraceLevel L2 = TraceLevel.L2;
        protected static readonly TraceLevel L3 = TraceLevel.L3;

        private static LogWorker m_LWLogger = LogWorker.prtyclsLWInstance;
        #endregion

        #region TRACE_METHOD
        public static void trace(TraceLevel eTLevel, params String[] arrstrTraces)
        {
            m_LWLogger.trace(eTLevel, arrstrTraces);
        }

        public static void trace(params String[] arrstrTraces)
        {
            m_LWLogger.trace(L3, arrstrTraces);
        }

        public static void trace(Object str)
        {
            trace(str.ToString());
        }
        #endregion

        protected class AnchorObj
        {
            public Control control;
            public AnchorStyle side;

            public AnchorObj(Control control, AnchorStyle side)
            {
                this.control = control;
                this.side = side;
            }
        }

        public enum AnchorStyle { LEFT, RIGHT, TOP, BOTTOM }

        protected AnchorObj AnchorLeft = null;
        protected AnchorObj AnchorRight = null;
        protected AnchorObj AnchorTop = null;
        protected AnchorObj AnchorBottom = null;

        protected Control m_ctrlContainer   = null;
        protected iPageRequest m_IPRequest = null;

        protected int MARGIN_LEFT = 0;
        protected int MARGIN_RIGHT = 0;
        protected int MARGIN_TOP = 0;
        protected int MARGIN_BOTTOM = 0;

        public XPanel(iPageRequest request,Control ctrlContainer)
        {
            m_IPRequest = request;
            m_ctrlContainer = ctrlContainer;
            m_ctrlContainer.Controls.Add(this);
            this.Paint += new PaintEventHandler(XPanel_OnPaint);
            m_ctrlContainer.Resize += new EventHandler(XPanel_OnResize);
            this.DoubleBuffered = true;
            Initialize();
        }

        private void Initialize()
        {
            trace("+Xpanel:Oninitialize()");
            AnchorTo(AnchorStyle.TOP, AnchorStyle.TOP);
            AnchorTo(AnchorStyle.LEFT, AnchorStyle.LEFT);
            XPanel_OnPreInitialize();
            if (!(this.Parent is XPanel)) // parent not XPanel, invoke PostInitialize
                PostInitialize();

            this.XPanel_OnResize(null, null);
            trace("-Xpanel:Oninitialize()");
        }

        public void PostInitialize()
        {
            trace("+"+this + ":OnPostInitialize()");
            XPanel_OnPostInitialize();
            foreach (Control ptrCtr in Controls)
                if (ptrCtr is XPanel)
                    (ptrCtr as XPanel).PostInitialize();
            trace("-"+this + ":OnPostInitialize()");
        }

        protected abstract void XPanel_OnPreInitialize();
        protected abstract void XPanel_OnPostInitialize();


        protected abstract void XPanel_OnPaint(object sender, PaintEventArgs e);

        protected virtual void XPanel_OnResize(object sender, EventArgs e)
        {
            int point_x = 0,point_y =0;

            if (AnchorLeft != null)
            {
                point_x = (AnchorLeft.control == m_ctrlContainer ?
                                    (AnchorLeft.side == AnchorStyle.LEFT ? 0 + MARGIN_LEFT : m_ctrlContainer.Width + MARGIN_LEFT)
                                    :
                                    (AnchorLeft.side == AnchorStyle.LEFT ?
                                            AnchorLeft.control.Location.X + MARGIN_LEFT :
                                            AnchorLeft.control.Location.X + AnchorLeft.control.Width + MARGIN_LEFT));
            }

            if(AnchorTop != null)
            {
                point_y = (AnchorTop.control == m_ctrlContainer ?
                                    (AnchorTop.side == AnchorStyle.TOP ? 0 + MARGIN_TOP : m_ctrlContainer.Height + MARGIN_TOP)
                                    :
                                    (AnchorTop.side == AnchorStyle.TOP ?
                                            AnchorTop.control.Location.X + MARGIN_TOP :
                                            AnchorTop.control.Location.X + AnchorTop.control.Height + MARGIN_TOP));
            }

            int point_width = 0, point_height = 0;


            if (AnchorRight != null)
            {
                point_width = (AnchorRight.control == m_ctrlContainer ?
                                    (AnchorRight.side == AnchorStyle.LEFT ? 0 - MARGIN_RIGHT : m_ctrlContainer.Width - MARGIN_RIGHT)
                                    :
                                    (AnchorRight.side == AnchorStyle.LEFT ?
                                            AnchorRight.control.Location.X - MARGIN_RIGHT :
                                            AnchorRight.control.Location.X + AnchorRight.control.Width - MARGIN_RIGHT));

                if(AnchorLeft != null)
                    this.Width = point_width - point_x;
            }

            if (AnchorBottom != null)
            {
                point_height = (AnchorBottom.control == m_ctrlContainer ?
                                (AnchorBottom.side == AnchorStyle.TOP ? 0 - MARGIN_BOTTOM : m_ctrlContainer.Height - MARGIN_BOTTOM)
                                :
                                (AnchorBottom.side == AnchorStyle.TOP ?
                                            AnchorBottom.control.Location.Y - MARGIN_BOTTOM :
                                            AnchorBottom.control.Location.Y + AnchorBottom.control.Height - MARGIN_BOTTOM));
                
                if(AnchorTop != null)
                    this.Height = point_height - point_y;
            }

            if (AnchorLeft == null && AnchorRight != null)
                point_x = AnchorRight.control.Width - this.Width -1 - MARGIN_RIGHT;
            else if (AnchorLeft == null && AnchorRight == null)
                point_x = 1;

            if (AnchorTop == null && AnchorBottom != null)
                point_y = AnchorBottom.control.Height - this.Height - 1 - MARGIN_BOTTOM;
            else if (AnchorTop == null && AnchorBottom == null)
                point_y = 1;

            this.Location = new Point(point_x, point_y);
            Invalidate();
        }

        public void AnchorTo(AnchorStyle style,AnchorStyle side)
        {
            AnchorTo(style, this.m_ctrlContainer, side);
        }

        public void AnchorTo(AnchorStyle style, Control PanelDock, AnchorStyle side)
        {
            trace(this + ":anchor-" + style + " to " + PanelDock + ":anchor-"+side);
            if (style == AnchorStyle.LEFT || style == AnchorStyle.RIGHT)
            {
                if (side != AnchorStyle.LEFT && side != AnchorStyle.RIGHT)
                {
                    trace("Exception found:" + "AnchorStyle not compatible: {" + style + " and " + side + "}");
                    throw new Exception("AnchorStyle not compatible: {" + style + " and " + side + "}");
                }
            }
            else
            {
                if (side != AnchorStyle.TOP && side != AnchorStyle.BOTTOM)
                {
                    trace("Exception found:" + "AnchorStyle not compatible: {" + style + " and " + side + "}");
                    throw new Exception("AnchorStyle not compatible: {" + style + " and " + side + "}");
                }
            }

            if (PanelDock != this.m_ctrlContainer && !m_ctrlContainer.Controls.Contains(PanelDock))
            {
                trace("Exception found:" + "Could not anchor to Control!");
                throw new Exception("Could not anchor to Control!");
            }


            PanelDock.Resize += new EventHandler(XPanel_OnResize);


            switch (style)
            {
                case AnchorStyle.LEFT:
                    AnchorLeft = new AnchorObj(PanelDock,side);
                    break;
                case AnchorStyle.RIGHT:
                    AnchorRight = new AnchorObj(PanelDock, side);
                    break;
                case AnchorStyle.TOP:
                    AnchorTop = new AnchorObj(PanelDock, side);
                    break;
                case AnchorStyle.BOTTOM:
                    AnchorBottom = new AnchorObj(PanelDock, side);
                    break;
            }

            XPanel_OnResize(null, null);
        }

        protected void margin(int margin_size)
        {
            margin(margin_size, margin_size, margin_size, margin_size);
        }

        protected void margin(int LEFT, int TOP, int RIGHT, int BOTTOM)
        {
            MARGIN_BOTTOM = BOTTOM;
            MARGIN_LEFT = LEFT;
            MARGIN_TOP = TOP;
            MARGIN_RIGHT = RIGHT;
        }



        public static void removeLoadingPane(Control ctrHolder){

            foreach(Control ctr in ctrHolder.Controls){
                if (ctr is XPanel_LoadingPane)
                {
                    ctrHolder.Controls.Remove(ctr);
                    (ctr as XPanel_LoadingPane).StopLoading();
                    return;
                }
            }
        }


        public Boolean isAnchoredToLeft() { return AnchorLeft != null; }
        public Boolean isAnchoredToTop() { return AnchorTop != null; }
    }
}
