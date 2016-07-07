using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WYJK.Entity;
using WYJK.HOME.Models;

namespace WYJK.HOME.Service
{
    public class PayHelper
    {
        //private static string CoNo = "019387";
        //private static string BranchID = "0532";
        private static string MiYao = "Superman19810928";

        //商户号
        public static string CoNo
        {
            get
            {
                return "019387";
            }
        }

        //商户开户分行号
        public static string BranchID
        {
            get
            {
                return "0532";
            }
        }

    }
}