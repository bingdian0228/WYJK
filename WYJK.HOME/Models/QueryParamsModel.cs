using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WYJK.Entity;

namespace WYJK.HOME.Models
{
    public class LoanQueryParamsModel: PagedParameter
    {
        public string Status { get; set; }
    }
}