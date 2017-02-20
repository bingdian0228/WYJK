using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Data.IService;
using WYJK.Data.IServices;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;
using WYJK.Web.Models;

namespace WYJK.Web.Controllers.Mvc
{
    [Authorize]
    public class AccumulationFundController : BaseController
    {
        private readonly ISocialSecurityService _socialSecurityService = new SocialSecurityService();
        IAccumulationFundService _accumulationFundService = new AccumulationFundService();
        private readonly IMemberService _memberService = new MemberService();
        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetMemberList1(string UserType)
        {
            List<Members> memberList = _memberService.GetMembersList();
            var list = memberList.Where(n => UserType == string.Empty ? true : n.UserType == UserType)
                .Select(item => new { MemberID = item.MemberID, MemberName = item.UserType == "0" ? item.MemberName : (item.UserType == "1" ? item.EnterpriseName : item.BusinessName) });
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 公积金业务总览
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<ActionResult> AccumulationFundOverview(AccumulationFundParameter parameter)
        {
            if (!string.IsNullOrEmpty(parameter.SocialSecurityPeopleName))
                parameter.SocialSecurityPeopleName = parameter.SocialSecurityPeopleName.Replace("'", "''");
            if (!string.IsNullOrEmpty(parameter.IdentityCard))
                parameter.IdentityCard = parameter.IdentityCard.Replace("'", "''");
            ViewData["SocialSecurityPeopleName"] = parameter.SocialSecurityPeopleName;
            ViewData["IdentityCard"] = parameter.IdentityCard;

            PagedResult<AccumulationFundShowModel> list = await _accumulationFundService.GetAccumulationFundList(parameter);

            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(UserTypeEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "全部", Value = "" });

            ViewData["UserType"] = new SelectList(UserTypeList, "Value", "Text", parameter.UserType);

            List<SelectListItem> StatusList = EnumExt.GetSelectList(typeof(SocialSecurityStatusEnum));
            StatusList.Insert(0, new SelectListItem { Text = "全部", Value = "", Selected = true });

            ViewData["Status"] = new SelectList(StatusList, "Value", "Text", parameter.Status);
            List<Members> memberList = _memberService.GetMembersList();
            memberList.ForEach(item =>
            {
                item.MemberName = item.UserType == "0" ? item.MemberName : (item.UserType == "1" ? item.EnterpriseName : item.BusinessName);
            });
            ViewBag.memberList = memberList;
            return View(list);
        }

        /// <summary>
        /// 公积金待办业务
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<ActionResult> AccumulationFundWaitingHandle(AccumulationFundParameter parameter)
        {
            if (!string.IsNullOrEmpty(parameter.SocialSecurityPeopleName))
                parameter.SocialSecurityPeopleName = parameter.SocialSecurityPeopleName.Replace("'", "''");
            if (!string.IsNullOrEmpty(parameter.IdentityCard))
                parameter.IdentityCard = parameter.IdentityCard.Replace("'", "''");
            ViewData["SocialSecurityPeopleName"] = parameter.SocialSecurityPeopleName;
            ViewData["IdentityCard"] = parameter.IdentityCard;

            PagedResult<AccumulationFundShowModel> list = await _accumulationFundService.GetAccumulationFundList(parameter);

            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(UserTypeEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "全部", Value = "" });

            ViewData["UserType"] = new SelectList(UserTypeList, "Value", "Text", parameter.UserType);
            List<Members> memberList = _memberService.GetMembersList();
            memberList.ForEach(item =>
            {
                item.MemberName = item.UserType == "0" ? item.MemberName : (item.UserType == "1" ? item.EnterpriseName : item.BusinessName);
            });
            ViewBag.memberList = memberList;
            return View(list);
        }

        /// <summary>
        /// 公积金归档业务
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<ActionResult> AccumulationFundNormal(AccumulationFundParameter parameter)
        {
            if (!string.IsNullOrEmpty(parameter.SocialSecurityPeopleName))
                parameter.SocialSecurityPeopleName = parameter.SocialSecurityPeopleName.Replace("'", "''");
            if (!string.IsNullOrEmpty(parameter.IdentityCard))
                parameter.IdentityCard = parameter.IdentityCard.Replace("'", "''");
            ViewData["SocialSecurityPeopleName"] = parameter.SocialSecurityPeopleName;
            ViewData["IdentityCard"] = parameter.IdentityCard;

            PagedResult<AccumulationFundShowModel> list = await _accumulationFundService.GetAccumulationFundList(parameter);

            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(UserTypeEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "全部", Value = "" });

            ViewData["UserType"] = new SelectList(UserTypeList, "Value", "Text", parameter.UserType);
            List<Members> memberList = _memberService.GetMembersList();
            memberList.ForEach(item =>
            {
                item.MemberName = item.UserType == "0" ? item.MemberName : (item.UserType == "1" ? item.EnterpriseName : item.BusinessName);
            });
            ViewBag.memberList = memberList;
            return View(list);
        }

        /// <summary>
        /// 公积金待续费业务
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<ActionResult> AccumulationFundRenew(AccumulationFundParameter parameter)
        {
            if (!string.IsNullOrEmpty(parameter.SocialSecurityPeopleName))
                parameter.SocialSecurityPeopleName = parameter.SocialSecurityPeopleName.Replace("'", "''");
            if (!string.IsNullOrEmpty(parameter.IdentityCard))
                parameter.IdentityCard = parameter.IdentityCard.Replace("'", "''");
            ViewData["SocialSecurityPeopleName"] = parameter.SocialSecurityPeopleName;
            ViewData["IdentityCard"] = parameter.IdentityCard;

            PagedResult<AccumulationFundShowModel> list = await _accumulationFundService.GetAccumulationFundList(parameter);

            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(UserTypeEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "全部", Value = "" });

            ViewData["UserType"] = new SelectList(UserTypeList, "Value", "Text", parameter.UserType);
            List<Members> memberList = _memberService.GetMembersList();
            memberList.ForEach(item =>
            {
                item.MemberName = item.UserType == "0" ? item.MemberName : (item.UserType == "1" ? item.EnterpriseName : item.BusinessName);
            });
            ViewBag.memberList = memberList;
            return View(list);
        }
        /// <summary>
        /// 公积金待办停业务
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<ActionResult> AccumulationFundWaitingStop(AccumulationFundParameter parameter)
        {
            if (!string.IsNullOrEmpty(parameter.SocialSecurityPeopleName))
                parameter.SocialSecurityPeopleName = parameter.SocialSecurityPeopleName.Replace("'", "''");
            if (!string.IsNullOrEmpty(parameter.IdentityCard))
                parameter.IdentityCard = parameter.IdentityCard.Replace("'", "''");
            ViewData["SocialSecurityPeopleName"] = parameter.SocialSecurityPeopleName;
            ViewData["IdentityCard"] = parameter.IdentityCard;

            PagedResult<AccumulationFundShowModel> list = await _accumulationFundService.GetAccumulationFundList(parameter);

            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(UserTypeEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "全部", Value = "" });

            ViewData["UserType"] = new SelectList(UserTypeList, "Value", "Text", parameter.UserType);
            List<Members> memberList = _memberService.GetMembersList();
            memberList.ForEach(item =>
            {
                item.MemberName = item.UserType == "0" ? item.MemberName : (item.UserType == "1" ? item.EnterpriseName : item.BusinessName);
            });
            ViewBag.memberList = memberList;
            return View(list);
        }

        /// <summary>
        /// 公积金已办停业务
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<ActionResult> AccumulationFundAlreadyStop(AccumulationFundParameter parameter)
        {
            if (!string.IsNullOrEmpty(parameter.SocialSecurityPeopleName))
                parameter.SocialSecurityPeopleName = parameter.SocialSecurityPeopleName.Replace("'", "''");
            if (!string.IsNullOrEmpty(parameter.IdentityCard))
                parameter.IdentityCard = parameter.IdentityCard.Replace("'", "''");
            ViewData["SocialSecurityPeopleName"] = parameter.SocialSecurityPeopleName;
            ViewData["IdentityCard"] = parameter.IdentityCard;

            PagedResult<AccumulationFundShowModel> list = await _accumulationFundService.GetAccumulationFundList(parameter);

            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(UserTypeEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "全部", Value = "" });

            ViewData["UserType"] = new SelectList(UserTypeList, "Value", "Text", parameter.UserType);
            List<Members> memberList = _memberService.GetMembersList();
            memberList.ForEach(item =>
            {
                item.MemberName = item.UserType == "0" ? item.MemberName : (item.UserType == "1" ? item.EnterpriseName : item.BusinessName);
            });
            ViewBag.memberList = memberList;
            return View(list);
        }
        /// <summary>
        /// 批量办结
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult BatchComplete(int[] SocialSecurityPeopleIDs)
        {
            //判断客户公积金号是否已经填写
            List<AccumulationFund> accumulationFundList = DbHelper.Query<AccumulationFund>($"select * from AccumulationFund where SocialSecurityPeopleID in ({string.Join(",", SocialSecurityPeopleIDs)})");
            foreach (var accumulationFund in accumulationFundList)
            {
                if (string.IsNullOrEmpty(accumulationFund.AccumulationFundNo))
                {
                    return Json(new { status = false, Message = "无法办结，客户公积金号没有填写完整" });
                }
            }


            //修改参保人公积金状态
            bool flag = _accumulationFundService.ModifyAccumulationFundStatus(SocialSecurityPeopleIDs, (int)SocialSecurityStatusEnum.Normal);

            #region 记录日志
            if (flag == true)
            {
                List<SocialSecurityPeople> socialSecurityPeopleList = DbHelper.Query<SocialSecurityPeople>($"select * from SocialSecurityPeople where SocialSecurityPeopleID in({string.Join(", ", SocialSecurityPeopleIDs)})");
                //string names = _socialSecurityService.GetSocialPeopleNames(SocialSecurityPeopleIDs);
                socialSecurityPeopleList.ForEach(n =>
                {
                    LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, MemberID = n.MemberID, SocialSecurityPeopleID = n.SocialSecurityPeopleID, Contents = string.Format("公积金业务办结，客户:{0}", n.SocialSecurityPeopleName) });
                });
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
                //string names = _socialSecurityService.GetSocialPeopleNames(SocialSecurityPeopleIDs);
                List<SocialSecurityPeople> socialSecurityPeopleList = DbHelper.Query<SocialSecurityPeople>($"select * from SocialSecurityPeople where SocialSecurityPeopleID in({string.Join(",", SocialSecurityPeopleIDs)})");
                socialSecurityPeopleList.ForEach(n =>
                {
                    LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, MemberID = n.MemberID, SocialSecurityPeopleID = n.SocialSecurityPeopleID, Contents = string.Format("公积金业务办停，客户:{0}", n.SocialSecurityPeopleName) });
                });

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


            return RedirectToAction("AccumulationFundWaitingHandle", new { Status = 2 });
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
                    await Data.DbHelper.ExecuteSqlCommandAsync($"update SocialSecurityPeople set status=0 where SocialSecurityPeopleid in ({model.PeopleIds});update AccumulationFund set AccumulationFundException='{model.Exception}',status=1,IsException=1 where SocialSecurityPeopleid in ({model.PeopleIds})");
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
                    memberLogStr = socialSecurityPeopleName + "公积金业务办理异常：" + model.Exception;
                    DbHelper.ExecuteSqlCommand($"insert into Message(MemberID,ContentStr) values({memberID},'{memberLogStr}')", null);
                    #endregion


                    LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, MemberID = socialSecurityPeople.MemberID, SocialSecurityPeopleID = socialSecurityPeople.SocialSecurityPeopleID, Contents = string.Format("办理公积金异常客户:{0},原因:{1}", names, model.Exception) });
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