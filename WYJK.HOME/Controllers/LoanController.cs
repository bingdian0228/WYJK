using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// 借款
        /// </summary>
        /// <returns></returns>
        public ActionResult Loan()
        {
            Members m = CommonHelper.CurrentUser;

            //判断是否已缴费//缴费不满三个月
            if (!sss.ExistSocialPeople(m.MemberID))
            {
                //添加社保人
                return Redirect("/UserInsurance/Add1");
            }

            if (!sss.PayedMonthCount(m.MemberID))
            {
                return Redirect("/UserInsurance/Index");
            }

            //判断是否计算过身价
            if (_loanSubjectService.GetMemberValue(m.MemberID) == 0)//没有进行过身价计算
            {
                return Redirect("/LoanCalculator/Calculator");
            }

            //还款期限
            ViewData["LoanTerm"] = new SelectList(CommonHelper.SelectListType(typeof(LoanTermEnum), "请选择还款期限"), "Value", "Text");
            //还款方式
            ViewData["LoanMethod"] =new SelectList(CommonHelper.SelectListType(typeof(LoanMethodEnum), "请选择还款方式"), "Value", "Text");

            MemberLoanAuditViewModel model = new MemberLoanAuditViewModel();
            model.AppayLoan = _loanMemberService.GetMemberLoanDetail(m.MemberID);

            return View(model);

        }

        /// <summary>
        /// 实现借款
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Loan(MemberLoanAuditViewModel model)
        {
            MemberLoanAuditParameter paramModel = new MemberLoanAuditParameter();

            paramModel.ApplyAmount = model.ApplyAmount;
            paramModel.LoanMethod = model.LoanMethod;
            paramModel.LoanTerm = model.LoanTerm;
            paramModel.MemberID = CommonHelper.CurrentUser.MemberID;

            bool result = _loanMemberService.SubmitLoanApply(paramModel);

            if (result)
            {
                return Redirect("/UserLoan/Index");
            }

            return RedirectToAction("Loan");

            
        }
    }
}