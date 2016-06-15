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
    /// 日志记录实现类
    /// </summary>
    public class LogService : ILogService
    {
        public PagedResult<Log> GetLogList(PagedParameter parameter)
        {
            string inner_sql_str = @"select Log.* from Log";

            string sqlstr = $@"select * from 
                            (select ROW_NUMBER() OVER(ORDER BY t.Dt desc )AS Row,t.* from ({inner_sql_str}) t) tt 
                            WHERE tt.Row BETWEEN @StartIndex AND @EndIndex";


            List<Log> modelList = DbHelper.Query<Log>(sqlstr, new
            {
                StartIndex = parameter.SkipCount,
                EndIndex = parameter.TakeCount
            });

            int totalCount = DbHelper.QuerySingle<int>($"SELECT COUNT(0) AS TotalCount FROM ({inner_sql_str}) t");

            return new PagedResult<Log>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = totalCount,
                Items = modelList
            };


        }

        /// <summary>
        /// 写入日志信息
        /// </summary>
        /// <param name="log"></param>
        public static void WriteLogInfo(Log log)
        {
            string sqlstr = $"insert into Log(UserName,MemberID,Contents) values('{log.UserName}','{log.MemberID}''{log.Contents}')";

            DbHelper.ExecuteSqlCommand(sqlstr, null);
        }
    }
}
