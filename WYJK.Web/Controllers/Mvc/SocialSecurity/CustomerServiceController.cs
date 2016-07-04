using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
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
    public class CustomerServiceController : BaseController
    {
        private readonly ICustomerService _customerService = new CustomerService();
        private readonly IOrderService _orderService = new OrderService();
        private readonly ISocialSecurityService _socialSecurityService = new SocialSecurityService();
        private readonly IAccumulationFundService _accumulationFundService = new AccumulationFundService();
        private readonly IMemberService _memberService = new MemberService();
        private readonly IEnterpriseService _enterpriseService = new EnterpriseService();
        private readonly IUserService _userService = new UserService();
        private readonly IParameterSettingService _parameterSettingService = new ParameterSettingService();

        private HttpClient client = new HttpClient();
        private string url = ConfigurationManager.AppSettings["ServerUrl"] + "/api";
        private JsonMediaTypeFormatter formatter = System.Web.Http.GlobalConfiguration.Configuration.Formatters.Where(f =>
        {
            return f.SupportedMediaTypes.Any(v => v.MediaType.Equals("application/json", StringComparison.CurrentCultureIgnoreCase));
        }).FirstOrDefault() as JsonMediaTypeFormatter;

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
            int socialSecurityPeopleID = SocialSecurityPeopleIDs[0];
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    //参保人变成已审核状态
                    _customerService.ModifyCustomerServiceStatus(new int[] { socialSecurityPeopleID }, (int)CustomerServiceAuditEnum.Pass);

                    //有关这个人社保的所有订单审核通过了，并且这个人社保的订单为已生成， 社保才能从未参保变成待办
                    SocialSecurity socialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select * from SocialSecurity where SocialSecurityPeopleID={socialSecurityPeopleID}");
                    if (socialSecurity != null && socialSecurity.IsGenerateOrder)
                    {
                        //这个人的所有社保订单都已经审核通过了
                        int result = DbHelper.QuerySingle<int>($"select count(0) from OrderDetails left join [Order] on [Order].OrderCode=OrderDetails.OrderCode where OrderDetails.SocialSecurityPeopleID={socialSecurityPeopleID} and [Order].Status in(0,1) and OrderDetails.IsPaySocialSecurity=1");
                        //社保从未参保变成待办
                        if (result == 0)
                            DbHelper.ExecuteSqlCommand($"update SocialSecurity set Status=2 where SocialSecurityPeopleID={socialSecurityPeopleID} and Status=1", null);
                    }

                    //有关这个人公积金的所有订单审核通过了，并且这个人公积金的订单为已生成， 公积金才能从未参保变成待办
                    AccumulationFund accumulationFund = DbHelper.QuerySingle<AccumulationFund>($"select * from AccumulationFund where SocialSecurityPeopleID={socialSecurityPeopleID}");
                    if (accumulationFund != null && accumulationFund.IsGenerateOrder)
                    {
                        //这个人的所有公积金订单都已经审核通过了
                        int result = DbHelper.QuerySingle<int>($"select count(0) from OrderDetails left join [Order] on [Order].OrderCode=OrderDetails.OrderCode where OrderDetails.SocialSecurityPeopleID={socialSecurityPeopleID} and [Order].Status in(0,1) and OrderDetails.IsPayAccumulationFund=1");
                        //公积金从未参保变成待办
                        if (result == 0)
                            DbHelper.ExecuteSqlCommand($"update AccumulationFund set Status=2 where SocialSecurityPeopleID={socialSecurityPeopleID} and Status=1", null);
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
                SocialSecurityPeopleID = 0;
            }
            else
            {
                socialSecurityPeople = _socialSecurityService.GetSocialSecurityPeopleForAdmin(SocialSecurityPeopleID.Value);
            }

            if (SocialSecurityPeopleID != null && _socialSecurityService.GetSocialSecurityDetail(SocialSecurityPeopleID.Value) != null)
            {
                socialSecurityPeople.socialSecurity = _socialSecurityService.GetSocialSecurityDetail(SocialSecurityPeopleID.Value);
                //企业签约单位列表
                List<EnterpriseSocialSecurity> SSList = _socialSecurityService.GetEnterpriseSocialSecurityByAreaList(socialSecurityPeople.socialSecurity.InsuranceArea, socialSecurityPeople.HouseholdProperty);
                EnterpriseSocialSecurity SS = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(socialSecurityPeople.socialSecurity.InsuranceArea, socialSecurityPeople.HouseholdProperty);
                ViewData["SSEnterpriseList"] = new SelectList(SSList, "EnterpriseID", "EnterpriseName", socialSecurityPeople.socialSecurity.RelationEnterprise);
                ViewData["SSMaxBase"] = Math.Round(SS.SocialAvgSalary * SS.MaxSocial / 100);
                ViewData["SSMinBase"] = Math.Round(SS.SocialAvgSalary * SS.MinSocial / 100);
            }
            if (SocialSecurityPeopleID != null && _accumulationFundService.GetAccumulationFundDetail(SocialSecurityPeopleID.Value) != null)
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


            List<AccountRecord> accountRecordList = _memberService.GetAccountRecordList(MemberID.Value).OrderByDescending(n => n.CreateTime).ToList();
            //获取账户列表
            ViewData["accountRecordList"] = accountRecordList;

            //获取社保缴费明细
            ViewData["SSAccountRecordList"] = accountRecordList.Where(n => n.SocialSecurityPeopleID == SocialSecurityPeopleID.Value && n.Type == "0").ToList();


            //获取公积金缴费明细
            ViewData["AFAccountRecordList"] = accountRecordList.Where(n => n.SocialSecurityPeopleID == SocialSecurityPeopleID.Value && n.Type == "1").ToList();

            //调整社平工资的缴费明细
            ViewData["SocialAvgSalaryRecordList"] = accountRecordList.Where(n => n.SocialSecurityPeopleID == SocialSecurityPeopleID.Value && n.Type == "2").ToList();


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
        /// 是否存在业务异常
        /// </summary>
        /// <returns></returns>
        public ActionResult IsExistsExceptionTip()
        {
            //当前客服所对应的对应的
            ExceptionTip exceptionTip = DbHelper.QuerySingle<ExceptionTip>($@"select top 1 * from
  (select SocialSecurityPeople.SocialSecurityPeopleID,  0 Type, SocialSecurityPeople.SocialSecurityPeopleName + '社保业务办理异常:' + SocialSecurity.SocialSecurityException ExceptionReason from SocialSecurityPeople
      left join SocialSecurity on SocialSecurity.SocialSecurityPeopleID = SocialSecurityPeople.SocialSecurityPeopleID
  where SocialSecurityPeople.CustomerServiceUserName = '{ HttpContext.User.Identity.Name}' and SocialSecurity.IsException = 1
  union all
  select SocialSecurityPeople.SocialSecurityPeopleID,  1 Type,SocialSecurityPeople.SocialSecurityPeopleName + '公积金业务办理异常:' + AccumulationFund.AccumulationFundException ExceptionReason from SocialSecurityPeople
      left join AccumulationFund on AccumulationFund.SocialSecurityPeopleID = SocialSecurityPeople.SocialSecurityPeopleID
  where SocialSecurityPeople.CustomerServiceUserName = '{ HttpContext.User.Identity.Name}' and AccumulationFund.IsException = 1) t");
            if (exceptionTip != null)
                return Json(new { status = true },JsonRequestBehavior.AllowGet);
            else
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 异常提示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ExceptionTip()
        {
            //当前客服所对应的对应的
            ExceptionTip exceptionTip = DbHelper.QuerySingle<ExceptionTip>($@"select top 1 * from
  (select SocialSecurityPeople.SocialSecurityPeopleID,  0 Type, SocialSecurityPeople.SocialSecurityPeopleName + '社保业务办理异常:' + SocialSecurity.SocialSecurityException ExceptionReason from SocialSecurityPeople
      left join SocialSecurity on SocialSecurity.SocialSecurityPeopleID = SocialSecurityPeople.SocialSecurityPeopleID
  where SocialSecurityPeople.CustomerServiceUserName =  '{ HttpContext.User.Identity.Name}' and SocialSecurity.IsException = 1
  union all
  select SocialSecurityPeople.SocialSecurityPeopleID,  1 Type,SocialSecurityPeople.SocialSecurityPeopleName + '公积金业务办理异常:' + AccumulationFund.AccumulationFundException ExceptionReason from SocialSecurityPeople
      left join AccumulationFund on AccumulationFund.SocialSecurityPeopleID = SocialSecurityPeople.SocialSecurityPeopleID
  where SocialSecurityPeople.CustomerServiceUserName = '{ HttpContext.User.Identity.Name}' and AccumulationFund.IsException = 1) t");

            return View(exceptionTip);

        }

        /// <summary>
        /// 异常提示
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExceptionTip(ExceptionTip exceptionTip)
        {
            switch (exceptionTip.Type)
            {
                case 0:
                    DbHelper.ExecuteSqlCommand($"update SocialSecurity set IsException=0 where SocialSecurityPeopleID={exceptionTip.SocialSecurityPeopleID}", null);
                    break;
                case 1:
                    DbHelper.ExecuteSqlCommand($"update AccumulationFund set IsException=0 where SocialSecurityPeopleID={exceptionTip.SocialSecurityPeopleID}", null);
                    break;
            }
            return View();
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
            socialSecurityPeople.SocialSecurityPeopleName = model.SocialSecurityPeopleName;
            socialSecurityPeople.IdentityCardPhoto = string.Join(";", model.ImgUrls).Replace(ConfigurationManager.AppSettings["ServerUrl"], string.Empty);

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
                    //日志记录
                    string logStr = string.Empty;

                    #region 更新参保人

                    #region 参保人的日志记录
                    //获取参保人旧数据
                    SocialSecurityPeople oldSocialSecurityPeople = DbHelper.QuerySingle<SocialSecurityPeople>($"select * from SocialSecurityPeople where SocialSecurityPeopleID={model.SocialSecurityPeopleID}");
                    //比较新旧值是否一致
                    if (oldSocialSecurityPeople.SocialSecurityPeopleName != socialSecurityPeople.SocialSecurityPeopleName)
                    {
                        logStr += "客服修改了{1}的姓名,从" + oldSocialSecurityPeople.SocialSecurityPeopleName + "到{1};";
                    }
                    if (oldSocialSecurityPeople.IdentityCard != socialSecurityPeople.IdentityCard)
                    {
                        logStr += "客服修改了{1}的身份证号,从" + oldSocialSecurityPeople.IdentityCard + "到" + socialSecurityPeople.IdentityCard + ";";
                    }
                    if (oldSocialSecurityPeople.HouseholdProperty != socialSecurityPeople.HouseholdProperty)
                    {
                        logStr += "客服修改了{1}的户籍性质,从" + oldSocialSecurityPeople.HouseholdProperty + "到" + socialSecurityPeople.HouseholdProperty + ";";
                    }
                    if (oldSocialSecurityPeople.IdentityCardPhoto != socialSecurityPeople.IdentityCardPhoto)
                    {
                        logStr += "客服修改了{1}的身份证照;";
                    }
                    #endregion

                    //参保人新数据更新

                    DbHelper.ExecuteSqlCommand($"update SocialSecurityPeople set SocialSecurityPeopleName='{socialSecurityPeople.SocialSecurityPeopleName}', IdentityCard='{socialSecurityPeople.IdentityCard}',HouseholdProperty='{socialSecurityPeople.HouseholdProperty}',IdentityCardPhoto='{socialSecurityPeople.IdentityCardPhoto}' where SocialSecurityPeopleID={model.SocialSecurityPeopleID}", null);
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

                    //查看是否有未付款的订单，如果有，则更改原来的订单，如果没有
                    //然后查看参保人是否已经支付，如果已经支付则需要生成一个待支付的差额订单

                    //查找参保人对应的社保和公积金的交费金额
                    decimal OldSSAmount = 0, NewSSAmount = 0, OldAFAmount = 0, NewAFAmount = 0;
                    if (_socialSecurityService.GetSocialSecurityDetail(model.SocialSecurityPeopleID) != null)
                    {
                        OldSSAmount = Math.Round(DbHelper.QuerySingle<decimal>($"select SocialSecurityBase*PayProportion/100 from SocialSecurity where SocialSecurityPeopleID={ model.SocialSecurityPeopleID}"), 2);
                        NewSSAmount = Math.Round(Convert.ToDecimal(model.SocialSecurityBase) * Convert.ToDecimal(model.ssPayProportion.TrimEnd('%')) / 100, 2);//现在社保金额
                    }

                    if (_accumulationFundService.GetAccumulationFundDetail(model.SocialSecurityPeopleID) != null)
                    {
                        OldAFAmount = Math.Round(DbHelper.QuerySingle<decimal>($"select AccumulationFundBase*PayProportion/100 from AccumulationFund where SocialSecurityPeopleID={ model.SocialSecurityPeopleID}"), 2);
                        NewAFAmount = Math.Round(Convert.ToDecimal(model.AccumulationFundBase) * Convert.ToDecimal(model.afPayProportion.TrimEnd('%')) / 100, 2);//现在公积金金额
                    }
                    //如果新社保金额大于原来金额
                    if (NewSSAmount > OldSSAmount)
                    {
                        //查看是否存在社保未付款的订单
                        if (DbHelper.QuerySingle<int>($"select count(0) from [Order] left join OrderDetails on [Order].OrderCode =OrderDetails.OrderCode where SocialSecurityPeopleID={ model.SocialSecurityPeopleID} and OrderDetails.IsPaySocialSecurity=1 and [Order].Status=0") > 0)
                        {
                            DbHelper.ExecuteSqlCommand($"update OrderDetails set SocialSecurityAmount='{NewSSAmount}' where OrderDetailID=(select OrderDetails.OrderDetailID form [Order] left join OrderDetails on [Order].OrderCode =OrderDetails.OrderCode where SocialSecurityPeopleID={ model.SocialSecurityPeopleID} and OrderDetails.IsPaySocialSecurity=1 and [Order].Status=0)", null);
                        }
                        //查看参保人是否已经支付，如果已经支付则需要生成一个待支付的差额订单
                        else if (DbHelper.QuerySingle<bool>($"select IsPay from SocialSecurity where SocialSecurityPeopleID={ model.SocialSecurityPeopleID}"))
                        {
                            SocialSecurity socialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select * from SocialSecurity where  SocialSecurityPeopleID={ model.SocialSecurityPeopleID}");
                            string orderCode = DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random().Next(1000).ToString().PadLeft(3, '0');
                            DbHelper.ExecuteSqlCommand($@"insert into [Order](OrderCode,MemberID,GenerateDate,Status,IsNotCancel) 
values('{orderCode}',{model.MemberID},'{DateTime.Now}',0,1);
insert into OrderDetails(OrderCode,SocialSecurityPeopleID,SocialSecurityPeopleName,SSPayTime,SocialSecurityAmount,SocialSecuritypayMonth,IsPaySocialSecurity)
 values('{orderCode}',{model.SocialSecurityPeopleID},'{model.SocialSecurityPeopleName}','{socialSecurity.PayTime}',{NewSSAmount - OldSSAmount},{socialSecurity.PayMonthCount},1)", null);
                        }
                    }

                    //如果新公积金金额大于原来金额
                    if (NewAFAmount > OldAFAmount)
                    {
                        //查看是否存在公积金未付款订单
                        if (DbHelper.QuerySingle<int>($"select count(0) from [Order] left join OrderDetails on [Order].OrderCode =OrderDetails.OrderCode where SocialSecurityPeopleID={ model.SocialSecurityPeopleID} and OrderDetails.IsPayAccumulationFund=1 and [Order].Status=0") > 0)
                        {
                            DbHelper.ExecuteSqlCommand($"update OrderDetails set AccumulationFundAmount='{NewAFAmount}' where OrderDetailID=(select OrderDetails.OrderDetailID form [Order] left join OrderDetails on [Order].OrderCode =OrderDetails.OrderCode where SocialSecurityPeopleID={ model.SocialSecurityPeopleID} and OrderDetails.IsPayAccumulationFund=1 and [Order].Status=0)", null);
                        }
                        else if (DbHelper.QuerySingle<bool>($"select IsPay from AccumulationFund where SocialSecurityPeopleID={ model.SocialSecurityPeopleID}"))
                        {
                            AccumulationFund accumulationFund = DbHelper.QuerySingle<AccumulationFund>($"select * from AccumulationFund where  SocialSecurityPeopleID={ model.SocialSecurityPeopleID}");
                            string orderCode = DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random().Next(1000).ToString().PadLeft(3, '0');
                            DbHelper.ExecuteSqlCommand($@"insert into [Order](OrderCode,MemberID,GenerateDate,Status,IsNotCancel) 
values('{orderCode}',{model.MemberID},'{DateTime.Now}',0,1);
insert into OrderDetails(OrderCode,SocialSecurityPeopleID,SocialSecurityPeopleName,AFPayTime,AccumulationFundAmount,AccumulationFundpayMonth,IsPayAccumulationFund)
 values('{orderCode}',{model.SocialSecurityPeopleID},'{model.SocialSecurityPeopleName}','{accumulationFund.PayTime}',{NewAFAmount - OldAFAmount},{accumulationFund.PayMonthCount},1)", null);
                        }
                    }


                    #endregion


                    if (_socialSecurityService.GetSocialSecurityDetail(model.SocialSecurityPeopleID) != null)
                    {
                        #region 社保的日志记录
                        //参保原数据
                        SocialSecurity oldSocialSecurity = _socialSecurityService.GetSocialSecurityDetail(model.SocialSecurityPeopleID);
                        //客户社保号
                        if (oldSocialSecurity.SocialSecurityNo != model.SocialSecurityNo)
                        {
                            logStr += "客服修改了{1}的客户社保号,从" + oldSocialSecurity.SocialSecurityNo + "到" + model.SocialSecurityNo + ";";
                        }
                        //签约单位
                        if (oldSocialSecurity.RelationEnterprise != Convert.ToInt32(model.SSEnterpriseList))
                        {
                            string oldRelationEnterprise = DbHelper.QuerySingle<string>("select EnterpriseName from EnterpriseSocialSecurity where EnterpriseID=" + oldSocialSecurity.RelationEnterprise);
                            string newRelationEnterprise = DbHelper.QuerySingle<string>("select EnterpriseName from EnterpriseSocialSecurity where EnterpriseID=" + model.SSEnterpriseList);
                            logStr += "客服修改了{1}的签约单位,从" + oldRelationEnterprise + "到" + newRelationEnterprise + ";";
                        }
                        //基数
                        if (oldSocialSecurity.SocialSecurityBase != Convert.ToDecimal(model.SocialSecurityBase))
                        {
                            logStr += "客服修改了{1}的基数,从" + oldSocialSecurity.SocialSecurityBase + "到" + model.SocialSecurityBase + ";";
                        }

                        #endregion

                        #region 更新社保
                        DbHelper.ExecuteSqlCommand($"update SocialSecurity set SocialSecurityNo='{model.SocialSecurityNo}',SocialSecurityBase='{model.SocialSecurityBase}',RelationEnterprise='{model.SSEnterpriseList}',PayProportion='{model.ssPayProportion.TrimEnd('%')}' where SocialSecurityPeopleID={model.SocialSecurityPeopleID}", null);
                        #endregion
                    }
                    if (_accumulationFundService.GetAccumulationFundDetail(model.SocialSecurityPeopleID) != null)
                    {
                        #region 公积金的日志记录
                        //公积金原数据
                        AccumulationFund oldAccumulationFund = _accumulationFundService.GetAccumulationFundDetail(model.SocialSecurityPeopleID);
                        //客户公积金号
                        if (oldAccumulationFund.AccumulationFundNo != model.AccumulationFundNo)
                        {
                            logStr += "客服修改了{1}的客户公积金号,从" + oldAccumulationFund.AccumulationFundNo + "到" + model.AccumulationFundNo + ";";
                        }
                        //签约单位
                        if (oldAccumulationFund.RelationEnterprise != Convert.ToInt32(model.AFEnterpriseList))
                        {
                            string oldRelationEnterprise = DbHelper.QuerySingle<string>("select EnterpriseName from EnterpriseSocialSecurity where EnterpriseID=" + oldAccumulationFund.RelationEnterprise);
                            string newRelationEnterprise = DbHelper.QuerySingle<string>("select EnterpriseName from EnterpriseSocialSecurity where EnterpriseID=" + model.AFEnterpriseList);
                            logStr += "客服修改了{1}的签约单位,从" + oldRelationEnterprise + "到" + newRelationEnterprise + ";";
                        }
                        //基数
                        if (oldAccumulationFund.AccumulationFundBase != Convert.ToDecimal(model.AccumulationFundBase))
                        {
                            logStr += "客服修改了{1}的基数,从" + oldAccumulationFund.AccumulationFundBase + "到" + model.AccumulationFundBase + ";";
                        }

                        #endregion

                        #region 更新公积金
                        DbHelper.ExecuteSqlCommand($"update AccumulationFund set AccumulationFundNo='{model.AccumulationFundNo}',AccumulationFundBase='{model.AccumulationFundBase}',RelationEnterprise='{model.AFEnterpriseList}',PayProportion='{model.afPayProportion.TrimEnd('%')}' where SocialSecurityPeopleID={model.SocialSecurityPeopleID}", null);
                        #endregion
                    }

                    if (logStr != string.Empty)
                        LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = logStr, SocialSecurityPeopleID = model.SocialSecurityPeopleID });

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

                        model.socialSecurity.SocialSecurityBase = model.SocialSecurityBase;
                        model.socialSecurity.PayProportion = model.SSPayProportion;
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

                        model.accumulationFund.AccumulationFundBase = model.AccumulationFundBase;
                        model.accumulationFund.PayProportion = model.SSPayProportion;
                        model.accumulationFund.AccumulationFundType = "1";

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
        public ActionResult Recharge(int? id)
        {
            return View(_memberService.GetAccountInfo(id.Value));
        }

        [HttpPost]
        public async Task<ActionResult> Recharge(int? id, string PayMethod, decimal? Amount)
        {
            try
            {
                var req = await client.PostAsJsonAsync(url + "/Member/SubmitRechargeAmount", new { MemberID = id, PayMethod, Amount });
                var result = await req.Content.ReadAsAsync<JsonResult<object>>();

                TempData["Message"] = result.Message;
            }
            catch (Exception ex)
            {
                TempData["Message"] = "充值失败！";
            }
            return RedirectToAction("CustomerPaymentManagement");
        }

        /// <summary>
        /// 交费
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Payment(int? id, int? peopleid)
        {
            ViewBag.SocialSecurityPeople = _socialSecurityService.GetSocialSecurityPeopleForAdmin(peopleid.Value);
            ViewBag.SocialSecurityDetail = _socialSecurityService.GetSocialSecurityDetail(peopleid.Value);
            ViewBag.AccumulationFund = _accumulationFundService.GetAccumulationFundDetail(peopleid.Value);
            ViewBag.AccountInfo = _memberService.GetAccountInfo(id.Value);

            ////未参保状态
            int status = (int)SocialSecurityStatusEnum.UnInsured;

            UnInsuredPeople item = (await _socialSecurityService.GetUnInsuredPeopleList(id.Value, status, peopleid.Value)).FirstOrDefault();

            if (item == null)
            {
                TempData["Message"] = "已参保。";
                return RedirectToAction("CustomerPaymentManagement");
            }

            if (item.IsPaySocialSecurity)
            {
                item.SocialSecurityAmount = item.SocialSecurityBase * item.SSPayProportion / 100 * item.SSPayMonthCount;
                item.socialSecurityFirstBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.SocialSecurity).BacklogCost;
            }
            if (item.IsPayAccumulationFund)
            {
                item.AccumulationFundAmount = item.AccumulationFundBase * item.AFPayProportion / 100 * item.AFPayMonthCount;
                item.AccumulationFundFirstBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.AccumulationFund).BacklogCost;
            }

            if (item.SSStatus != (int)SocialSecurityStatusEnum.UnInsured)
            {
                item.SSStatus = 0;
            }

            if (item.AFStatus != (int)SocialSecurityStatusEnum.UnInsured)
            {
                item.AFStatus = 0;
            }

            return View(item);
        }

        [HttpPost]
        public async Task<ActionResult> Payment(int? id, int? peopleid, string PayMethod, decimal? Amount)
        {
            try
            {
                var req = await client.PostAsJsonAsync(url + "/Order/GenerateOrder", new { MemberID = id, SocialSecurityPeopleIDS = new[] { peopleid } });

                var result = await req.Content.ReadAsAsync<JsonResult<Dictionary<bool, string>>>();
                TempData["Message"] = result.Message;

                if (result.status)
                {
                    var ordercode = result.Data[true];
                    var payreq = await client.PostAsJsonAsync(url + "/Order/OrderPayment", new { OrderCode = ordercode, MemberID = id, PaymentMethod = PayMethod, GenerateDate = DateTime.Now, Status = 2, PayTime = DateTime.Now, AuditTime = DateTime.Now });

                    var result1 = await payreq.Content.ReadAsAsync<JsonResult<Dictionary<bool, string>>>();
                    TempData["Message"] = result1.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "交费失败。";
            }

            return RedirectToAction("CustomerPaymentManagement");
        }

        /// <summary>
        /// 买服务
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> BuyService(int? id)
        {
            var req1 = await client.GetAsync(url + $"/Member/GetAccountStatus?MemberID={id}");
            if ((await req1.Content.ReadAsAsync<JsonResult<string>>()).Data == "待续费")
            {
                var req = await client.GetAsync(url + $"/Member/GetRenewalServiceList?MemberID={id}");
                ViewBag.RenewalServiceList = (await req.Content.ReadAsAsync<JsonResult<List<KeyValuePair<int, decimal>>>>()).Data;
                return View(_memberService.GetAccountInfo(id.Value));
            }
            else
            {
                TempData["Message"] = "该账户未到续费时间。";
                return RedirectToAction("CustomerPaymentManagement");
            }
        }

        [HttpPost]
        public async Task<ActionResult> BuyService(int? id, string PayMethod, string MonthCountAmount)
        {
            try
            {
                var ma = MonthCountAmount.Split('-');
                var req = await client.PostAsJsonAsync(url + "/Member/SubmitRenewalService", new { MemberID = id, PayMethod, MonthCount = ma[0], Amount = ma[1] });

                var result = await req.Content.ReadAsAsync<JsonResult<object>>();
                TempData["Message"] = result.Message;

            }
            catch (Exception ex)
            {
                TempData["Message"] = "续费失败。";
            }
            return RedirectToAction("CustomerPaymentManagement");
        }

        public class SocialSecurityPeopleDetail
        {
            public int SocialSecurityPeopleID { get; set; }
            public string SocialSecurityPeopleName { get; set; }
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
