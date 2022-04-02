using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceDLL.worker;

namespace ParseEngine
{
    class DataFeedWorker : ResourceWorker 
    {
        public DataFeedWorker(ParseEngineController clsPEController)
            : base(clsPEController)
        {
        }
    }
}
