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
    /// 参数设置服务类
    /// </summary>
    public class ParameterSettingService : IParameterSettingService
    {


        public CostParameterSetting GetCostParameter(int status)
        {
            string sqlstr = $"select * from CostParameterSetting where Status={status}";
            CostParameterSetting model = DbHelper.QuerySingle<CostParameterSetting>(sqlstr);
            return model;
        }

        /// <summary>
        /// 添加费用参数
        /// </summary>
        /// <param name="costParameterSetting"></param>
        /// <returns></returns>
        public bool AddCostParameter(CostParameterSetting costParameterSetting)
        {
            string sqlstr = $"insert into CostParameterSetting(BaseServiceCharge,BacklogCost,FreezingAmount,PayBeforeServiceCost,RenewServiceCost,Status) values('{costParameterSetting.BaseServiceCharge}',{costParameterSetting.BacklogCost},{costParameterSetting.FreezingAmount},{costParameterSetting.PayBeforeServiceCost},'{costParameterSetting.RenewServiceCost}',{costParameterSetting.Status})";
            int result = DbHelper.ExecuteSqlCommand(sqlstr, null);
            return result > 0;
        }

        /// <summary>
        /// 修改费用参数
        /// </summary>
        /// <param name="costParameterSetting"></param>
        /// <returns></returns>
        public bool UpdateCostParameter(CostParameterSetting costParameterSetting)
        {
            string sqlstr = $"update CostParameterSetting set BaseServiceCharge='{costParameterSetting.BaseServiceCharge}', BacklogCost ={costParameterSetting.BacklogCost},FreezingAmount={costParameterSetting.FreezingAmount},PayBeforeServiceCost={costParameterSetting.PayBeforeServiceCost},RenewServiceCost='{costParameterSetting.RenewServiceCost}' where Status={costParameterSetting.Status} ";
            int result = DbHelper.ExecuteSqlCommand(sqlstr, null);
            return result > 0;
        }
    }
}
