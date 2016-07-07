using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Entity;

namespace WYJK.HOME.Service
{
    public class RegionService
    {
        /// <summary>
        /// 获取所有省份
        /// </summary>
        /// <returns></returns>
        public List<Region> GetProvince()
        {
            List<Region> list = new List<Region>();

            string sql = @"select 
	                        RegionCode,
	                        RegionName 
                        From Region
	                        where RegionLevel = 0";

            return DbHelper.Query<Region>(sql);
        }

        public List<Region> GetCitys(int code)
        {
            List<Region> list = new List<Region>();

            string sql = $@"select 
	                        RegionId,
	                        RegionName
                        From Region
	                        where ParentCode = {code}";

            return DbHelper.Query<Region>(sql);
        }

    }
}