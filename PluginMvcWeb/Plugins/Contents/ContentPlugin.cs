namespace PluginMvc.Contents
{
    using System.Web.Mvc;
    using System.Web.Routing;

    using PluginMvc.Framework;

    /// <summary>
    /// 内容插件。
    /// </summary>
    public class ContentPlugin : IPlugin
    {
        public string Name
        {
            get { return "Contents"; }
        }

        public void Initialize()
        {
            //RouteTable.Routes.MapRoute(
            //    "Default",                                              // Route name
            //    "{controller}/{action}/{id}",                           // URL with parameters
            //    new { controller = "Home", action = "Index", id = "" }  // Parameter defaults
            //);

            RouteTable.Routes.MapRoute(
                name: "content",
                url: "content/{controller}/{action}/{id}",
                defaults: new { controller = "Content", action = "Index", id = UrlParameter.Optional, pluginName = this.Name }
            );
        }

        public void Unload()
        {
            RouteTable.Routes.Remove(RouteTable.Routes["content"]);
        }
    }
}