using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using GraphicDLL.common;
using GraphicDLL.XPanels;

namespace GraphicDLL.XControls
{
    public class XControl_LifeLine : XControl
    {



        private String LifeLine_Name = "";
        private static readonly Object lockPosition = new Object();
        private int position_x = 0;
        private int last_offset = 0;
        public Color CurrBackColor = SharedResource.LIFELINE_HEAD_COLOR;
        public static XControl_LifeLine TopLifeLine = null;
        //private Boolean FirstOnResize = false;

        public XControl_LifeLine(iPageRequest request,String LifeLine_Name, int position_x,Control holder)
            : base(request,holder,LifeLine_Name, position_x)
        {

        }

        protected override void XControl_OnInitialize(params object[] data)
        {
            //FirstOnResize = true;
            this.BackColor = Color.Transparent;
            this.LifeLine_Name = data[0] as String;
            this.position_x = (int)data[1];
            this.MouseMove += new MouseEventHandler(XControl_OnMouseMove);
            this.MouseUp += new MouseEventHandler(XControl_OnMouseUp);
            this.MouseDown += new MouseEventHandler(XControl_OnMouseDown);
        }

        public void setName(String LifelineName) { this.LifeLine_Name = LifelineName; }
        public String getName() { return this.LifeLine_Name; }

        protected override void XControl_OnResize(object sender, EventArgs e)
        {
            int StringHeight = (int)CreateGraphics().MeasureString(LifeLine_Name, SharedResource.LIFELINE_HEAD_FONT).Height;
            int StringWidth =  (int)CreateGraphics().MeasureString(LifeLine_Name, SharedResource.LIFELINE_HEAD_FONT).Width;

            this.Width = StringWidth + (2 * SharedResource.LIFELINE_HEAD_MARGIN);
            if (SharedResource.LIFELINE_MIN_WIDTH > Width) Width = SharedResource.LIFELINE_MIN_WIDTH;
            this.Height = StringHeight + SharedResource.LIFELINE_HEAD_MARGIN;
            this.Location = new Point(position_x - (Width / 2), SharedResource.LIFELINE_TOP_GAP);

            /*
            if (FirstOnResize)
            {
                if (Parent.Width < Location.X + Width + 2 && Location.X > 10)
                {
                    Location = new Point(Parent.Width - Width - 2, Location.Y);
                    position_x = Location.X + (Width / 2);
                }
            }
            */
        }

        protected override void XControl_OnPaint(object sender, PaintEventArgs e)
        {

        }


        protected  void XControl_OnMouseMove(object sender, MouseEventArgs e)
        {
            if ((m_ctrlContainer as XPanel_LifeLinePane).getLifeline("movable").CurrBackColor != SharedResource.LIFELINE_HEAD_COLOR_ALT)
                this.Cursor = Cursors.Hand;
            else
            {
                this.BackColor = Color.Transparent;
                Point MousePosition = this.Parent.PointToClient(this.PointToScreen(new Point(e.X, e.Y)));
                int new_pointX = MousePosition.X - last_offset;

                if (this.Location.X > new_pointX)
                    this.Cursor = Cursors.PanWest;

                if (this.Location.X < new_pointX)
                    this.Cursor = Cursors.PanEast;


                if (new_pointX > 0 && (new_pointX+this.Width) < this.Parent.Width)
                {
                     this.Location = new Point(new_pointX, this.Location.Y);
                     (m_ctrlContainer as XPanel_LifeLinePane).getLifeline("movable").position_x = this.Location.X + (this.Width / 2);
                }

                this.Parent.Invalidate();
            }

        }

        protected void XControl_OnMouseDown(object sender, MouseEventArgs e)
        {
            TopLifeLine = this;
            CurrBackColor = Color.Transparent;
            (m_ctrlContainer as XPanel_LifeLinePane).getLifeline("movable").CurrBackColor = SharedResource.LIFELINE_HEAD_COLOR_ALT;
            if (e.X < this.Width / 2)
                this.Cursor = Cursors.PanWest;
            else
                this.Cursor = Cursors.PanEast;

            last_offset = e.X;


            (m_ctrlContainer as XPanel_LifeLinePane).getLifeline("movable").Visible = true;
            (m_ctrlContainer as XPanel_LifeLinePane).getLifeline("movable").Location = this.Location;
            (m_ctrlContainer as XPanel_LifeLinePane).getLifeline("movable").setName(this.getName());
            (m_ctrlContainer as XPanel_LifeLinePane).getLifeline("movable").BringToFront();
            this.Parent.Refresh();
        }
        protected void XControl_OnMouseUp(object sender, MouseEventArgs e)
        {
            CurrBackColor = SharedResource.LIFELINE_HEAD_COLOR;
            (m_ctrlContainer as XPanel_LifeLinePane).getLifeline("movable").CurrBackColor = SharedResource.LIFELINE_HEAD_COLOR;
            position_x = (m_ctrlContainer as XPanel_LifeLinePane).getLifeline("movable").position_x;
            (m_ctrlContainer as XPanel_LifeLinePane).getLifeline("movable").Visible = false;
            this.BringToFront();

            this.Parent.Refresh();
            (this.Parent.Parent as XPanel_EventPane).refreshTimeStamp();
        }

        public int getPositionX() { return position_x; }
    }
}
