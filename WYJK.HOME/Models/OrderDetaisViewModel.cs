using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WYJK.Entity;

namespace WYJK.HOME.Models
{
    public class OrderDetaisViewModel: OrderDetails
    {
        public string HouseholdProperty { get; set; }

        public string InsuranceArea { get; set; }

        public decimal SocialSecurityBase { get; set; }

        public decimal AccumulationFundBase { get; set; }

    }
}