using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using CustomBuilder;
using System.Linq;
using UnityEditor.Build.Reporting;

namespace CustomBuilder.Editor
{
    public class Builder
    {
        public static string[] SCENES = FindEnabledEditorScenes();

        private static string[] FindEnabledEditorScenes()
        {
            List<string> EditorScenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled) continue;
                EditorScenes.Add(scene.path);
            }
            return EditorScenes.ToArray();
        }

        public static void GenericBuild(string[] scenes, string target_dir, BuildTargetGroup build_target_group, BuildTarget build_target, BuildOptions build_options)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(build_target_group, build_target);

            BuilderSteps steps = new BuilderSteps(build_options);

            BuildReport report;

            try
            {
                steps.ActionBefore();

                // 打包
                Debug.Log($"Building {build_target} into {target_dir}.");
                report = BuildPipeline.BuildPlayer(scenes, target_dir, build_target, build_options);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                steps.ActionAfter();
            }

            // 处理错误
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                throw new Exception("BuildPlayer failure: " + report.summary.totalErrors + " Errors.");
            }

            Debug.Log($"Build {build_target} done.");
        }

        public static void GenericBuild(BuildConfig config)
        {
            GenericBuild(SCENES, Path.Combine("Build", config.FullPath), config.GetTargetGroup(), config.Target, config.Options);
        }

        [MenuItem("Custom/CI/构建所有项目")]
        static void BuildAll()
        {
            var configs = ReadConfigs();
            foreach (var config in configs)
            {
                GenericBuild(config);
            }
        }

        static void BuildWindows()
        {
            BuildWithTarget(new BuildTarget[] { BuildTarget.StandaloneWindows, BuildTarget.StandaloneWindows64 });
        }

        static void BuildAndroid()
        {
            BuildWithTarget(new BuildTarget[] { BuildTarget.Android });
        }

        static void BuildLinux()
        {
            BuildWithTarget(new BuildTarget[] { BuildTarget.StandaloneLinux64 });
        }

        static void BuildWithTarget(BuildTarget[] targets)
        {
            var configs = ReadConfigs();
            foreach (var config in configs)
            {
                if (targets.Contains(config.Target))
                    GenericBuild(config);
            }
        }

        /// <summary>
        /// 从命令行中读取需要构建的平台和配置
        /// </summary>
        static void BuildFromArg() 
        {
            var configString = TestRunner.GetArgument("-buildConfig");
            var configs = configString.Split(';');
            var hashset = new HashSet<string>();
            foreach (var c in configs)
            {
                if (c.Length == 0)
                    continue;
                hashset.Add(c.Trim());
            }
            foreach (var c in hashset)
            {
                GenericBuild(new BuildConfig(c));
            }
        }

        const string ConfigPath = "BuilderConfig.txt";
        public static BuildConfig[] ReadConfigs()
        {
            List<BuildConfig> configs = new List<BuildConfig>();
            if (File.Exists(ConfigPath))
            {
                var lines = File.ReadAllLines(ConfigPath);
                foreach (var line in lines)
                {
                    if (line.Length > 0 && !line.StartsWith("#"))
                    {
                        try
                        {
                            var strs = line.Split(',');
                            if (strs.Length == 2)
                            {
                                var target = int.Parse(strs[0]);
                                var option = int.Parse(strs[1]);

                                configs.Add(new BuildConfig((BuildTarget)target, (BuildOptions)option));
                            }
                        }
                        catch { }
                    }
                }
            }

            return configs.ToArray();
        }

        public static void WriteConfigs(BuildConfig[] configs)
        {
            string result = "# CustomBuilder 构建配置文件 \n\n";
            foreach (var config in configs)
            {
                result += $"{(int)config.Target},{(int)config.Options}\n";
            }
            File.WriteAllText(ConfigPath, result);
        }
    }
}
