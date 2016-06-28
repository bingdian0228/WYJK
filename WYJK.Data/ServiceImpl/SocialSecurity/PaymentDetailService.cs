using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Data.IService;
using WYJK.Entity;

namespace WYJK.Data.ServiceImpl
{
    public class PaymentDetailService : IPaymentDetailService
    {
        public void AddPaymentDetail(List<PaymentDetail> list)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var paymentDetail in list)
            {
                builder.Append($@"insert into PaymentDetail(
                            PersonnelNumber
                            , IdentityCard
                            , TrueName
                            , PayTime
                            , BusinessTime
                            , PaymentType
                            , SocialInsuranceBase
                            , PersonalExpenses
                            , CompanyExpenses
                            , PaymentMark
                            , CompanyNumber
                            , CompanyName
                            , SettlementMethod
                            , SocialSecurityType)
                    values(
                            '{paymentDetail.PersonnelNumber}'
                            ,'{paymentDetail.IdentityCard}'
                            ,'{paymentDetail.TrueName}'
                            ,'{paymentDetail.PayTime}'
                            ,'{paymentDetail.BusinessTime}'
                            ,'{paymentDetail.PaymentType}'
                            ,'{paymentDetail.SocialInsuranceBase}'
                            ,'{paymentDetail.PersonalExpenses}'
                            ,'{paymentDetail.CompanyExpenses}'
                            ,'{paymentDetail.PaymentMark}'
                            ,'{paymentDetail.CompanyNumber}'
                            ,'{paymentDetail.CompanyName}'
                            ,'{paymentDetail.SettlementMethod}'
                            ,'{paymentDetail.SocialSecurityType}');");
            }

            DbHelper.ExecuteSqlCommand(builder.ToString(), null);
        }
    }
}
