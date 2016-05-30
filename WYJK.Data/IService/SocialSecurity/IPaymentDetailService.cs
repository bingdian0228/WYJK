using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Entity;

namespace WYJK.Data.IService
{
    public interface IPaymentDetailService
    {
        /// <summary>
        /// 添加缴费明细
        /// </summary>
        /// <param name="list"></param>
        void AddPaymentDetail(List<PaymentDetail> list);
    }
}
