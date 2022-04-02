using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GraphicDLL.common;
using System.Drawing;

namespace GraphicDLL.XPanels
{
    abstract class  XResizablePanel : XPanel
    {
        private static Control MovingBar = null;
        private Boolean bOnDrag = false;

        protected enum ResizeOrientation { VERTICAL, HORIZONTAL }
        public XResizablePanel(iPageRequest request, Control holder)
            : base(request,holder)
        {

            this.MouseMove += new MouseEventHandler(XPanel_ResizeOnMove);
            this.MouseDown += new MouseEventHandler(XPanel_ResizeOnDown);
            this.MouseUp += new MouseEventHandler(XPanel_ResizeOnUp);
        }

        private Control MoveBar
        {
            get
            {
                if (MovingBar == null)
                {
                    MovingBar = new Control();
                    MovingBar.BackColor = Color.Gray;
                    if (!Parent.Controls.Contains(MoveBar))
                        Parent.Controls.Add(MoveBar);
                }
                return MovingBar;
            }
        }

        protected abstract ResizeOrientation getOrientation();

        private void XPanel_ResizeOnDown(object sender, MouseEventArgs e)
        {
            if (this.Cursor == Cursors.VSplit || this.Cursor == Cursors.HSplit)
            {
                bOnDrag = true;

                if (this.Cursor == Cursors.HSplit)
                {
                    MoveBar.Width = this.Width;
                    MoveBar.Height = 3;
                   //MoveBar.Location = new Point(0, isAnchoredToTop() ? this.Location.Y + this.Height : this.Location.Y);
                }
                else
                {
                    MoveBar.Width = 3;
                    MoveBar.Height = this.Height;
                    //MoveBar.Location = new Point(isAnchoredToLeft() ? this.Location.X + this.Width : this.Location.X, 0);

                }

                MoveBar.Show();
                MoveBar.BringToFront();
            }
        }

        private void XPanel_ResizeOnUp(object sender, MouseEventArgs e)
        {
            if (bOnDrag)
            {
                if (this.Cursor == Cursors.HSplit)
                {
                    if (isAnchoredToTop())
                    {
                        if (e.Y >= MinimumExpandSize() && e.Y <= MaximumExpandSize())
                        {
                            this.Height = e.Y;
                        }
                        else
                            this.Height = e.Y >= MinimumExpandSize() ? MaximumExpandSize() : MinimumExpandSize();
                    }
                    else
                    {
                        Point MousePosition = this.Parent.PointToClient(this.PointToScreen(new Point(e.X, e.Y)));
                        int nextHeight = this.Parent.Height - MousePosition.Y;
                        if (nextHeight >= MinimumExpandSize() && nextHeight <= MaximumExpandSize())
                        {
                            this.Location = new Point(this.Location.X, MousePosition.Y - MARGIN_BOTTOM - 1);
                            this.Height = nextHeight - this.MARGIN_BOTTOM;
                        }
                        else
                        {
                            int new_height = nextHeight >= MinimumExpandSize() ? MaximumExpandSize() : MinimumExpandSize();
                            this.Location = new Point(this.Location.X, Parent.Height - new_height);
                            this.Height = new_height;
                        }
                            
                    }
                }
                else
                {
                    if (isAnchoredToLeft())
                    {
                        if (e.X >= MinimumExpandSize() && e.X <= MaximumExpandSize())
                            this.Width = e.X;
                        else
                            this.Width = e.X >= MinimumExpandSize() ? MaximumExpandSize() : MinimumExpandSize();
                    }
                    else
                    {
                        Point MousePosition = this.Parent.PointToClient(this.PointToScreen(new Point(e.X, e.Y)));
                        int nextWidth = this.Parent.Width - MousePosition.X;
                        if (nextWidth >= MinimumExpandSize() && nextWidth <= MaximumExpandSize())
                        {
                            this.Location = new Point(MousePosition.X - MARGIN_RIGHT, this.Location.Y);
                            this.Width = nextWidth;
                        }
                        else
                        {
                            int new_width = nextWidth >= MinimumExpandSize() ? MaximumExpandSize() : MinimumExpandSize();
                            this.Location = new Point(Parent.Width - new_width, this.Location.Y);
                            this.Width = new_width;
                        }
                    }

                }
            }


            bOnDrag = false;
            MoveBar.Hide();
        }

        private void XPanel_ResizeOnMove(object sender, MouseEventArgs e)
        {
            if (getOrientation() == ResizeOrientation.VERTICAL)
            {
                XPanel_ResizeOnMove_Vertical(sender, e);
            }
            else
            {
                XPanel_ResizeOnMove_Horizontal(sender, e);
            }

        }

        private void XPanel_ResizeOnMove_Vertical(object sender, MouseEventArgs e)
        {
            if (isAnchoredToLeft())
            {
                if (this.Width - SharedResource.TIP_TOLERANCE_POINT < e.X &&
                    this.Width + SharedResource.TIP_TOLERANCE_POINT > e.X)
                {
                    this.Cursor = Cursors.VSplit;
                }
                else
                {
                    if (!bOnDrag)
                    {
                        this.Cursor = Cursors.Arrow;
                    }
                }
            }
            else
            {
                if (-SharedResource.TIP_TOLERANCE_POINT < e.X &&
                   (SharedResource.TIP_TOLERANCE_POINT > e.X))
                {
                    this.Cursor = Cursors.VSplit;
                }
                else
                {
                    if (!bOnDrag)
                    {
                        this.Cursor = Cursors.Arrow;
                    }
                }
            }


            if (this.Cursor == Cursors.VSplit)
            {

                if (isAnchoredToLeft())
                {
                    if (MousePosition.X >= MinimumExpandSize() && MousePosition.X <= MaximumExpandSize())
                    {
                        MoveBar.Location = new Point(MousePosition.X + MARGIN_RIGHT, 0);
                    }
                }
                else
                {
                    Point MousePosition = this.Parent.PointToClient(this.PointToScreen(new Point(e.X, e.Y)));
                    int nextWidth = this.Parent.Width - MousePosition.X;
                    if (bOnDrag && nextWidth >= MinimumExpandSize() && nextWidth <= MaximumExpandSize())
                    {

                        MoveBar.Location = new Point(MousePosition.X + MARGIN_RIGHT, 0);
                    }
                }
                /*
                if (isAnchoredToLeft())
                {
                    if (bOnDrag && e.X >= MinimumExpandSize() && e.X <= MaximumExpandSize())
                        this.Width = e.X;
                }
                else
                {
                    Point MousePosition = this.Parent.PointToClient(this.PointToScreen(new Point(e.X, e.Y)));
                    int nextWidth = this.Parent.Width - MousePosition.X;
                    if (bOnDrag && nextWidth >= MinimumExpandSize() && nextWidth <= MaximumExpandSize())
                    {
                        this.Location = new Point(MousePosition.X - MARGIN_RIGHT, this.Location.Y);
                        this.Width = nextWidth;
                    }

                }*/
            }
            
            this.Refresh();

        }

        private void XPanel_ResizeOnMove_Horizontal(object sender, MouseEventArgs e)
        {
            if (isAnchoredToTop())
            {
                if (this.Height - SharedResource.TIP_TOLERANCE_POINT < e.Y &&
                        this.Height + SharedResource.TIP_TOLERANCE_POINT > e.Y)
                {
                    this.Cursor = Cursors.HSplit;
                }
                else
                {
                    if (!bOnDrag)
                    {
                        this.Cursor = Cursors.Arrow;
                    }
                }
            }
            else
            {
                if (-SharedResource.TIP_TOLERANCE_POINT < e.Y &&
                   (SharedResource.TIP_TOLERANCE_POINT > e.Y))
                {
                    this.Cursor = Cursors.HSplit;
                }
                else
                {
                    if (!bOnDrag)
                    {
                        this.Cursor = Cursors.Arrow;
                    }
                }
            }

            if (this.Cursor == Cursors.HSplit)
            {
                if (isAnchoredToTop())
                {
                    if (MousePosition.Y >= MinimumExpandSize() && MousePosition.Y <= MaximumExpandSize())
                    {
                        MoveBar.Location = new Point(0, MousePosition.Y);
                    }
                }
                else
                {
                    Point MousePosition = this.Parent.PointToClient(this.PointToScreen(new Point(e.X, e.Y)));
                    int nextHeight = this.Parent.Height - MousePosition.Y;
                    if (bOnDrag && nextHeight >= MinimumExpandSize() && nextHeight <= MaximumExpandSize())
                    {
                        MoveBar.Location = new Point(0, MousePosition.Y);
                    }
                }

                /*
                if (isAnchoredToTop())
                {
                    if (bOnDrag && e.Y >= MinimumExpandSize() && e.Y <= MaximumExpandSize())
                    {
                        this.Height = e.Y;
                    }
                }
                else
                {
                    Point MousePosition = this.Parent.PointToClient(this.PointToScreen(new Point(e.X, e.Y)));
                    int nextHeight = this.Parent.Height - MousePosition.Y;
                    if (bOnDrag && nextHeight >= MinimumExpandSize() && nextHeight <= MaximumExpandSize())
                    {
                        this.Location = new Point(this.Location.X, MousePosition.Y - MARGIN_BOTTOM - 1);
                        this.Height = nextHeight - this.MARGIN_BOTTOM;
                    }
                }
                 */
            }


            this.Refresh();
        }


        protected abstract int MaximumExpandSize();

        protected abstract int MinimumExpandSize();
    }
}
