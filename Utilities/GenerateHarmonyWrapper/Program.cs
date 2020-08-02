using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GenerateHarmonyWrapper {
    class Program {
        static void Main(string[] args) {
            string targetNamespace = "WitchyMods.AbsoluteProfessionPriorities.Framework";
            string targetClassName = "NewYieldMicroInteraction";
            string targetDir = @"C:\temp\";
            Type sourceType = typeof(YieldMicroInteraction);

            int indent = 0;
            using (FileStream fs = new FileStream(targetDir + targetClassName + ".cs", FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(fs)) {
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine("using System.Linq;");
                writer.WriteLine("using HarmonyLib;");
                writer.WriteLine("");
                writer.WriteLine("namespace " + targetNamespace + " {"); indent++;
                writer.WriteLine("");
                writer.WriteLine($"{GetIndent(indent)}public partial class {targetClassName} : {sourceType.FullName} {{"); indent++;
                writer.WriteLine("");

                BindingFlags nonPublic = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

                //Fields
                WriteFields(sourceType, ref indent, writer, nonPublic);

                //Properties
                WriteProperties(sourceType, ref indent, writer, nonPublic);

                //Constructors
                WriteConstructors(targetClassName, sourceType, ref indent, writer);

                //Methods
                WriteMethods(sourceType, ref indent, writer); indent--;

                writer.WriteLine($"{GetIndent(indent)}}}"); indent--; //End of class
                writer.WriteLine($"{GetIndent(indent)}}}");  //End of namespace
            }
        }

        private static void WriteMethods(Type sourceType, ref int indent, StreamWriter writer) {
            foreach (var m in sourceType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
                if (m.Name.StartsWith("set_") || m.Name.StartsWith("get_") || m.Name.StartsWith("<")) continue;

                string retType = GetTypeName(m.ReturnType);
                writer.Write($"{GetIndent(indent)}public {retType} {m.Name}(");

                bool isFirst = true;
                StringBuilder sbArgs = new StringBuilder();
                StringBuilder sbArgsType = new StringBuilder();
                foreach (var p in m.GetParameters()) {
                    string typeName = GetTypeName(p.ParameterType);

                    if (!isFirst) { writer.Write(", "); sbArgs.Append(", "); sbArgsType.Append(", "); }
                    if (p.IsOut) writer.Write("out ");
                    if (p.IsRetval) writer.Write("ref ");
                    writer.Write($"{typeName} {p.Name}");
                    if (p.IsOptional) writer.Write(" = " + (p.DefaultValue == null ? "null" : p.DefaultValue.ToString()));

                    sbArgs.Append(p.Name);
                    sbArgsType.Append($"typeof({typeName})");
                    isFirst = false;
                }

                writer.WriteLine(") {"); indent++;

                //Body of method
                foreach (var pOut in m.GetParameters().Where(x => x.IsOut)) {
                    writer.WriteLine($"{GetIndent(indent)}{pOut.Name} = default({GetTypeName(pOut.ParameterType)});");
                }

                writer.WriteLine($"{GetIndent(indent)}var args = new object[]{{{sbArgs.ToString()}}};");
                writer.WriteLine($"{GetIndent(indent)}var argTypes = new Type[]{{{sbArgsType.ToString()}}};");
                writer.WriteLine("");

                if (m.ReturnType.Name != "Void") {
                    writer.Write($"{GetIndent(indent)}var result = ");
                } else { writer.Write($"{GetIndent(indent)}"); }

                writer.WriteLine($"AccessTools.Method(base.GetType(), \"{m.Name}\", argTypes).Invoke(this, args);");

                if (m.ReflectedType.Name != "Void") {
                    writer.WriteLine($"{GetIndent(indent)}return result == null ? default({GetTypeName(m.ReturnType)}) : ({GetTypeName(m.ReturnType)})result;");
                }

                //End body of method

                indent--; writer.WriteLine($"{GetIndent(indent)}}}");
                writer.WriteLine("");
            }
        }

        private static void WriteConstructors(string targetClassName, Type sourceType, ref int indent, StreamWriter writer) {
            foreach (var c in sourceType.GetConstructors()) {
                writer.Write($"{GetIndent(indent)}public {targetClassName}(");

                StringBuilder sbParameters = new StringBuilder();
                StringBuilder sbBase = new StringBuilder();

                bool isFirst = true;
                foreach (var p in c.GetParameters()) {
                    string typeName = GetTypeName(p.ParameterType);

                    if (!isFirst) { sbParameters.Append(", "); sbBase.Append(", "); }
                    if (p.IsOut) { sbParameters.Append("out "); sbBase.Append("out "); }
                    if (p.IsRetval) { sbParameters.Append("ref "); sbBase.Append("ref "); }
                    sbParameters.Append($"{typeName} {p.Name}"); sbBase.Append(p.Name);
                    if (p.IsOptional) sbParameters.Append(" = " + (p.DefaultValue == null ? "null" : p.DefaultValue.ToString()));
                    isFirst = false;
                }

                writer.WriteLine($"{sbParameters.ToString()}) : base({sbBase.ToString()}) {{");
                writer.WriteLine($"{GetIndent(indent)}}}");
                writer.WriteLine("");
            }
        }

        private static void WriteProperties(Type sourceType,ref int indent, StreamWriter writer, BindingFlags nonPublic) {
            foreach (var pi in sourceType.GetProperties(nonPublic)) {
                if (pi.Name.Contains("<")) continue;
                string typeName = GetTypeName(pi.PropertyType);

                writer.WriteLine($"{GetIndent(indent)}public {typeName} {pi.Name} {{"); indent++;
                writer.WriteLine($"{GetIndent(indent)}get {{ var value = AccessToolds.Field(this.GetType(),\"{pi.Name}\").GetValue(this); if(value==null) return default({typeName}); else return ({typeName})value; }}");
                writer.WriteLine($"{GetIndent(indent)}set {{ AccessToolds.Field(this.GetType(),\"{pi.Name}\").SetValue(this, value); }}"); indent--;
                writer.WriteLine($"{GetIndent(indent)}}}");
                writer.WriteLine("");
            }
        }

        private static void WriteFields(Type sourceType,ref int indent, StreamWriter writer, BindingFlags nonPublic) {
            foreach (var fi in sourceType.GetFields(nonPublic)) {
                if (fi.Name.Contains("<")) continue;
                string typeName = GetTypeName(fi.FieldType);

                writer.WriteLine($"{GetIndent(indent)}public {typeName} {fi.Name} {{"); indent++;
                writer.WriteLine($"{GetIndent(indent)}get {{ var value = AccessToolds.Field(this.GetType(),\"{fi.Name}\").GetValue(this); if(value==null) return default({typeName}); else return ({typeName})value; }}");
                writer.WriteLine($"{GetIndent(indent)}set {{ AccessToolds.Field(this.GetType(),\"{fi.Name}\").SetValue(this, value); }}"); indent--;
                writer.WriteLine($"{GetIndent(indent)}}}");
                writer.WriteLine("");
            }
        }

        private static string GetIndent(int indent) {
            string str = "";
            for (int i = 0; i < indent; i++) str += "\t";
            return str;
        }

        private static string GetTypeName(Type type) {
            if (type == null) return "void";

            string typeName = type.Name.Replace("&", "");

            if (type.IsGenericType) {
                Type genericType = type.GetGenericTypeDefinition();
                StringBuilder sb = new StringBuilder();
                sb.Append(genericType.Name.Substring(0, genericType.Name.IndexOf("`")) + "<");

                Type[] args = type.GetGenericArguments();
                for(int i = 0; i < args.Length; i++) {
                    if (i != 0) sb.Append(",");
                    sb.Append(GetTypeName(args[i]));
                }
                sb.Append(">");
                return sb.ToString();
            } else {
                switch (typeName) {
                    case "Void": return "void";
                    case "Boolean": return "bool";
                    case "Single": return "float";
                    case "Byte": return "byte";
                    case "Int32": return "int";
                    case "Double": return "double";
                    case "Int64": return "long";
                    default: return typeName;
                }
            }
        }
    }
}
