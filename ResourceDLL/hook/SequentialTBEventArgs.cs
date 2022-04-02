using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ResourceDLL.hook
{
    public class SequentialTBEventArgs : EventArgs
    {
        public int EventCode { get; set; }


        public SequentialTBEventArgs(int code)
        {
            EventCode = code;
        }
    }
}
