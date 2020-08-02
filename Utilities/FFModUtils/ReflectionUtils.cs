using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace FFModUtils
{
    public static class ReflectionUtils
    {
        public static T GetPropertyValue<T>(this object instance, string name) {
            object value = AccessTools.Property(instance.GetType(), name).GetValue(instance);
            if (value == null) return default(T);
            return (T)value;
        }

        public static T GetFieldValue<T>(this object instance, string name) {
            object value = AccessTools.Field(instance.GetType(), name).GetValue(instance);
            if (value == null) return default(T);
            return (T)value;
        }

        public static void SetPropertyValue(this object instance, string name, object value) {
            AccessTools.Property(instance.GetType(), name).SetValue(instance, value);
        }

        public static void SetFieldValue(this object instance, string name, object value) {
            AccessTools.Field(instance.GetType(), name).SetValue(instance, value);
        }

        public static void InvokeMethod(this object instance, string name, params object[] args) {
            if (args == null) args = new object[] { };
            AccessTools.Method(instance.GetType(), name, args.Select(x => x.GetType()).ToArray()).Invoke(instance, args);
        }

        public static T InvokeFunction<T>(this object instance, string name, params object[] args) {
            if (args == null) args = new object[] { };

            object result = AccessTools.Method(instance.GetType(), name, args.Select(x => x.GetType()).ToArray()).Invoke(instance, args);
            if (result == null) return default(T);
            return (T)result;
        }
    }
}
