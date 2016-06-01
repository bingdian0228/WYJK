using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WYJK.HOME.Controllers
{
    [CusomterActionFilterAttributer]
    public class BaseController : Controller
    {
        public String WeekString = "";
        protected override IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state)
        {
            DateTime now = DateTime.Now;
            int n = (int)now.DayOfWeek;
            string[] weekDays = { "星期天", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
            requestContext.HttpContext.Session["WeekString"] = weekDays[n];
            return base.BeginExecute(requestContext, callback, state);
        }
    }
}