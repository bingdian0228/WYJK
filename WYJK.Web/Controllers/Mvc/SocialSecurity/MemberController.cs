﻿using System;
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

namespace WYJK.Web.Controllers.Mvc
{
    [Authorize]
    public class MemberController : Controller
    {
        private readonly IMemberService _memberService = new MemberService();
        private readonly ISocialSecurityService _socialSecurityService = new SocialSecurityService();
        private readonly IAccumulationFundService _accumulationFundService = new AccumulationFundService();
        // GET: Member
        public ActionResult GetMemberList(MembersParameters parameter)
        {
            PagedResult<MembersStatistics> membersStatisticsList = _memberService.GetMembersStatisticsList(parameter);

            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(UserTypeEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "全部", Value = "" });

            ViewData["UserType"] = new SelectList(UserTypeList, "Value", "Text");

            ViewBag.memberList = _memberService.GetMembersList();

            return View(membersStatisticsList);
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetMemberList1(string UserType)
        {
            List<Members> memberList = _memberService.GetMembersList();
            var list = memberList.Where(n => UserType == string.Empty ? true : n.UserType == UserType)
                .Select(item => new { MemberID = item.MemberID, MemberName = item.MemberName });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 新建编辑
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> EditMemberExtensionInformation(int MemberID, int type)
        {
            ExtensionInformationParameter model = await _memberService.GetMemberExtensionInformation(MemberID);

            #region 证件类型
            var CertificateTypeList = new List<string> { "请选择" }.Concat(GetCertificateType()).Select(
                                        item => new SelectListItem
                                        {
                                            Text = item,
                                            Value = item == "请选择" ? "" : item,
                                            Selected = item == model.CertificateType

                                        }).ToList();

            //ViewData["CertificateType"] = new SelectList(CertificateTypeList, "Value", "Text");

            ViewBag.CertificateType = new SelectList(CertificateTypeList, "Value", "Text");

            #endregion

            #region 政治面貌
            var PoliticalStatusList = new List<string> { "请选择" }.Concat(GetPoliticalStatus()).Select(
                                        item => new SelectListItem
                                        {
                                            Text = item,
                                            Value = item == "请选择" ? "" : item
                                        }).ToList();

            ViewData["PoliticalStatus"] = new SelectList(PoliticalStatusList, "Value", "Text", model.PoliticalStatus);
            #endregion

            #region 学历
            var EducationList = new List<string> { "请选择" }.Concat(GetEducation()).Select(
                                item => new SelectListItem
                                {
                                    Text = item,
                                    Value = item == "请选择" ? "" : item
                                }).ToList();

            ViewData["Education"] = new SelectList(EducationList, "Value", "Text", model.Education);
            #endregion

            #region 户口性质
            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(HouseholdPropertyEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "请选择", Value = "" });
            int householdType = 0;
            foreach (var item in UserTypeList)
            {
                if (item.Text == model.HouseholdType)
                {
                    householdType = Convert.ToInt32(item.Value);
                    break;
                }
            }

            model.HouseholdType = householdType.ToString();

            ViewData["HouseholdType"] = new SelectList(UserTypeList, "Value", "Text");
            #endregion

            if (type == 1)
            {
                ViewData["SocialSecurityList"] = _socialSecurityService.GetSocialSecurityList(MemberID);
                ViewData["AccumulationFundList"] = _accumulationFundService.GetAccumulationFundList(MemberID);
            }

            return View(model);
        }


        /// <summary>
        /// 提交补充信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> EditMemberExtensionInformation(ExtensionInformationParameter model)
        {
            model.HouseholdType = EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)Convert.ToInt32(model.HouseholdType));

            bool flag = await _memberService.ModifyMemberExtensionInformation(model);

            TempData["Message"] = flag ? "保存成功" : "保存失败";

            #region 日志记录
            if (flag == true)
            {
                LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("修改了用户{0}信息", (await _memberService.GetMemberInfo(model.MemberID)).MemberName) });
            }
            #endregion

            return RedirectToAction("GetMemberList");
        }



        /// <summary>
        /// 获取证件类型
        /// </summary>
        /// <returns></returns>
        public List<string> GetCertificateType()
        {
            List<string> list = new List<string>() {
               "身份证","居住证","签证","护照","户口本","军人证","团员证","党员证","港澳通行证"
            };
            return list;
        }

        /// <summary>
        /// 获取政治面貌
        /// </summary>
        /// <returns></returns>
        public List<string> GetPoliticalStatus()
        {
            List<string> list = new List<string>() {
                "中共党员","共青团员","群众"
            };
            return list;
        }

        /// <summary>
        /// 获取学历
        /// </summary>
        /// <returns></returns>
        public List<string> GetEducation()
        {
            List<string> list = new List<string>() {
                "中专","高中","高职（大专）","本科","硕士","博士","博士后"
            };
            return list;
        }
    }
}