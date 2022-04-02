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
    class XPanel_FilterPane : XResizablePanel
    {
        public XPanel_FilterPane(iPageRequest request, XPanel_Main holder) : base(request,holder) { }
        private Boolean bIsExpand = false;
        private Boolean bHoverExpand = false;

        protected override void XPanel_OnPreInitialize()
        {
            this.MouseMove += new MouseEventHandler(XPanel_OnMove);
            this.MouseLeave += new EventHandler(XPanel_OnLeave);
            this.MouseClick += new MouseEventHandler(XPanel_OnClick);

            this.Width = MinimumExpandSize();
            margin(1, 1, 0, 0);
        }

        protected override void XPanel_OnPostInitialize()
        {

            XPanel_Main holder = (XPanel_Main)m_ctrlContainer;
            AnchorTo(AnchorStyle.BOTTOM, holder.getEventPanel(),AnchorStyle.BOTTOM);
        }


        protected override ResizeOrientation getOrientation() { return ResizeOrientation.VERTICAL; }

        protected override int MaximumExpandSize() { return 200; }
        protected override int MinimumExpandSize() { return 30; }


        protected void XPanel_OnClick(object sender, MouseEventArgs e)
        {
            if (bHoverExpand)
            {
                this.Width = !bIsExpand ? MaximumExpandSize() : MinimumExpandSize();
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
            bIsExpand = this.Width > MinimumExpandSize();
            using (Pen p = new Pen(SharedResource.BORDER_COLOR))
            {
                p.DashStyle = DashStyle.Solid;
                using (SolidBrush b = new SolidBrush(SharedResource.ALT_BACKGROUND_COLOR)) e.Graphics.FillRectangle(b, 0, 0, Width - 1, Height - 1);
                e.Graphics.DrawRectangle(p, new Rectangle(-1, -1, this.Width, this.Height + 1));
                p.Color = Color.White;
                e.Graphics.DrawLine(p, new Point(0, 0), new Point(0, this.Height));
            }

            Rectangle HeaderRec = new Rectangle(-1, -1, this.Width, SharedResource.FILTER_HEADER_HEIGHT);

            using (LinearGradientBrush brush = new LinearGradientBrush(HeaderRec,
                                                                       SharedResource.HEADER_TOP_COLOR, SharedResource.HEADER_BOT_COLOR,
                                                                       90F, true))
            {
                e.Graphics.FillRectangle(brush, HeaderRec);
            }

            if (bHoverExpand)
            {
                using (Brush brush = new SolidBrush(SharedResource.ALT_BACKGROUND_COLOR2))
                {
                    e.Graphics.FillRectangle(brush, this.Width - MinimumExpandSize(), 0, MinimumExpandSize(), SharedResource.FILTER_HEADER_HEIGHT - 1);
                }
            }

            using (Pen pen = new Pen(Color.Gray))
                e.Graphics.DrawRectangle(pen, HeaderRec);
            using (Pen pen = new Pen(Color.WhiteSmoke))
            {
                e.Graphics.DrawLine(pen, 0, 0, 0, SharedResource.FILTER_HEADER_HEIGHT - 2);
                e.Graphics.DrawLine(pen, 0, 0, this.Width - 2, 0);

                pen.Width = 2;
                int ARROW_WIDTH = 4;
                int ARROW_HEIGHT = 8;

                if (bIsExpand)
                {
                    e.Graphics.DrawLine(pen,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2 - ARROW_HEIGHT / 2);
                    e.Graphics.DrawLine(pen,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2 + ARROW_HEIGHT / 2);

                    e.Graphics.DrawLine(pen,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH + 6,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 + 6,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2 - ARROW_HEIGHT / 2);
                    e.Graphics.DrawLine(pen,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH + 6,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 + 6,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2 + ARROW_HEIGHT / 2);

                    pen.Color = Color.Gray;
                    e.Graphics.DrawLine(pen,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH - 1,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - 1,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2 - ARROW_HEIGHT / 2);
                    e.Graphics.DrawLine(pen,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH - 1,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - 1,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2 + ARROW_HEIGHT / 2);

                    e.Graphics.DrawLine(pen,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH + 5,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 + 5,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2 - ARROW_HEIGHT / 2);
                    e.Graphics.DrawLine(pen,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH + 5,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 + 5,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2 + ARROW_HEIGHT / 2);
                }
                else
                {
                    e.Graphics.DrawLine(pen,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2 - ARROW_HEIGHT / 2);
                    e.Graphics.DrawLine(pen,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2 + ARROW_HEIGHT / 2);

                    e.Graphics.DrawLine(pen,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 + 6,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH + 6,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2 - ARROW_HEIGHT / 2);
                    e.Graphics.DrawLine(pen,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 + 6,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH + 6,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2 + ARROW_HEIGHT / 2);

                    pen.Color = Color.Gray;
                    e.Graphics.DrawLine(pen,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - 1,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH - 1,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2 - ARROW_HEIGHT / 2);
                    e.Graphics.DrawLine(pen,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - 1,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH - 1,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2 + ARROW_HEIGHT / 2);

                    e.Graphics.DrawLine(pen,

                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 + 5,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH + 5,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2 - ARROW_HEIGHT / 2);
                    e.Graphics.DrawLine(pen,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 + 5,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2,
                                            this.Width - SharedResource.LIFELINE_HEAD_MARGIN2 - ARROW_WIDTH + 5,
                                            SharedResource.FILTER_HEADER_HEIGHT / 2 + ARROW_HEIGHT / 2);
                }

            }

            SolidBrush Stringbrush = new SolidBrush(Color.Gray);
            String FINAL_HEADER_TITLE = SharedResource.FILTER_HEADER_TITLE;
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
          
            Stringbrush.Dispose();
        }

    }
}
