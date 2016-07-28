using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Entity;

namespace WYJK.Data.IService
{
    public interface ILoanMemberService
    {
        /// <summary>
        /// 获取用户借款列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        PagedResult<MemberLoanList> GetMemberLoanList(MemberLoanParameter parameter);

        /// <summary>
        /// 获取用户借款详情
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        AppayLoan GetMemberLoanDetail(int MemberID);

        /// <summary>
        /// 提交借款申请
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool SubmitLoanApply(MemberLoanAuditParameter model);

        /// <summary>
        /// 获取借款审核列表
        /// </summary>
        /// <param name="memberID"></param>
        /// <returns></returns>
        Task<PagedResult<MemberLoanAudit>> GetMemberLoanAuditList(int memberID, PagedParameter parameter, string status = "");

        /// <summary>
        /// 获取借款详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        MemberLoanAudit GetMemberLoanAuditDetail(int id);
        /// <summary>
        /// 根据用户ID获取正在审核额度之和
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        decimal GetTotalAuditAmountByMemberID(int MemberID);

        /// <summary>
        /// 修改用户借款
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool UpdateMemberLoan(MemberLoan model);
    }
}
