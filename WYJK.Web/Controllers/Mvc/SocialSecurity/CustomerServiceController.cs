﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Data.IService;
using WYJK.Data.IServices;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;

namespace WYJK.Web.Controllers.Mvc
{
    /// <summary>
    /// 客户管理
    /// </summary>
    [Authorize]
    public class CustomerServiceController : Controller
    {
        private readonly ICustomerService _customerService = new CustomerService();
        private readonly IOrderService _orderService = new OrderService();
        private readonly ISocialSecurityService _socialSecurityService = new SocialSecurityService();
        private readonly IAccumulationFundService _accumulationFundService = new AccumulationFundService();
        private readonly IMemberService _memberService = new MemberService();
        private readonly IEnterpriseService _enterpriseService = new EnterpriseService();
        private readonly IUserService _userService = new UserService();
        /// <summary>
        /// 获取客户管理列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCustomerServiceList(CustomerServiceParameter parameter)
        {
            PagedResult<CustomerServiceViewModel> customerServiceList = _customerService.GetCustomerServiceList(parameter);


            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(UserTypeEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "全部", Value = "" });

            ViewData["UserType"] = new SelectList(UserTypeList, "Value", "Text");

            ViewBag.memberList = _memberService.GetMembersList();

            return View(customerServiceList);
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
        /// 批量通过  注： 参保人客户服务状态变成已审核，并判断对应的订单财务是否已经审核通过
        /// 遍历参保人对应的订单，只要有一个是已付款或已完成（如果没有，则需提示），则客服审核状态变成审核通过，如果是订单是已完成，则参保人状态变成待办
        /// </summary>
        /// <param name="SocialSecurityPeopleIDs"></param>
        /// <returns></returns>
        public JsonResult BatchComplete(int[] SocialSecurityPeopleIDs)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    foreach (var socialSecurityPeopleID in SocialSecurityPeopleIDs)
                    {
                        List<Order> orderList = _orderService.GetOrderList(socialSecurityPeopleID);
                        bool flag = false;
                        foreach (var order in orderList)
                        {
                            if (Convert.ToInt32(order.Status) == (int)OrderEnum.Auditing || Convert.ToInt32(order.Status) == (int)OrderEnum.completed)
                            {
                                _customerService.ModifyCustomerServiceStatus(new int[] { socialSecurityPeopleID }, (int)CustomerServiceAuditEnum.Pass);
                                flag = true;
                            }

                            if (Convert.ToInt32(order.Status) == (int)OrderEnum.completed)
                            {
                                //更新社保和公积金为待办状态
                                _socialSecurityService.ModifySocialStatus(new int[] { socialSecurityPeopleID }, (int)SocialSecurityStatusEnum.WaitingHandle);
                                _accumulationFundService.ModifyAccumulationFundStatus(new int[] { socialSecurityPeopleID }, (int)SocialSecurityStatusEnum.WaitingHandle);
                            }
                        }
                        if (!flag) throw new Exception("所选参保人不符合申请条件");
                    }

                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    return Json(new { status = false, message = ex.Message });
                }
                finally
                {
                    transaction.Dispose();
                }
            }

            return Json(new { status = true, message = "申请通过" });
        }

        /// <summary>
        /// 根据参保人ID获取详情
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public ActionResult GetSocialSecurityPeopleDetail(int SocialSecurityPeopleID, int MemberID, int Type)
        {

            SocialSecurityPeople socialSecurityPeople = _socialSecurityService.GetSocialSecurityPeopleForAdmin(SocialSecurityPeopleID);

            if (socialSecurityPeople.IsPaySocialSecurity)
            {
                socialSecurityPeople.socialSecurity = _socialSecurityService.GetSocialSecurityDetail(SocialSecurityPeopleID);
                //企业签约单位列表
                List<EnterpriseSocialSecurity> SSList = _socialSecurityService.GetEnterpriseSocialSecurityByAreaList(socialSecurityPeople.socialSecurity.InsuranceArea, socialSecurityPeople.HouseholdProperty);
                EnterpriseSocialSecurity SS = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(socialSecurityPeople.socialSecurity.InsuranceArea, socialSecurityPeople.HouseholdProperty);
                ViewData["SSEnterpriseList"] = new SelectList(SSList, "EnterpriseID", "EnterpriseName", socialSecurityPeople.socialSecurity.RelationEnterprise);
                ViewData["SSMaxBase"] = Math.Round(SS.SocialAvgSalary * SS.MaxSocial / 100);
                ViewData["SSMinBase"] = Math.Round(SS.SocialAvgSalary * SS.MinSocial / 100);
            }
            if (socialSecurityPeople.IsPayAccumulationFund)
            {
                socialSecurityPeople.accumulationFund = _accumulationFundService.GetAccumulationFundDetail(SocialSecurityPeopleID);
                //企业签约单位列表
                List<EnterpriseSocialSecurity> AFList = _socialSecurityService.GetEnterpriseSocialSecurityByAreaList(socialSecurityPeople.accumulationFund.AccumulationFundArea, socialSecurityPeople.HouseholdProperty);
                EnterpriseSocialSecurity AF = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(socialSecurityPeople.accumulationFund.AccumulationFundArea, socialSecurityPeople.HouseholdProperty);
                ViewData["AFEnterpriseList"] = new SelectList(AFList, "EnterpriseID", "EnterpriseName", socialSecurityPeople.accumulationFund.RelationEnterprise);
                ViewData["AFMaxBase"] = AF.MaxAccumulationFund;
                ViewData["AFMinBase"] = AF.MinAccumulationFund;
            }

            //获取会员信息
            ViewData["member"] = _memberService.GetMemberInfoForAdmin(MemberID);

            //获取账户列表
            ViewData["accountRecordList"] = _memberService.GetAccountRecordList(MemberID).OrderByDescending(n => n.CreateTime).ToList();

            #region 户口性质
            List<SelectListItem> list = EnumExt.GetSelectList(typeof(HouseholdPropertyEnum));

            int householdType = 0;
            foreach (var item in list)
            {
                if (item.Text == socialSecurityPeople.HouseholdProperty)
                {
                    householdType = Convert.ToInt32(item.Value);
                    break;
                }
            }

            ViewData["HouseholdProperty"] = new SelectList(list, "value", "text", householdType);
            #endregion


            return View(socialSecurityPeople);
        }

        /// <summary>
        /// 根据参保人ID获取订单列表
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <returns></returns>
        public ActionResult GetOrderList(int SocialSecurityPeopleID)
        {
            List<Order> orderList = _orderService.GetOrderList(SocialSecurityPeopleID);
            return View(orderList);
        }


        /// <summary>
        /// 获取企业列表
        /// </summary>
        /// <param name="area"></param>
        /// <param name="HouseHoldProperty"></param>
        /// <returns></returns>
        public ActionResult GetEnterpriseSocialSecurityByAreaList(string area, string HouseHoldProperty)
        {
            List<EnterpriseSocialSecurity> list = _socialSecurityService.GetEnterpriseSocialSecurityByAreaList(area, HouseHoldProperty);
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 根据签约单位ID获取签约单位信息
        /// </summary>
        /// <param name="EnterpriseID"></param>
        /// <returns></returns>
        public ActionResult GetSSEnterprise(int EnterpriseID,int HouseholdProperty)
        {
            EnterpriseSocialSecurity model = _enterpriseService.GetEnterpriseSocialSecurity(EnterpriseID);
            decimal SSMaxBase = Math.Round(model.SocialAvgSalary * model.MaxSocial / 100);
            decimal SSMinBase = Math.Round(model.SocialAvgSalary * model.MinSocial / 100);
            decimal value = 0;
            if (HouseholdProperty == (int)HouseholdPropertyEnum.InRural ||
                HouseholdProperty == (int)HouseholdPropertyEnum.OutRural)
            {
                value = model.PersonalShiYeRural;
            }
            else if (HouseholdProperty == (int)HouseholdPropertyEnum.InTown ||
                HouseholdProperty == (int)HouseholdPropertyEnum.OutTown)
            {
                value = model.PersonalShiYeTown;
            }
            decimal SSPayProportion = model.CompYangLao + model.CompYiLiao + model.CompShiYe + model.CompGongShang + model.CompShengYu
                + model.PersonalYangLao + model.PersonalYiLiao + value + model.PersonalGongShang + model.PersonalShengYu;
            decimal SSMonthAccount = Math.Round(Math.Round(Convert.ToDecimal(SSMinBase)) * Convert.ToDecimal(SSPayProportion) / 100, 2);

            return Json(new
            {
                SSMaxBase = SSMaxBase,
                SSMinBase = SSMinBase,
                SSPayProportion = SSPayProportion,
                SSMonthAccount = SSMonthAccount
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 根据签约单位ID获取签约单位信息
        /// </summary>
        /// <param name="EnterpriseID"></param>
        /// <returns></returns>
        public ActionResult GetAFEnterprise(int EnterpriseID)
        {
            EnterpriseSocialSecurity model = _enterpriseService.GetEnterpriseSocialSecurity(EnterpriseID);
            decimal AFMaxBase = model.MaxAccumulationFund;
            decimal AFMinBase = model.MinAccumulationFund;
            decimal AFPayProportion = model.CompProportion + model.PersonalProportion;
            decimal AFMonthAccount = Math.Round(AFMinBase * AFPayProportion / 100, 2);

            return Json(new
            {
                AFMaxBase = AFMaxBase,
                AFMinBase = AFMinBase,
                AFPayProportion = AFPayProportion,
                AFMonthAccount = AFMonthAccount
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveEnterprise(SocialSecurityPeopleDetail model)
        {

            SocialSecurityPeople socialSecurityPeople = new SocialSecurityPeople();
            socialSecurityPeople.IdentityCard = model.IdentityCard;

            #region 户籍性质
            List<SelectListItem> list = EnumExt.GetSelectList(typeof(HouseholdPropertyEnum));

            foreach (var item in list)
            {
                if (item.Value == model.HouseholdProperty)
                {
                    socialSecurityPeople.HouseholdProperty = item.Text;
                    break;
                }
            }
            #endregion
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    #region 更新参保人
                    socialSecurityPeople.IdentityCardPhoto = string.Join(";", model.ImgUrls).Replace(ConfigurationManager.AppSettings["ServerUrl"], string.Empty);
                    DbHelper.ExecuteSqlCommand($"update SocialSecurityPeople set IdentityCard='{socialSecurityPeople.IdentityCard}',HouseholdProperty='{socialSecurityPeople.HouseholdProperty}',IdentityCardPhoto='{socialSecurityPeople.IdentityCardPhoto}' where SocialSecurityPeopleID={model.SocialSecurityPeopleID}", null);
                    #endregion

                    #region 更新用户
                    string inviteCode = string.Empty;
                    if (!string.IsNullOrEmpty(model.InviteCode))
                    {
                        inviteCode = _userService.GetUserInfoByUserID(model.InviteCode).InviteCode;
                    }
                    DbHelper.ExecuteSqlCommand($"update Members set InviteCode='{inviteCode}' where MemberID={model.MemberID}", null);
                    #endregion

                    if (model.IsPaySocialSecurity)
                    {
                        #region 更新社保
                        DbHelper.ExecuteSqlCommand($"update SocialSecurity set SocialSecurityNo='{model.SocialSecurityNo}',SocialSecurityBase='{model.SocialSecurityBase}',RelationEnterprise='{model.SSEnterpriseList}',PayProportion='{model.ssPayProportion.TrimEnd('%')}' where SocialSecurityPeopleID={model.SocialSecurityPeopleID}", null);
                        #endregion
                    }
                    if (model.IsPayAccumulationFund)
                    {
                        #region 更新公积金
                        DbHelper.ExecuteSqlCommand($"update AccumulationFund set AccumulationFundNo='{model.AccumulationFundNo}',AccumulationFundBase='{model.AccumulationFundBase}',RelationEnterprise='{model.AFEnterpriseList}',PayProportion='{model.afPayProportion.TrimEnd('%')}' where SocialSecurityPeopleID={model.SocialSecurityPeopleID}", null);
                        
                        #endregion
                    }
                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "更新失败";
                    return RedirectToAction("GetCustomerServiceList");
                }
                finally
                {
                    transaction.Dispose();
                }
            }
            TempData["Message"] = "更新成功";
            return RedirectToAction("GetCustomerServiceList");
        }

        public class SocialSecurityPeopleDetail
        {
            public int SocialSecurityPeopleID { get; set; }
            public string IdentityCard { get; set; }
            public string HouseholdProperty { get; set; }
            public string[] ImgUrls { get; set; }

            public int MemberID { get; set; }
            public string InviteCode { get; set; }


            public bool IsPaySocialSecurity { get; set; }
            public string SocialSecurityNo { get; set; }
            /// <summary>
            /// 签约单位
            /// </summary>
            public string SSEnterpriseList { get; set; }
            public string SocialSecurityBase { get; set; }
            public string ssPayProportion { get; set; }


            public bool IsPayAccumulationFund { get; set; }
            public string AccumulationFundNo { get; set; }
            /// <summary>
            /// 签约单位
            /// </summary>
            public string AFEnterpriseList { get; set; }
            public string AccumulationFundBase { get; set; }
            public string afPayProportion { get; set; }

        }
    }
}
