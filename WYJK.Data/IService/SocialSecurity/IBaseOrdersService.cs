using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Entity;

namespace WYJK.Data.IService
{
    public interface IBaseOrdersService
    {
        Task<PagedResult<BaseOrders>> GetBaseOrderList(BaseOrdersParameter parameter);
    }
}
