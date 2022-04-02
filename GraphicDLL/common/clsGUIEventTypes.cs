using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphicDLL.common
{
    public abstract class clsGUIEventTypes
    {

        public enum HotKeyEventID : int
        {
            FIND_ID = 0,
            GOTO_ID,
            KEY_DOWN_ID,
            KEY_UP_ID
        }
    }
}
