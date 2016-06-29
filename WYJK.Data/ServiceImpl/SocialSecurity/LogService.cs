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
        public PagedResult<Log> GetLogList(LogParameter parameter)
        {
            //Members.MemberName like '%{parameter.MemberName}%' or SocialSecurityPeople.SocialSecurityPeopleName like '%{parameter.SocialSecurityPeopleName}%'
            StringBuilder stringBuilder = new StringBuilder(" where 1=1");

            //if (!string.IsNullOrEmpty(parameter.MemberName)&& !string.IsNullOrEmpty(parameter.SocialSecurityPeopleName)) {
            //    stringBuilder.Append($" and (Members.MemberName like '%{parameter.MemberName}%'  or SocialSecurityPeople.SocialSecurityPeopleName like '%{parameter.SocialSecurityPeopleName}%')");
            //}

            //if (!string.IsNullOrEmpty(parameter.MemberName) && string.IsNullOrEmpty(parameter.SocialSecurityPeopleName))
            //{
            //    stringBuilder.Append($" and Members.MemberName like '%{parameter.MemberName}%' ");
            //}

            //if (string.IsNullOrEmpty(parameter.MemberName) && !string.IsNullOrEmpty(parameter.SocialSecurityPeopleName))
            //{
            //    stringBuilder.Append($" and SocialSecurityPeople.SocialSecurityPeopleName like '%{parameter.SocialSecurityPeopleName}%'");
            //}

            if (string.IsNullOrEmpty(parameter.MemberName))
            {
                stringBuilder.Append($" and Members.MemberName is null");
            }
            else {
                stringBuilder.Append($" and Members.MemberName like '%{parameter.MemberName}%'");
            }

            if (string.IsNullOrEmpty(parameter.SocialSecurityPeopleName))
            {
                stringBuilder.Append($" and SocialSecurityPeople.SocialSecurityPeopleName is null");
            }
            else {
                stringBuilder.Append($" and SocialSecurityPeople.SocialSecurityPeopleName like '%{parameter.SocialSecurityPeopleName}%'");
            }

            string inner_sql_str = $@"select Log.*,Members.UserType,Members.MemberName,Members.EnterpriseName,Members.BusinessName,SocialSecurityPeople.SocialSecurityPeopleName from Log left join Members on Members.MemberID=Log.MemberID left join SocialSecurityPeople on SocialSecurityPeople.SocialSecurityPeopleID=Log.SocialSecurityPeopleID {stringBuilder.ToString()} ";

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
            string sqlstr = $"insert into Log(UserName,MemberID,SocialSecurityPeopleID,Contents) values('{log.UserName}','{log.MemberID}','{log.SocialSecurityPeopleID}','{log.Contents}')";

            DbHelper.ExecuteSqlCommand(sqlstr, null);
        }
    }
}
