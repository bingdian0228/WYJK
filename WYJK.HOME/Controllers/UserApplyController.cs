using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Data.IServices;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.Captcha;
using WYJK.Framework.EnumHelper;
using WYJK.HOME.Models;

namespace WYJK.HOME.Controllers
{
    public class UserApplyController : BaseFilterController
    {
        public ActionResult Index(InsuranceQueryParamModel parameter)
        { 

            return View();
        }

           
    }
}