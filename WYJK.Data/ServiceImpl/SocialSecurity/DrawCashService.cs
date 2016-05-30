using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WYJK.Data.IServices;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;

namespace WYJK.Data.ServiceImpl
{
    public class DrawCashService : IDrawCashService
    {       
        /// <summary>
        /// 条件查询申请列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<PagedResult<DrawCashViewModel>> GetDrawCashListAsync(DrawCashParameter parameter)
        {
            string sqlWhere = $@" where m.MemberName like '%{parameter.SocialSecurityPeopleName}%' and (dc.applystatus = {parameter.Status} or {parameter.Status}=-1)";

          
            StringBuilder sbSql = new StringBuilder();
            sbSql.AppendFormat("{0};{1}", $@"SELECT * from( SELECT ROW_NUMBER() OVER(ORDER BY dc.applytime desc) AS rowindex,dc.* ,
        m.MemberName ,
        ISNULL(( SELECT SUM(SocialSecurity.SocialSecurityBase
                            * SocialSecurity.PayProportion / 100)
                 FROM   SocialSecurityPeople
                        LEFT JOIN SocialSecurity ON SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID
                 WHERE  MemberID = m.MemberID
                        AND SocialSecurity.Status = 4
               ), 0)
        + ISNULL(( SELECT   SUM(AccumulationFund.AccumulationFundBase
                                * AccumulationFund.PayProportion / 100)
                   FROM     SocialSecurityPeople
                            LEFT JOIN AccumulationFund ON SocialSecurityPeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID
                   WHERE    MemberID = m.MemberID
                            AND AccumulationFund.Status = 4
                 ), 0) ArrearAmount ,
        dc.leftAccount as Account ,
        m.BankCardNo ,
        m.BankAccount
FROM    dbo.DrawCash dc
        LEFT JOIN dbo.Members m ON dc.MemberId = m.MemberID
      {sqlWhere}) a  WHERE RowIndex BETWEEN {parameter.SkipCount} AND {parameter.TakeCount}", $"select count(0) from DrawCash dc left join  dbo.Members m ON dc.MemberId = m.MemberID   {sqlWhere}");
        
            var tuple = await DbHelper.QueryMultipleAsync<DrawCashViewModel, int>(sbSql.ToString(), null);
            return new PagedResult<DrawCashViewModel>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = tuple.Item2?.FirstOrDefault() ?? 0,
                Items = tuple.Item1
            };
        }
    }
}
