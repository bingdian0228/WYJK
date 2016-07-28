using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Data.IService;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;

namespace WYJK.Data.ServiceImpl
{
    public class LoanAuditService : ILoanAuditService
    {

        /// <summary>
        /// 获取借款审核列表
        /// </summary>
        /// <returns></returns>
        public PagedResult<MemberLoanAuditList> GetLoanAuditList(MemberLoanAuditListParameter parameter)
        {
            StringBuilder builder = new StringBuilder(" where 1 = 1");
            if (!string.IsNullOrEmpty(parameter.Status))
            {
                builder.Append($" and Status = {parameter.Status}");
            }

            builder.Append($" and Members.MemberName like '%{parameter.MemberName}%'");

            string innersqlstr = $@"select MemberLoanAudit.ID,Members.MemberID,Members.MemberName,Members.UserType,members.MemberPhone,members.EnterpriseName,members.BusinessName,
MemberLoan.TotalAmount,memberloan.AlreadyUsedAmount,memberloan.AvailableAmount,
MemberLoanAudit.ApplyAmount,MemberLoanAudit.Status,MemberLoanAudit.ApplyDate,MemberLoanAudit.AuditDate,MemberLoanAudit.AlreadyLoanDate
from MemberLoanAudit
left join MemberLoan on MemberLoanAudit.MemberID = MemberLoan.MemberID
left join Members on  MemberLoanAudit.MemberID = Members.MemberID"
+ builder.ToString();

            string sqlstr = "select * from (select ROW_NUMBER() OVER(ORDER BY t.ApplyDate )AS Row,t.* from"
                             + $" ({innersqlstr}) t) tt"
                             + " where tt.Row BETWEEN @StartIndex AND @EndIndex";

            List<MemberLoanAuditList> list = DbHelper.Query<MemberLoanAuditList>(sqlstr, new
            {
                StartIndex = parameter.SkipCount,
                EndIndex = parameter.TakeCount
            });
            int totalCount = DbHelper.QuerySingle<int>($"select count(0) from ({innersqlstr}) t");

            return new PagedResult<MemberLoanAuditList>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = totalCount,
                Items = list
            };
        }


        /// <summary>
        /// 用户借款审核
        /// </summary>
        /// <param name="MembersStr"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        public bool MemberLoanAudit(int ID, string Status)
        {
            string sqlstr = string.Empty;
            if (Status == Convert.ToString((int)LoanAuditEnum.NoPass) || Status == Convert.ToString((int)LoanAuditEnum.Pass))
            {
                sqlstr += $"update MemberLoanAudit set Status={Status} ,AuditDate=getdate() where ID ={ID};";
            }

            if (Status == Convert.ToString((int)LoanAuditEnum.AlreadyLoan))
            {
                string sqlstr1 = $"select ApplyAmount from MemberLoanAudit where ID={ID}";
                sqlstr += $"update MemberLoan set AlreadyUsedAmount+=({sqlstr1}),AvailableAmount-=({sqlstr1}) where MemberID=(select MemberID from MemberLoanAudit where ID={ID});";
                sqlstr += $"update MemberLoanAudit set Status={Status} ,AlreadyLoanDate=getdate(),LoanBalance=ApplyAmount where ID ={ID};";
            }

            int result = DbHelper.ExecuteSqlCommand(sqlstr, null);
            return result > 0;
        }

        /// <summary>
        /// 查找未审核列表
        /// </summary>
        /// <param name="IDsStr"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        public List<MemberLoanAudit> GetNoAuditedList(string IDsStr, string Status)
        {
            string sqlstr = $"select * from MemberLoanAudit where ID in({IDsStr}) and Status = {Status}";
            List<MemberLoanAudit> list = DbHelper.Query<MemberLoanAudit>(sqlstr);
            return list;
        }
    }
}
