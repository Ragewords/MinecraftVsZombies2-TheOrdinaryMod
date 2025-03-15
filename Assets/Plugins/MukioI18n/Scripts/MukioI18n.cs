using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NGettext;
using System.Globalization;
using System.IO;
using System;
using UnityEngine.Networking;
using System.Threading.Tasks;

namespace MukioI18n
{
    /// <summary>
    /// 翻译接口
    /// </summary>
    public interface ITranslator
    {
        /// <summary>
        /// 翻译
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        string _(string text);
        /// <summary>
        /// 翻译
        /// </summary>
        /// <param name="text">带有占位符的源文本，占位符为{0},{1}...</param>
        /// <param name="args">替换翻译文本内的占位符</param>
        /// <returns></returns>
        string _(string text, params object[] args);
        /// <summary>
        /// 复数形式的翻译
        /// </summary>
        /// <param name="text">单数形式文本</param>
        /// <param name="pluralText">复数形式文本</param>
        /// <param name="n">当前数字</param>
        /// <returns></returns>
        string _n(string text, string pluralText, long n);
        /// <summary>
        /// 复数形式的翻译
        /// </summary>
        /// <param name="text">单数形式文本</param>
        /// <param name="pluralText">复数形式文本</param>
        /// <param name="n">当前数字</param>
        /// <param name="args"></param>
        /// <returns></returns>
        string _n(string text, string pluralText, long n, params object[] args);
        /// <summary>
        /// 带上下文的翻译
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="text">源文本</param>
        /// <returns></returns>
        string _p(string context, string text);
        /// <summary>
        /// 带上下文的翻译
        /// </summary>
        /// <param name="context"></param>
        /// <param name="text"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        string _p(string context, string text, params object[] args);

        /// <summary>
        /// 带上下文和复数形式的翻译
        /// </summary>
        /// <param name="context"></param>
        /// <param name="text"></param>
        /// <param name="pluralText"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        string _pn(string context, string text, string pluralText, long n);

        string _pn(string context, string text, string pluralText, long n, params object[] args);
    }

    public class I18n : ITranslator
    {
        /// <summary>
        /// 载入一个目录下的指定语言的翻译文件
        /// </summary>
        /// <param name="directory">相对于StreamingAsset的相对路径</param>
        /// <param name="culture"></param>
        public void Load(string directory, CultureInfo culture)
        {
            var fullPath = Path.Combine(directory, culture.Name + ".mo");
            try
            {
                LoadFile(fullPath, culture);
            }
            catch
            {
                try
                {
                    fullPath = Path.Combine(directory, culture.Name.Replace("-", "_") + ".mo");
                    LoadFile(fullPath, culture);
                }
                catch (FileNotFoundException)
                {
                    throw new FileNotFoundException($"没有找到语言{culture}对应的文件。路径：{fullPath}", fullPath);
                }
            }
        }

        /// <summary>
        /// 异步加载文件
        /// </summary>
        /// <param name="file">相对于StreamingAsset的相对路径</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public Task LoadFileAsync(string file, CultureInfo culture)
        {
            return Task.Run(() =>
            {
                LoadFile(file, culture);
            });
        }

        /// <summary>
        /// 加载一个语言翻译文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="culture"></param>
        public void LoadFile(string file, CultureInfo culture)
        {
            Stream stream = FileLoader.ReadStreamingAsset(file);

            var catalog = new Catalog(stream, culture);
            if (!Catalogs.ContainsKey(culture.Name))
            {
                Catalogs.Add(culture.Name, catalog);
            }
            else
            {
                Debug.LogWarning("重复载入了语言文件");
                Catalogs[culture.Name] = catalog;
            }

            stream.Close();
        }

        /// <summary>
        /// 在默认的目录中载入一个语言文件
        /// </summary>
        /// <param name="culture"></param>
        public void Load(CultureInfo culture)
        {
            Load("i18n", culture);
        }

        /// <summary>
        /// 添加一个空的Catalog
        /// </summary>
        /// <param name="culture"></param>
        public void AddEmpty(CultureInfo culture)
        {
            var catalog = new Catalog(culture);
            if (!Catalogs.ContainsKey(culture.Name))
            {
                Catalogs.Add(culture.Name, catalog);
            }
        }

        Dictionary<string, ICatalog> Catalogs = new Dictionary<string, ICatalog>();

        /// <summary>
        /// 获取指定语言的翻译信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public ICatalog GetCatalog(CultureInfo info)
        {
            return Catalogs[info.Name];
        }

        /// <summary>
        /// 获取默认语言的翻译信息
        /// </summary>
        /// <returns></returns>
        public ICatalog GetCatalog()
        {
            if (!Catalogs.ContainsKey(CurrentLang.Name))
            {
                Debug.LogError($"尝试访问语言{CurrentLang.Name}，但未找到");
                return new Catalog();
            }
            return Catalogs[CurrentLang.Name];
        }

        /// <summary>
        /// 当前语言
        /// </summary>
        public CultureInfo CurrentLang { get; private set; } = null;

        /// <summary>
        /// 设置当前语言
        /// </summary>
        /// <param name="info"></param>
        public void SetLang(CultureInfo info)
        {
            CurrentLang = info;
            OnLanguageChange?.Invoke();
        }

        /// <summary>
        /// 当前语言发生改变
        /// </summary>
        public event Action OnLanguageChange;

        /// <summary>
        /// 卸载语言
        /// </summary>
        /// <remarks>
        /// 其实并没有什么作用，GC的时候可能才会卸载
        /// </remarks>
        /// <param name="info"></param>
        public void Unload(CultureInfo info)
        {
            if (Catalogs.ContainsKey(info.Name))
            {
                Catalogs.Remove(info.Name);
            }
        }

        /// <summary>
        /// 翻译
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string _(string text)
        {
            return GetCatalog().GetString(text);
        }

        /// <summary>
        /// 翻译
        /// </summary>
        /// <param name="text">带有占位符的源文本，占位符为{0},{1}...</param>
        /// <param name="args">替换翻译文本内的占位符</param>
        /// <returns></returns>
        public string _(string text, params object[] args)
        {
            return GetCatalog().GetString(text, args);
        }

        /// <summary>
        /// 复数形式的翻译
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pluralText"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public string _n(string text, string pluralText, long n)
        {
            return GetCatalog().GetPluralString(text, pluralText, n);
        }

        public string _n(string text, string pluralText, long n, params object[] args)
        {
            return GetCatalog().GetPluralString(text, pluralText, n, args);
        }

        /// <summary>
        /// 带上下文的翻译
        /// </summary>
        /// <param name="context"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public string _p(string context, string text)
        {
            return GetCatalog().GetParticularString(context, text);
        }

        public string _p(string context, string text, params object[] args)
        {
            return GetCatalog().GetParticularString(context, text, args);
        }
        /// <summary>
        /// 带上下文和复数形式的翻译
        /// </summary>
        /// <param name="context"></param>
        /// <param name="text"></param>
        /// <param name="pluralText"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public string _pn(string context, string text, string pluralText, long n)
        {
            return GetCatalog().GetParticularPluralString(context, text, pluralText, n);
        }

        public string _pn(string context, string text, string pluralText, long n, params object[] args)
        {
            return GetCatalog().GetParticularPluralString(context, text, pluralText, n, args);
        }
    }
}

