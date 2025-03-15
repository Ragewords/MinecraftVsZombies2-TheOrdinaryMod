using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace CustomBuilder
{
    public class GitDescriber
    {
        string GitDir { get; }
        /// <summary>
        /// 创建新的目录
        /// </summary>
        /// <param name="gitDir">.git目录</param>
        public GitDescriber(string gitDir)
        {
            if (!Directory.Exists(gitDir))
                throw new FileNotFoundException(gitDir);

            GitDir = gitDir;
        }
        /// <summary>
        /// 获取当前commit的HEAD
        /// </summary>
        /// <returns></returns>
        public string GetHead()
        {
            var head = ReadTextFile("HEAD");
            if (head.StartsWith("ref: "))
            {
                var newFile = head.Remove(0, 5).Trim();
                head = ReadTextFile(newFile);
            }
            head = head.Trim();
            return head;
        }

        struct GitTag
        {
            public string Name;
            public string TagHash;
        }

        public string FindTag(string commitHash)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<GitTag> allTags = new List<GitTag>();
            // packed-refs 存储的TAG
            if (FileExists("packed-refs"))
            {
                var content = ReadTextLines("packed-refs");
                foreach (var item in content)
                {
                    if (item.StartsWith("#")) continue;
                    var seg = item.Split(' ');
                    if (seg.Length != 2) continue;
                    if (!seg[1].StartsWith("refs/tags/")) continue;

                    allTags.Add(new GitTag() { Name = seg[1].Replace("refs/tags/", ""), TagHash = seg[0].Trim() });
                }
            }

            // refs/tags/目录下存储的tag
            var tagsDir = Path.Combine(GitDir, "refs", "tags");
            if (Directory.Exists(tagsDir))
            {
                var files = Directory.GetFiles(tagsDir);
                foreach (var file in files)
                {
                    var content = File.ReadAllText(file).Trim();
                    allTags.Add(new GitTag() { Name = Path.GetFileName(file), TagHash = content });
                }
            }

            Dictionary<string, GitTag> commitDict = new Dictionary<string, GitTag>();

            // 读取tag并解析commit
            foreach (var tag in allTags)
            {
                if (!ObjectExists(tag.TagHash)) continue;
                var tagContent = ReadObjectFile(tag.TagHash);
                var commit = TagCommitParser(tagContent);
                if (!string.IsNullOrEmpty(commit))
                {
                    commitDict.Add(commit, tag);
                }
                // UnityEngine.Debug.Log($"Tag: {tag.Name}, Hash: {commit}");
            }

            // 向前查找所有可能的Tag，广度优先遍历
            HashSet<string> checkedCommit = new HashSet<string>();
            List<string> commitsToCheck = new List<string>() { commitHash };
            int depth = 0;

            while (commitsToCheck.Count > 0)
            {
                var currentCommits = commitsToCheck.ToArray();
                commitsToCheck.Clear();

                foreach (var commit in currentCommits)
                {
                    Stopwatch commitStopwatch = new Stopwatch();
                    commitStopwatch.Start();
                    if (checkedCommit.Contains(commit)) continue;
                    if (commitDict.ContainsKey(commit))
                    {
                        var tag = commitDict[commit].Name;
                        cleanUp();
                        return depth == 0 ? tag : $"{tag}-{depth}";
                    }
                    checkedCommit.Add(commit);
                    var parents = GetParentsCommit(commit);
                    commitsToCheck.AddRange(parents);
                    // UnityEngine.Debug.Log("查找提交" + commit + "，耗时：" + commitStopwatch.Elapsed.TotalSeconds + "秒, 父节点: " + parents.Count());
                }

                depth++;
            }
            cleanUp();
            return null;
            void cleanUp()
            {
                //优化措施，清理文件读取内存
                foreach (var fs in _fileStreamDict.Values)
                {
                    fs.Close();
                }
                _fileStreamDict.Clear();
                float memoryInGBBeforeGC = GC.GetTotalMemory(false) / 1073741824f;
                GC.Collect();
                float memoryInGBAfterGC = GC.GetTotalMemory(false) / 1073741824f;
                stopwatch.Stop();
                UnityEngine.Debug.Log("寻找GitTag完毕" +
                    "，提交总数：" + depth +
                    "，释放内存：" + (memoryInGBBeforeGC - memoryInGBAfterGC) + "GB" +
                    "，耗时：" + stopwatch.Elapsed.TotalSeconds + "秒");
            }
        }

        string ReadTextFile(string name)
        {
            return File.ReadAllText(Path.Combine(GitDir, name));
        }
        /// <summary>
        /// 解压并读取Object
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        byte[] ReadObjectFile(string hash)
        {
            string pref = hash.Substring(0, 2);
            string remain = hash.Substring(2);

            string path = Path.Combine(GitDir, "objects", pref, remain);

            // 文件不存在，可能是被打包了
            if (!File.Exists(path))
            {
                try
                {
                    return ReadPackedObjectFile(hash);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"无法读取{hash}: {e}");
                    return new byte[0];
                }
            }

            // 解压zlib压缩的object
            var rawData = File.ReadAllBytes(path);
            using (var src = new MemoryStream())
            {
                // Deflate相对于zlib少了2byte的头和4byte的校验
                src.Write(rawData, 2, rawData.Length - 2 - 4);
                src.Seek(0, SeekOrigin.Begin);
                using (var cs = new DeflateStream(src, CompressionMode.Decompress))
                {
                    using (var sr = new MemoryStream())
                    {
                        cs.CopyTo(sr);
                        return sr.ToArray();
                    }
                }
            }
        }

        Dictionary<string, Tuple<string, uint>> packedObjectDict = new Dictionary<string, Tuple<string, uint>>();

        uint getPackedObjectFileOffsetUncached(string hash, out string pack_file)
        {
            string pref = hash.Substring(0, 2);
            int id = Convert.ToUInt16(pref, 16);

            var files = Directory.GetFiles(Path.Combine(GitDir, "objects", "pack"), "*.idx");
            foreach (var file in files)
            {
                using (var fs = File.OpenRead(file))
                {
                    byte[] tempBuffer = new byte[4];
                    // 8byte header + offset - 1
                    fs.Seek(id == 0 ? 8 : (id * 4 + 4), SeekOrigin.Begin);

                    UInt32 last_offset = 0, offset = 0;
                    if (id > 0)
                    {
                        fs.Read(tempBuffer, 0, 4);
                        last_offset = getByteVal(tempBuffer);
                    }
                    fs.Read(tempBuffer, 0, 4);
                    offset = getByteVal(tempBuffer);

                    // 没有对应的数字，退出
                    if (last_offset == offset) continue;

                    // 获取object的数量
                    fs.Seek(8 + 255 * 4, SeekOrigin.Begin);
                    fs.Read(tempBuffer, 0, 4);
                    UInt32 totalCount = getByteVal(tempBuffer);

                    fs.Seek(8 + 256 * 4 + last_offset * 20, SeekOrigin.Begin);

                    byte[] hashBuffer = new byte[20];
                    for (UInt32 i = last_offset; i < offset; i++)
                    {
                        fs.Read(hashBuffer, 0, 20);
                        bool match = true;
                        for (int j = 0; j < 20; j++)
                        {
                            if (Convert.ToUInt16(hash.Substring(j * 2, 2), 16) != hashBuffer[j])
                            {
                                match = false;
                                break;
                            }
                        }
                        if (!match) continue;

                        // offset
                        fs.Seek(8 + 256 * 4 + totalCount * (20 + 4) + i * 4, SeekOrigin.Begin);
                        fs.Read(tempBuffer, 0, 4);
                        uint file_offset = getByteVal(tempBuffer);
                        pack_file = Path.ChangeExtension(file, "pack");
                        return file_offset;
                    }
                }
            }

            pack_file = null;
            return 0;
        }

        /// <summary>
        /// 获取指定Hash的文件位置和Offset
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="pack_file"></param>
        /// <returns></returns>
        uint GetPackedObjectFileOffset(string hash, out string pack_file)
        {
            // cache
            if (packedObjectDict.ContainsKey(hash))
            {
                pack_file = packedObjectDict[hash].Item1;
                return packedObjectDict[hash].Item2;
            }

            var offset = getPackedObjectFileOffsetUncached(hash, out pack_file);
            packedObjectDict.Add(hash, new Tuple<string, uint>(pack_file, offset));
            return offset;
        }

        enum GitObjectType
        {
            Invalid = 0,
            Commit = 1,
            Tree = 2,
            Blob = 3,
            Tag = 4,
            OfsDelta = 6,
            RefDelta = 7,
        }

        /// <summary>
        /// 解析类型和长度
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        int ParseTypeLen(Stream fs, out GitObjectType type)
        {
            int tmp = fs.ReadByte();
            int bit_offset = 4;
            int len = tmp & 0x0F;
            type = (GitObjectType)((tmp & 0x70) >> 4);
            while (tmp >= 0x80)
            {
                tmp = fs.ReadByte();
                len += (tmp & 0x7F) << bit_offset;
                bit_offset += 7;
            }
            return len;
        }

        /// <summary>
        /// 解析长度
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        int ParseLen(Stream fs)
        {
            int tmp = fs.ReadByte();
            int bit_offset = 4;
            int len = tmp & 0x0F;
            while (tmp >= 0x80)
            {
                tmp = fs.ReadByte();
                len += (tmp & 0x7F) << bit_offset;
                bit_offset += 7;
            }
            return len;
        }

        /// <summary>
        /// 解析Offset
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        int ParseOfsOffset(Stream fs)
        {
            int tmp = fs.ReadByte();
            int len = tmp & 0x7F;
            while (tmp >= 0x80)
            {
                tmp = fs.ReadByte();
                len = (len + 1) << 7;
                len += (tmp & 0x7F);
            }
            return len;
        }

        /// <summary>
        /// 读取打包的文件
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        byte[] ReadPackedObjectFile(string hash)
        {
            using (var fs = ReadPackedObjectByHash(hash))
            {
                return fs.ToArray();
            }
        }

        /// <summary>
        /// 根据Hash读取打包的文件流
        /// 用完记得关闭
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        MemoryStream ReadPackedObjectByHash(string hash)
        {
            uint offset = GetPackedObjectFileOffset(hash, out string file);
            var stream = ReadPackedObjectByFileOffset(file, offset);
            return stream;
        }

        /// <summary>
        /// 复制流
        /// </summary>
        /// <param name="src">源</param>
        /// <param name="dst">目标</param>
        /// <param name="len">长度</param>
        void StreamCopyTo(Stream src, Stream dst, uint len)
        {
            byte[] buff = new byte[65536];
            while (len > 0)
            {
                int size = src.Read(buff, 0, (int)Math.Min(len, 65536));
                if (size == 0)
                {
                    UnityEngine.Debug.Log($"Copy overflow!");
                    break;
                }
                dst.Write(buff, 0, size);
                len -= (uint)size;
            }
        }

        /// <summary>
        /// 应用Delta数据
        /// </summary>
        /// <param name="baseObj"></param>
        /// <param name="deltaObj"></param>
        /// <returns></returns>
        MemoryStream ObjectApplyDelta(Stream baseObj, Stream deltaObj)
        {
            var srcLen = ParseLen(deltaObj);
            var dstLen = ParseLen(deltaObj);

            // UnityEngine.Debug.Log($"Apply delta. SrcLen={srcLen}, DstLen={dstLen}, Pos={deltaObj.Position}/{deltaObj.Length}");

            var dst = new MemoryStream();
            while (deltaObj.Position < deltaObj.Length)
            {
                int cmd = deltaObj.ReadByte();
                if ((cmd & 0x80) > 0)
                {
                    // copy command
                    byte[] dat = { 0, 0, 0, 0, 0, 0, 0, 0 };
                    for (int i = 0; i < 7; i++)
                        if ((cmd & (1 << i)) > 0)
                            dat[i] = (byte)deltaObj.ReadByte();

                    uint offset = BitConverter.ToUInt32(dat, 0);
                    uint size = BitConverter.ToUInt32(dat, 4);
                    if (size == 0) size = 0x10000;

                    baseObj.Seek(offset, SeekOrigin.Begin);
                    // UnityEngine.Debug.Log($"Cmd: {cmd}, Copy Offfset={offset}, Size={size}");
                    StreamCopyTo(baseObj, dst, size);
                }
                else
                {
                    // add command
                    // UnityEngine.Debug.Log($"Cmd: {cmd}, Add Size={cmd}");
                    StreamCopyTo(deltaObj, dst, (uint)cmd);
                }
            }
            dst.Seek(0, SeekOrigin.Begin);
            return dst;
        }

        private MemoryStream ReadPackedObjectByFileOffset(string file, uint offset)
        {
            if (offset <= 0)
            {
                UnityEngine.Debug.LogError("Offset is invalid.");
                return null;
            }

            FileStream fs = OpenReadOrGetFileStream(file);
            fs.Seek(offset, SeekOrigin.Begin);

            // 解析Meta
            int len = ParseTypeLen(fs, out GitObjectType type);
            UnityEngine.Debug.Log($"Read Packed: {file}, Type: {type}, Offset: {offset}, Len: {len}");

            Stream deltaBase = null;
            if (type == GitObjectType.RefDelta)
            {
                byte[] hashBuffer = new byte[20];
                fs.Read(hashBuffer, 0, 20);
                string baseHash = string.Join("", hashBuffer.Select(x => x.ToString("x2")));
                long origPos = fs.Position;
                deltaBase = ReadPackedObjectByHash(baseHash);
                fs.Seek(origPos, SeekOrigin.Begin);
            }

            if (type == GitObjectType.OfsDelta)
            {
                int ofsOffset = ParseOfsOffset(fs);
                long origPos = fs.Position;
                deltaBase = ReadPackedObjectByFileOffset(file, (uint)(offset - ofsOffset));
                fs.Seek(origPos, SeekOrigin.Begin);
            }

            try
            {
                // 移除 2byte 头 + 4 byte 尾部 hash
                fs.Seek(2, SeekOrigin.Current);

                using (var ds = new DeflateStream(fs, CompressionMode.Decompress, true))
                {
                    var cs = new MemoryStream();
                    ds.CopyTo(cs);
                    cs.Seek(0, SeekOrigin.Begin);
                    if (deltaBase == null)
                    {
                        return cs;
                    }
                    else
                    {
                        var st = ObjectApplyDelta(deltaBase, cs);
                        deltaBase.Close();
                        cs.Close();
                        return st;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"读取{file}({offset})时出错。", e);
            }
        }
        private FileStream OpenReadOrGetFileStream(string file)
        {
            FileStream fs;
            if (_fileStreamDict.TryGetValue(file, out fs))
            {
                fs.Position = 0;
                return fs;
            }
            else
            {
                fs = File.OpenRead(file);
                _fileStreamDict[file] = fs;
                return fs;
            }
        }
        Dictionary<string, FileStream> _fileStreamDict = new Dictionary<string, FileStream>();
        /// <summary>
        /// 从Tag里面解析出Commit的Hash
        /// </summary>
        /// <param name="rawContent"></param>
        /// <returns></returns>
        string TagCommitParser(byte[] rawContent)
        {
            return getValueOfKey(rawContent, "object");
        }

        /// <summary>
        /// 获取一个提交的父提交
        /// </summary>
        /// <param name="commit"></param>
        /// <returns></returns>
        public string[] GetParentsCommit(string commit)
        {
            if (!ObjectExists(commit)) return new string[0];

            var content = ReadObjectFile(commit);
            return getValuesOfKey(content, "parent");
        }

        /// <summary>
        /// 以Git格式描述当前HEAD提交
        /// </summary>
        /// <remarks>
        /// 注意因为这个用作版本号，所以如果没有tag则默认会给一个"0.0.0"的版本
        /// </remarks>
        /// <returns></returns>
        public string Describe()
        {
            var head = GetHead();
            var tag = FindTag(head);
            if (string.IsNullOrEmpty(tag)) tag = "0.0.0";

            return tag + "-g" + head.Substring(0, 7);
        }

        private static string getValueOfKey(byte[] rawContent, string keyToFind)
        {
            var a = getValuesOfKey(rawContent, keyToFind);
            if (a.Length == 0) return null;
            return a[0];
        }

        private static UInt32 getByteVal(byte[] data)
        {
            return (UInt32)data[3] + ((UInt32)data[2] << 8) + ((UInt32)data[1] << 16) + ((UInt32)data[0] << 24);
        }

        private static string[] getValuesOfKey(byte[] rawContent, string keyToFind)
        {
            List<string> list = new List<string>();
            int index_i = 0;
            for (int i = 0; i < rawContent.Length; i++)
            {
                int index_j = -2;
                if (rawContent[i] == 0) index_j = i - 1;
                if (i == rawContent.Length - 1) index_j = i;
                if (index_j < index_i) continue;

                string part = System.Text.UTF8Encoding.UTF8.GetString(rawContent, index_i, index_j - index_i);

                var lines = part.Split('\n');
                foreach (var line in lines)
                {
                    var spaceIndex = line.IndexOf(' ');
                    if (spaceIndex < 0) continue;

                    var key = line.Substring(0, spaceIndex);
                    var val = line.Substring(spaceIndex + 1);

                    if (key == keyToFind)
                    {
                        list.Add(val);
                    }
                }

                index_i = i + 1;
            }

            return list.ToArray();
        }

        string[] ReadTextLines(string name)
        {
            return File.ReadAllLines(Path.Combine(GitDir, name));
        }
        /// <summary>
        /// 指定的文件是否寸
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool FileExists(string name)
        {
            return File.Exists(Path.Combine(GitDir, name));
        }
        /// <summary>
        /// 指定的Object是否存在
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        bool ObjectExists(string hash)
        {
            string pref = hash.Substring(0, 2);
            string remain = hash.Substring(2);
            return File.Exists(Path.Combine(GitDir, "objects", pref, remain)) || PackedObjectExists(hash);
        }

        /// <summary>
        /// 指定的PackedObject 是否存在
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        bool PackedObjectExists(string hash)
        {
            string file;
            uint offset = GetPackedObjectFileOffset(hash, out file);

            return offset > 0;
        }
    }
}
