﻿using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WYJK.Web.Models;
using System.IO;
using System.Drawing.Imaging;
using WYJK.Framework.Captcha;
using WYJK.Framework.Helpers;
using WYJK.Entity;
using WYJK.Data;
using Newtonsoft.Json;
using System.Web.Security;
using System.Collections.Generic;
using WYJK.Data.IService;
using WYJK.Data.ServiceImpl;
using System.Data.SqlClient;
using System.Text;

namespace WYJK.Web.Controllers.Mvc
{
    /// <summary>
    /// 员工
    /// </summary>
    //[Authorize]
    public class UserController : Controller
    {
        IUserService _userService = new UserService();


        ////
        //// GET: /Account/Login
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            string url = Request.QueryString["ReturnUrl"];
            return View();
        }

        public ActionResult Index() {
            return View();
        }
        public ActionResult Qc_back() {
            return View();
        }

        #region 显示验证码
        /// <summary>
        /// 显示验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public FileContentResult Captcha()
        {
            CaptchaOptions options = new CaptchaOptions
            {
                GaussianDeviation = 0.4,
                Height = 35,
                Background = NoiseLevel.Low,
                Line = NoiseLevel.Low
            };
            using (ICapatcha capatch = new FluentCaptcha())
            {
                capatch.Options = options;
                CaptchaResult captchaResult = capatch.DrawBackgroud().DrawLine().DrawText().Atomized().DrawBroder().DrawImage();
                using (captchaResult)
                {
                    MemoryStream ms = new MemoryStream();
                    captchaResult.Bitmap.Save(ms, ImageFormat.Gif);
                    Session["Captcha"] = captchaResult.Text;
                    return File(ms.ToArray(), "image/gif");
                }
            }
        }
        #endregion

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(UserViewModel model)
        {

            if (ModelState.IsValid)
            {
                if (model.VerificationCode.Equals(Session["Captcha"] + "", StringComparison.OrdinalIgnoreCase) == false)
                {
                    ViewBag.ErrorMessage = "验证码不正确";
                    return View(model);
                }
                string password = SecurityHelper.HashPassword(model.Password, model.Password);
                string sql = $"SELECT * FROM Users where UserName='{model.UserName}' and Password='{password}'";

                Users users = await DbHelper.QuerySingleAsync<Users>(sql);



                if (users != null)
                {

                    string data = JsonConvert.SerializeObject(users);
                    SetAuthCookie(users.UserName, data);
                    return RedirectToAction("Index", "Home");
                }
                ViewBag.ErrorMessage = "用户名或密码错误";
            }

            return View();
        }



        #region 私有方法
        /// <summary>
        /// 设置授权Cookie
        /// </summary>
        /// <param name="account"></param>
        /// <param name="data"></param>
        private void SetAuthCookie(string account, string data)
        {
            //FormsAuthenticationTicket Ticket = new FormsAuthenticationTicket(1, account, DateTime.Now, DateTime.Now.AddDays(1), false, data);
            //HttpCookie Cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(Ticket));//加密身份信息，保存至Cookie
            //Response.Cookies.Add(Cookie);



            int expiration = 1440;
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(2, account, DateTime.Now, DateTime.Now.AddDays(1), true, data);
            string cookieValue = FormsAuthentication.Encrypt(ticket);
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, cookieValue)
            {
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL,
                Domain = FormsAuthentication.CookieDomain,
                Path = FormsAuthentication.FormsCookiePath
            };
            if (expiration > 0)
            {
                cookie.Expires = DateTime.Now.AddMinutes(expiration);
            }

            //Response.Cookies.Remove(cookie.Name);
            Response.Cookies.Add(cookie);
        }
        #endregion

        #region 退出登录
        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();
            //HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, "")
            //{
            //    Expires = DateTime.Now.AddDays(-1)
            //};
            //Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
            //Response.Cookies.Add(cookie);
            FormsAuthentication.SignOut();

            return RedirectToAction("Login");
        }
        #endregion

        #region 用户角色权限
        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetAllRoles(RolesParameter parameter)
        {
            PagedResult<WYJK.Entity.Roles> rolesList = await _userService.GetAllRoles(parameter);
            return View(rolesList);
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <returns></returns>
        public ActionResult GetRolesList()
        {
            var roleList = DbHelper.Query<Entity.Roles>("select * from Roles").Select(n => new { id = n.RoleID, text = n.RoleName }).ToList();
            return Json(new { list = roleList }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCheckedRolesList(int userid)
        {
            var roleList = DbHelper.Query<Entity.Roles>("select * from Roles").Select(n => new TempRoles { id = n.RoleID, text = n.RoleName }).ToList();
            var checkedRoleList = DbHelper.Query<Entity.Roles>($"select * from UserRole where UserID={userid} ").Select(n => new { id = n.RoleID }).ToList();

            foreach (var role in roleList)
            {
                foreach (var checkedRole in checkedRoleList)
                {
                    if (role.id == checkedRole.id)
                    {
                        role.check = true;
                        break;
                    }

                }
            }
            return Json(new { list = roleList }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 新建添加
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult RolesAdd()
        {
            return View();
        }

        /// <summary>
        /// 保存添加
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> RolesAdd(RolesViewModel model)
        {
            if (!ModelState.IsValid) return View();
            //判断是否重名
            bool isExists = await _userService.IsExistsRole(model.RoleName);
            if (isExists)
            {
                ViewBag.ErrorMessage = "角色名称已存在";
                return View();
            }
            try
            {
                int roleID = await _userService.RoleAdd(model.ExtensionToModel());
                if (model.PermissionID != "")
                {
                    string sqlstr = string.Empty;
                    foreach (var id in model.PermissionID.Split(','))
                    {
                        sqlstr += $"insert into RolePermission values({roleID},{id});";
                    }
                    DbHelper.ExecuteSqlCommand(sqlstr, null);
                }
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "保存失败";
            }


            ViewBag.ErrorMessage = "保存成功";

            return RedirectToAction("GetAllRoles");
        }

        /// <summary>
        /// 新建角色编辑
        /// </summary>
        /// <param name="roldID"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> RolesEdit(int roldID)
        {
            Entity.Roles role = await _userService.GetRolesInfo(roldID);
            RolesViewModel model = role.ExtensionToViewModel();

            return View(model);
        }

        /// <summary>
        /// 保存编辑
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> RolesEdit(RolesViewModel viewModel)
        {
            if (!ModelState.IsValid) return View();
            Entity.Roles role = viewModel.ExtensionToModel();
            //判断是否重名
            string sql = "select COUNT(*) from roles where RoleName <>(select rolename from Roles where RoleID = @RoleID) and rolename = @rolename";

            int result = await DbHelper.QuerySingleAsync<int>(sql, new { RoleID = role.RoleID, RoleName = role.RoleName });
            if (result > 0)
            {
                ViewBag.ErrorMessage = "角色名已存在";
                return View(viewModel);
            }
            //更新
            bool flag = await _userService.UpdateRoles(role);

            DbHelper.ExecuteSqlCommand($"delete from RolePermission where RoleID={role.RoleID}", null);
            if (viewModel.PermissionID != "")
            {
                string sqlstr = string.Empty;
                foreach (var id in viewModel.PermissionID.Split(','))
                {
                    sqlstr += $"insert into RolePermission values({role.RoleID},{id});";
                }
                DbHelper.ExecuteSqlCommand(sqlstr, null);
            }

            //ViewBag.ErrorMessage = flag ? "更新成功" : "更新失败";

            return RedirectToAction("GetAllRoles");
        }

        /// <summary>
        /// 批量删除角色
        /// </summary>
        /// <param name="roleids"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> BatchDeleteRoles(int[] roleids)
        {
            //判断是否已经被引用
            int count = DbHelper.QuerySingle<int>($"select count(0) from UserRole where RoleID in({string.Join(",", roleids)})");
            if (count > 0)
                return Json(new { status = false, message = "该角色被引用，不能删除" });

            string roleid = string.Join(",", roleids);
            string sql = $"delete Roles where RoleID in ({roleid});" + $"delete from RolePermission where RoleID in({roleid})";
            int result = await DbHelper.ExecuteSqlCommandAsync(sql);
            return Json(new { status = true, message = "删除成功" });
        }

        /// <summary>
        /// 异步判断角色名称是否已存在
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public async Task<JsonResult> CheckRoleName(string roleName)
        {
            bool isExists = await _userService.IsExistsRole(roleName);
            return Json(!isExists, JsonRequestBehavior.AllowGet);
        }

        #endregion

        /// <summary>
        /// 获取所有权限
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> GetAllPermissions()
        {
            List<Permissions> permissionList = await _userService.GetAllPermissions();
            return View(permissionList);
        }

        public async Task<ActionResult> GetAllPermissionList()
        {
            List<Permissions> permissionList = await _userService.GetAllPermissions();

            return Json(new { list = permissionList.Select(n => new TempPermissions { id = n.PermissionID, pId = Convert.ToInt32(n.ParentID), name = n.Description, @checked = false }) }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetAllCheckedPermissionList(int roleID)
        {
            List<Permissions> permissionList1 = await _userService.GetAllPermissions();
            List<TempPermissions> permissionList = permissionList1.Select(n => new TempPermissions { id = n.PermissionID, pId = Convert.ToInt32(n.ParentID), name = n.Description }).ToList();
            List<Permissions> checkedPermissionList1 = DbHelper.Query<Permissions>($"select * from RolePermission where RoleID={roleID}").ToList();
            List<TempPermissions> checkedPermissionList = checkedPermissionList1.Select(n => new TempPermissions { id = n.PermissionID, pId = Convert.ToInt32(n.ParentID), name = n.Description }).ToList();
            foreach (var permission in permissionList)
            {
                foreach (var checkedPermission in checkedPermissionList)
                {
                    if (permission.id == checkedPermission.id)
                    {
                        permission.@checked = true;
                        break;
                    }
                }
            }

            return Json(new { list = permissionList }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 权限新建编辑
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> PermissionEdit(int? PermissionID)
        {
            Permissions model = null;
            if (PermissionID > 0)
            {
                model = await _userService.GetPermission(PermissionID);
            }
            return PartialView("_PermissionEdit", model);
        }

        /// <summary>
        /// 权限确认编辑
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> PermissionEdit(Permissions model)
        {
            //await Task.Delay(1000);
            if (model.PermissionID == 0)
            {
                //添加
                //权限编号重复
                if (await _userService.IsExistsPermission(model.Code))
                    return Json(new { status = false, message = "权限编号重复,请重新添加！" });

                bool flag = await _userService.PermissionAdd(model);

                return Json(new { status = flag, message = flag ? "添加成功！" : "添加失败！" });
            }
            else {
                //判断是否重名
                string sql = "select COUNT(*) from Permissions where Code <>(select Code from Permissions where PermissionID = @PermissionID) and Code = @Code";

                int result = await DbHelper.QuerySingleAsync<int>(sql, new { PermissionID = model.PermissionID, Code = model.Code });
                if (result > 0)
                {
                    return Json(new { status = false, message = "权限编号重复,请重新编辑！" });
                }
                //更新
                bool flag = await _userService.UpdatePermissions(model);

                return Json(new { status = flag, message = flag ? "更新成功！" : "更新失败！" });
            }
        }

        /// <summary>
        /// 获取员工列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUserList(UsersParameter parameter)
        {
            PagedResult<Users> userList = _userService.GetUserList(parameter);
            return View(userList);
        }

        /// <summary>
        /// 添加员工
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult UserAdd()
        {
            return View();
        }
        /// <summary>
        /// 添加员工
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UserAdd(Users user)
        {
            //判断是否有重复的用户名
            if (DbHelper.QuerySingle<int>($"select count(0) from Users where UserName='{user.UserName}'") > 0)
            {
                ViewBag.ErrorMessage = "用户名重复";
                return View(user);
            }

            int userid = DbHelper.ExecuteSqlCommandScalar($"insert into Users(UserName,RegType,Password,TrueName) values('{user.UserName}',0,'{user.HashPassword}','{user.TrueName}')", new System.Data.Common.DbParameter[] { });
            //UserRole
            if (user.RoleID != null)
            {
                foreach (var roleid in user.RoleID)
                {
                    DbHelper.ExecuteSqlCommand($"insert into UserRole values({userid},{roleid})", null);
                }
            }

            return RedirectToAction("GetUserList");
        }

        /// <summary>
        /// 员工编辑
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult UserEdit(int userID)
        {
            Users user = DbHelper.QuerySingle<Users>($"select * from Users where UserID={userID}");

            return View(user);
        }

        /// <summary>
        /// 保存员工编辑
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public ActionResult UserEdit(Users user)
        {
            //判断用户名是否已存在
            if (DbHelper.QuerySingle<int>($"select count(0) from Users where UserName='{user.UserName}' and UserID!={user.UserID}") > 0)
            {
                ViewBag.ErrorMessage = "用户名已存在";
                return View(user);
            }
            string sql = $"update Users set UserName='{user.UserName}',TrueName='{user.TrueName}'";
            if (string.IsNullOrWhiteSpace(user.Password) == false)
            {
                user.Password = user.HashPassword;
                sql = $"{sql} ,Password = '{user.Password}' ";
            }
            sql = $"{sql}  where UserID={user.UserID}";
            //保存用户
            DbHelper.ExecuteSqlCommand(sql, null);

            //删除之前的角色
            DbHelper.ExecuteSqlCommand($"delete from UserRole where UserID={user.UserID}", null);

            if (user.RoleID != null)
            {
                //保存角色
                foreach (var roleid in user.RoleID)
                {
                    DbHelper.ExecuteSqlCommand($"insert into UserRole values({user.UserID},{roleid})", null);
                }
            }


            return RedirectToAction("GetUserList");
        }


        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="userids"></param>
        /// <returns></returns>
        public ActionResult BatchDeleteUsers(int[] userids)
        {

            DbHelper.ExecuteSqlCommand($"delete from Users where UserID in({string.Join(",", userids)});", null);
            return Json(new { status = true, message = "删除成功" });
        }

        [HttpGet]
        public ActionResult TestDapper()
        {
            SocialSecurityPeople model = new SocialSecurityPeople();
            return View(model);
        }

        //[HttpPost]
        //public ActionResult TestDapper(SocialSecurityPeople model)
        //{
        //    string sql = "select Users.UserID, Users.UserName , Roles.RoleID, Roles.RoleName from Users left join UserRole on Users.UserID = UserRole.UserID left join Roles on UserRole.RoleID = Roles.RoleID";
        //    List<Users> list = DbHelper.CustomQuery<Users, Entity.Roles, Users>(sql,
        //        (user, role) =>
        //    {
        //        user.roles = role;
        //        return user;
        //    },
        //    "RoleID").ToList();
        //    //string sql = "update Test set name=1 where name='1';update Test set name=2 where name='王五'";
        //    //int result = DbHelper.ExecuteSqlCommand(sql, null);


        //    return View();
        //}

        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();
            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
            var lstTitle = new List<string> { "编号", "姓名", "年龄", "创建时间" };
            foreach (var item in lstTitle)
            {
                sbHtml.AppendFormat("<td style='font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25'>{0}</td>", item);
            }
            sbHtml.Append("</tr>");

            for (int i = 0; i < 10; i++)
            {
                sbHtml.Append("<tr>");
                sbHtml.AppendFormat("<td>{0}</td>", i);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>屌丝{0}号</td>", i);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", new Random().Next(20, 30) + i);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", DateTime.Now);
                sbHtml.Append("</tr>");
            }
            sbHtml.Append("</table>");

            //第一种:使用FileContentResult

            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());
            return File(fileContents, "application/ms-excel", "fileContents.xls");

            ////第二种:使用FileStreamResult
            //var fileStream = new MemoryStream(fileContents);
            //return File(fileStream, "application/ms-excel", "fileStream.xls");

            ////第三种:使用FilePathResult
            ////服务器上首先必须要有这个Excel文件,然会通过Server.MapPath获取路径返回.
            //var fileName = Server.MapPath("~/Files/fileName.xls");
            //return File(fileName, "application/ms-excel", "fileName.xls");
        }
    }
}