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
                builder.Append($@"
                    if exists(select 1 from PaymentDetail where IdentityCard='{paymentDetail.IdentityCard}' and PayTime='{paymentDetail.PayTime}' and CompanyName='{paymentDetail.CompanyName}' and SocialSecurityType='{paymentDetail.SocialSecurityType}' and Year={DateTime.Now.Year})
                    begin
                        update PaymentDetail set  
                                PersonnelNumber='{paymentDetail.PersonnelNumber}',
                                TrueName='{paymentDetail.TrueName}',
                                PayTime='{paymentDetail.PayTime}',
                                BusinessTime='{paymentDetail.BusinessTime}',
                                PaymentType='{paymentDetail.PaymentType}',
                                SocialInsuranceBase='{paymentDetail.SocialInsuranceBase}',
                                PersonalExpenses='{paymentDetail.PersonalExpenses}', 
                                CompanyExpenses='{paymentDetail.CompanyExpenses}',
                                PaymentMark='{paymentDetail.PaymentMark}',
                                CompanyNumber='{paymentDetail.CompanyNumber}',
                                SettlementMethod='{paymentDetail.SettlementMethod}'
                            where IdentityCard='{paymentDetail.IdentityCard}' and PayTime='{paymentDetail.PayTime}' and CompanyName='{paymentDetail.CompanyName}' and SocialSecurityType='{paymentDetail.SocialSecurityType}' and Year={DateTime.Now.Year}
                    end
                    else
                    begin
                    insert into PaymentDetail(
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
                            , SocialSecurityType,Year)
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
                            ,'{paymentDetail.SocialSecurityType}',{DateTime.Now.Year});
                    end");
            }

            DbHelper.ExecuteSqlCommand(builder.ToString(), null);
        }
    }
}
