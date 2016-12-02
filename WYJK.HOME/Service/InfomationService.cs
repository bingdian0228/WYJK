using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WYJK.Data;
using WYJK.Entity;

namespace WYJK.HOME.Service
{
    public class InfomationService
    {
        /// <summary>
        /// 根据条数获取新闻
        /// </summary>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<Information> InsuranceOfPageSize(int pageSize, string flag)
        {
            List<Information> list = new List<Information>();
            string type = string.Empty;
            switch (flag)
            {
                case "1":
                    type = "新闻";
                    break;
                case "2":
                    type = "资讯";
                    break;
            }

            string sql = $@"select top {pageSize} ID,Name,CreateTime from Information where Type = '{type}'  order by CreateTime desc";

            return DbHelper.Query<Information>(sql);
        }

    }
}