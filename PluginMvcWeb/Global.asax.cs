namespace PluginMvcWeb
{
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Diagnostics;
    using System;

    /// <summary>
    /// MVC 应用程序。
    /// </summary>
    public class MvcApplication : HttpApplication
    {
        /// <summary>
        /// 应用程序启动。
        /// </summary>
        protected void Application_Start()
        {
            var logfile = string.Format("{0}App_Data\\{1:yyMMdd}.log", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, DateTime.Now);
#if DEBUG
            logfile = string.Format("{0}App_Data\\{1:yyMMddhhmmss}.log", System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, DateTime.Now);
#endif
            var traceListener = new TextWriterTraceListener(logfile);
            Debug.Listeners.Add(traceListener);

            //Debug.WriteLine(logfile);
            //Trace.WriteLine(logfile);

            RouteCollection routes = RouteTable.Routes;
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

        }
    }
}