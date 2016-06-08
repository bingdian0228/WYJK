using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Data.IService;
using WYJK.Entity;

namespace WYJK.Data.ServiceImpl
{
    /// <summary>
    /// 客服实现
    /// </summary>
    public class CustomerService : ICustomerService
    {
        /// <summary>
        /// 获取客户服务列表
        /// </summary>
        /// <returns></returns>
        public PagedResult<CustomerServiceViewModel> GetCustomerServiceList(CustomerServiceParameter parameter)
        {
            StringBuilder builder = new StringBuilder(" where 1 = 1");
            if (!string.IsNullOrEmpty(parameter.UserType))
            {
                builder.Append($" and Members.UserType = {parameter.UserType}");
            }

            if (!string.IsNullOrEmpty(parameter.MemberID))
            {
                builder.Append($" and Members.MemberID = {parameter.MemberID}");
            }

            if (!string.IsNullOrEmpty(parameter.SocialSecurityPeopleName))
            {
                builder.Append($" and SocialSecurityPeople.SocialSecurityPeopleName like '%{parameter.SocialSecurityPeopleName}%'");
            }

            if (!string.IsNullOrEmpty(parameter.IdentityCard))
            {
                builder.Append($" and SocialSecurityPeople.IdentityCard like '%{parameter.IdentityCard}%'");
            }
            //builder.Append(" order by SocialSecurityPeople.CreateDt desc");

            string sqlCs = $@"select members.MemberID,members.UserType,Members.MemberName,Members.MemberPhone,members.Account,members.EnterpriseName,members.BusinessName,SocialSecurityPeople.CreateDt, SocialSecurityPeople.SocialSecurityPeopleName,SocialSecurityPeople.SocialSecurityPeopleID ,SocialSecurityPeople.Status CustomerServiceStatus, SocialSecurityPeople.IdentityCard,SocialSecurity.Status SSstatus, socialsecurity.SocialSecurityException,AccumulationFund.Status AFStatus, AccumulationFund.AccumulationFundException,[order].OrderCode,[order].Status OrderStatus,
                              ISNULL((select SUM(SocialSecurity.SocialSecurityBase*socialsecurity.PayProportion/100) from SocialSecurityPeople
 left join SocialSecurity on SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID
 where MemberID = Members.MemberID and SocialSecurity.Status = 4 ),0) +
 ISNULL((select  SUM(AccumulationFund.AccumulationFundBase*AccumulationFund.PayProportion/100)  from SocialSecurityPeople
 left join AccumulationFund on SocialSecurityPeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID
 where MemberID = Members.MemberID and AccumulationFund.Status = 4),0) ArrearAmount 
                          from Members
                           left join SocialSecurityPeople on Members.MemberID = SocialSecurityPeople.MemberID
                           left join SocialSecurity on SocialSecurityPeople.SocialSecurityPeopleID = socialsecurity.SocialSecurityPeopleID
                           left join AccumulationFund on socialsecuritypeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID
               left join OrderDetails on socialsecuritypeople.SocialSecurityPeopleID = OrderDetails.SocialSecurityPeopleID
               left join [order] on[order].OrderCode = OrderDetails.OrderCode"
            + builder.ToString();

            string sqlStr = $"select * from (select ROW_NUMBER() OVER(ORDER BY Cs.MemberID )AS Row,Cs.* from ({sqlCs}) Cs ) ss WHERE ss.Row BETWEEN @StartIndex AND @EndIndex  order by ss.CreateDt desc";

            List<CustomerServiceViewModel> modelList = DbHelper.Query<CustomerServiceViewModel>(sqlStr, new
            {
                StartIndex = parameter.SkipCount,
                EndIndex = parameter.TakeCount
            });

            int totalCount = DbHelper.QuerySingle<int>($"select count(0) as TotalCount from ({sqlCs}) Cs");

            return new PagedResult<CustomerServiceViewModel>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = totalCount,
                Items = modelList
            };
        }

        /// <summary>
        /// 修改客户服务状态变成已通过
        /// </summary>
        /// <param name="SocialSecurityPeopleIDs"></param>
        /// <returns></returns>
        public bool ModifyCustomerServiceStatus(int[] SocialSecurityPeopleIDs, int status)
        {
            string SocialSecurityPeopleIDStr = string.Join("','", SocialSecurityPeopleIDs);
            string sqlstr = $"update SocialSecurityPeople set Status ={status} where SocialSecurityPeopleID in ('{SocialSecurityPeopleIDStr}')";
            int result = DbHelper.ExecuteSqlCommand(sqlstr, null);
            return result > 0;
        }
    }
}
