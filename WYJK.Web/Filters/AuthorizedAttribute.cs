using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using WYJK.Data;
using WYJK.Data.IService;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;

namespace WYJK.Web.Filters
{
    /// <summary>
    /// 身份验证过滤器
    /// </summary>
    public class CustomAuthorizedAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var isAuth = false;
            if (!filterContext.RequestContext.HttpContext.Request.IsAuthenticated)
            {
                isAuth = false;
            }
            else
            {
                if (filterContext.RequestContext.HttpContext.User.Identity != null)
                {
                    var actionDescriptor = filterContext.ActionDescriptor;
                    var action = actionDescriptor.ActionName;

                    var controllerDescriptor = actionDescriptor.ControllerDescriptor;
                    var controller = controllerDescriptor.ControllerName;

                    var ticket = (filterContext.RequestContext.HttpContext.User.Identity as FormsIdentity).Ticket;
                    Users users = JsonConvert.DeserializeObject<Users>(ticket.UserData);

                   
                    //获取权限列表
                    List<Permissions> permissionList = DbHelper.Query<Permissions>($@"select PERMISSIONS.* from Users
left join UserRole on users.UserID=UserRole.UserID
left join RolePermission on userrole.RoleID = RolePermission.RoleID
left join Permissions on RolePermission.PermissionID = PERMISSIONS.PermissionID
where users.UserName='{users.UserName}'");
                   
                    if (permissionList != null)
                    {
                        isAuth = permissionList.Any(n => n.Controller.ToLower() == controller.ToLower() && n.Action.ToLower() == action.ToLower());
                    }
                    //判断当前用户有没有访问此功能的权限
                }
            }
        }
    }
}