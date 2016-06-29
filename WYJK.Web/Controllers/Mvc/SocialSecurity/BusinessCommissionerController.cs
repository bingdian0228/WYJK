using System;
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
    /// 业务专员
    /// </summary>
    [Authorize]
    public class BusinessCommissionerController : Controller
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
        /// 获取业务专员客户列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetBusinessCommissionerList(CustomerServiceParameter parameter)
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

            if (_socialSecurityService.GetSocialSecurityDetail(SocialSecurityPeopleID.Value) != null)
            {
                socialSecurityPeople.socialSecurity = _socialSecurityService.GetSocialSecurityDetail(SocialSecurityPeopleID.Value);
                //企业签约单位列表
                List<EnterpriseSocialSecurity> SSList = _socialSecurityService.GetEnterpriseSocialSecurityByAreaList(socialSecurityPeople.socialSecurity.InsuranceArea, socialSecurityPeople.HouseholdProperty);
                EnterpriseSocialSecurity SS = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(socialSecurityPeople.socialSecurity.InsuranceArea, socialSecurityPeople.HouseholdProperty);
                ViewData["SSEnterpriseList"] = new SelectList(SSList, "EnterpriseID", "EnterpriseName", socialSecurityPeople.socialSecurity.RelationEnterprise);
                ViewData["SSMaxBase"] = Math.Round(SS.SocialAvgSalary * SS.MaxSocial / 100);
                ViewData["SSMinBase"] = Math.Round(SS.SocialAvgSalary * SS.MinSocial / 100);
            }
            if (_accumulationFundService.GetAccumulationFundDetail(SocialSecurityPeopleID.Value) != null)
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
    }
}