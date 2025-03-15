using System;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEngine;

namespace CustomBuilder.Editor
{
    /// <summary>
    /// 老的Hook
    /// </summary>
    [BuilderStep(-1)]
    [Obsolete]
    public class StepOldExternalHook : IBuilderStep
    {
        public void ActionAfter()
        {
        }

        public void ActionBefore(object option)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var gameAssemblies = CompilationPipeline.GetAssemblies();

            HashSet<string> nameSet = new HashSet<string>();
            foreach (var item in gameAssemblies) nameSet.Add(item.name);

            foreach (var assembly in assemblies)
            {
                // 过滤不需要的东西
                var assemblyName = assembly.GetName().Name;
                if (!nameSet.Contains(assemblyName)) continue;

                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        var methods = type.GetMethods();
                        foreach (var method in methods)
                        {
                            if (method.GetCustomAttributes(typeof(ExecuteBeforeBuildAttribute), true).Length > 0)
                            {
                                Debug.Log("Executing " + assemblyName + " | " + type + "." + method + "();");

                                var obj = Activator.CreateInstance(type);
                                method.Invoke(obj, null);
                            }
                        }
                    }
                }
                catch (System.Reflection.ReflectionTypeLoadException e)
                {
                    Debug.Log($"Error when loading types of {assemblyName}, {e.Message}. Skip it.");
                }
            }
        }
    }
}
