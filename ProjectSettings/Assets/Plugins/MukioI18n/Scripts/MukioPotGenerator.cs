using System.Collections.Generic;
using System;
using System.Text;
using System.IO;
using UnityEngine;

namespace MukioI18n
{
    public class MukioPotGenerator
    {
        bool removeDuplicate = true;

        List<PotEntry> entries = new List<PotEntry>();

        Dictionary<PotTranslate, PotTranslate> keySet = new Dictionary<PotTranslate, PotTranslate>();

        /// <summary>
        /// 新建一个Pot生成器
        /// </summary>
        /// <param name="projectName">项目名称</param>
        /// <param name="teamName">翻译团队名称</param>
        /// <param name="allowDup">是否允许重复的翻译文本</param>
        public MukioPotGenerator(string projectName, string teamName, bool allowDup = false)
        {
            AddInitialHeader(projectName, teamName);
            removeDuplicate = !allowDup;
        }
        /// <summary>
        /// 添加一行翻译文本
        /// </summary>
        /// <param name="msg"></param>
        public void AddString(string msg)
        {
            AddString(new PotTranslate(msg));
        }

        /// <summary>
        /// 添加一行翻译文本
        /// </summary>
        /// <param name="msg">文本</param>
        /// <param name="reference">文本参考来源</param>
        public void AddString(string msg, string reference)
        {
            AddString(new PotTranslate(msg, reference));
        }

        /// <summary>
        /// 添加一行翻译文本
        /// </summary>
        /// <param name="tr"></param>
        public void AddString(PotTranslate tr)
        {
            if (keySet.ContainsKey(tr))
            {
                var last = keySet[tr];
                Debug.LogWarningFormat("Duplicated translate message {0} ({1}) in {2}. Last occur: {3}", tr.MsgID, tr.Context, tr.Reference, last.Reference);
                if (removeDuplicate) return;
            }

            keySet[tr] = tr;
            addString(tr);
        }

        void addString(PotTranslate tr)
        {
            entries.Add(tr);
        }

        /// <summary>
        /// 添加一个翻译文件头
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddHeader(string key, string value)
        {
            AddHeader(new PotHeader(key, value));
        }

        /// <summary>
        /// 添加一个翻译文件头
        /// </summary>
        /// <param name="header"></param>
        public void AddHeader(PotHeader header)
        {
            entries.Add(header);
        }

        /// <summary>
        /// 添加初始的Header
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="teamName"></param>
        public void AddInitialHeader(string projectName, string teamName)
        {
            addString(new PotTranslate("") { Flag = "fuzzy" });
            AddHeader("Project-Id-Version", projectName);
            // HACK
            AddHeader("POT-Creation-Date", DateTime.Now.ToString("yyyy-MM-dd hh:mmzz") + "00");
            AddHeader("Language-Team", teamName);
            AddHeader("Language", "zh_CN");
            AddHeader("Content-Type", "text/plain; charset=UTF-8");
            AddHeader("Content-Transfer-Encoding", "8bit");
            AddHeader("Plural-Forms", "nplurals=2; plural=n != 1;");
            AddHeader("X-Poedit-KeywordsList", "GetString;GetPluralString:1,2;GetParticularString:1c,2;GetParticularPluralString:1c,2,3;_;_n:1,2;_p:1c,2;_pn:1c,2,3");
        }

        /// <summary>
        /// 写出Pot文件到指定目录
        /// </summary>
        /// <param name="path"></param>
        public void WriteOut(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            bool headerFlag = false;
            
            using (var stream = File.Open(path, FileMode.Create))
            {
                Encoding utf8WithoutBom = new UTF8Encoding(false);
                using (StreamWriter streamWriter = new StreamWriter(stream, utf8WithoutBom))
                {
                    foreach (var item in entries)
                    {
                        // HACK: 在Header后面添加一行换行
                        if (headerFlag && !(item is PotHeader))
                            streamWriter.WriteLine("");

                        streamWriter.WriteLine(item.ToString());

                        headerFlag = item is PotHeader;
                    }
                }
            }
        }

    }

    public abstract class PotEntry { }

    public class PotHeader : PotEntry
    {
        public string Key;
        public string Value;

        public override string ToString()
        {
            return $"\"{Key}: {Value}\\n\"";
        }

        public PotHeader() { }
        public PotHeader(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

    public class PotTranslate : PotEntry
    {
        /// <summary>
        /// 翻译标签（原文）
        /// </summary>
        public string MsgID { get; set; } = null;
        /// <summary>
        /// 上下文
        /// </summary>
        public string Context { get; set; } = null;
        /// <summary>
        /// 参考来源
        /// </summary>
        public string Reference { get; set; } = null;
        /// <summary>
        /// 翻译者留言
        /// </summary>
        public string TranslatorComment { get; set; } = null;
        /// <summary>
        /// 字符串提取注释
        /// </summary>
        public string ExtractedComment { get; set; } = null;
        /// <summary>
        /// （特殊）标志
        /// </summary>
        public string Flag { get; set; } = null;

        /// <summary>
        /// 新建一个翻译条目
        /// </summary>
        /// <param name="textID">翻译内容</param>
        public PotTranslate(string textID)
        {
            MsgID = textID;
        }
        /// <summary>
        /// 新建一个翻译条目
        /// </summary>
        /// <param name="textID">翻译内容</param>
        /// <param name="reference">参考来源</param>
        public PotTranslate(string textID, string reference)
        {
            MsgID = textID;
            Reference = reference;
        }
        /// <summary>
        /// 新建一个翻译条目
        /// </summary>
        /// <param name="textID">翻译内容</param>
        /// <param name="reference">参考来源>
        /// <param name="comment">注释</param>
        public PotTranslate(string textID, string reference, string comment)
        {
            MsgID = textID;
            Reference = reference;
            ExtractedComment = comment;
        }

        /// <summary>
        /// 新建一个翻译条目
        /// </summary>
        /// <param name="textID">翻译内容</param>
        /// <param name="reference">参考上下文</param>
        /// <param name="comment">注释</param>
        /// <param name="context">上下文</param>
        public PotTranslate(string textID, string reference, string comment, string context)
        {
            MsgID = textID;
            Reference = reference;
            ExtractedComment = comment;
            Context = context;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(TranslatorComment))
            {
                var strs = TranslatorComment.Split('\n');
                foreach (var str in strs)
                {
                    sb.Append("# ");
                    sb.Append(str);
                    sb.Append("\n");
                }
            }
            if (!string.IsNullOrEmpty(ExtractedComment))
            {
                var strs = ExtractedComment.Split('\n');
                foreach (var str in strs)
                {
                    sb.Append("#. ");
                    sb.Append(str);
                    sb.Append("\n");
                }
            }
            if (!string.IsNullOrEmpty(Flag))
            {
                sb.Append("#, ");
                sb.Append(Flag);
                sb.Append("\n");
            }
            if (!string.IsNullOrEmpty(Reference))
            {
                var strs = Reference.Split('\n');
                foreach (var str in strs)
                {
                    sb.Append("#: ");
                    sb.Append(str);
                    sb.Append("\n");
                }
            }
            if (!string.IsNullOrEmpty(Context))
            {
                sb.Append("msgctxt ");
                appendStr(sb, Context);
            }

            sb.Append("msgid ");
            appendStr(sb, MsgID);
            sb.Append("msgstr \"\"\n");

            return sb.ToString();
        }

        private void appendStr(StringBuilder sb, string str)
        {
            if (str.Contains("\n"))
            {
                sb.Append("\"\"\n");
                var strs = str.Split('\n');
                for (int i = 0; i < strs.Length; i++)
                {
                    var line = strs[i];
                    var isLast = i == strs.Length - 1;
                    if (string.IsNullOrEmpty(line) && isLast)
                        continue;
                    sb.Append("\"");
                    sb.Append(escapeString(line));
                    if (!isLast)
                        sb.Append("\\n");
                    sb.Append("\"\n");
                }
            }
            else
            {
                sb.Append("\"");
                sb.Append(escapeString(str));
                sb.Append("\"\n");
            }
        }

        private static string escapeString(string str)
        {
            str = str.Replace("\\", "\\\\");
            str = str.Replace("\"", "\\\"");
            return str;
        }
        

        public override bool Equals(object obj)
        {
            if (obj == null && this == null) return true;
            if (obj == null || this == null) return false;
            if (!(obj is PotTranslate)) return false;

            var other = obj as PotTranslate;
            return MsgID == other.MsgID && ((string.IsNullOrEmpty(Context) && string.IsNullOrEmpty(other.Context)) || Context == other.Context);
        }

        public override int GetHashCode()
        {
            var context = Context ?? string.Empty;
            return new Tuple<string, string>(MsgID, context).GetHashCode();
        }
    }
}

