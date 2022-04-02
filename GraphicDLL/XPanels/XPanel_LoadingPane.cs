using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GraphicDLL.common;
using System.Drawing.Drawing2D;
using System.Threading;
using CoreDLL.utilities;

namespace GraphicDLL.XPanels
{
    public class XPanel_LoadingPane : XPanel
    {
        private PictureBox m_LoadingIcon            = null;
        private Boolean    m_bWithBorder            = false;
        private Boolean    m_bLoadingState          = false;
        private Thread     m_loadingTextThread      = null;
        private int        m_loadingTextDotCount    = 0;


        private String m_strDisplayableText = "";

        public XPanel_LoadingPane(iPageRequest request, Control holder) : this(request,holder, false) { }
        public XPanel_LoadingPane(iPageRequest request, Control holder, Boolean bWithBorder)
            : base(request, holder)
        {
            this.m_bWithBorder = bWithBorder;

            if (bWithBorder)
                margin(5);
        }

        protected override void XPanel_OnPreInitialize()
        {
            this.BackColor = Color.White;
            AnchorTo(AnchorStyle.RIGHT, AnchorStyle.RIGHT);
            AnchorTo(AnchorStyle.BOTTOM, AnchorStyle.BOTTOM);
            m_LoadingIcon = constructLoadImage();
            Controls.Add(m_LoadingIcon);
            centerLoadImage();
        }

        protected override void XPanel_OnPostInitialize()
        {
            m_bLoadingState = true;
            m_loadingTextThread = new Thread(new ThreadStart(ThreadTextLoad));
            m_loadingTextThread.Name = this.GetType().ToString() + "::LoadingTextThread";
            GraphicEngine.RegisterThread(m_loadingTextThread);
            m_loadingTextThread.Start();
        }


        private void ThreadTextLoad()
        {
            while (m_bLoadingState)
            {
                m_loadingTextDotCount++;
                if (m_loadingTextDotCount > 20) m_loadingTextDotCount = 1;
                this.Invalidate();

                Thread.Sleep(1000);
            }
        }

        public void StopLoading()
        {
            m_bLoadingState = false;
            GraphicEngine.UnRegisterThread(m_loadingTextThread);
        }

        protected override void XPanel_OnPaint(object sender, PaintEventArgs e)
        {
            centerLoadImage();
            if (m_bWithBorder)
            {
                using (Pen p = new Pen(SharedResource.BORDER_COLOR))
                {
                    p.DashStyle = DashStyle.Solid;
                    using (SolidBrush b = new SolidBrush(SharedResource.BACKGROUND_COLOR)) e.Graphics.FillRectangle(b, 0, 0, Width - 1, Height - 1);
                    e.Graphics.DrawRectangle(p, new Rectangle(0, 0, Width - 1, Height - 1));
                }
            }


            using (SolidBrush Stringbrush = new SolidBrush(SharedResource.LOADING_TEXT_COLOR))
            {
                String finalTextDisplayable = m_strDisplayableText;
                float StringHeight = e.Graphics.MeasureString(finalTextDisplayable, SharedResource.LOADING_TEXT_FONT).Height;
                float StringWidth = e.Graphics.MeasureString(finalTextDisplayable, SharedResource.LOADING_TEXT_FONT).Width;


                e.Graphics.DrawString(finalTextDisplayable, SharedResource.LOADING_TEXT_FONT, Stringbrush,
                                      (this.Width / 2) - (StringWidth / 2), (this.Height / 2) - (StringHeight / 2)+13);

                finalTextDisplayable = StringHelper.repeat(".", m_loadingTextDotCount);
                StringHeight = e.Graphics.MeasureString(finalTextDisplayable, SharedResource.LOADING_TEXT_FONT).Height;
                StringWidth = e.Graphics.MeasureString(finalTextDisplayable, SharedResource.LOADING_TEXT_FONT).Width;

                e.Graphics.DrawString(finalTextDisplayable, SharedResource.LOADING_TEXT_FONT, Stringbrush,
                      (this.Width / 2) - (StringWidth / 2), (this.Height / 2) - (StringHeight / 2) + 9 + StringHeight);
            }
        }

        private PictureBox constructLoadImage()
        {
            PictureBox LoadImage = new PictureBox();
            LoadImage.Size = new Size(100, 100);
            LoadImage.Image = GraphicDLL.Properties.Resources.loading;
            LoadImage.BackColor = Color.Transparent;
            LoadImage.SizeMode = PictureBoxSizeMode.Zoom;
            LoadImage.Enabled = true;
            return LoadImage;
        }

        private void centerLoadImage()
        {

            int StringHeight = (int)this.CreateGraphics().MeasureString(m_strDisplayableText,SharedResource.LOADING_TEXT_FONT).Height + 32;
            int Xpos = (this.Width / 2) - (m_LoadingIcon.Width / 2);
            int ypos = (this.Height / 2) - ((m_LoadingIcon.Height + StringHeight) / 2);
            m_LoadingIcon.Location = new Point(Xpos, ypos);
        }

        public void setDisplayableText(String DisplayableText)
        {
            m_strDisplayableText = DisplayableText;
           // m_loadingTextDotCount = 1;
        }
    }
}
