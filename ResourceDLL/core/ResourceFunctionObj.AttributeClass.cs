using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ResourceDLL.core
{
        [AttributeUsage(AttributeTargets.Method)]
        public class ResouceXmlFunctionAttribute : System.Attribute
        {

            public static List<MethodInfo> getRegisteredFunction()
            {
                List<MethodInfo> RegisteredMethod = new List<MethodInfo>();

                    foreach (MethodInfo method in typeof(ResourceFunctionObj).GetMethods())
                    {
                        if (isXmlFunction(method))
                            RegisteredMethod.Add(method);
                    }

                return RegisteredMethod;
            }

            private static Boolean isXmlFunction(MethodInfo method)
            {
                foreach (object attribute in method.GetCustomAttributes(true))
                {
                    if (attribute is ResouceXmlFunctionAttribute)
                    {
                        return true;
                    }
                }

                return false;
            }
        }


        [AttributeUsage(AttributeTargets.Method)]
        public class ResourceXmlActionAttribute : System.Attribute
        {
            private String m_strActionName = null;
            public  String getActionName() { return m_strActionName; }
            public ResourceXmlActionAttribute(String strActionName)
            {
                m_strActionName = strActionName;
            }

            public static List<ResourceXmlActionAttribute> getRegisteredAction()
            {
                List<ResourceXmlActionAttribute> RegisteredAction = new List<ResourceXmlActionAttribute>();

                foreach (MethodInfo method in typeof(ResourceFunctionObj).GetMethods())
                {
                    ResourceXmlActionAttribute xmlAction = null;
                    if ((xmlAction=getActionInfo(method)) != null)
                        RegisteredAction.Add(xmlAction);
                }

                return RegisteredAction;
            }

            private static ResourceXmlActionAttribute getActionInfo(MethodInfo method)
            {
                foreach (object attribute in method.GetCustomAttributes(true))
                    if (attribute is ResourceXmlActionAttribute)
                        return attribute as ResourceXmlActionAttribute;

                return null;
            }

            public static List<MethodInfo> getMethodRegisteredWithAction(ResourceXmlActionAttribute action)
            {
                List<MethodInfo> methodList = new List<MethodInfo>();

                foreach(MethodInfo method in typeof(ResourceFunctionObj).GetMethods())
                {
                    Boolean bFoundAttribute = false;
                    foreach (Attribute attribute in method.GetCustomAttributes(true))
                    {
                        if (attribute is ResourceXmlActionAttribute &&
                          (attribute as ResourceXmlActionAttribute).getActionName().Equals(action.getActionName()))
                        { bFoundAttribute = true; break; }
                    }

                    if (bFoundAttribute)
                        methodList.Add(method);
                }

                return methodList;
            }
        }

        public struct MethodObj
        {
            public MethodInfo method;
            public String parameters;

            public MethodObj(MethodInfo method, String parameters)
            {
                this.method = method;
                this.parameters = parameters;
            }
        }

}
