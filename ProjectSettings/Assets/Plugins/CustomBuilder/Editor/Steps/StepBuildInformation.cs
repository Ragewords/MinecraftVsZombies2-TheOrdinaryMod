using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CustomBuilder.Editor
{
    /// <summary>
    /// 构建信息
    /// </summary>
    [BuilderStep]
    public class StepBuildInformation : IBuilderStep
    {
        string oldBundleVersion { get; set; }

        /// <summary>
        /// 重置打包信息
        /// </summary>
        public void ActionAfter()
        {
            PlayerSettings.bundleVersion = oldBundleVersion;

            AssetDatabase.DeleteAsset("Assets/Resources/" + BuildInformation.FileName + ".asset");
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// 设置打包信息
        /// </summary>
        public void ActionBefore(object arg)
        {
            var so = ScriptableObject.CreateInstance<BuildInformation>();

            // 优先使用环境变量提供的版本
            var buildTag = Environment.GetEnvironmentVariable("BUILD_GIT_TAG");
            if (string.IsNullOrEmpty(buildTag))
                buildTag = GetCurrentVersion();

            Debug.Log($"Build Tag: {buildTag}");
            oldBundleVersion = PlayerSettings.bundleVersion;
            PlayerSettings.bundleVersion = buildTag;
            so.Version = buildTag;
            so.BuildDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            AssetDatabase.CreateAsset(so, "Assets/Resources/" + BuildInformation.FileName + ".asset");
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Custom/CI/获取当前版本")]
        public static string GetCurrentVersion()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var gitDir = Path.Combine(currentDir, ".git");
            var ver = "0.0.0-Unknown";

            if (!Directory.Exists(gitDir))
            {
                Debug.Log("当前非Git工作目录");
                return ver;
            }
            GitDescriber git = new GitDescriber(gitDir);
            try
            {
                ver = git.Describe();
                Debug.Log("当前版本: " + ver);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return ver;
        }
    }
}
