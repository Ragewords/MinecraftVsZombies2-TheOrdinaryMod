using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace CustomBuilder.Editor
{
    public class BuilderSteps
    {
        BuildOptions options { get; }
        public BuilderSteps(BuildOptions options)
        {
            this.options = options;
        }

        List<Tuple<BuilderStepAttribute, IBuilderStep>> steps;

        /// <summary>
        /// 打包前设置
        /// </summary>
        public void ActionBefore()
        {
            steps = ExternalStepsScan();
            foreach (var step in steps)
            {
                var attr = step.Item1;
                var obj = step.Item2;

                // todo: attr 判断
                Debug.Log("执行打包前步骤: " + obj.GetType().Name);
                try
                {
                    obj.ActionBefore(options);
                }
                catch (Exception ex)
                {
                    Debug.LogError("打包步骤执行出错: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// 打包后设置
        /// </summary>
        public void ActionAfter()
        {
            if (steps != null)
            {
                for (int i = steps.Count - 1; i >= 0; i--)
                {
                    var step = steps[i];
                    var attr = step.Item1;
                    var obj = step.Item2;

                    // todo: attr 判断
                    Debug.Log("执行打包后步骤: " + obj.GetType().Name);
                    try
                    {
                        obj.ActionAfter();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("打包步骤执行出错: " + ex.Message);
                    }
                }
            }
        }

        class PlayerNameComparer : IComparer<Tuple<BuilderStepAttribute, IBuilderStep>>
        {
            public int Compare(Tuple<BuilderStepAttribute, IBuilderStep> x, Tuple<BuilderStepAttribute, IBuilderStep> y)
            {
                return x.Item1.CompareTo(y.Item1);
            }
        }

        /// <summary>
        /// 扫描所有可用步骤
        /// </summary>
        /// <returns></returns>
        public List<Tuple<BuilderStepAttribute, IBuilderStep>> ExternalStepsScan()
        {
            var result = new List<Tuple<BuilderStepAttribute, IBuilderStep>>();

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
                        if (typeof(IBuilderStep).IsAssignableFrom(type) && !type.IsAbstract)
                        {
                            var attrs = type.GetCustomAttributes(typeof(BuilderStepAttribute), true);
                            BuilderStepAttribute attr = attrs.Length > 0 ? (attrs[0] as BuilderStepAttribute) : new BuilderStepAttribute();

                            var obj = Activator.CreateInstance(type) as IBuilderStep;

                            result.Add(new Tuple<BuilderStepAttribute, IBuilderStep>(attr, obj));
                        }
                    }
                }
                catch (System.Reflection.ReflectionTypeLoadException e)
                {
                    Debug.LogWarning($"Error when loading types of {assemblyName}, {e.Message}. Skip it.");
                }
            }

            result.Sort(new PlayerNameComparer());
            return result;
        }

        [MenuItem("Custom/CI/构建步骤执行测试")]
        static void ExecuteBeforeHook()
        {
            var step = new BuilderSteps(0);
            step.ActionBefore();
            step.ActionAfter();
        }
    }
}
