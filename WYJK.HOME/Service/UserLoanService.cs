using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WYJK.Data;
using WYJK.Entity;

namespace WYJK.HOME.Service
{
    public class UserLoanService
    {
        /// <summary>
        /// 获取用户借款信息
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public List<MemberLoanAudit> GetUserLoans(int memberId,string whereStr="")
        {
            List<MemberLoanAudit> list = new List<MemberLoanAudit>();

            string sql = $@"select 
                            ID,
                            ApplyAmount,
	                        LoanTerm,
	                        LoanMethod,
	                        Status,
	                        ApplyDate,
	                        case when AuditDate is null then 0 else AuditDate end AuditDate 
                            From MemberLoanAudit where memberId= {memberId} {whereStr}";

            return DbHelper.Query<MemberLoanAudit>(sql);
        }

        /// <summary>
        /// 获取借款详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MemberLoanAudit GetLoadAuditDetail(int id)
        {
            string sql = $@"select 
                            ID,
                            ApplyAmount,
	                        LoanTerm,
	                        LoanMethod,
	                        Status 
                           From MemberLoanAudit where ID = {id}";

            return DbHelper.QuerySingle<MemberLoanAudit>(sql);
        }

    }
}