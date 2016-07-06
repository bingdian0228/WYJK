using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WYJK.HOME.Controllers
{
    public class BaseFilterController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string returnUrl = filterContext.HttpContext.Request.Url.ToString();

            if (filterContext.HttpContext.Session["UserInfo"] == null)
            {
                filterContext.Result = new RedirectResult("~/User/Login?returnUrl="+ returnUrl);
            }

            base.OnActionExecuting(filterContext);
        }


        protected void assignMessage(string msg, bool success)
        {
            TempData["Message"] = msg;
            TempData["MessageType"] = success;
        }

    }

}