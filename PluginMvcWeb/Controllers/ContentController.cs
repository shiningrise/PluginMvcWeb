namespace PluginMvcWeb.Controllers
{
    using System.Web.Mvc;

    using System;

    /// <summary>
    /// 内容控制器。
    /// </summary>
    public class ContentController : Controller
    {
        public ActionResult List()
        {
         //   ContentItem contentItem = new ContentItem { Id = 1, Title = "cccccccccccc_list 111。" + AppDomain.CurrentDomain.DynamicDirectory };

            return View();
        }

        public ActionResult Index()
        {
        //    ContentItem contentItem = new ContentItem { Id = 1, Title = "abc_index 2" };

            return View();
        }
    }
}