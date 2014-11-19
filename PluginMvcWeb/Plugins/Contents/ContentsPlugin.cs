namespace Plugin.Contents
{
    using System.Web.Mvc;
    using System.Web.Routing;

    using PluginMvc.Framework;
    using System.Diagnostics;
    using System;
    using System.Reflection;

    /// <summary>
    /// 内容插件。
    /// </summary>
    public class ContentsPlugin : PluginBase, IPlugin
    {
        public override string Name
        {
            get
            {
                return "Contents";
            }
        }

        public override System.Collections.Generic.List<string> DependentAssembly
        {
            get
            {
                var assems = new System.Collections.Generic.List<string>();
                assems.Add("Plugin.Contents.Models");
                return assems;
            }
        }

        public override void Initialize()
        {

            RouteTable.Routes.MapRoute(
                name: this.Name,
                url: this.Name + "/{controller}/{action}/{id}",
                defaults: new { controller = "Content", action = "Index", id = UrlParameter.Optional, pluginName = this.Name }
            );
        }
    }
}