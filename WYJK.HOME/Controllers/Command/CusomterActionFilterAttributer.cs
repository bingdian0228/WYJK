using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WYJK.HOME.Controllers
{
    public class CusomterActionFilterAttributer : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            object[] attrsAnonymous = filterContext.ActionDescriptor.GetCustomAttributes(typeof(NeedLoginAttribute), true);
            if (attrsAnonymous.Length >0)
            {
                if (filterContext.HttpContext.Session["UserInfo"] == null)
                {
                    filterContext.RequestContext.HttpContext.Response.Redirect("~/User/Login");
                }
            }

        }
    }
}