using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ActionDLL
{
    public abstract class ProcActionType
    {
        public abstract String getNameType();

        public static void readAction(String ActionType){
            foreach(Type cls in Assembly.GetExecutingAssembly().GetTypes()){

            }
        }
    }
}
