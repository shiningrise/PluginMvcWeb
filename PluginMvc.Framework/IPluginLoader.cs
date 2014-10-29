namespace PluginMvc.Framework
{
    using System.Collections.Generic;

    /// <summary>
    /// 插件加载器。
    /// </summary>
    public interface IPluginLoader
    {
        /// <summary>
        /// 加载插件。
        /// </summary>
        /// <returns></returns>
        IEnumerable<PluginDescriptor> Load();
    }
}