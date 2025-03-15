using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Compilation;

namespace MukioI18n
{
    public class TranslateMsgAttributeFinder
    {
        public static void FindAll(MukioPotGenerator pot)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var gameAssemblies = CompilationPipeline.GetAssemblies();

            HashSet<string> nameSet = new HashSet<string>();
            foreach (var item in gameAssemblies) nameSet.Add(item.name);

            foreach (var assembly in assemblies)
            {
                // 仅查找当前游戏里面的程序集
                var assemblyName = assembly.GetName().Name;
                if (!nameSet.Contains(assemblyName)) continue;

                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        | BindingFlags.Static);
                    foreach (var field in fields)
                    {
                        if (field.IsLiteral && field.FieldType == typeof(string)) // const string
                        {
                            var attrs = field.GetCustomAttributes(typeof(TranslateMsgAttribute), true) as TranslateMsgAttribute[];
                            foreach (var attr in attrs) // 虽然必定只有一个但还是用Foreach吧
                            {
                                var tr = new PotTranslate(field.GetValue(null) as string, type.FullName + ":" + field.Name);
                                if (attr.Comment != null) tr.TranslatorComment = attr.Comment;
                                if (attr.Reference != null) tr.Reference = attr.Reference;
                                if (attr.Context != null) tr.Context = attr.Context;
                                pot.AddString(tr);
                            }
                        }
                    }
                }
            }
        }
    }
}
