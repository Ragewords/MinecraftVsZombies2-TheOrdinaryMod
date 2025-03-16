using System.Runtime.CompilerServices;

namespace MukioI18n
{
    /// <summary>
    /// 翻译标志
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class TranslateMsgAttribute : System.Attribute
    {
        readonly string comment = null;
        readonly string context = null;
        readonly string reference = null;

        /// <summary>
        /// 将此字符串标记为翻译源字符串
        /// </summary>
        public TranslateMsgAttribute(
            [CallerLineNumber] int callingFileLineNumber = 0,
            [CallerFilePath]  string callingFilePath = ""
        )
        {
            reference = callingFilePath + ":" + callingFileLineNumber;
        }
        /// <summary>
        /// 将此字符串标记为翻译源字符串
        /// </summary>
        /// <param name="comment">注释</param>
        public TranslateMsgAttribute(
            string comment,
            [CallerLineNumber] int callingFileLineNumber = 0,
            [CallerFilePath] string callingFilePath = ""
        )
        {
            this.comment = comment;
            reference = callingFilePath + ":" + callingFileLineNumber;
        }
        /// <summary>
        /// 将此字符串标记为翻译源字符串。
        /// 
        /// 带上下文的字符串需要使用 _p 方法提供上下文翻译。
        /// </summary>
        /// <param name="comment">注释</param>
        /// <param name="context">上下文</param>
        public TranslateMsgAttribute(
            string comment, 
            string context,
            [CallerLineNumber] int callingFileLineNumber = 0,
            [CallerFilePath] string callingFilePath = ""
        )
        {
            this.comment = comment;
            this.context = context;
            reference = callingFilePath + ":" + callingFileLineNumber;
        }

        public string Comment => comment;

        public string Reference => reference;

        public string Context => context;
    }
}
