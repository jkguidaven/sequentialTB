using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GraphicDLL.common;
using System.Drawing.Drawing2D;
using GraphicDLL.XControls;
using System.Threading;

namespace GraphicDLL.XPanels
{
    class XPanel_LoadLogPane : XPanel
    {
        private XControl_FilePathLoader filePathLoader = null;
        public XPanel_LoadLogPane(iPageRequest request) : base(request,request.getTabPage()) 
        {
        }

        protected override void XPanel_OnPreInitialize()
        {

            filePathLoader = new XControl_FilePathLoader(m_IPRequest,this,SharedResource.LOGLOADER_TITLE);
            this.Controls.Add(filePathLoader);
            AnchorTo(AnchorStyle.RIGHT, AnchorStyle.RIGHT);
            AnchorTo(AnchorStyle.BOTTOM, AnchorStyle.BOTTOM);
            margin(5);

            filePathLoader.getLoadButton().MouseClick += new MouseEventHandler(XPanel_OnLoadClick);
        }

        protected override void XPanel_OnPostInitialize()
        {

        }

        protected void XPanel_OnLoadClick(object sender, MouseEventArgs e)
        {
            if (!m_IPRequest.STBEventHandler_OnLoadLogs(filePathLoader.getPathValue(), filePathLoader.includeBAK()))
            {
                filePathLoader.ReleaseLoadingState();
            }
        }

        protected override void XPanel_OnPaint(object sender, PaintEventArgs e)
        {
            int LOGLOADER_X = this.Width / 2 - SharedResource.LOGLOADER_WIDTH / 2;
            int LOGLOADER_Y = this.Height / 2 - SharedResource.LOGLOADER_HEIGHT / 2;
            filePathLoader.Location = new Point(LOGLOADER_X, LOGLOADER_Y);

        }


        public void setDisplayableText(String DisplayableText)
        {
            foreach (Control ptrControl in filePathLoader.m_ctrlPanel.Controls)
            {
                if (ptrControl is XPanel_LoadingPane)
                {
                    (ptrControl as XPanel_LoadingPane).setDisplayableText(DisplayableText);
                    ptrControl.Refresh();
                    break;
                }
            }
        }

        public void StopLoading()
        {
            foreach (Control ptrControl in filePathLoader.m_ctrlPanel.Controls)
            {
                if (ptrControl is XPanel_LoadLogPane){
                    (ptrControl as XPanel_LoadingPane).StopLoading();
                    break;
                }
            }
        }
    }
}
