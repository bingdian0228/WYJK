using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WYJK.Entity;

namespace WYJK.HOME.Models
{
    public class OrderViewModel:Order
    {
        public string OrderID { get; set; }


        public decimal TotalMoney { get; set; }



    }
}