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
        public List<Information> InsuranceOfPageSize(int pageSize)
        {
            List<Information> list = new List<Information>();

            string sql = $@"select top {pageSize} ID,Name from Information";
            
            return DbHelper.Query<Information>(sql);
        }

    }
}