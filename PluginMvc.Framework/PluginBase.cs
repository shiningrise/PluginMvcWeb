using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace PluginMvc.Framework
{
    public abstract class PluginBase : IPlugin
    {
        public virtual string Name
        {
            get 
            {
                var AssemblyName = Assembly.GetExecutingAssembly().FullName;
                Debug.WriteLine(AssemblyName);
                return AssemblyName;
            }
        }

        public virtual List<string> DependentAssembly
        {
            get { return new List<string>(); }
        }

        public virtual void Initialize()
        {
            //RouteTable.Routes.MapRoute(
            //    "Default",                                              // Route name
            //    "{controller}/{action}/{id}",                           // URL with parameters
            //    new { controller = "Home", action = "Index", id = "" }  // Parameter defaults
            //);

            RouteTable.Routes.MapRoute(
                name: this.Name,
                url: this.Name + "/{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, pluginName = this.Name }
            );
        }

        public virtual void Unload()
        {
            RouteTable.Routes.Remove(RouteTable.Routes[this.Name]);
        }

    }
}
