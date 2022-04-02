using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace GraphicDLL.common
{
    public abstract class XControl : UserControl
    {
        protected  Control m_ctrlContainer = null;
        protected iPageRequest m_IPRequest = null;
        public XControl(iPageRequest request,Control holder,params object[] data)
        {
            this.m_IPRequest = request;
            this.m_ctrlContainer = holder;
            this.Paint += new PaintEventHandler(XControl_OnPaint);
            holder.Controls.Add(this);
            holder.Resize += new EventHandler(XControl_OnResize);
            this.DoubleBuffered = true;
            XControl_OnInitialize(data);
        }

        protected abstract void XControl_OnResize(object sender, EventArgs e);
        protected abstract void XControl_OnInitialize(params object[] data);

        protected abstract void XControl_OnPaint(object sender, PaintEventArgs e);
    }      

}
