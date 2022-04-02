using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GraphicDLL.common;
using System.Drawing.Drawing2D;

namespace GraphicDLL.XPanels
{
    class XPanel_ActivityPane : XResizablePanel
    {
        private Boolean bHoverExpand = false;
        private Boolean bIsExpand = false;
        private Boolean bOnMaxExpand = false;
        public XPanel_ActivityPane(iPageRequest request, XPanel_Main holder) : base(request,holder) { }


        protected override void XPanel_OnPostInitialize() { 
        }
        protected override void XPanel_OnPreInitialize()
        {
            
            this.MouseMove += new MouseEventHandler(XPanel_OnMove);
            this.MouseLeave += new EventHandler(XPanel_OnLeave);
            this.MouseClick += new MouseEventHandler(XPanel_OnClick);
            this.Height = MinimumExpandSize();
            AnchorTop = null;
            AnchorTo(AnchorStyle.BOTTOM, AnchorStyle.BOTTOM);
            AnchorTo(AnchorStyle.LEFT, AnchorStyle.LEFT);
            AnchorTo(AnchorStyle.RIGHT, AnchorStyle.RIGHT);
            margin(1, 0, 1, 0);
        }


        protected override int MaximumExpandSize() { return m_ctrlContainer.Height-1; }
        protected override int MinimumExpandSize() { return 30; }
        protected override ResizeOrientation getOrientation() { return ResizeOrientation.HORIZONTAL; }
        

        protected void XPanel_OnClick(object sender, MouseEventArgs e)
        {
            if (bHoverExpand)
            {
                if (!bIsExpand)
                {
                    this.Location = new Point(this.Location.X, this.Parent.Height - MaximumExpandSize()-1 - MARGIN_BOTTOM);
                    this.Height = MaximumExpandSize();
                    bOnMaxExpand = true;
                }
                else
                {
                    this.Location = new Point(this.Location.X, this.Parent.Height - MinimumExpandSize() - MARGIN_BOTTOM);
                    this.Height = MinimumExpandSize()-1;
                    bOnMaxExpand = false;
                }
           
                bIsExpand = !bIsExpand;
            }
        }
        protected void XPanel_OnLeave(object sender, EventArgs e)
        {
            Boolean prev_bHoverExpand = bHoverExpand;
            bHoverExpand = false;
            if (prev_bHoverExpand != bHoverExpand)
                Invalidate();
        }

        protected void XPanel_OnMove(object sender, MouseEventArgs e)
        {
            Boolean prev_bHoverExpand = bHoverExpand;

            if (e.X > (this.Width - 40) && e.Y < SharedResource.FILTER_HEADER_HEIGHT)
                bHoverExpand = true;
            else
                bHoverExpand = false;
            if (prev_bHoverExpand != bHoverExpand)
                Refresh();
        }



        protected override void XPanel_OnPaint(object sender, PaintEventArgs e)
        {

            bIsExpand = this.Parent.Height - MinimumExpandSize() > (Location.Y + 1);

            using (SolidBrush b = new SolidBrush(SharedResource.ALT_BACKGROUND_COLOR)) e.Graphics.FillRectangle(b, 0, 0, Width+1, Height);
            Rectangle HeaderRec = new Rectangle(-1, -1, this.Width+1, SharedResource.FILTER_HEADER_HEIGHT);

            using (LinearGradientBrush brush = new LinearGradientBrush(HeaderRec,
                                                                       SharedResource.HEADER_TOP_COLOR, SharedResource.HEADER_BOT_COLOR,
                                                                       90F, true))
                e.Graphics.FillRectangle(brush, HeaderRec);

            if (bHoverExpand)
            {
                using (Brush brush = new SolidBrush(SharedResource.ALT_BACKGROUND_COLOR2))
                {
                    e.Graphics.FillRectangle(brush, this.Parent.Width - 40, 1, 39, SharedResource.FILTER_HEADER_HEIGHT - 1);
                }
            }

            using (Pen pen = new Pen(Color.Gray))
            {
                e.Graphics.DrawLine(pen, 0, 0, this.Width + 1,0);
                e.Graphics.DrawRectangle(pen, HeaderRec);
            }


            SolidBrush Stringbrush = new SolidBrush(Color.Gray);
            String FINAL_HEADER_TITLE = SharedResource.ACITIVITY_HEADER_TITLE;
            float StringHeight = e.Graphics.MeasureString(FINAL_HEADER_TITLE, SharedResource.FILTER_HEADER_FONT).Height;
            float StringWidth = e.Graphics.MeasureString(FINAL_HEADER_TITLE, SharedResource.FILTER_HEADER_FONT).Width;

            if (this.Width > MinimumExpandSize())
            {
                Boolean bNoError = true;
                while (StringWidth + 20 > this.Width && FINAL_HEADER_TITLE.Length > 2)
                {
                    if (FINAL_HEADER_TITLE.Length <= 3) { bNoError = false; break; }
                    FINAL_HEADER_TITLE = FINAL_HEADER_TITLE.Substring(0, FINAL_HEADER_TITLE.Length - 4) + ".. ";
                    StringWidth = e.Graphics.MeasureString(FINAL_HEADER_TITLE, SharedResource.FILTER_HEADER_FONT).Width;
                }

                if (bNoError)
                {
                    Stringbrush.Color = Color.White;
                    e.Graphics.DrawString(FINAL_HEADER_TITLE, SharedResource.FILTER_HEADER_FONT, Stringbrush, 5,
                                                  (SharedResource.FILTER_HEADER_HEIGHT / 2) - (StringHeight / 2));
                    Stringbrush.Color = Color.Gray;
                    e.Graphics.DrawString(FINAL_HEADER_TITLE, SharedResource.FILTER_HEADER_FONT, Stringbrush, 4,
                                              (SharedResource.FILTER_HEADER_HEIGHT / 2) - (StringHeight / 2));
                }
            }


            using (Pen pen = new Pen(Color.WhiteSmoke))
            {
                pen.Width = 2;
                int ARROW_WIDTH = 10;
                int ARROW_HEIGHT = 8;


                if (bIsExpand)
                {
                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH ,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 1,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2)+1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - 1);

                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 + 2,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 1,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2)+1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - 1);

                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 7,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2)+1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) + 5);

                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 + 2,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 7,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2)+1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) + 5);

                    pen.Color = Color.Gray;
                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH - 1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 1,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2),
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - 1);

                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2  + 1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 1,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2),
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - 1);

                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH - 1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 7,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2),
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) + 5);

                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 + 1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 7,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2),
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) + 5);
                }
                else
                {
                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2) + 1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 1,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - 1);

                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2),
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 2,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - 1);

                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2) + 1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 1,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - 1);

                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2),
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 2,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - 1);


                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2) + 1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 7,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) + 5);

                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2),
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 8,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) + 5);

                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2) + 1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 7,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) + 5);

                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2),
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 8,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) + 5);


                    pen.Color = Color.Gray;
                    e.Graphics.DrawLine(pen,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2),
                                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 1,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH - 1,
                                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - 1);

                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2) - 1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 2,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - 1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - 1);

                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2),
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 1,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH - 1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - 1);

                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2) - 1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 2,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - 1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - 1);


                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2),
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 7,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH - 1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) + 5);

                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2) - 1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 8,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - 1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) + 5);

                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2),
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 7,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH - 1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) + 5);

                    e.Graphics.DrawLine(pen,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - (ARROW_WIDTH / 2) - 1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) - ARROW_HEIGHT + 8,
                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - 1,
                            (SharedResource.FILTER_HEADER_HEIGHT / 2) + 5);
                }

            }


            Stringbrush.Dispose();
        }



      protected override void XPanel_OnResize(object sender, EventArgs e)
        {
          if(bIsExpand) if (Parent.Height < this.Height) bOnMaxExpand = true;
            base.XPanel_OnResize(sender, e);
            if (bOnMaxExpand)
                this.Height = MaximumExpandSize();
        }
    }
}
