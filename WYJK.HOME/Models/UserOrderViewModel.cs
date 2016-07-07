using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WYJK.Entity;

namespace WYJK.HOME.Models
{
    public class UserOrderViewModel: OrderListForMobile
    {
        public DateTime GenerateDate { get; set; }

        public int Status { get; set; }

    }
}