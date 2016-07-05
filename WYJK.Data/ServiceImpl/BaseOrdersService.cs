using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Data.IService;
using WYJK.Entity;

namespace WYJK.Data.ServiceImpl
{
    public class BaseOrdersService : IBaseOrdersService
    {
        public async Task<PagedResult<BaseOrders>> GetBaseOrderList(BaseOrdersParameter parameter)
        {
            string sqlWhere = $@" where m.MemberName like '%{parameter.SocialSecurityPeopleName}%' and ro.Status!=0 and (ro.Status = {parameter.Status} or {parameter.Status}=-1)";


            StringBuilder sbSql = new StringBuilder();
            sbSql.AppendFormat("{0};{1}", $@"SELECT * from( SELECT ROW_NUMBER() OVER(ORDER BY ro.PayTime desc) AS rowindex,ro.* ,m.MemberName 
        FROM BaseOrders ro
        LEFT JOIN Members m ON ro.MemberId = m.MemberID
      {sqlWhere}) a  WHERE RowIndex BETWEEN {parameter.SkipCount} AND {parameter.TakeCount}", $"select count(0) from RechargeOrders ro left join  dbo.Members m on ro.MemberId = m.MemberID   {sqlWhere}");

            var tuple = await DbHelper.QueryMultipleAsync<BaseOrders, int>(sbSql.ToString(), null);
            return new PagedResult<BaseOrders>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = tuple.Item2?.FirstOrDefault() ?? 0,
                Items = tuple.Item1
            };
        }
    }
}
