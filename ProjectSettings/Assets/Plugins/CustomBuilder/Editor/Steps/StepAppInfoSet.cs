using System;
using UnityEditor;
using UnityEngine;

namespace CustomBuilder.Editor
{
    /// <summary>
    /// 应用信息设置
    /// </summary>
    [BuilderStep]
    public class StepAppInfoSet : IBuilderStep
    {
        string savedProductName, savedAndroidID;
        string savedKeystoreName, savedKeystorePass, savedKeyaliasName, savedKeyaliasPass;
        int savedBundleVersionCode;
        bool savedUseCustomKeystore;
        BuildOptions options;
        /// <summary>
        /// 还原应用程序信息
        /// </summary>
        public void ActionAfter()
        {
            // 还原修改的包名和名字
            if (options.HasFlag(BuildOptions.Development))
            {
                PlayerSettings.productName = savedProductName;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, savedAndroidID);
            }

            PlayerSettings.Android.bundleVersionCode = savedBundleVersionCode;

            // 还原签名信息
            PlayerSettings.Android.keystoreName = savedKeystoreName;
            PlayerSettings.Android.keystorePass = savedKeystorePass;
            PlayerSettings.Android.keyaliasName = savedKeyaliasName;
            PlayerSettings.Android.keyaliasPass = savedKeyaliasPass;
            PlayerSettings.Android.useCustomKeystore = savedUseCustomKeystore;
        }

        /// <summary>
        /// 设置各种信息
        /// </summary>
        public void ActionBefore(object opt)
        {
            options = (BuildOptions)opt;

            savedBundleVersionCode = PlayerSettings.Android.bundleVersionCode;
            savedProductName = PlayerSettings.productName;
            savedAndroidID = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);

            savedKeystoreName = PlayerSettings.Android.keystoreName;
            savedKeystorePass = PlayerSettings.Android.keystorePass;
            savedKeyaliasName = PlayerSettings.Android.keyaliasName;
            savedKeyaliasPass = PlayerSettings.Android.keyaliasPass;
            savedUseCustomKeystore = PlayerSettings.Android.useCustomKeystore;

            // 更新Bundle版本
            int buildNum = (int)(DateTime.UtcNow - new DateTime(2019, 1, 1)).TotalHours;
            Debug.Log($"Set Bundle Version to: {buildNum}");
            PlayerSettings.Android.bundleVersionCode = buildNum;

            // 如果指定了签名则用指定的签名
            var keystorePath = Environment.GetEnvironmentVariable("KEYSTORE_PATH");
            var keystorePass = Environment.GetEnvironmentVariable("KEYSTORE_PASS");
            var keyaliasName = Environment.GetEnvironmentVariable("KEYALIAS_NAME");
            var keyaliasPass = Environment.GetEnvironmentVariable("KEYALIAS_PASS");
            if (!string.IsNullOrEmpty(keystorePath) && !string.IsNullOrEmpty(keyaliasName))
            {
                PlayerSettings.Android.keystoreName = keystorePath;
                PlayerSettings.Android.keystorePass = keystorePass;
                PlayerSettings.Android.keyaliasName = keyaliasName;
                PlayerSettings.Android.keyaliasPass = keyaliasPass;
                PlayerSettings.Android.useCustomKeystore = true;
            }

            // 为测试版本添加特殊的包名和名字
            if (options.HasFlag(BuildOptions.Development))
            {
                PlayerSettings.productName = $"{savedProductName}Test";
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, savedAndroidID + ".test");
            }
        }
    }
}
