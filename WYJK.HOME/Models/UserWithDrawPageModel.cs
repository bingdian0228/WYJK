using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WYJK.Entity;

namespace WYJK.HOME.Models
{
    public class UserWithDrawPageModel: PagedParameter
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }

    }
}