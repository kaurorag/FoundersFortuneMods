#if !MODKIT
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

public static class DebugLogger
{
    private const String PATH = @"C:\temp\logs.txt";

    static DebugLogger()
    {
#if DEBUG
        if (File.Exists(PATH)) File.Delete(PATH);
#endif
    }

    public static void Log(String name, object obj)
    {
        Log($"{name}: {obj}");
    }


    public static void LogObj(String name, object obj, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
    {
#if DEBUG
        using (FileStream fs = new FileStream(PATH, FileMode.Append, FileAccess.Write))
        using (StreamWriter writer = new StreamWriter(fs))
        {
            Log(name, obj, writer, "", flags);
        }
#endif
    }

    private static void Log(String name, object obj, StreamWriter writer, String indent, BindingFlags flags)
    {
        writer.WriteLine($"{name}({obj?.GetType()}): {obj}");

        if (obj != null)
        {
            Type type = obj.GetType();

            foreach (var member in (type.GetFields(flags)
                .Select(x => new { Name = x.Name, Type = x.FieldType, Value = (x.IsStatic ? x.GetValue(null) : x.GetValue(obj)) }))
                .Union(
                type.GetProperties(flags)
                .Select(x => new { Name = x.Name, Type = x.PropertyType, Value = (x.GetMethod.IsStatic ? x.GetValue(null) : x.GetValue(obj)) }))
                .OrderBy(x => x.Name))
            {
                writer.WriteLine($"\t{member.Name}({member.Type}): {member.Value}");
            }
        }
    }

    public static void Log(Exception ex)
    {
#if DEBUG
        Log(ex.ToString());
#endif
    }

    private static object LogLock = new object();
    public static void Log(String text)
    {
#if DEBUG
        lock (LogLock) {
            using (FileStream fs = new FileStream(PATH, FileMode.Append, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(fs))
                writer.WriteLine(text);
        }
#endif
    }

    public static void LogStackTrace()
    {
#if DEBUG
        Log(new StackTrace(true).ToString());
#endif
    }

    public static void LogUnityStrackTrace()
    {
#if DEBUG
        Log(StackTraceUtility.ExtractStackTrace());
#endif
    }
}

public static class UnityUtils
{
    private static Type[] IgnoredTypes = new Type[] { typeof(Component), typeof(Behaviour), typeof(Transform), typeof(CanvasRenderer), typeof(UIBehaviour), typeof(MonoBehaviour) };

    public static void LogUnityObject(GameObject obj, UnityObjectLogType logType = UnityObjectLogType.ALL)
    {
#if DEBUG
        XmlDocument doc = new XmlDocument();
        LogUnityObjectProps(obj, doc, doc, logType);

        doc.Save(@"C:\temp\gameobject.xml");
#endif
    }

    private static void LogUnityObjectProps(GameObject obj, XmlNode parentNode, XmlDocument doc, UnityObjectLogType logType)
    {
        XmlNode objNode = parentNode.AppendChild(doc.CreateElement("GameObject"));

        objNode.Attributes.Append(doc.CreateAttribute("name")).Value = obj.name;
        objNode.Attributes.Append(doc.CreateAttribute("active")).Value = obj.activeInHierarchy.ToString();

        if (logType.HasFlag(UnityObjectLogType.LogComponents))
        {
            XmlNode cNode = objNode.AppendChild(doc.CreateElement("Components"));
            cNode.Attributes.Append(doc.CreateAttribute("count")).Value = obj.GetComponents<Component>().Length.ToString();

            foreach (Component c in obj.GetComponents<Component>())
            {
                LogTypedComponent(c, c.GetType(), cNode, doc, logType);
            }

        }

        if (logType.HasFlag(UnityObjectLogType.LogChildren))
        {
            XmlNode cNode = objNode.AppendChild(doc.CreateElement("Children"));
            cNode.Attributes.Append(doc.CreateAttribute("count")).Value = obj.transform.childCount.ToString();

            for (int i = 0; i < obj.transform.childCount; i++)
            {
                GameObject child = obj.transform.GetChild(i).gameObject;
                LogUnityObjectProps(child, cNode, doc, logType);
            }

        }
    }

    public static void LogComponent<T>(T component, UnityObjectLogType logType = UnityObjectLogType.None)
    where T : Component
    {
#if DEBUG
        XmlDocument doc = new XmlDocument();

        LogTypedComponent(component, component.GetType(), doc, doc, logType);

        doc.Save(@"C:\temp\component.xml");
#endif
    }

    private static void LogTypedComponent(Component component, Type type, XmlNode parentNode, XmlDocument doc, UnityObjectLogType logType, bool isFirst =true)
    {
        if (IgnoredTypes.Contains(type)) return;

        XmlNode cNode = parentNode.AppendChild(doc.CreateElement(type.Name));

        if (isFirst && typeof(Behaviour).IsAssignableFrom(type))
            cNode.Attributes.Append(doc.CreateAttribute("Enabled")).Value = ((Behaviour)component).enabled.ToString();

        LogTypedComponent(component, type.BaseType, cNode, doc, logType, false);

        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic;


        foreach (var member in (type.GetFields(flags).Where(x => !x.IsInitOnly && (x.IsPublic || x.GetCustomAttribute<SerializeField>() != null))
                .Select(x => new { x.Name, Type = x.FieldType, Value = (x.IsStatic ? x.GetValue(null) : x.GetValue(component)) }))
                .Union(
                type.GetProperties(flags).Where(x => x.SetMethod != null && x.SetMethod.IsPublic && x.GetIndexParameters().Length == 0)
                .Select(x => new { x.Name, Type = x.PropertyType, Value = (x.GetMethod.IsStatic ? x.GetValue(null) : x.GetValue(component)) }))
                .OrderBy(x => x.Name))
        {
            XmlNode childNode = cNode.AppendChild(doc.CreateElement(member.Name));

            if (member.Value != null && typeof(Component).IsAssignableFrom(member.Type) && ((Component)member.Value).gameObject.transform == null)
            {
                childNode.Attributes.Append(doc.CreateAttribute("IsPrefabTemplate")).Value = "True";
                LogUnityObjectProps((GameObject)((Component)member.Value).gameObject, childNode, doc, logType);
            }
            else if (member.Value != null && (member.Type == typeof(ColorBlock) || member.Type == typeof(SpriteState)))
                LogFieldsAndPropertiesForValueType(member.Value, childNode, doc);
            else
                childNode.InnerText = $"{member.Value}";
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

#endif