﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private readonly IParameterSettingService _parameterSettingService = new ParameterSettingService();

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

            List<Members> memberList = _memberService.GetMembersList();
                memberList.ForEach(item =>
                {
                    item.MemberName = item.UserType == "0" ? item.MemberName : (item.UserType == "1" ? item.EnterpriseName : item.BusinessName);
                });
            ViewBag.memberList = memberList;

            List<SelectListItem> CustomerServiceAudit = EnumExt.GetSelectList(typeof(CustomerServiceAuditEnum));
            CustomerServiceAudit.Insert(0, new SelectListItem { Text = "全部", Value = "" });

            ViewData["CustomerServiceStatus"] = new SelectList(CustomerServiceAudit, "Value", "Text");

            //.Select(item => new { MemberID = item.MemberID, MemberName = item.UserType == "0" ? item.MemberName : (item.UserType == "1" ? item.EnterpriseName : item.BusinessName) }); ;

            return View(customerServiceList);
        }

        /// <summary>
        /// 获取客户管理列表
        /// </summary>
        /// <returns></returns>
        public ActionResult CustomerPaymentManagement(CustomerServiceParameter parameter)
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
                .Select(item => new { MemberID = item.MemberID, MemberName = item.UserType == "0" ? item.MemberName : (item.UserType == "1" ? item.EnterpriseName : item.BusinessName) });
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
                        //参保人变成已审核状态
                        _customerService.ModifyCustomerServiceStatus(new int[] { socialSecurityPeopleID }, (int)CustomerServiceAuditEnum.Pass);




                        List<Order> orderList = _orderService.GetOrderList(socialSecurityPeopleID);

                        foreach (var order in orderList)
                        {
                            //if (Convert.ToInt32(order.Status) == (int)OrderEnum.Auditing || Convert.ToInt32(order.Status) == (int)OrderEnum.completed)
                            //{
                            //    _customerService.ModifyCustomerServiceStatus(new int[] { socialSecurityPeopleID }, (int)CustomerServiceAuditEnum.Pass);
                            //    flag = true;
                            //}

                            //if (Convert.ToInt32(order.Status) == (int)OrderEnum.completed)
                            //{
                            //    //更新社保和公积金为待办状态
                            //    _socialSecurityService.ModifySocialStatus(new int[] { socialSecurityPeopleID }, (int)SocialSecurityStatusEnum.WaitingHandle);
                            //    _accumulationFundService.ModifyAccumulationFundStatus(new int[] { socialSecurityPeopleID }, (int)SocialSecurityStatusEnum.WaitingHandle);
                            //}

                            //有关这个人社保的所有订单审核通过了，社保才能从未参保变成待办


                            //有关这个人公积金的所有订单审核通过了，公积金才能从未参保变成待办

                        }
                        
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
        public ActionResult GetSocialSecurityPeopleDetail(int? SocialSecurityPeopleID, int? MemberID, int Type, int? IsAdd)
        {
            SocialSecurityPeople socialSecurityPeople = null;

            if (SocialSecurityPeopleID == null)
            {
                socialSecurityPeople = new SocialSecurityPeople() { socialSecurity = new SocialSecurity(), accumulationFund = new AccumulationFund() };
            }
            else
            {
                socialSecurityPeople = _socialSecurityService.GetSocialSecurityPeopleForAdmin(SocialSecurityPeopleID.Value);
            }

            if (socialSecurityPeople.IsPaySocialSecurity)
            {
                socialSecurityPeople.socialSecurity = _socialSecurityService.GetSocialSecurityDetail(SocialSecurityPeopleID.Value);
                //企业签约单位列表
                List<EnterpriseSocialSecurity> SSList = _socialSecurityService.GetEnterpriseSocialSecurityByAreaList(socialSecurityPeople.socialSecurity.InsuranceArea, socialSecurityPeople.HouseholdProperty);
                EnterpriseSocialSecurity SS = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(socialSecurityPeople.socialSecurity.InsuranceArea, socialSecurityPeople.HouseholdProperty);
                ViewData["SSEnterpriseList"] = new SelectList(SSList, "EnterpriseID", "EnterpriseName", socialSecurityPeople.socialSecurity.RelationEnterprise);
                ViewData["SSMaxBase"] = Math.Round(SS.SocialAvgSalary * SS.MaxSocial / 100);
                ViewData["SSMinBase"] = Math.Round(SS.SocialAvgSalary * SS.MinSocial / 100);
            }
            if (socialSecurityPeople.IsPayAccumulationFund)
            {
                socialSecurityPeople.accumulationFund = _accumulationFundService.GetAccumulationFundDetail(SocialSecurityPeopleID.Value);
                //企业签约单位列表
                List<EnterpriseSocialSecurity> AFList = _socialSecurityService.GetEnterpriseSocialSecurityByAreaList(socialSecurityPeople.accumulationFund.AccumulationFundArea, socialSecurityPeople.HouseholdProperty);
                EnterpriseSocialSecurity AF = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(socialSecurityPeople.accumulationFund.AccumulationFundArea, socialSecurityPeople.HouseholdProperty);
                ViewData["AFEnterpriseList"] = new SelectList(AFList, "EnterpriseID", "EnterpriseName", socialSecurityPeople.accumulationFund.RelationEnterprise);
                ViewData["AFMaxBase"] = AF.MaxAccumulationFund;
                ViewData["AFMinBase"] = AF.MinAccumulationFund;
            }

            //获取会员信息
            ViewData["member"] = _memberService.GetMemberInfoForAdmin(MemberID.Value);

            //获取账户列表
            ViewData["accountRecordList"] = _memberService.GetAccountRecordList(MemberID.Value).OrderByDescending(n => n.CreateTime).ToList();

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

        public ActionResult GetESList(string area, string household)
        {
            var SSList = _socialSecurityService.GetEnterpriseSocialSecurityByAreaList(area, household).Select(a => new { a.EnterpriseID, a.EnterpriseName }).ToList();
            var SS = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(area, household);
            return Json(new
            {
                SSMaxBase = Math.Round(SS.SocialAvgSalary * SS.MaxSocial / 100),
                SSMinBase = Math.Round(SS.SocialAvgSalary * SS.MinSocial / 100),
                List = SSList
            });
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
        public ActionResult GetSSEnterprise(int EnterpriseID, int HouseholdProperty)
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

                    #region 生成子订单


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

        [HttpPost]
        public async Task<ActionResult> AddSocialSecurityPeople(SocialSecurityPeople model)
        {
            //验证身份证
            if (!Regex.IsMatch(model.IdentityCard, @"(^\d{18}$)|(^\d{15}$)"))
            {
                TempData["Message"] = "身份证号填写错误";
                return RedirectToAction("GetCustomerServiceList");
            }

            //判断身份证是否已存在
            if (_socialSecurityService.IsExistsSocialSecurityPeopleIdentityCard(model.IdentityCard))
            {
                TempData["Message"] = "身份证已存在";
                return RedirectToAction("GetCustomerServiceList");
            }
            model.IdentityCardPhoto = string.Join(";", model.ImgUrls).Replace(ConfigurationManager.AppSettings["ServerUrl"], string.Empty);


            int SocialSecurityID = 0;
            decimal SocialSecurityAmount = 0;
            int SocialSecurityMonthCount = 0;
            decimal SocialSecurityBacklogCost = 0;
            int AccumulationFundID = 0;
            decimal AccumulationFundAmount = 0;
            int AccumulationFundMonthCount = 0;
            decimal AccumulationFundBacklogCost = 0;

            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    //保存社保参保方案
                    if (model.socialSecurity != null)
                    {
                        //if (model.socialSecurity.PayTime.Day > 14)
                        //    model.socialSecurity.PayTime.AddMonths(1);

                        //返回社保ID
                        SocialSecurityID = _socialSecurityService.AddSocialSecurity(model.socialSecurity);
                        //查询社保金额
                        SocialSecurityAmount = _socialSecurityService.GetSocialSecurityAmount(SocialSecurityID);
                        //查询社保月数
                        SocialSecurityMonthCount = _socialSecurityService.GetSocialSecurityMonthCount(SocialSecurityID);
                    }

                    //保存公积金参保方案
                    if (model.accumulationFund != null)
                    {
                        //if (model.accumulationFund.PayTime.Day > 14)
                        //    model.accumulationFund.PayTime.AddMonths(1);
                        //返回公积金ID
                        AccumulationFundID = _socialSecurityService.AddAccumulationFund(model.accumulationFund);
                        //查询公积金金额
                        AccumulationFundAmount = _socialSecurityService.GetAccumulationFundAmount(AccumulationFundID);
                        //查询公积金月数
                        AccumulationFundMonthCount = _socialSecurityService.GetAccumulationFundMonthCount(AccumulationFundID);
                    }

                    transaction.Complete();


                    TempData["Message"] = "添加成功";
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "添加失败";
                }
                finally
                {
                    transaction.Dispose();
                }
                model.IsPaySocialSecurity = true;
                model.IsPayAccumulationFund = true;

                model.accumulationFund.AccumulationFundID = AccumulationFundID;
                model.socialSecurity.SocialSecurityID = SocialSecurityID;
                var result = await _socialSecurityService.AddSocialSecurityPeople(model);

                TempData["Message"] = result ? "添加成功" : "添加失败";
            }
            return RedirectToAction("GetCustomerServiceList");
        }

        /// <summary>
        /// 充值
        /// </summary>
        /// <returns></returns>
        public ActionResult Recharge(int? id, int? memberid)
        {

            return View();
        }

        /// <summary>
        /// 缴费
        /// </summary>
        /// <returns></returns>
        public ActionResult Payment(int? id)
        {
            ////未参保状态
            //int status = (int)SocialSecurityStatusEnum.UnInsured;

            //UnInsuredPeople item = (await _socialSecurityService.GetUnInsuredPeopleList(id.Value, status, peopleid.Value)).FirstOrDefault();

            //if (item.IsPaySocialSecurity)
            //{
            //    item.SocialSecurityAmount = item.SocialSecurityBase * item.SSPayProportion / 100 * item.SSPayMonthCount;
            //    item.socialSecurityFirstBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.SocialSecurity).BacklogCost;
            //}
            //if (item.IsPayAccumulationFund)
            //{
            //    item.AccumulationFundAmount = item.AccumulationFundBase * item.AFPayProportion / 100 * item.AFPayMonthCount;
            //    item.AccumulationFundFirstBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.AccumulationFund).BacklogCost;
            //}

            //if (item.SSStatus != (int)SocialSecurityStatusEnum.UnInsured)
            //{
            //    item.SSStatus = 0;
            //}

            //if (item.AFStatus != (int)SocialSecurityStatusEnum.UnInsured)
            //{
            //    item.AFStatus = 0;
            //}

            return View();
        }

        /// <summary>
        /// 买服务
        /// </summary>
        /// <returns></returns>
        public ActionResult BuyService(int? id)
        {
            return View();
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
