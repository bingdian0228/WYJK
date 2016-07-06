using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WYJK.Entity;

namespace WYJK.HOME.Models
{
    public class UserAccountPageModel:PagedParameter
    {
        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }


    }
}