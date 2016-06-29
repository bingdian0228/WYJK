using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Entity;

namespace WYJK.Data.IService
{
    /// <summary>
    /// 信息接口
    /// </summary>
    public interface IInformationService
    {
        /// <summary>
        /// 新闻通知(Admin)
        /// </summary>
        /// <returns></returns>
        Task<PagedResult<Information>> GetInsuredIntroduceList(InformationParameter parameter);

        /// <summary>
        /// 信息添加(Admin)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> InformationAdd(Information model);

        /// <summary>
        /// 根据信息ID获取基本信息(Admin)
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        Task<Information> GetInfomationDetail(int ID);

        /// <summary>
        /// 更新信息(Admin)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> ModifyInformation(Information model);

        /// <summary>
        /// 批量删除信息(Admin)
        /// </summary>
        /// <param name="infoidsStr"></param>
        /// <returns></returns>
        bool BatchDeleteInfos(string infoidsStr);
    }
}
