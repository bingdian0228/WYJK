using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WYJK.Data.IService;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;

namespace WYJK.Web.Controllers.Mvc.Loan
{
    /// <summary>
    /// 借款用户
    /// </summary>
    [Authorize]
    public class LoanMemberController : Controller
    {
        private ILoanMemberService _LoanMemberService = new LoanMemberService();

        /// <summary>
        /// 获取借款用户列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetLoanMemberList(MemberLoanParameter parameter)
        {
            if (!string.IsNullOrEmpty(parameter.MemberName))
                parameter.MemberName = parameter.MemberName.Replace("'", "''");
            PagedResult<MemberLoanList> memberLoanList = _LoanMemberService.GetMemberLoanList(parameter);

            return View("/Views/Loan/LoanMember/GetLoanMemberList.cshtml", memberLoanList);
        }

    }
}