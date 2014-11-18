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