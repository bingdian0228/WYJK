using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Entity;

namespace WYJK.Data.IServices
{
    /// <summary>
    /// 用户
    /// </summary>
    public interface IDrawCashService
    {
        /// <summary>
        /// 获取提现列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        Task<PagedResult<DrawCashViewModel>> GetDrawCashListAsync(DrawCashParameter parameter);
    }
}
