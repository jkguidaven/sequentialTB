using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphicDLL.common;
using System.Windows.Forms;
using System.Drawing;

namespace GraphicDLL.XControls
{
    class XControlLifelineLinker : XControl
    {
        XControl_LifeLineBody m_clsLifeLineBody = null;
        public XControlLifelineLinker(iPageRequest request,XControl_LifeLineBody clsLifeLineBody, Control holder)
            : base(request,holder, clsLifeLineBody)
        {
        }

        protected override void XControl_OnInitialize(params object[] data)
        {
            m_clsLifeLineBody = data[0] as XControl_LifeLineBody;
            BackColor = Color.Transparent;
        }


        protected override void XControl_OnResize(object sender, EventArgs e)
        {
            Width = m_ctrlContainer.Width;
            Height = m_ctrlContainer.Height;
        }


        protected override void XControl_OnPaint(object sender, PaintEventArgs e)
        {
        }

    }
}
