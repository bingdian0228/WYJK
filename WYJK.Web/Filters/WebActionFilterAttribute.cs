using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WYJK.Data;
using WYJK.Entity;

namespace WYJK.Web.Filters
{
    /// <summary>
    /// 权限验证 
    /// </summary>
    public class WebActionFilterAttribute : ActionFilterAttribute
    {

        /// <summary>
        /// 权限验证 
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var isAuth = false;

            var action = filterContext.ActionDescriptor.ActionName;
            var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;

            HttpCookie authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];//获取cookie
            FormsAuthenticationTicket Ticket = FormsAuthentication.Decrypt(authCookie.Value);//解密
            Users users = JsonConvert.DeserializeObject<Users>(Ticket.UserData);


            if (users.PermissionList != null) {
                isAuth = users.PermissionList.Any(n => n.Controller.ToLower() == controller.ToLower() && n.Action.ToLower() == action.ToLower());
            }

            if (isAuth == false) {
                ContentResult Content = new ContentResult();
                Content.Content = "<script type='text/javascript'>alert('权限验证不通过！');history.go(-1);</script>";
                filterContext.Result = Content;
                
            }

            base.OnActionExecuting(filterContext);
        }
    }
}