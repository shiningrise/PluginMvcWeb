namespace PluginMvc.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web;
    using System.Web.Hosting;
    using System.Xml;

    /// <summary>
    /// 插件加载器。
    /// </summary>
    public static class PluginLoader
    {
        #region Const

        private const string InstalledPluginsFilePath = "~/App_Data/InstalledPlugins.txt";
        private const string PluginsPath = "~/Plugins";
        private const string ShadowCopyPath = "~/App_Data/Plugins";

        #endregion

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
            PluginFolder = new DirectoryInfo(HostingEnvironment.MapPath(PluginsPath));
            TempPluginFolder = new DirectoryInfo(AppDomain.CurrentDomain.DynamicDirectory);
#if DEBUG
            TempPluginFolder = new DirectoryInfo(HostingEnvironment.MapPath(ShadowCopyPath));
#endif
            var FrameworkPrivateBin = new DirectoryInfo(System.AppDomain.CurrentDomain.SetupInformation.PrivateBinPath);
            FrameworkPrivateBinFiles = FrameworkPrivateBin.GetFiles().Select(p => p.Name).ToList();
        }

        /// <summary>
        /// 加载插件。
        /// </summary>
        public static IEnumerable<PluginDescriptor> Load()
        {
            List<PluginDescriptor> plugins = new List<PluginDescriptor>();

            if (PluginFolder == null)
                throw new ArgumentNullException("pluginFolder");

            //create list (<file info, parsed plugin descritor>)
            var descriptionFiles = new List<KeyValuePair<FileInfo, PluginDescriptor>>();
            //add display order and path to list
            foreach (var descriptionFile in PluginFolder.GetFiles("Description.txt", SearchOption.AllDirectories))
            {
                if (!IsPackagePluginFolder(descriptionFile.Directory))
                    continue;

                //parse file
                var pluginDescriptor = PluginFileParser.ParsePluginDescriptionFile(descriptionFile.FullName);

                //populate list
                descriptionFiles.Add(new KeyValuePair<FileInfo, PluginDescriptor>(descriptionFile, pluginDescriptor));
            }

            //sort list by display order. NOTE: Lowest DisplayOrder will be first i.e 0 , 1, 1, 1, 5, 10
            //it's required: http://www.nopcommerce.com/boards/t/17455/load-plugins-based-on-their-displayorder-on-startup.aspx
            descriptionFiles.Sort((firstPair, nextPair) => firstPair.Value.DisplayOrder.CompareTo(nextPair.Value.DisplayOrder));
            //return result;

            //程序集复制到临时目录。
            CopyToTempPluginFolderDirectory(descriptionFiles);

            //加载 bin 目录下的所有程序集。
            IEnumerable<Assembly>  assemblies = AppDomain.CurrentDomain.GetAssemblies();
            //加载临时目录下的所有程序集。
            assemblies = assemblies.Union(TempPluginFolder.GetFiles("*.dll", SearchOption.AllDirectories).Select(x => Assembly.LoadFile(x.FullName)));
            plugins.AddRange(InitPlugins(assemblies, descriptionFiles));

            return plugins;
        }

        /// <summary>
        /// Determines if the folder is a bin plugin folder for a package
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private static bool IsPackagePluginFolder(DirectoryInfo folder)
        {
            if (folder == null) return false;
            if (folder.Parent == null) return false;
            if (!folder.Parent.Name.Equals("Plugins", StringComparison.InvariantCultureIgnoreCase)) return false;
            return true;
        }

        /// <summary>
        /// Gets the full path of InstalledPlugins.txt file
        /// </summary>
        /// <returns></returns>
        private static string GetInstalledPluginsFilePath()
        {
            var filePath = HostingEnvironment.MapPath(InstalledPluginsFilePath);
            return filePath;
        }

        /// <summary>
        /// 获得插件信息。
        /// </summary>
        /// <param name="pluginType"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static IPlugin GetPluginInstance(Type pluginType, Assembly assembly, IEnumerable<Assembly> assemblies)
        {
            if (pluginType != null)
            {
                var plugin = (IPlugin)Activator.CreateInstance(pluginType);
                return plugin;
                //if (plugin != null)
                //{
                //    var assems = assemblies.Where(p => plugin.DependentAssembly.Contains(p.GetName().Name)).ToList();
                //    return new PluginDescriptor(plugin, assembly, assembly.GetTypes(), assems);//
                //}
            }

            return null;
        }

        /// <summary>
        /// 程序集复制到临时目录。
        /// </summary>
        private static void CopyToTempPluginFolderDirectory(List<KeyValuePair<FileInfo, PluginDescriptor>> pluginDescriptions)
        {
            var installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile(GetInstalledPluginsFilePath());
            Directory.CreateDirectory(PluginFolder.FullName);
            Directory.CreateDirectory(TempPluginFolder.FullName);

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
            //var pluginDirectories = PluginFolder.GetDirectories().Where(p => installedPluginSystemNames.Contains(p.Name)).ToList();
            var list = pluginDescriptions.Where(p=>installedPluginSystemNames.Contains(p.Value.SystemName));
            foreach (var plugin in list)
            {
                var PluginFileNames = plugin.Value.PluginFileName.Split(',');
                var dir = new DirectoryInfo(Path.Combine(plugin.Key.Directory.FullName, "bin"));
                var plugindlls = dir.GetFiles("*.dll", SearchOption.TopDirectoryOnly).Where(p => PluginFileNames.Contains(p.Name) == true && FrameworkPrivateBinFiles.Contains(p.Name) == false);
                foreach (var plugindll in plugindlls)
                {
                    try
                    {
                        var srcPath = plugindll.FullName;
                        var toPath = Path.Combine(TempPluginFolder.FullName, plugindll.Name);
#if DEBUG
                        Debug.WriteLine(string.Format("from:\t{0}", srcPath));
                        Debug.WriteLine(string.Format("to:\t{0}", toPath));
#endif
                        File.Copy(srcPath, toPath, true);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        /// <summary>
        /// 根据程序集列表获得该列表下的所有插件信息。
        /// </summary>
        /// <param name="assemblies">程序集列表</param>
        /// <returns>插件信息集合。</returns>
        private static IEnumerable<PluginDescriptor> InitPlugins(IEnumerable<Assembly> assemblies, List<KeyValuePair<FileInfo, PluginDescriptor>> descriptionFiles)
        {
            IList<PluginDescriptor> descriptors = new List<PluginDescriptor>();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var pluginTypes = assembly.GetTypes().Where(type => type.GetInterface(typeof(IPlugin).Name) != null && type.IsClass && !type.IsAbstract);

                    foreach (var pluginType in pluginTypes)
                    {
                        if (pluginType != null)
                        {
                            var plugin = (IPlugin)Activator.CreateInstance(pluginType);
                            var descriptor = descriptionFiles.Where(p => p.Value.SystemName == plugin.Name).Select(p=>p.Value).FirstOrDefault();
                            if(descriptor != null)
                            {
                                descriptor.Init(plugin, assemblies);
                                descriptors.Add(descriptor);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(assembly.FullName);
                    Debug.WriteLine(ex.Message);
                    //    throw ex;
                }

            }

            return descriptors;
        }
    }
}