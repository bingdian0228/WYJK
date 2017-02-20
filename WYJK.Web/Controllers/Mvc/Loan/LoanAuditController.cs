using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Data.IService;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;

namespace WYJK.Web.Controllers.Mvc.Loan
{
    /// <summary>
    /// 借款审核
    /// </summary>
    public class LoanAuditController : Controller
    {
        private ILoanAuditService _loanAuditService = new LoanAuditService();

        /// <summary>
        /// 获取借款审核列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetLoanAuditList(MemberLoanAuditListParameter parameter)
        {
            if (!string.IsNullOrEmpty(parameter.MemberName))
                parameter.MemberName = parameter.MemberName.Replace("'", "''");
            PagedResult<MemberLoanAuditList> list = _loanAuditService.GetLoanAuditList(parameter);

            List<SelectListItem> selectList = new List<SelectListItem> { new SelectListItem() { Value = "", Text = "全部" } };
            selectList.AddRange(EnumExt.GetSelectList(typeof(LoanAuditEnum)));
            ViewData["StatusType"] = selectList;

            return View("~/Views/Loan/LoanAudit/GetLoanAuditList.cshtml", list);
        }

        /// <summary>
        /// 批量审核
        /// 1、修改审核状态和审核时间
        /// 2、修改用户借款表的已用额度和可用额度
        /// 3、修改用户账户额度(暂去)
        /// 4、记录流水(暂去)
        /// </summary>
        /// <param name="IDs"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        public ActionResult BatchAudit(int ID, string Status)
        {
            //string IDsStr = string.Join(",", IDs);
            //如果未审核，则进行下面的操作
            //List<MemberLoanAudit> NoAuditedList = _loanAuditService.GetNoAuditedList(IDsStr, Convert.ToString((int)LoanAuditEnum.NoAudited));
            //if (NoAuditedList != null && NoAuditedList.Count > 0)
            //{
            //    int[] IDs1 = new int[NoAuditedList.Count];

            //    for (int i = 0; i < NoAuditedList.Count; i++) {
            //        IDs1[i] = NoAuditedList[i].ID;
            //    }

            bool flag = _loanAuditService.MemberLoanAudit(ID, Status);
            if (flag == false)
                return Json(new { status = false, message = "审核失败" });

            //}
            //else {
            //    return Json(new { status = false, message = "此选项已经审核过" });
            //}

            return Json(new { status = true, message = "审核成功" });

        }

        /// <summary>
        /// 获取还款记录
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult GetMemberLoanRepaymentList(int ID, string Status = null)
        {

            StringBuilder builder = new StringBuilder(" and 1 = 1");
            if (!string.IsNullOrEmpty(Status))
            {
                builder.Append($" and Status = {Status}");
            }

            List<MemberLoanRepayment> memberLoanRepaymentList = DbHelper.Query<MemberLoanRepayment>($"select * from MemberLoanRepayment where JieID={ID} and Status<>0" + builder.ToString());

            memberLoanRepaymentList.ForEach(n =>
            {
                n.RepaymentType = EnumExt.GetEnumCustomDescription((RepaymentTypeEnum)(Convert.ToInt32(n.RepaymentType)));
            });

            List<SelectListItem> selectList = new List<SelectListItem> { new SelectListItem() { Value = "", Text = "全部" } };
            selectList.AddRange(EnumExt.GetSelectList(typeof(RepaymentAuditEnum)));
            ViewData["StatusType"] = selectList;

            return View("~/Views/Loan/LoanAudit/GetMemberLoanRepaymentList.cshtml", memberLoanRepaymentList);
        }

        /// <summary>
        /// 还款审核
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        public ActionResult BatchRepaymentAudit(int ID, string Status)
        {
            int result = DbHelper.ExecuteSqlCommand($"update MemberLoanRepayment set Status={Status},AuditDt=getdate() where ID={ID}", null);

            if (result > 0)
                return Json(new { status = true, message = "审核成功" });
            else
                return Json(new { status = false, message = "审核失败" });
        }

        /// <summary>
        /// 获取还款详情
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult GetMemberLoanRepaymentDetail(int ID)
        {
            MemberLoanRepayment memberLoanRepayment = DbHelper.QuerySingle<MemberLoanRepayment>($"select * from MemberLoanRepayment where ID={ID}");
            memberLoanRepayment.DetailList = DbHelper.Query<MemberLoanRepaymentDetail>($"select * from MemberLoanRepaymentDetail where HuanID={ID}");
            return View("~/Views/Loan/LoanAudit/GetMemberLoanRepaymentDetail.cshtml", memberLoanRepayment);
        }
    }
}