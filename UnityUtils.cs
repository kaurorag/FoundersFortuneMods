using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine.UI;
using UnityEngine;
using System.Reflection;
using System.Xml;
using UnityEngine.EventSystems;

[Flags]
public enum UnityObjectLogType : int
{
    None = 0,
    LogChildren = 1,
    LogChildrenComponents = 2,
    LogComponents = 4,
    Fields = 8,
    Properties = 16,
    InheritedFieldsProperties = 32,

    ALL = LogChildren | LogChildrenComponents | LogComponents | Fields | Properties | InheritedFieldsProperties
}

public static class UnityUtils
{
    public static void LogUnityObject(GameObject obj, UnityObjectLogType logType = UnityObjectLogType.ALL)
    {
        XmlDocument doc = new XmlDocument();
        LogUnityObjectProps(obj, doc, doc, logType, new List<object>());

        doc.Save(@"C:\temp\gameobject.xml");
    }

    private static void LogUnityObjectProps(GameObject obj, XmlNode parentNode, XmlDocument doc, UnityObjectLogType logType, List<System.Object> loggedObjs, bool parentIsContainer = false)
    {
        if (loggedObjs.Contains(obj))
        {
            parentNode.InnerText = "Referenced" + obj.ToString();
            return;
        }

        loggedObjs.Add(obj);

        XmlNode objNode = parentIsContainer ? parentNode : parentNode.AppendChild(doc.CreateElement("GameObject"));

        objNode.Attributes.Append(doc.CreateAttribute("name")).Value = obj.name;
        objNode.Attributes.Append(doc.CreateAttribute("active")).Value = obj.activeInHierarchy.ToString();

        if (logType.HasFlag(UnityObjectLogType.LogComponents))
        {
            XmlNode cNode = objNode.AppendChild(doc.CreateElement("Components"));
            cNode.Attributes.Append(doc.CreateAttribute("count")).Value = obj.GetComponents<Component>().Length.ToString();

            foreach (Component c in obj.GetComponents<Component>())
            {
                LogTypedComponent(c, c.GetType(), cNode, doc, loggedObjs, logType);
            }

        }

        if (logType.HasFlag(UnityObjectLogType.LogChildren))
        {
            XmlNode cNode = objNode.AppendChild(doc.CreateElement("Children"));
            cNode.Attributes.Append(doc.CreateAttribute("count")).Value = obj.transform.childCount.ToString();

            for (int i = 0; i < obj.transform.childCount; i++)
            {
                GameObject child = obj.transform.GetChild(i).gameObject;
                LogUnityObjectProps(child, cNode, doc, logType, loggedObjs);
            }

        }
    }

    public static void LogComponent<T>(T component, UnityObjectLogType logType = UnityObjectLogType.None)
    where T : Component
    {
        XmlDocument doc = new XmlDocument();

        LogTypedComponent(component, component.GetType(), doc, doc, new List<object>(), logType);

        doc.Save(@"C:\temp\component.xml");
    }

    private static void LogTypedComponent(Component component, Type type, XmlNode parentNode, XmlDocument doc, List<System.Object> loggedObjs, UnityObjectLogType logType, bool parentIsContainer = false, bool isFirstPass = true)
    {
        if (type.In<Type>(typeof(Transform), typeof(CanvasRenderer), typeof(UIBehaviour), typeof(MonoBehaviour), typeof(Graphics)))
            return;

        XmlNode cNode = parentIsContainer ? parentNode : parentNode.AppendChild(doc.CreateElement(type.Name));

        if (type.BaseType != typeof(Component) && typeof(Component).IsAssignableFrom(type.BaseType))
        {
            LogTypedComponent(component, type.BaseType, cNode, doc, loggedObjs, logType, false);
        }

        if (type == typeof(RectTransform))
        {
            RectTransform rect = (RectTransform)component;
            cNode.Attributes.Append(doc.CreateAttribute("rect")).Value = rect.rect.ToString();

            cNode.AppendChild(doc.CreateElement("anchoredPosition")).InnerText = rect.anchoredPosition.ToString();
            cNode.AppendChild(doc.CreateElement("sizeDelta")).InnerText = rect.sizeDelta.ToString();
            cNode.AppendChild(doc.CreateElement("anchorMin")).InnerText = rect.anchorMin.ToString();
            cNode.AppendChild(doc.CreateElement("anchorMax")).InnerText = rect.anchorMax.ToString();
            cNode.AppendChild(doc.CreateElement("pivot")).InnerText = rect.pivot.ToString();
        }
        else
        {
            if (type.BaseType == typeof(Graphic))
            {
                cNode.AppendChild(doc.CreateElement("color")).InnerText = ((Graphic)component).color.ToString();
            }

            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

            Dictionary<String, bool> fieldsPropsName = type.GetFields(flags)
                    .Select(x => new Tuple<String, bool>(x.Name, true))
                    .Union(type.GetProperties(flags)
                    .Select(x => new Tuple<String, bool>(x.Name, false))).ToDictionary(x => x.Item1, x => x.Item2);


            foreach (var namePair in fieldsPropsName.OrderBy(x => x.Key))
            {
                System.Object value = null;

                if (namePair.Value)
                    value = type.GetField(namePair.Key, flags).GetValue(component);
                else
                    value = type.GetProperty(namePair.Key, flags).GetValue(component);

                XmlNode childNode = cNode.AppendChild(doc.CreateElement(namePair.Key));

                if (value == null)
                    childNode.InnerText = "null";
                else if (value.GetType() == typeof(ColorBlock) || value.GetType() == typeof(SpriteState))
                    LogFieldsAndPropertiesForValueType(value, childNode, doc);               
                else
                    childNode.InnerText = value == null ? "null" : value.ToString();
            }
        }
    }

    private static void LogFieldsAndPropertiesForValueType(System.Object obj, XmlNode node, XmlDocument doc)
    {

        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        Type type = obj.GetType();

        Dictionary<String, bool> fieldsPropsName = type.GetFields(flags)
                .Select(x => new Tuple<String, bool>(x.Name, true))
                .Union(type.GetProperties(flags)
                .Select(x => new Tuple<String, bool>(x.Name, false))).ToDictionary(x => x.Item1, x => x.Item2);

        foreach (var namePair in fieldsPropsName.OrderBy(x => x.Key))
        {
            System.Object value = null;

            if (namePair.Value)
                value = type.GetField(namePair.Key, flags).GetValue(obj);
            else
            {
                PropertyInfo p = type.GetProperty(namePair.Key, flags);
                if (p.GetIndexParameters().Length != 0)
                    continue;

                value = p.GetValue(obj);
            }

            XmlNode childNode = node.AppendChild(doc.CreateElement(namePair.Key));

            childNode.InnerText = value == null ? "null" : value.ToString();
        }
    }

    private static bool In<T>(this T obj, params T[] objs)
    {
        return objs.Contains(obj);
    }
}

public class LogIndenter : IDisposable
{
    private String _Log = String.Empty;

    public static bool InError = false;

    public LogIndenter(String log)
    {
        _Log = log;

        FileLog.Log(_Log);
        FileLog.Log("{");
        FileLog.indentLevel++;
    }

    public void Dispose()
    {
        if (!InError)
        {
            FileLog.indentLevel--;
            FileLog.Log("}");
        }
    }
}
