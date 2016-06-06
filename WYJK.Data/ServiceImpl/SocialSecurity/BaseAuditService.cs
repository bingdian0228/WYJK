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
    /// <summary>
    /// 基数审核服务类
    /// </summary>
    public class BaseAuditService : IBaseAuditService
    {
        /// <summary>
        /// 获取基数审核列表
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        public PagedResult<BaseAuditList> GetBaseAuditList(BaseAuditParameter parameter)
        {
            StringBuilder strb = new StringBuilder("where 1 = 1");
            strb.Append($" and Type = {parameter.Type}");

            if (!string.IsNullOrEmpty(parameter.MemberID))
            {
                strb.Append($" and m.MemberID = {parameter.MemberID} ");
            }

            strb.Append($" and (ssp.SocialSecurityPeopleName like '%{parameter.SocialSecurityPeopleName}%')");

            if (!string.IsNullOrEmpty(parameter.Status))
            {
                strb.Append($" and  t.Status = {parameter.Status}");
            }


            string innersqlstr = $@" select ssp.SocialSecurityPeopleName SocialSecurityPeopleName,m.UserType UserType,m.MemberName MemberName,m.EnterpriseName,m.BusinessName,  t.* 
 from BaseAudit t
left join SocialSecurity ss on t.SocialSecurityPeopleID = ss.SocialSecurityPeopleID
left join AccumulationFund af on t.SocialSecurityPeopleID = af.SocialSecurityPeopleID
left join SocialSecurityPeople ssp on t.SocialSecurityPeopleID = ssp.SocialSecurityPeopleID
left join Members m on ssp.MemberID = m.MemberID
 {strb.ToString()}";
            string sqlstr = $"select * from (select ROW_NUMBER() OVER(ORDER BY t.ApplyDate desc )AS Row,t.* from ({innersqlstr}) t ) tt WHERE tt.Row BETWEEN @StartIndex AND @EndIndex";

            List<BaseAuditList> modelList = DbHelper.Query<BaseAuditList>(sqlstr, new
            {
                StartIndex = parameter.SkipCount,
                EndIndex = parameter.TakeCount
            });

            int totalCount = DbHelper.QuerySingle<int>($"select count(0) from  ({innersqlstr}) t ");

            return new PagedResult<BaseAuditList>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = totalCount,
                Items = modelList
            };
        }

        /// <summary>
        /// 批量审核
        /// </summary>
        /// <param name="idsStr"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool BatchAudit(int[] IDs, string Status, int Type)
        {
            string sqlstr = string.Empty;
            foreach (var id in IDs)
            {
                sqlstr += $"update BaseAudit set AuditDate=getdate(), Status={Status} where ID={id}";
            }

            if (Status == Convert.ToString((int)BaseAuditEnum.Pass))
            {
                sqlstr += "declare @SocialSecurityPeopleID int,@BaseAdjusted decimal(18, 2)";
                foreach (var id in IDs)
                {
                    if (Status == Convert.ToString((int)BaseAuditEnum.Pass))
                    {
                        sqlstr += $"select @SocialSecurityPeopleID=SocialSecurityPeopleID,@BaseAdjusted=BaseAdjusted from BaseAudit where ID={id}";
                        if (Type == 0)
                        {
                            sqlstr += $"update SocialSecurity set SocialSecurityBase=@BaseAdjusted where SocialSecurityPeopleID=@SocialSecurityPeopleID;";
                        }
                        else if (Type == 1)
                        {
                            sqlstr += $"update AccumulationFund set AccumulationFundBase=@BaseAdjusted where SocialSecurityPeopleID=@SocialSecurityPeopleID;";
                        }
                    }
                }
            }

            int result = DbHelper.ExecuteSqlCommand(sqlstr, null);

            return result > 0;
        }
    }
}
