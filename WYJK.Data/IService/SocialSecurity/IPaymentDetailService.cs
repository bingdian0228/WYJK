using System.Collections.Generic;
using WYJK.Entity;

namespace WYJK.Data.ServiceImpl
{
    public interface IPaymentDetailService
    {
        void AddPaymentDetail(List<PaymentDetail> list);

    }
}