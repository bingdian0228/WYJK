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

        public decimal Total { get; set; }

        public decimal AllTotal { get; set; }

        public decimal Bucha { get; set; }

        public decimal ServiceCost { get; set; }

        public int SSPayMonthCount { get; set; }
        public DateTime PayTime { get; set; }
    }
}