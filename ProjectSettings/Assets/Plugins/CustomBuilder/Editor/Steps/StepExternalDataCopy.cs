using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CustomBuilder.Editor
{
    /// <summary>
    /// 外部数据复制
    /// </summary>
    [BuilderStep]
    public class StepExternalDataCopy : IBuilderStep
    {
        public const string ExternalFolderPath = "ExternalData";

        string[] FilesToRemove, DirsToRemove;

        public void ActionAfter()
        {
            foreach (var item in FilesToRemove)
            {
                File.Delete(item);

                var itemMeta = item + ".meta";
                if (File.Exists(itemMeta))
                    File.Delete(itemMeta);

            }
            foreach (var item in DirsToRemove)
            {
                Directory.Delete(item);

                var itemMeta = item + ".meta";
                if (File.Exists(itemMeta))
                    File.Delete(itemMeta);
            }
        }

        public void ActionBefore(object option)
        {
            List<string> targets = new List<string>();
            List<string> dirsCreated = new List<string>();

            Queue<string> dirsToCheck = new Queue<string>();
            dirsToCheck.Enqueue(ExternalFolderPath);

            while (dirsToCheck.Count > 0)
            {
                var currentDir = dirsToCheck.Dequeue();

                var dirName = currentDir.Remove(0, ExternalFolderPath.Length);
                if (dirName.StartsWith("/") || dirName.StartsWith("\\"))
                    dirName = dirName.Remove(0, 1);

                Debug.Log($"Dir: {currentDir}, Name: {dirName}");
                var dstDir = Path.Combine(Application.streamingAssetsPath, dirName);

                if (!Directory.Exists(dstDir))
                {
                    Directory.CreateDirectory(dstDir);
                    dirsCreated.Add(dstDir);
                }

                var files = Directory.GetFiles(currentDir);
                foreach (var item in files)
                {
                    // Ignore hidden files
                    if (item.StartsWith("."))
                        continue;

                    var toName = Path.Combine(dstDir, Path.GetFileName(item));
                    targets.Add(toName);
                    Debug.Log($"Copy File {item} to {toName}");
                    File.Copy(item, toName, true);
                }

                var dirs = Directory.GetDirectories(currentDir);
                foreach (var item in dirs)
                {
                    dirsToCheck.Enqueue(item);
                }
            }

            // 让子文件夹排在前面，防止删除的时候出错
            dirsCreated.Reverse();

            FilesToRemove = targets.ToArray();
            DirsToRemove = dirsCreated.ToArray();
        }
    }
}
