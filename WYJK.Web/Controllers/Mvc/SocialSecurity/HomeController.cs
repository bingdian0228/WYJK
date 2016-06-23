using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WYJK.Entity;
using WYJK.Web.Filters;

namespace WYJK.Web.Controllers
{
    [Authorize]
    [ErrorAttribute]
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                string name = HttpContext.User.Identity.Name;
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        /// <summary>
        /// 更新当前在线用户
        /// </summary>
        /// <param name="onlineUser"></param>
        /// <returns></returns>
        public ActionResult UpdateCurrentOnlineUser(OnlineUser onlineUser)
        {
            onlineUser.ActiveTime = DateTime.Now;
            List<OnlineUser> currentOnlineUserList = new List<OnlineUser>();
            List<OnlineUser> onlineUserList = HttpContext.Cache["CurrentUser"] as List<OnlineUser>;
            if (onlineUserList == null)
            {
                onlineUserList = new List<OnlineUser>() { onlineUser };
                HttpContext.Cache.Insert("CurrentUser", onlineUserList);
            }
            else {
                bool flag = false;
                foreach (var item in onlineUserList)
                {
                    if (item.UserName == onlineUser.UserName)
                    {
                        item.ActiveTime = onlineUser.ActiveTime;
                        flag = true;
                        break;
                    }
                }

                if (flag == false)
                {
                    onlineUserList.Add(onlineUser);
                }
                HttpContext.Cache.Insert("CurrentUser", onlineUserList);
            }

            return Json(new { status = true, message = "当前用户更新成功" });
        }
    }
}