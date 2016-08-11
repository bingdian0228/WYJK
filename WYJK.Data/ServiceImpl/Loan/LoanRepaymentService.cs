using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Data.IService;
using WYJK.Entity;

namespace WYJK.Data.ServiceImpl
{
    public class LoanRepaymentService : ILoanRepaymentService
    {
        /// <summary>
        ///  获取还款详情
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public List<MemberLoanRepayment> GetMemberLoanRepaymentList(int ID)
        {
            List<MemberLoanRepayment> list
                 = DbHelper.Query<MemberLoanRepayment>($@"select * from MemberLoanRepayment 
        left join MemberLoanRepaymentDetail on MemberLoanRepayment.id=MemberLoanRepaymentDetail.HuanID  where JieID={ID} and Status=2");
            return list;
        }

        /// <summary>
        /// 根据memberID获取还款列表
        /// </summary>
        /// <param name="memberID"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>

        public async Task<PagedResult<MemberLoanRepayment>> GetRepaymentList(int memberID, PagedParameter parameter)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.AppendFormat("{0};{1}", $@"select * from (select ROW_NUMBER() OVER(ORDER BY t.RepaymentDt desc )AS Row,t.* from
                              (select MemberLoanRepayment.* from MemberLoanRepayment left join MemberLoanAudit on MemberLoanAudit.ID = MemberLoanRepayment.JieID  where MemberLoanAudit.MemberID ={ memberID}) t) tt
where tt.Row BETWEEN @StartIndex AND @EndIndex", $"select count(*) from MemberLoanRepayment left join MemberLoanAudit on MemberLoanAudit.ID = MemberLoanRepayment.JieID  where MemberLoanAudit.MemberID ={ memberID}");

            DbParameter[] parameters = new DbParameter[] {
                    new SqlParameter("@StartIndex", parameter.SkipCount),
                    new SqlParameter("@EndIndex", parameter.TakeCount)
            };

            var tuple = await DbHelper.QueryMultipleAsync<MemberLoanRepayment, int>(sbSql.ToString(), parameters);
            return new PagedResult<MemberLoanRepayment>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                Items = tuple.Item1,
                TotalItemCount = tuple.Item2?.FirstOrDefault() ?? 0,
            };
        }
    }
}
