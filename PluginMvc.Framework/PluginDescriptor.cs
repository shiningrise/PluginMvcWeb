﻿namespace PluginMvc.Framework
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Web.Mvc;
    using System.Linq;

    /// <summary>
    /// 插件信息。
    /// </summary>
    public class PluginDescriptor
    {
        /// <summary>
        /// 控制器类型字典。
        /// </summary>
        private readonly IDictionary<string, Type> _controllerTypes = new Dictionary<string, Type>();

        /// <summary>
        /// 构造器。
        /// </summary>
        public PluginDescriptor(IPlugin plugin, Assembly assembly, IEnumerable<Type> types, IEnumerable<Assembly> dependentAssemblys = null)
        {
            this.Plugin = plugin;
            this.Assembly = assembly;
            this.Types = types;

            this._controllerTypes = new Dictionary<string, Type>();

            foreach (var type in types)
            {
                this.AddControllerType(type);
            }
            if (DependentAssemblys == null)
            {
                DependentAssemblys = new List<Assembly>();
            }
            DependentAssemblys.AddRange(dependentAssemblys);
        }

        public PluginDescriptor()
        {
            // TODO: Complete member initialization
            this._controllerTypes = new Dictionary<string, Type>();
        }

        public void Init(IPlugin plugin, IEnumerable<System.Reflection.Assembly> assemblies)
        {
            this.Plugin = plugin;
            this.Assembly = plugin.GetType().Assembly;
            this.Types = plugin.GetType().Assembly.GetTypes();

            foreach (var type in Types)
            {
                this.AddControllerType(type);
            }
            if (DependentAssemblys == null)
            {
                DependentAssemblys = new List<Assembly>();
            }
            var assems = assemblies.Where(p => plugin.DependentAssembly.Contains(p.GetName().Name)).ToList();
            DependentAssemblys.AddRange(assems);

        }

        /// <summary>
        /// 插件信息。
        /// </summary>
        public IPlugin Plugin { get; private set; }

        /// <summary>
        /// 程序集。
        /// </summary>
        public Assembly Assembly { get; private set; }

        public List<Assembly> DependentAssemblys { get; private set; }

        /// <summary>
        /// 类型。
        /// </summary>
        public IEnumerable<Type> Types { get; private set; }

        /// <summary>
        /// 根据控制器类型名称获得控制器类型。
        /// </summary>
        /// <param name="coltrollerTypeName">控制器类型名称。</param>
        /// <returns>控制器类型。</returns>
        public Type GetControllerType(string coltrollerTypeName)
        {
            if (this._controllerTypes.ContainsKey(coltrollerTypeName))
            {
                return this._controllerTypes[coltrollerTypeName];
            }

            return null;
        }

        /// <summary>
        /// 增加控制器类型。
        /// </summary>
        /// <param name="type">类型。</param>
        private void AddControllerType(Type type)
        {
            if (type.GetInterface(typeof(IController).Name) != null && type.Name.Contains("Controller") && type.IsClass && !type.IsAbstract)
            {
                this._controllerTypes.Add(type.Name, type);
            }
        }

        /// <summary>
        /// Plugin type
        /// </summary>
        public virtual string PluginFileName { get; set; }

        /// <summary>
        /// Plugin type
        /// </summary>
        public virtual Type PluginType { get; set; }

        /// <summary>
        /// The assembly that has been shadow copied that is active in the application
        /// </summary>
        public virtual Assembly ReferencedAssembly { get; internal set; }

        /// <summary>
        /// The original assembly file that a shadow copy was made from it
        /// </summary>
        public virtual FileInfo OriginalAssemblyFile { get; internal set; }

        /// <summary>
        /// Gets or sets the plugin group
        /// </summary>
        public virtual string Group { get; set; }

        /// <summary>
        /// Gets or sets the friendly name
        /// </summary>
        public virtual string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the system name
        /// </summary>
        public virtual string SystemName { get; set; }

        /// <summary>
        /// Gets or sets the version
        /// </summary>
        public virtual string Version { get; set; }

        /// <summary>
        /// Gets or sets the supported versions of nopCommerce
        /// </summary>
        public virtual IList<string> SupportedVersions { get; set; }

        /// <summary>
        /// Gets or sets the author
        /// </summary>
        public virtual string Author { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public virtual int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the list of store identifiers in which this plugin is available. If empty, then this plugin is available in all stores
        /// </summary>
        public virtual IList<int> LimitedToStores { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether plugin is installed
        /// </summary>
        public virtual bool Installed { get; set; }



        public int CompareTo(PluginDescriptor other)
        {
            if (DisplayOrder != other.DisplayOrder)
                return DisplayOrder.CompareTo(other.DisplayOrder);

            return FriendlyName.CompareTo(other.FriendlyName);
        }

        public override string ToString()
        {
            return FriendlyName;
        }

        public override bool Equals(object obj)
        {
            var other = obj as PluginDescriptor;
            return other != null &&
                SystemName != null &&
                SystemName.Equals(other.SystemName);
        }

        public override int GetHashCode()
        {
            return SystemName.GetHashCode();
        }

    }
}