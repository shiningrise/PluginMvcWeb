namespace Admin
{
    using System.Web.Mvc;
    using System.Web.Routing;

    using PluginMvc.Framework;

    /// <summary>
    /// 内容插件。
    /// </summary>
    public class AdminPlugin : PluginBase,IPlugin
    {
        public override void Initialize()
        {
            //RouteTable.Routes.MapRoute(
            //    "Default",                                              // Route name
            //    "{controller}/{action}/{id}",                           // URL with parameters
            //    new { controller = "Home", action = "Index", id = "" }  // Parameter defaults
            //);

            RouteTable.Routes.MapRoute(
                name: this.Name,
                url: this.Name + "/{controller}/{action}/{id}",
                defaults: new { controller = "Content", action = "Index", id = UrlParameter.Optional, pluginName = this.Name }
            );
        }
    }
}