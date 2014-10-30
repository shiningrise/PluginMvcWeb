namespace PluginMvc.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web.Hosting;

    /// <summary>
    /// 插件加载器。
    /// </summary>
    public static class PluginLoader
    {
        /// <summary>
        /// 插件目录。
        /// </summary>
        private static readonly DirectoryInfo PluginFolder;

        /// <summary>
        /// 插件临时目录。
        /// </summary>
        private static readonly DirectoryInfo TempPluginFolder;

        private static readonly List<string> FrameworkPrivateBinFiles;

        /// <summary>
        /// 初始化。
        /// </summary>
        static PluginLoader()
        {
            PluginFolder = new DirectoryInfo(HostingEnvironment.MapPath("~/Plugins"));
            TempPluginFolder = new DirectoryInfo(AppDomain.CurrentDomain.DynamicDirectory);
            //new DirectoryInfo(HostingEnvironment.MapPath("~/App_Data/Dependencies"));
            //System.AppDomain.CurrentDomain.BaseDirectory 
            var FrameworkPrivateBin = new DirectoryInfo(System.AppDomain.CurrentDomain.SetupInformation.PrivateBinPath);
            FrameworkPrivateBinFiles = FrameworkPrivateBin.GetFiles().Select(p => p.Name).ToList();
        }

        /// <summary>
        /// 加载插件。
        /// </summary>
        public static IEnumerable<PluginDescriptor> Load()
        {
            List<PluginDescriptor> plugins = new List<PluginDescriptor>();

            //程序集复制到临时目录。
            CopyToDynamicDirectory();

            IEnumerable<Assembly> assemblies = null;

            //加载 bin 目录下的所有程序集。
            assemblies = AppDomain.CurrentDomain.GetAssemblies();

            plugins.AddRange(GetAssemblies(assemblies));

            //加载临时目录下的所有程序集。
            assemblies = TempPluginFolder.GetFiles("*.dll", SearchOption.AllDirectories).Select(x => Assembly.LoadFile(x.FullName));

            plugins.AddRange(GetAssemblies(assemblies));

            return plugins;
        }

        /// <summary>
        /// 获得插件信息。
        /// </summary>
        /// <param name="pluginType"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static PluginDescriptor GetPluginInstance(Type pluginType, Assembly assembly)
        {
            if (pluginType != null)
            {
                var plugin = (IPlugin)Activator.CreateInstance(pluginType);

                if (plugin != null)
                {
                    return new PluginDescriptor(plugin, assembly, assembly.GetTypes());
                }
            }

            return null;
        }

        /// <summary>
        /// 程序集复制到临时目录。
        /// </summary>
        private static void CopyToDynamicDirectory()
        {
            Directory.CreateDirectory(PluginFolder.FullName);
            Directory.CreateDirectory(TempPluginFolder.FullName);

            Debug.WriteLine("\r\nFrameworkPrivateBinFiles:");
            foreach (var item in FrameworkPrivateBinFiles)
            {
                Debug.WriteLine(item);
            }

            //清理临时文件。
            Debug.WriteLine("清理临时文件");
            var pluginsTemp = TempPluginFolder.GetFiles("*.dll", SearchOption.AllDirectories).Where(p => FrameworkPrivateBinFiles.Contains(p.Name) == false);
            foreach (var file in pluginsTemp)
            {
                try
                {
                    Debug.WriteLine(file.FullName);
                    file.Delete();
                }
                catch (Exception)
                {

                }

            }

            //复制插件进临时文件夹。
            Debug.WriteLine("复制插件进临时文件夹");
            var plugins = PluginFolder.GetFiles("*.dll", SearchOption.AllDirectories).Where(p => FrameworkPrivateBinFiles.Contains(p.Name) == false);
            //var plugins = PluginFolder.GetFiles("*.dll", SearchOption.AllDirectories);
            foreach (var plugin in plugins)
            {
                try
                {
                    var di = Directory.CreateDirectory(TempPluginFolder.FullName);
                    var srcPath = plugin.FullName;
                    var toPath = Path.Combine(di.FullName, plugin.Name);
                    Debug.WriteLine(string.Format("from:\t{0}", srcPath));
                    Debug.WriteLine(string.Format("to:\t{0}", toPath));
                    File.Copy(srcPath,toPath,true);
                }
                catch (Exception)
                {

                }
            }
        }

        /// <summary>
        /// 根据程序集列表获得该列表下的所有插件信息。
        /// </summary>
        /// <param name="assemblies">程序集列表</param>
        /// <returns>插件信息集合。</returns>
        private static IEnumerable<PluginDescriptor> GetAssemblies(IEnumerable<Assembly> assemblies)
        {
            IList<PluginDescriptor> plugins = new List<PluginDescriptor>();

            foreach (var assembly in assemblies)
            {
                var pluginTypes = assembly.GetTypes().Where(type => type.GetInterface(typeof(IPlugin).Name) != null && type.IsClass && !type.IsAbstract);

                foreach (var pluginType in pluginTypes)
                {
                    var plugin = GetPluginInstance(pluginType, assembly);

                    if (plugin != null)
                    {
                        plugins.Add(plugin);
                    }
                }
            }

            return plugins;
        }
    }
}