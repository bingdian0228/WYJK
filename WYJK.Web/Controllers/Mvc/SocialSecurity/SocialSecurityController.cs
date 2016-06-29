using WYJK.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WYJK.Data.IServices;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;
using WYJK.Web.Models;
using WYJK.Data;

namespace WYJK.Web.Controllers.Mvc
{
    [Authorize]
    public class SocialSecurityController : BaseController
    {
        private readonly ISocialSecurityService _socialSecurityService = new SocialSecurityService();

        /// <summary>
        /// 社保业务总览
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<ActionResult> SocialSecurityOverview(SocialSecurityParameter parameter)
        {
            ViewData["SocialSecurityPeopleName"] = parameter.SocialSecurityPeopleName;
            ViewData["IdentityCard"] = parameter.IdentityCard;

            PagedResult<SocialSecurityShowModel> list = await _socialSecurityService.GetSocialSecurityList(parameter);

            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(UserTypeEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "全部", Value = "", Selected = true });

            ViewData["UserType"] = new SelectList(UserTypeList, "Value", "Text", parameter.UserType);

            List<SelectListItem> StatusList = EnumExt.GetSelectList(typeof(SocialSecurityStatusEnum));
            StatusList.Insert(0, new SelectListItem { Text = "全部", Value = "", Selected = true });

            ViewData["Status"] = new SelectList(StatusList, "Value", "Text", parameter.Status);


            return View(list);
        }

        /// <summary>
        /// 社保待办业务
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<ActionResult> SocialSecurityWaitingHandle(SocialSecurityParameter parameter)
        {
            ViewData["SocialSecurityPeopleName"] = parameter.SocialSecurityPeopleName;
            ViewData["IdentityCard"] = parameter.IdentityCard;

            PagedResult<SocialSecurityShowModel> list = await _socialSecurityService.GetSocialSecurityList(parameter);

            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(UserTypeEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "全部", Value = "", Selected = true });

            ViewData["UserType"] = new SelectList(UserTypeList, "Value", "Text", parameter.UserType);

            return View(list);
        }

        /// <summary>
        /// 社保归档业务
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<ActionResult> SocialSecurityNormal(SocialSecurityParameter parameter)
        {
            ViewData["SocialSecurityPeopleName"] = parameter.SocialSecurityPeopleName;
            ViewData["IdentityCard"] = parameter.IdentityCard;

            PagedResult<SocialSecurityShowModel> list = await _socialSecurityService.GetSocialSecurityList(parameter);

            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(UserTypeEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "全部", Value = "" });

            ViewData["UserType"] = new SelectList(UserTypeList, "Value", "Text", parameter.UserType);

            return View(list);
        }

        /// <summary>
        /// 社保待续费业务
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<ActionResult> SocialSecurityRenew(SocialSecurityParameter parameter)
        {
            ViewData["SocialSecurityPeopleName"] = parameter.SocialSecurityPeopleName;
            ViewData["IdentityCard"] = parameter.IdentityCard;

            PagedResult<SocialSecurityShowModel> list = await _socialSecurityService.GetSocialSecurityList(parameter);

            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(UserTypeEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "全部", Value = "" });

            ViewData["UserType"] = new SelectList(UserTypeList, "Value", "Text", parameter.UserType);

            return View(list);
        }

        /// <summary>
        /// 社保待办停业务
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<ActionResult> SocialSecurityWaitingStop(SocialSecurityParameter parameter)
        {
            ViewData["SocialSecurityPeopleName"] = parameter.SocialSecurityPeopleName;
            ViewData["IdentityCard"] = parameter.IdentityCard;

            PagedResult<SocialSecurityShowModel> list = await _socialSecurityService.GetSocialSecurityList(parameter);

            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(UserTypeEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "全部", Value = "" });

            ViewData["UserType"] = new SelectList(UserTypeList, "Value", "Text", parameter.UserType);

            return View(list);
        }

        /// <summary>
        /// 社保已办停业务
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<ActionResult> SocialSecurityAlreadyStop(SocialSecurityParameter parameter)
        {
            ViewData["SocialSecurityPeopleName"] = parameter.SocialSecurityPeopleName;
            ViewData["IdentityCard"] = parameter.IdentityCard;

            PagedResult<SocialSecurityShowModel> list = await _socialSecurityService.GetSocialSecurityList(parameter);

            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(UserTypeEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "全部", Value = "" });

            ViewData["UserType"] = new SelectList(UserTypeList, "Value", "Text", parameter.UserType);

            return View(list);
        }



        /// <summary>
        /// 批量办结
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult BatchComplete(int[] SocialSecurityPeopleIDs)
        {
            //判断客户社保号是否已经填写
            List<SocialSecurity> socialSecurityList = DbHelper.Query<SocialSecurity>($"select * from SocialSecurity where SocialSecurityPeopleID in ({string.Join(",", SocialSecurityPeopleIDs)})");
            foreach (var socialSecurity in socialSecurityList)
            {
                if (string.IsNullOrEmpty(socialSecurity.SocialSecurityNo))
                {
                    return Json(new { status = false, Message = "无法办结，客户社保号没有填写完整" });
                }
            }


            //修改参保人社保状态
            bool flag = _socialSecurityService.ModifySocialStatus(SocialSecurityPeopleIDs, (int)SocialSecurityStatusEnum.Normal);

            #region 记录日志
            if (flag == true)
            {

                string names = _socialSecurityService.GetSocialPeopleNames(SocialSecurityPeopleIDs);
                LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("社保业务办结，客户:{0}", names) });
            }
            #endregion

            return Json(new { status = flag, Message = flag ? "办结成功" : "办结失败" });
        }

        /// <summary>
        /// 社保办停
        /// </summary>
        /// <param name="SocialSecurityPeopleIDs"></param>
        /// <returns></returns>
        public JsonResult BatchStop(int SocialSecurityPeopleIDs)
        {
            // 修改参保人社保状态
            string sql = $"update SocialSecurity set Status={(int)SocialSecurityStatusEnum.AlreadyStop},StopDate=getdate() where SocialSecurityPeopleID in({SocialSecurityPeopleIDs})";

            int result = DbHelper.ExecuteSqlCommand(sql, null);

            #region 记录日志
            if (result > 0)
            {
                string names = _socialSecurityService.GetSocialPeopleNames(new int[] { SocialSecurityPeopleIDs });
                LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("社保业务办停，客户:{0}", names) });
            }
            #endregion

            return Json(new { status = result > 0, Message = result > 0 ? "办停成功" : "办停失败" });
        }

        /// <summary>
        /// 社保邮寄办停  填写邮寄单号和快递公司
        /// </summary>
        /// <param name="SocialSecurityPeopleIDs"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SocialSecurityMailStop(int SocialSecurityPeopleIDs)
        {
            SocialSecurityMailStop model = new SocialSecurityMailStop
            {
                SocialSecurityPeopleIDs = SocialSecurityPeopleIDs
            };
            return View(model);
        }

        /// <summary>
        ///  社保邮寄办停  填写邮寄单号和快递公司
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SocialSecurityMailStop(SocialSecurityMailStop model)
        {
            if (!ModelState.IsValid) return View();
            string message = string.Empty;

            // 修改参保人社保状态
            string sql = $"update SocialSecurity set Status={(int)SocialSecurityStatusEnum.AlreadyStop},StopDate=getdate(),MailOrder='{model.MailOrder}',ExpressCompany='{model.ExpressCompany}' where SocialSecurityPeopleID in({model.SocialSecurityPeopleIDs})";

            int result = DbHelper.ExecuteSqlCommand(sql, null);


            #region 记录日志
            if (result > 0)
            {
                message = "办停成功";
                string names = _socialSecurityService.GetSocialPeopleNames(new int[] { model.SocialSecurityPeopleIDs });
                LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("社保业务办停，客户:{0}", names) });
            }
            else {
                message = "办停失败";
            }

            #endregion
            return Content(message);
            //return Json(new { status = result > 0, Message = result > 0 ? "办停成功" : "办停失败" });
        }

        /// <summary>
        /// 查询社保详情
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <returns></returns>
        public ActionResult SocialSecurityDetail(int SocialSecurityPeopleID)
        {
            SocialSecurity model = _socialSecurityService.GetSocialSecurityDetail(SocialSecurityPeopleID);

            return View(model);
        }

        /// <summary>
        /// 保存社保号
        /// </summary>
        /// <param name="SocialSecurityNo"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveSocialSecurityNo(int SocialSecurityPeopleID, string SocialSecurityNo)
        {
            bool flag = _socialSecurityService.SaveSocialSecurityNo(SocialSecurityPeopleID, SocialSecurityNo);
            TempData["Message"] = flag ? "更新成功" : "更新失败";

            #region 记录日志
            if (flag == true)
            {
                string names = _socialSecurityService.GetSocialPeopleNames(new int[] { SocialSecurityPeopleID });
                LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("更新社保客户:{0}，社保号为：{1}", names, SocialSecurityNo) });
            }
            #endregion

            return RedirectToAction("SocialSecurityDetail", new { SocialSecurityPeopleID = SocialSecurityPeopleID });
        }

        //异常处理
        [HttpGet]
        public ActionResult SocialException(int[] SocialSecurityPeopleIDs)
        {


            SocialSecurityException model = new SocialSecurityException()
            {
                PeopleIds = string.Join(",", SocialSecurityPeopleIDs)
            };

            return View(model);
        }



        [HttpPost]
        public async Task<ActionResult> SocialException(SocialSecurityException model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await Data.DbHelper.ExecuteSqlCommandAsync($"update SocialSecurityPeople set status=0 where SocialSecurityPeopleid in ({model.PeopleIds});update socialsecurity set SocialSecurityException='{model.Exception}',status=1,IsException=1 where SocialSecurityPeopleid in ({model.PeopleIds})");
                    string[] strArray = model.PeopleIds.Split(',');
                    int[] intArray;
                    intArray = Array.ConvertAll<string, int>(strArray, s => int.Parse(s));
                    string names = _socialSecurityService.GetSocialPeopleNames(intArray);

                    #region 提示客服并提示客户
                    //参保人名称
                    SocialSecurityPeople socialSecurityPeople = DbHelper.QuerySingle<SocialSecurityPeople>($"select * from SocialSecurityPeople where SocialSecurityPeopleID={model.PeopleIds[0]}");
                    string socialSecurityPeopleName = socialSecurityPeople.SocialSecurityPeopleName;
                    int memberID = socialSecurityPeople.MemberID;


                    //向客户发送站内信
                    string memberLogStr = string.Empty;
                    memberLogStr = socialSecurityPeopleName + "社保业务办理异常：" + model.Exception;
                    DbHelper.ExecuteSqlCommand($"insert into Message(MemberID,ContentStr) values({memberID},'{memberLogStr}')", null);
                    #endregion

                    LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("办理社保异常客户:{0},原因:{1}", names, model.Exception) });
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = ex.Message;
                }
            }
            return View(model);
        }

    }
}