using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Entity;

namespace WYJK.Data.IService
{
   public interface  IInsuredIntroduceService
    {

        /// <summary>
        /// 参保介绍
        /// </summary>
        /// <returns></returns>
        Task<PagedResult<InsuredIntroduce>> GetInsuredIntroduceList(PagedParameter parameter);
        /// <summary>
        /// 参保列表
        /// </summary>
        /// <returns></returns>
        Task<List<InsuredIntroduce>> GetInsuredIntroduceList();
        /// <summary>
        /// 参保介绍添加
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> InsuredIntroduceAdd(InsuredIntroduce model);

        /// <summary>
        /// 获取参保介绍详情
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        Task<InsuredIntroduce> GetInsuredIntroduceDetail(int ID);

        /// <summary>
        ///更新参保介绍
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> ModifyInsuredIntroduce(InsuredIntroduce model);

        /// <summary>
        /// 批量删除参保介绍
        /// </summary>
        /// <param name="infoidsStr"></param>
        /// <returns></returns>
        bool BatchDeleteInfos(string infoidsStr);
    }
}
