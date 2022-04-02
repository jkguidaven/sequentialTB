using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GraphicDLL.common
{
    public abstract class SharedResource
    {
        public static readonly Font LIFELINE_HEAD_FONT = new Font("Calibri", 12, FontStyle.Bold);
        public static readonly Font LOADING_TEXT_FONT = new Font("Arial", 10, FontStyle.Regular);
        public static readonly Font EVENT_TEXT_FONT = new Font("Calibri",8, FontStyle.Regular);

        public static readonly int LIFELINE_HEAD_MARGIN = 20;
        public static readonly int LIFELINE_HEAD_MARGIN2 = 15;

        public static readonly int LIFELINE_TOP_GAP = 18;
        public static readonly int LIFELINE_MIN_WIDTH = 100;
        public static readonly int ALLOWANCE_TIMESTAMP_BOT = 50;

        public static readonly Color BORDER_COLOR               = Color.FromArgb(130, 130, 130);
        public static readonly Color HEADER_TOP_COLOR           = Color.FromArgb(240, 240, 240);
        public static readonly Color HEADER_BOT_COLOR           = Color.FromArgb(195, 195, 195);
        public static readonly Color ALT_BACKGROUND_COLOR       = Color.FromArgb(236, 235, 230);
        public static readonly Color ALT_BACKGROUND_COLOR2      = Color.FromArgb(221, 221, 221);
        public static readonly Color BACKGROUND_COLOR           = Color.White; 
        public static readonly Color LIFELINE_HEAD_STROKE_COLOR = Color.Black;
        public static readonly Color LOADING_TEXT_COLOR         = Color.Black;
        public static readonly Color LIFELINE_STROKE_COLOR      = Color.Black;
        public static readonly Color LIFELINE_HEAD_COLOR        = Color.FromArgb(0, 162, 232);
        public static readonly Color LIFELINE_HEAD_COLOR_ALT    = Color.FromArgb(255, 242, 0);
        public static readonly Color LIFELINE_HEAD_COLOR_ALT2   = Color.FromArgb(237, 28, 36);
        public static readonly Color LIFELINE_HEAD_COLOR_SHADOW = Color.FromArgb(166, 228, 255);
        public static readonly Color TIMESTAMP_SLICE_LINE_COLOR = Color.FromArgb(234, 234, 234);
        public static readonly Color TIMESTAMP_SLICE_LINE_COLOR2 = Color.FromArgb(167, 167, 167);
        public static readonly Color TIMESTAMP_SLICE_ALT_COLOR  = Color.FromArgb(240, 240, 240);
        public static readonly Color TIMESTAMP_TEXT_COLOR       = Color.FromArgb(0, 162, 232);


        public static readonly String FILTER_HEADER_TITLE = "Filtering Options";
        public static readonly String ACITIVITY_HEADER_TITLE = "Assist Tool";
        public static readonly String LOGLOADER_TITLE = "Diags Path";


        public static readonly Font FILTER_HEADER_FONT  = new Font("Arial", 10, FontStyle.Bold);

        public static readonly int FILTER_HEADER_HEIGHT = 30;
        public static readonly int MARGIN_SIZE          = 5;
        public static readonly int TIP_TOLERANCE_POINT = 3;
        public static readonly int LOGLOADER_WIDTH = 300;
        public static readonly int LOGLOADER_HEIGHT = 150;


        public static readonly int EVENT_START_YPOINT = 20;
        public static readonly int EVENT_OBJ_HEIGHT = 20;
        public static readonly int EVENT_OBJ_WIDTH = 11;
        public static readonly int EVENT_GRID_WIDTH = 20;
    }
}
