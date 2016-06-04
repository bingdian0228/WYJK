using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Entity;

namespace WYJK.Data.IService
{
    /// <summary>
    /// 签约企业接口类
    /// </summary>
    public interface IEnterpriseService
    {
        /// <summary>
        /// 根据企业ID获取签约企业实体(Admin)
        /// </summary>
        /// <param name="EnterpriseID"></param>
        /// <returns></returns>
        EnterpriseSocialSecurity GetEnterpriseSocialSecurity(int EnterpriseID);

        /// <summary>
        /// 获取参保企业列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        PagedResult<EnterpriseSocialSecurity> GetEnterpriseList(EnterpriseSocialSecurityParameter parameter);

        /// <summary>
        /// 城市社平管理列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        PagedResult<EnterpriseCity> GetEnterpriseCityList(PagedParameter parameter);

        /// <summary>
        /// 添加企业
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool AddEnterprise(EnterpriseSocialSecurity model);

        /// <summary>
        /// 批量删除企业
        /// </summary>
        /// <param name=""></param>
        /// <param name="EnterpriseIDs"></param>
        /// <returns></returns>
        bool BatchDeleteEnterprise(int[] EnterpriseIDs);

        /// <summary>
        /// 是否存在该企业
        /// </summary>
        /// <param name="EnterpriseName"></param>
        /// <param name="EnterpriseID"></param>
        /// <returns></returns>
        bool IsExistsEnterprise(string EnterpriseName, int EnterpriseID = 0);

        /// <summary>
        /// 更新企业
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool UpdateEnterprise(EnterpriseSocialSecurity model);

        /// <summary>
        /// 根据地址更新企业默认值
        /// </summary>
        /// <param name="EnterpriseArea"></param>
        /// <param name="HouseholdProperty"></param>
        /// <returns></returns>
        void UpdateEnterpriseDefault(string EnterpriseArea,int EnterpriseID);

        /// <summary>
        /// 获取企业名称列表
        /// </summary>
        /// <param name="EnterpriseIDs"></param>
        /// <returns></returns>
        string GetEnterpriseNames(int[] EnterpriseIDs);


        /// <summary>
        /// 获取企业缴费明细列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        PagedResult<PaymentDetail> GetPaymentDetailsList(PaymentDetailsParameter parameter);
    }
}
