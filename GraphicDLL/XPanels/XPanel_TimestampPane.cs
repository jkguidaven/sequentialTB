using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using GraphicDLL.common;
using System.Drawing.Drawing2D;
using GraphicDLL.XControls;

namespace GraphicDLL.XPanels
{
    class XPanel_TimestampPane : XPanel
    {

        public XControl_TimestampSlice XControl_TSSlice = null;

        public XPanel_TimestampPane(iPageRequest request, XPanel_EventPane holder) : base(request, holder) { }

        protected override void XPanel_OnPreInitialize()
        {
            AnchorTo(AnchorStyle.TOP,(m_ctrlContainer as XPanel_EventPane).getLifeLinePanel(), AnchorStyle.BOTTOM);
            AnchorTo(AnchorStyle.BOTTOM, AnchorStyle.BOTTOM);
            AnchorTo(AnchorStyle.RIGHT, AnchorStyle.RIGHT);
            this.BackColor = Color.Transparent;
            XControl_TSSlice = new XControl_TimestampSlice(m_IPRequest,m_ctrlContainer);
            Controls.Add(XControl_TSSlice);
            this.AutoScroll = true;

            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;
            this.Focus();
        }

        protected override void XPanel_OnPostInitialize()
        {
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.Focus();
            base.OnMouseDown(e);
        }


        public void moveView(ArrowDirection direction)
        {
            try
            {
                switch (direction)
                {
                    case ArrowDirection.Up:
                        this.VerticalScroll.Value -= 10;
                        break;

                    case ArrowDirection.Down:
                        this.VerticalScroll.Value += 10;
                        break;
                }

                this.Refresh();
            }
            catch (Exception) { }
        }

        protected override Point ScrollToControl(Control activeControl)
        {
            return this.AutoScrollPosition;
        }


        protected override void XPanel_OnPaint(object sender, PaintEventArgs e)
        {
            using (Pen p = new Pen(SharedResource.LIFELINE_STROKE_COLOR))
            {
                XPanel_LifeLinePane LifeLinePanel = (m_ctrlContainer as XPanel_EventPane).getLifeLinePanel();
                p.DashStyle = DashStyle.Dash;
                e.Graphics.DrawLine(p, LifeLinePanel.getLifeline("POS").getPositionX(), 0,
                                        LifeLinePanel.getLifeline("POS").getPositionX(), this.Height);
                e.Graphics.DrawLine(p, LifeLinePanel.getLifeline("POSM").getPositionX(), 0,
                                        LifeLinePanel.getLifeline("POSM").getPositionX(), this.Height);
                e.Graphics.DrawLine(p, LifeLinePanel.getLifeline("FLM").getPositionX(), 0,
                                        LifeLinePanel.getLifeline("FLM").getPositionX(), this.Height);
                e.Graphics.DrawLine(p, LifeLinePanel.getLifeline("FL").getPositionX(), 0,
                                        LifeLinePanel.getLifeline("FL").getPositionX(), this.Height);


            }
        }




        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);

        /*
            SB_HORZ = 0,
            SB_VERT = 1,
            SB_CTL = 2,
            SB_BOTH = 3
         */

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            ShowScrollBar(this.Handle, 0, false);
            base.WndProc(ref m);
        }
    }


}
