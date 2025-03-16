using System;

namespace CustomBuilder
{
    /// <summary>
    /// 打包前执行Attribute.
    /// 若在某方法上附上此Attribute，则会在打包前调用此方法。
    /// 此方法已过时，请使用IBuilderStep代替。
    /// </summary>
    /// <see cref="IBuilderStep"/>
    [Obsolete("Use IBuilderStep instead.")]
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class ExecuteBeforeBuildAttribute : Attribute
    {
        // This is a positional argument
        public ExecuteBeforeBuildAttribute()
        {
        }
    }
}
