using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GraphicDLL.common;
using System.Drawing;
using System.Drawing.Drawing2D;
using GraphicDLL.XPanels;
using System.IO;

namespace GraphicDLL.XControls
{
    class XControl_FilePathLoader : XControl
    {
        private String  m_strTitle   = "";
        private Button  m_btnLoad    = null;
        private TextBox m_txtPath    = null;
        private Label   m_lblPath    = null;
        private Label   m_lblBAKInclude = null;
        public  Control m_ctrlPanel     = null;
        public  CheckBox m_chkBAKInclude = null;
        private Boolean bIsPositionOnce = false;
        public XControl_FilePathLoader(iPageRequest request,Control holder,String strTitle)
            : base(request,holder,strTitle)
        {
        }


        public void ReleaseLoadingState()
        {
            m_ctrlPanel.Controls.Clear();
            AddMainControlToPanel();
        }

        private void AddMainControlToPanel()
        {
            m_ctrlPanel.Controls.Add(m_lblPath);
            m_ctrlPanel.Controls.Add(m_txtPath);
            m_ctrlPanel.Controls.Add(m_btnLoad);
            m_ctrlPanel.Controls.Add(m_lblBAKInclude);
            m_ctrlPanel.Controls.Add(m_chkBAKInclude);
            if (!bIsPositionOnce)
            {
                m_txtPath.Width = m_ctrlPanel.Width - 130;
                m_lblPath.Location = new Point(10, (m_ctrlPanel.Height / 2) - m_lblPath.Height - 5);

                m_txtPath.Location = new Point(m_ctrlPanel.Width - m_txtPath.Width - 70,
                                               (m_ctrlPanel.Height / 2) - m_txtPath.Height - 10);

                m_lblBAKInclude.Location = new Point(10 + m_txtPath.Width+ 50,
                                                     (m_ctrlPanel.Height / 2) - m_lblPath.Height - 5);
                m_chkBAKInclude.Location = new Point(m_lblBAKInclude.Location.X + m_lblBAKInclude.Width,
                                                     m_lblBAKInclude.Location.Y-5);
                m_btnLoad.Location = new Point(m_ctrlPanel.Width / 2 - m_btnLoad.Width / 2,
                                               m_ctrlPanel.Height / 2 + m_btnLoad.Height / 2);
                bIsPositionOnce = true;
            }
        }

        protected override void XControl_OnInitialize(params object[] data)
        {
            m_ctrlPanel = new Control();
            m_ctrlPanel.BackColor = Color.White;
            m_strTitle = data[0] as String;
            m_btnLoad = new Button();
            m_txtPath = new TextBox();
            m_lblPath = new Label();
            m_lblBAKInclude = new Label();
            m_lblBAKInclude.Text = "+BAK: ";
            m_lblBAKInclude.Width = 40;
            m_chkBAKInclude = new CheckBox();
            m_lblPath.Text = "Path:";
            m_lblPath.Width = 30;
            m_btnLoad.Text = "Load";
            m_btnLoad.MouseClick += new MouseEventHandler(XControl_OnLoadClick);
            m_txtPath.MouseClick += new MouseEventHandler(XControl_OnLoadClick);
            this.Controls.Add(m_ctrlPanel);

        }

        protected override void XControl_OnResize(object sender, EventArgs e)
        {
            this.Width = SharedResource.LOGLOADER_WIDTH;
            this.Height = SharedResource.LOGLOADER_HEIGHT;
            m_ctrlPanel.Width = this.Width - 3;
            m_ctrlPanel.Height = this.Height - SharedResource.FILTER_HEADER_HEIGHT - 3;
            m_ctrlPanel.Location = new Point(2, SharedResource.FILTER_HEADER_HEIGHT + 2);
            AddMainControlToPanel();

        }

        protected override void XControl_OnPaint(object sender, PaintEventArgs e)
        {
            int LOGLOADER_X = 0;
            int LOGLOADER_Y = 0;
            Rectangle HeaderRec = new Rectangle(LOGLOADER_X, LOGLOADER_Y,
                                                SharedResource.LOGLOADER_WIDTH,
                                                SharedResource.FILTER_HEADER_HEIGHT);

            using (Brush brush = new SolidBrush(Color.White))
            {
                e.Graphics.FillRectangle(brush, 0, 0,
                                                SharedResource.LOGLOADER_WIDTH,
                                                SharedResource.LOGLOADER_HEIGHT);
            }

            using (LinearGradientBrush brush = new LinearGradientBrush(HeaderRec,
                                   SharedResource.HEADER_TOP_COLOR, SharedResource.HEADER_BOT_COLOR,
                                               90F, true))
            {
                e.Graphics.FillRectangle(brush, HeaderRec);
            }

            using (Pen p = new Pen(SharedResource.BORDER_COLOR))
            {
                p.DashStyle = DashStyle.Solid;
                e.Graphics.DrawRectangle(p, new Rectangle(0, 0, Width - 1, Height - 1));


                e.Graphics.DrawRectangle(p, new Rectangle(LOGLOADER_X,
                                                          LOGLOADER_Y,
                                                          SharedResource.LOGLOADER_WIDTH,
                                                          SharedResource.LOGLOADER_HEIGHT));


                e.Graphics.DrawLine(p, LOGLOADER_X,
                                       LOGLOADER_Y + SharedResource.FILTER_HEADER_HEIGHT,
                                       LOGLOADER_X + SharedResource.LOGLOADER_WIDTH,
                                       LOGLOADER_Y + SharedResource.FILTER_HEADER_HEIGHT);

                p.Color = Color.White;
                e.Graphics.DrawLine(p, LOGLOADER_X + 1, LOGLOADER_Y + 1,
                                    LOGLOADER_X + 1, LOGLOADER_Y + SharedResource.FILTER_HEADER_HEIGHT - 1);
                e.Graphics.DrawLine(p, LOGLOADER_X + 1, LOGLOADER_Y + 1,
                                        LOGLOADER_X + SharedResource.LOGLOADER_WIDTH - 2, LOGLOADER_Y + 1);
            }

            SolidBrush Stringbrush = new SolidBrush(Color.Gray);
            String FINAL_HEADER_TITLE = m_strTitle;
            float StringHeight = e.Graphics.MeasureString(FINAL_HEADER_TITLE, SharedResource.FILTER_HEADER_FONT).Height;
            float StringWidth = e.Graphics.MeasureString(FINAL_HEADER_TITLE, SharedResource.FILTER_HEADER_FONT).Width;

            Stringbrush.Color = Color.White;
            e.Graphics.DrawString(FINAL_HEADER_TITLE, SharedResource.FILTER_HEADER_FONT, Stringbrush,
                                LOGLOADER_X + SharedResource.MARGIN_SIZE,
                                LOGLOADER_Y + (SharedResource.FILTER_HEADER_HEIGHT / 2) - (StringHeight / 2));
            Stringbrush.Color = Color.Gray;
            e.Graphics.DrawString(FINAL_HEADER_TITLE, SharedResource.FILTER_HEADER_FONT, Stringbrush,
                                            LOGLOADER_X + SharedResource.MARGIN_SIZE - 1,
                                            LOGLOADER_Y + (SharedResource.FILTER_HEADER_HEIGHT / 2) - (StringHeight / 2));

            Stringbrush.Dispose();
        }



        protected void XControl_OnLoadClick(object sender, EventArgs e)
        {
            if (sender is Button)
            {
                    m_ctrlPanel.Controls.Clear();
                    new XPanel_LoadingPane(m_IPRequest,m_ctrlPanel);
            }
            else
            {
                string folderpath = "";
                FolderBrowserDialog fbd=new FolderBrowserDialog();

                fbd.SelectedPath =  m_txtPath.Text;


                DialogResult dr=fbd.ShowDialog();

                if (dr == DialogResult.OK)
                {
                     folderpath=fbd.SelectedPath;
                }

                m_txtPath.Text = folderpath;
            }
        }

        public Button getLoadButton() { return m_btnLoad;       }
        public String getPathValue()  { return m_txtPath.Text;  }

        public Boolean includeBAK() { return m_chkBAKInclude.Checked; }
    }
}
