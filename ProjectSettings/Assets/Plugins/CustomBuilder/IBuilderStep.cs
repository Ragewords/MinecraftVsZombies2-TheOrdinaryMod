using System;

namespace CustomBuilder
{
    /// <summary>
    /// 打包步骤
    /// 实现此类可以自定义打包过程中的步骤
    /// </summary>
    public interface IBuilderStep
    {
        /// <summary>
        /// 打包前动作
        /// 通常用于额外数据生成
        /// </summary>
        /// <param name="option">BuildOption</param>
        /// <remarks>
        /// 为保证工作空间清洁，若此动作修改了部分文件，请在打包后动作内还原对应的文件。
        /// </remarks>
        void ActionBefore(object option);

        /// <summary>
        /// 打包后动作
        /// 通常用于额外数据清理。
        /// </summary>
        void ActionAfter();
    }

    /// <summary>
    /// 打包步骤配置
    /// 可以配置如何运行打包配置。
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class BuilderStepAttribute : Attribute, IComparable<BuilderStepAttribute>
    {
        /// <summary>
        /// 执行顺序优先级
        /// 优先级越低越先执行
        /// </summary>
        public int Priority { get; set; } = 0;
        public BuilderStepAttribute(int priority = 0)
        {
            Priority = priority;
        }

        public int CompareTo(BuilderStepAttribute obj)
        {
            if (obj == null) return 1;

            return Priority.CompareTo(obj.Priority);
        }
    }
}
