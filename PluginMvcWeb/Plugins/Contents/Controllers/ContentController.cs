namespace PluginMvc.Contents.Controllers
{
    using System.Web.Mvc;

    using PluginMvc.Contents.Models;

    /// <summary>
    /// 内容控制器。
    /// </summary>
    public class ContentController : Controller
    {
        public ActionResult List()
        {
            ContentItem contentItem = new ContentItem { Id = 1, Title = "fuck_list 111。" };

            return View(contentItem);
        }

        public ActionResult Index()
        {
            ContentItem contentItem = new ContentItem { Id = 1, Title = "abc_index 2" };

            return View(contentItem);
        }
    }
}