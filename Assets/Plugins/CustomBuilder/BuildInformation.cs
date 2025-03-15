using System;
using System.IO;
using UnityEngine;

namespace CustomBuilder
{
    [CreateAssetMenu(menuName = "Custom/BuildInformation")]
    public class BuildInformation : ScriptableObject
    {
        public const string FileName = "BuildInfo";

        [SerializeField]
        string version;
        public string Version
        {
            get { return version; }
            set { version = value; }
        }

        [SerializeField]
        string buildDate;
        public string BuildDate
        {
            get { return buildDate; }
            set { buildDate = value; }
        }

        public void Reset()
        {
            version = "";
            buildDate = "";
        }

        public BuildInformation()
        {
            this.Reset();
        }
        public void SetDummyVersion(string version)
        {
            this.version = version;
            buildDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        static BuildInformation _instance;

        public static BuildInformation Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<BuildInformation>(FileName);
                }
                // 如果没有正确的打包信息，那就只能尝试用工作目录的，或者是用程序内置的版本信息
                if (_instance == null)
                {
                    string version = Application.version;
#if UNITY_EDITOR
                    try
                    {
                        var currentDir = Directory.GetCurrentDirectory();
                        var gitDir = Path.Combine(currentDir, ".git");
                        if (Directory.Exists(gitDir))
                        {
                            GitDescriber git = new GitDescriber(gitDir);
                            version = git.Describe();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("读取Git描述失败：" + e);
                        version = Application.version;
                    }
#endif
                    _instance = CreateInstance<BuildInformation>();
                    _instance.SetDummyVersion(version);
                }
                return _instance;
            }
        }
    }
}
