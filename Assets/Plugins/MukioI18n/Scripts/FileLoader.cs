using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System;

namespace MukioI18n
{
    public static class FileLoader
    {
        /// <summary>
        /// 路径
        /// </summary>
        public const string FolderPath = "ExternalData";

        public static Stream ReadStreamingAsset(string path)
        {
            if (Application.isEditor && File.Exists(Path.Combine(FolderPath, path)))
            {
                return File.OpenRead(Path.Combine(FolderPath, path));
            }
            else
            {
                var stPath = Path.Combine(Application.streamingAssetsPath, path);
                if (Application.platform == RuntimePlatform.Android)
                {
                    UnityWebRequest request = UnityWebRequest.Get(stPath);
                    var op = request.SendWebRequest();
                    while (!op.isDone) ;

                    if (op.webRequest.result == UnityWebRequest.Result.ConnectionError)
                    {
                        throw new Exception(op.webRequest.error);
                    }
                    if (op.webRequest.result == UnityWebRequest.Result.ProtocolError)
                    {
                        if (op.webRequest.responseCode == 404)
                        {
                            throw new FileNotFoundException($"Unable to load file {stPath}", stPath); ;
                        }
                        else
                        {
                            throw new Exception(op.webRequest.error);
                        }
                    }
                    var data = op.webRequest.downloadHandler.data;
                    return new MemoryStream(data);
                }
                else
                {
                    return File.OpenRead(stPath);
                }
            }
        }
    }
}

