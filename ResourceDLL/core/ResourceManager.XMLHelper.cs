using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceDLL.worker;
using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using CoreDLL.logworker;
using ResourceDLL.core;

namespace ResourceDLL.Manager
{

    public partial class ResourceManager 
    {
        public Boolean LoadingXML(iResourceXMLWorker clsXMLWorker)
        {
            trace("+loadingXML: " + clsXMLWorker.getXML());
            XMLBlueprint blueprint = clsXMLWorker.getXMLStructure();

            using (XmlReader reader = XmlReader.Create(clsXMLWorker.getXML())) // start reading xml file 
            {
                try
                {
                    foreach (FieldInfo clsFieldInfo in blueprint.GetType().GetFields())
                    {
                        if (clsFieldInfo.GetValue(blueprint) is XMLNode)
                        {
                            XMLNode clsXMLNode = (XMLNode)clsFieldInfo.GetValue(blueprint);
                            processXMLNode(clsXMLNode, reader);
                        }
                    }


                    blueprint.ProcessBluePrint(this);
                }
                catch (XmlException xmlEx)
                {
                    // catches all thrown XmlException and generate a trace for debugging
                    IXmlLineInfo xmlInfo = (IXmlLineInfo)reader;
                    int lineNumber = xmlInfo.LineNumber;
                    trace(L2, "Exception object line, pos: (" + xmlEx.LineNumber + "," + xmlEx.LinePosition + ")");
                    trace(L2, "Exception Description: " + xmlEx.Message);
                    trace(L2, "Exception Discovery: XmlFile=[" + clsXMLWorker.getXML() + "];lineNumber=" + lineNumber);
                    trace("-loadingXML: " + clsXMLWorker.getXML() + "|Error");
                    return false;
                }
            }


            trace("-loadingXML: " + clsXMLWorker.getXML());
            return true;
        }

        private  XMLNode processXMLNode(XMLNode clsXMLNode, XmlReader reader)
        {
            trace("+ProcessXMLNode: " + clsXMLNode.getName());
            if (reader.ReadToFollowing(clsXMLNode.getName()))
            {
                String strOuterXML = reader.ReadOuterXml();
                if (clsXMLNode is iParameterizeXMLNode)
                {
                    List<XMLParamStruct> lstXMLParam = new List<XMLParamStruct>();
                    using (XmlReader AttributeReader = XmlReader.Create(new StringReader(strOuterXML)))
                    {
                        AttributeReader.ReadToFollowing(clsXMLNode.getName());
                        if (AttributeReader.HasAttributes)
                        {
                            while (AttributeReader.MoveToNextAttribute())
                                lstXMLParam.Add(new XMLParamStruct(AttributeReader.Name, AttributeReader.Value));
                        }
                    }

                    (clsXMLNode as iParameterizeXMLNode).processXMLNodeParameters(lstXMLParam.ToArray());

                }

                if (clsXMLNode is RootableXMLNode)
                {
                    RootableXMLNode clsRXMLNode = (RootableXMLNode)clsXMLNode;
                    List<FieldInfo> lstclsFieldInfo = new List<FieldInfo>();

                    // loop to all variable members and filter all  XMLNode or List<XMLNode> type
                    foreach (FieldInfo clsFieldInfo in clsRXMLNode.GetType().GetFields())
                    {

                        if (clsFieldInfo.GetValue(clsRXMLNode) is XMLNode)
                        {
                            lstclsFieldInfo.Add(clsFieldInfo);
                        }
                        else if (typeof(IList).IsAssignableFrom(clsFieldInfo.FieldType)) // if Identified as List
                        {

                            IList PtrList = (clsFieldInfo.GetValue(clsRXMLNode) as IList);
                            Type ListGenericType = PtrList.GetType().GetGenericArguments()[0];

                            // if typeof iXMLNode
                            if (typeof(XMLNode).IsAssignableFrom(ListGenericType))
                            {
                                lstclsFieldInfo.Add(clsFieldInfo);
                            }
                        }
                    }

                    // compare XML file to list of Expected XMLNode filtered in lstclsFieldInfo
                        using (XmlReader Loop = XmlReader.Create(new StringReader(strOuterXML)))
                        {
                            Loop.ReadToFollowing(clsXMLNode.getName());
                            while (Loop.Read())
                            {
                                if (Loop.NodeType == XmlNodeType.Element)
                                {
                                    Boolean isExpectedNode = false;
                                    foreach (FieldInfo clsFieldInfo in lstclsFieldInfo)
                                    {

                                        if (typeof(IList).IsAssignableFrom(clsFieldInfo.FieldType)) // if Identified as List
                                        {

                                            IList PtrList = (clsFieldInfo.GetValue(clsRXMLNode) as IList);
                                            Type ListGenericType = PtrList.GetType().GetGenericArguments()[0];
                                            String strNodeName = (Activator.CreateInstance(ListGenericType) as XMLNode).getName();

                                            if(Loop.Name.Equals(strNodeName)){
                                                isExpectedNode = true;
                                                XMLNode node = Activator.CreateInstance(ListGenericType) as XMLNode; node.setParent(clsRXMLNode);
                                                node.setParent(clsRXMLNode);
                                                node = processXMLNode(node, XmlReader.Create(new StringReader(Loop.ReadOuterXml())));
                                                PtrList.Add(node);
                                            }
                                        }
                                        else
                                        {
                                            (clsFieldInfo.GetValue(clsRXMLNode) as XMLNode).setParent(clsRXMLNode);
                                            if(Loop.Name.Equals( ((XMLNode)clsFieldInfo.GetValue(clsRXMLNode)).getName()) ){
                                                isExpectedNode = true;
                                                processXMLNode((XMLNode)clsFieldInfo.GetValue(clsRXMLNode), XmlReader.Create(new StringReader((Loop.ReadOuterXml()))));
                                            }
                                        }
                                    }

                                    if (!isExpectedNode) 
                                        throw new XmlException("Unknown '" + Loop.Name + "' found in '" + clsXMLNode.getName() + "' node!");
                                }

                            }
                        }


                    XmlReader innerReader = XmlReader.Create(new StringReader(strOuterXML));
                    innerReader.Read();
                    clsRXMLNode.processXMLContent(innerReader.ReadInnerXml());
                }

            }
            else
                throw new XmlException("Enable to find Node[" + clsXMLNode.getName() + "].");

            trace("-ProcessXMLNode: " + clsXMLNode.getName());
            return clsXMLNode;
        }
    }

    public abstract class XMLBlueprint : clsResourceTrace
    {
        public XMLBlueprint() : base(L2) { }
        public abstract void ProcessBluePrint(ResourceManager clsRManager);
    }

    public abstract class XMLNode : clsResourceTrace
    {
        public XMLNode() : base(L3) { }

        protected XMLNode XMLNodeParent = null;
        public XMLNode setParent(XMLNode node)
        {
            XMLNodeParent = node;
            return this;
        }

        public abstract String getName();
    }

    public abstract class RootableXMLNode : XMLNode
    {
        public abstract void processXMLContent(String strXMLContent);
    }

    public interface iParameterizeXMLNode
    {
        void processXMLNodeParameters(params XMLParamStruct[] XPSParam);
    }

    public struct XMLParamStruct
    {
        public String m_strParam;
        public String m_strVal;

        public XMLParamStruct(String strParam, String strVal)
        {
            this.m_strParam = strParam;
            this.m_strVal = strVal;
        }
    }
}
