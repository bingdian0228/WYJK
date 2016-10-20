using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WYJK.Data.IService;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;
using WYJK.HOME.Models;
using WYJK.HOME.Service;

namespace WYJK.HOME.Controllers
{
    public class LoanController : BaseFilterController
    {
        Service.SocialSecurityService sss = new Service.SocialSecurityService();
        private ILoanMemberService _loanMemberService = new LoanMemberService();
        private ILoanSubjectService _loanSubjectService = new LoanSubjectService();

        private string url = ConfigurationManager.AppSettings["ServerUrl"] + "/api";

        /// <summary>
        /// 借款
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Loan()
        {
            Members m = CommonHelper.CurrentUser;

            //判断是否已缴费//缴费不满三个月
            //if (!sss.ExistSocialPeople(m.MemberID))
            //{
            //    //添加社保人
            //    return Redirect("/UserInsurance/Add1");
            //}

            if (!sss.PayedMonthCount(m.MemberID) && !(_loanSubjectService.GetMemberValue(m.MemberID) > 0))
            {
                return Redirect("/UserInsurance/Index");
            }

            //判断是否计算过身价
            if (_loanSubjectService.GetMemberValue(m.MemberID) == 0)//没有进行过身价计算
            {
                return Redirect("/LoanCalculator/Calculator");
            }

            //还款期限
            ViewData["LoanTerm"] = new SelectList(CommonHelper.SelectListType(typeof(LoanTermEnum), ""), "Value", "Text");
            //还款方式
            ViewData["LoanMethod"] = new SelectList(CommonHelper.SelectListType(typeof(LoanMethodEnum), ""), "Value", "Text");

            //MemberLoanAuditViewModel model = new MemberLoanAuditViewModel();
            //model.AppayLoan = _loanMemberService.GetMemberLoanDetail(m.MemberID);

            HttpClient client = new HttpClient();
            var req = await client.GetAsync(url + "/Loan/GetApplyloan?MemberID=" + m.MemberID);

            AppayLoan appayLoan = (await req.Content.ReadAsAsync<JsonResult<AppayLoan>>()).Data;

            return View(appayLoan);

        }

        /// <summary>
        /// 实现借款
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Loan(MemberLoanAuditParameter parameter)
        {
            if (parameter.ApplyAmount <= 0)
            {
                TempData["Message"] = "申请金额必须大于0";
                return RedirectToAction("Loan");
            }
            parameter.MemberID = CommonHelper.CurrentUser.MemberID;
            HttpClient client = new HttpClient();
            var req = await client.PostAsJsonAsync(url + "/Loan/SubmitLoanApply", parameter);

            JsonResult<dynamic> result = (await req.Content.ReadAsAsync<JsonResult<dynamic>>());
            if (!result.status)
            {
                TempData["Message"] = result.Message;
                return RedirectToAction("Loan");
            }
            else
                return Redirect("/UserLoan/Index");

        }
    }
}