using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Data.IService;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;

namespace WYJK.Data.ServiceImpl
{
    public class LoanMemberService : ILoanMemberService
    {
        /// <summary>
        /// 获取用户借款列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public PagedResult<MemberLoanList> GetMemberLoanList(MemberLoanParameter parameter)
        {
            string innersqlstr = $"select Members.MemberID,Members.UserType,Members.MemberName,Members.EnterpriseName,Members.BusinessName,members.MemberPhone,MemberLoan.TotalAmount,memberloan.AlreadyUsedAmount,memberloan.AvailableAmount from MemberLoan left join Members on members.MemberID= MemberLoan.MemberID where Members.MemberName like '%{parameter.MemberName}%'";

            string sqlstr = "select * from (select ROW_NUMBER() OVER(ORDER BY t.MemberID )AS Row,t.* from"
                             + $" ({innersqlstr}) t) tt"
                             + " where tt.Row BETWEEN @StartIndex AND @EndIndex";

            List<MemberLoanList> memberLoanList = DbHelper.Query<MemberLoanList>(sqlstr, new
            {
                StartIndex = parameter.SkipCount,
                EndIndex = parameter.TakeCount
            });
            int totalCount = DbHelper.QuerySingle<int>($"select count(0) from ({innersqlstr}) t");

            return new PagedResult<MemberLoanList>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = totalCount,
                Items = memberLoanList
            };
        }

        /// <summary>
        /// 获取用户借款详情
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public AppayLoan GetMemberLoanDetail(int MemberID)
        {
            string sqlstr = $"select AlreadyUsedAmount,AvailableAmount from MemberLoan where MemberID={MemberID}";
            AppayLoan appayLoan = DbHelper.QuerySingle<AppayLoan>(sqlstr);
            return appayLoan;
        }

        /// <summary>
        /// 提交借款申请
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool SubmitLoanApply(MemberLoanAuditParameter model)
        {
            string sqlstr = $@"insert into MemberLoanAudit(MemberID,ApplyAmount,LoanTerm,LoanMethod)
                                values({model.MemberID},{model.ApplyAmount},{model.LoanTerm},{model.LoanMethod})";
            int result = DbHelper.ExecuteSqlCommandScalar(sqlstr, new DbParameter[] { });
            return result > 0;
        }

        /// <summary>
        /// 根据用户ID获取正在审核额度之和
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public decimal GetTotalAuditAmountByMemberID(int MemberID)
        {
            string sqlstr = $"select ISNULL(sum(ApplyAmount),0) from MemberLoanAudit where MemberID={MemberID} and Status={(int)LoanAuditEnum.NoAudited}";
            decimal result = DbHelper.QuerySingle<decimal>(sqlstr);
            return result;
        }

        /// <summary>
        /// 修改用户借款
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool UpdateMemberLoan(MemberLoan model)
        {
            //string sqlstr = "update MemberLoan set ";
            return true;
        }
    }
}
