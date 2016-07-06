using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WYJK.Entity;

namespace WYJK.HOME.Models
{
    public class SocialSecurityPeopleViewModel: SocialSecurityPeople
    {
        public string InsuranceArea { get; set; }

        public DateTime PayTime { get; set; }
        //社保基数
        public decimal SocialSecurityBase { get; set; }
        //公积金基数
        public decimal AccumulationFundBase { get; set; }

        public decimal CurrentBase { get; set; }

        public decimal BaseAdjusted { get; set; }

        public SocialSecurityCalculation Calculation { get; set; }

        //支付方式
        public string PaymentMethod { get; set; }




    }
}