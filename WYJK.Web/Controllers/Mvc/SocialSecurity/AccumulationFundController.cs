using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WYJK.Data.IService;
using WYJK.Data.IServices;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;
using WYJK.Web.Models;

namespace WYJK.Web.Controllers.Mvc
{
    [Authorize]
    public class AccumulationFundController : Controller
    {
        private readonly ISocialSecurityService _socialSecurityService = new SocialSecurityService();
        IAccumulationFundService _accumulationFundService = new AccumulationFundService();

        /// <summary>
        /// 公积金待办业务
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<ActionResult> AccumulationFundWaitingHandle(AccumulationFundParameter parameter)
        {
            ViewData["SocialSecurityPeopleName"] = parameter.SocialSecurityPeopleName;
            ViewData["IdentityCard"] = parameter.IdentityCard;

            PagedResult<AccumulationFundShowModel> list = await _accumulationFundService.GetAccumulationFundList(parameter);

            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(UserTypeEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "全部", Value = "" });

            ViewData["UserType"] = new SelectList(UserTypeList, "Value", "Text", parameter.UserType);

            return View(list);
        }

        /// <summary>
        /// 公积金归档业务
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<ActionResult> AccumulationFundNormal(AccumulationFundParameter parameter)
        {
            ViewData["SocialSecurityPeopleName"] = parameter.SocialSecurityPeopleName;
            ViewData["IdentityCard"] = parameter.IdentityCard;

            PagedResult<AccumulationFundShowModel> list = await _accumulationFundService.GetAccumulationFundList(parameter);

            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(UserTypeEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "全部", Value = "" });

            ViewData["UserType"] = new SelectList(UserTypeList, "Value", "Text", parameter.UserType);

            return View(list);
        }

        /// <summary>
        /// 公积金待续费业务
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<ActionResult> AccumulationFundRenew(AccumulationFundParameter parameter)
        {
            ViewData["SocialSecurityPeopleName"] = parameter.SocialSecurityPeopleName;
            ViewData["IdentityCard"] = parameter.IdentityCard;

            PagedResult<AccumulationFundShowModel> list = await _accumulationFundService.GetAccumulationFundList(parameter);

            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(UserTypeEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "全部", Value = "" });

            ViewData["UserType"] = new SelectList(UserTypeList, "Value", "Text", parameter.UserType);

            return View(list);
        }
        /// <summary>
        /// 公积金待办停业务
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<ActionResult> AccumulationFundWaitingStop(AccumulationFundParameter parameter)
        {
            ViewData["SocialSecurityPeopleName"] = parameter.SocialSecurityPeopleName;
            ViewData["IdentityCard"] = parameter.IdentityCard;

            PagedResult<AccumulationFundShowModel> list = await _accumulationFundService.GetAccumulationFundList(parameter);

            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(UserTypeEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "全部", Value = "" });

            ViewData["UserType"] = new SelectList(UserTypeList, "Value", "Text", parameter.UserType);

            return View(list);
        }

        /// <summary>
        /// 公积金已办停业务
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<ActionResult> AccumulationFundAlreadyStop(AccumulationFundParameter parameter)
        {
            ViewData["SocialSecurityPeopleName"] = parameter.SocialSecurityPeopleName;
            ViewData["IdentityCard"] = parameter.IdentityCard;

            PagedResult<AccumulationFundShowModel> list = await _accumulationFundService.GetAccumulationFundList(parameter);

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
            //修改参保人公积金状态
            bool flag = _accumulationFundService.ModifyAccumulationFundStatus(SocialSecurityPeopleIDs, (int)SocialSecurityStatusEnum.Normal);
            
            #region 记录日志
            if (flag == true)
            {
                string names = _socialSecurityService.GetSocialPeopleNames(SocialSecurityPeopleIDs);
                LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("公积金业务办结，客户:{0}", names) });
            }
            #endregion

            return Json(new { status = flag, Message = flag ? "办结成功" : "办结失败" });
        }


        /// <summary>
        /// 批量办停
        /// </summary>
        /// <param name="SocialSecurityPeopleIDs"></param>
        /// <returns></returns>
        public JsonResult BatchStop(int[] SocialSecurityPeopleIDs)
        {
            //修改参保人社保状态
            bool flag = _accumulationFundService.ModifyAccumulationFundStatus(SocialSecurityPeopleIDs, (int)SocialSecurityStatusEnum.AlreadyStop);

            #region 记录日志
            if (flag == true)
            {
                string names = _socialSecurityService.GetSocialPeopleNames(SocialSecurityPeopleIDs);
                LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("公积金业务办停，客户:{0}", names) });
            }
            #endregion

            return Json(new { status = flag, Message = flag ? "办停成功" : "办停失败" });
        }

        /// <summary>
        /// 查询公积金详情
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <returns></returns>
        public ActionResult AccumulationFundDetail(int SocialSecurityPeopleID)
        {
            AccumulationFund model = _accumulationFundService.GetAccumulationFundDetail(SocialSecurityPeopleID);

            return View(model);
        }

        /// <summary>
        /// 保存公积金号
        /// </summary>
        /// <param name="AccumulationFundNo"></param>
        /// <returns></returns>
        public ActionResult SaveAccumulationFundNo(int SocialSecurityPeopleID, string AccumulationFundNo)
        {
            bool flag = _accumulationFundService.SaveAccumulationFundNo(SocialSecurityPeopleID, AccumulationFundNo);
            TempData["Message"] = flag ? "更新成功" : "更新失败";


            #region 记录日志
            if (flag == true)
            {
                string names = _socialSecurityService.GetSocialPeopleNames(new int[] { SocialSecurityPeopleID });
                LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("更新公积金客户:{0}社保号为：{1}", names, AccumulationFundNo) });
            }
            #endregion


            return RedirectToAction("AccumulationFundDetail", new { SocialSecurityPeopleID = SocialSecurityPeopleID });
        }

        //异常处理
        [HttpGet]
        public ActionResult AccumulationException(int[] SocialSecurityPeopleIDs)
        {


            SocialSecurityException model = new SocialSecurityException()
            {
                PeopleIds = string.Join(",", SocialSecurityPeopleIDs)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> AccumulationException(SocialSecurityException model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await Data.DbHelper.ExecuteSqlCommandAsync($"update SocialSecurityPeople set status=0 where SocialSecurityPeopleid in ({model.PeopleIds});update AccumulationFund set AccumulationFundException='{model.Exception}',status=1 where SocialSecurityPeopleid in ({model.PeopleIds})");
                    string[] strArray = model.PeopleIds.Split(',');
                    int[] intArray;
                    intArray = Array.ConvertAll<string, int>(strArray, s => int.Parse(s));
                    string names = _socialSecurityService.GetSocialPeopleNames(intArray);
                    LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("办理公积金异常客户:{0},原因:{1}", names, model.Exception) });
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