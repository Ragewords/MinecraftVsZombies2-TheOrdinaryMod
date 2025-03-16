using System.IO;
using UnityEditor;
using UnityEngine;

namespace CustomBuilder.Editor
{
    public class BuildConfig
    {
        public BuildConfig(string name, BuildTarget target, BuildOptions options)
        {
            Target = target;
            Options = options;
            ApplicationName = name;
        }
        public BuildConfig(BuildTarget target, BuildOptions options) : this(Application.productName, target, options) { }

        /// <summary>
        /// 从字符串解析配置
        /// </summary>
        /// <param name="configString">
        /// 平台字符串，诸如win64-debug。
        /// 如果不指定是debug或release，则默认使用Release的配置
        /// </param>
        public BuildConfig(string configString)
        {
            bool debug = false;
            var strs = configString.Split('-');
            if (strs.Length >= 1)
            {
                switch (strs[0].ToLower())
                {
                    case "win32":
                        Target = BuildTarget.StandaloneWindows;
                        break;
                    case "win64":
                        Target = BuildTarget.StandaloneWindows64;
                        break;
                    case "linux":
                    case "linux64":
                        Target = BuildTarget.StandaloneLinux64;
                        break;
                    case "android":
                        Target = BuildTarget.Android;
                        break;
                    case "ios":
                        Target = BuildTarget.iOS;
                        break;
                    case "switch":
                        Target = BuildTarget.Switch;
                        break;
                    case "webgl":
                        Target = BuildTarget.WebGL;
                        break;
                    case "xbox":
                        Target = BuildTarget.XboxOne;
                        break;
                    default:
                        throw new System.ArgumentException($"无法识别的平台名称：{strs[0]}");
                }
            }
            if (strs.Length >= 2)
            {
                switch (strs[1].ToLower())
                {
                    case "debug":
                        debug = true;
                        break;
                    case "release":
                        debug = false;
                        break;
                    default:
                        throw new System.ArgumentException($"无法识别的后缀名称：{strs[1]}");
                }
            }
            if (debug)
            {
                Options |= BuildOptions.Development;
            }
            ApplicationName = Application.productName;
        }
        public BuildTargetGroup GetTargetGroup()
        {
            switch (Target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneOSX:
                case BuildTarget.StandaloneLinux64:
                    return BuildTargetGroup.Standalone;
                case BuildTarget.Android:
                    return BuildTargetGroup.Android;
                case BuildTarget.iOS:
                    return BuildTargetGroup.iOS;
                case BuildTarget.Switch:
                    return BuildTargetGroup.Switch;
                case BuildTarget.WebGL:
                    return BuildTargetGroup.WebGL;
                case BuildTarget.XboxOne:
                    return BuildTargetGroup.XboxOne;
                default:
                    return BuildTargetGroup.Unknown;
            }
        }
        public BuildTarget Target { get; set; }
        public BuildOptions Options { get; set; }

        /// <summary>
        /// 程序名称
        /// </summary>
        public string ApplicationName { get; }

        /// <summary>
        /// 平台名称
        /// </summary>
        public string TargetName
        {
            get
            {
                switch (Target)
                {
                    case BuildTarget.StandaloneWindows:
                        return "win32";
                    case BuildTarget.StandaloneWindows64:
                        return "win64";
                    case BuildTarget.StandaloneOSX:
                        return "osx";
                    // case BuildTarget.StandaloneLinux:
                    // case BuildTarget.StandaloneLinuxUniversal:
                    case BuildTarget.StandaloneLinux64:
                        return "linux64";
                    case BuildTarget.Android:
                        return "android";
                    case BuildTarget.iOS:
                        return "ios";
                    case BuildTarget.Switch:
                        return "switch";
                    case BuildTarget.WebGL:
                        return "webGL";
                    case BuildTarget.XboxOne:
                        return "xbox";
                    default:
                        return Target.ToString();
                }
            }
        }

        /// <summary>
        /// 类别后缀
        /// </summary>
        public string SubName
        {
            get
            {
                if ((Options & BuildOptions.Development) == BuildOptions.Development)
                {
                    return "debug";
                }
                else
                {
                    return "release";
                }
            }
        }

        /// <summary>
        /// 输出扩展名
        /// </summary>
        public string Suffix
        {
            get
            {
                switch (Target)
                {
                    case BuildTarget.Android:
                        return ".apk";
                    case BuildTarget.StandaloneWindows:
                    case BuildTarget.StandaloneWindows64:
                        return ".exe";
                    default:
                        return "";
                }
            }
        }

        /// <summary>
        /// 完整路径
        /// </summary>
        public string FullPath
        {
            get
            {
                return Path.Combine($"{TargetName}_{SubName}", ApplicationName + Suffix);
            }
        }
    }
}
