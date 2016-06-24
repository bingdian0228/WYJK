using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WYJK.Data.IService;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;

namespace WYJK.Web.Controllers.Mvc
{
    /// <summary>
    /// 参数设置
    /// </summary>
    [Authorize]
    public class ParameterSettingController : BaseController
    {
        IParameterSettingService _parameterSettingService = new ParameterSettingService();
        /// <summary>
        /// 参数设置
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCostParameter(int Status)
        {
            CostParameterSetting model = _parameterSettingService.GetCostParameter(Status);
            if (model == null)
                model = new CostParameterSetting();
            else {
                if (!string.IsNullOrEmpty(model.RenewServiceCost))
                {
                    string[] array = model.RenewServiceCost.Split(';');
                    model.StartTime = new int[array.Length];
                    model.EndTime = new int[array.Length];
                    model.ServiceCost = new decimal[array.Length];
                    for (int i = 0; i < array.Length; i++)
                    {
                        string[] array1 = array[i].Split(',');
                        model.StartTime[i] = Convert.ToInt32(array1[0]);
                        model.EndTime[i] = Convert.ToInt32(array1[1]);
                        model.ServiceCost[i] = Convert.ToDecimal(array1[2]);
                    }
                }
            }

            if (Status == (int)PayTypeEnum.SocialSecurity)
                return View("GetSocialSecurityCostParameter", model);
            else if (Status == (int)PayTypeEnum.AccumulationFund)
                return View("GetAccumulationFundCostParameter", model);
            else
                return HttpNotFound();
        }

        /// <summary>
        /// 保存收费参数
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public ActionResult SaveCostParameter(CostParameterSetting parameter)
        {
            CostParameterSetting model = _parameterSettingService.GetCostParameter(parameter.Status);

            string RenewServiceCost = string.Empty;
            for (int i = 0; i < parameter.StartTime.Length; i++)
            {
                if (parameter.StartTime[i] != 0 && parameter.EndTime[i] != 0)
                {
                    RenewServiceCost += parameter.StartTime[i] + "," + parameter.EndTime[i] + "," + parameter.ServiceCost[i] + ";";

                }
            }

            parameter.RenewServiceCost = RenewServiceCost.Trim(';');

            bool flag = false;
            if (model == null)
            {
                //添加
                flag = _parameterSettingService.AddCostParameter(parameter);

                #region 记录日志
                if (flag == true)
                    LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = "修改了社保收费标准" });
                #endregion
            }
            else {
                //更新
                flag = _parameterSettingService.UpdateCostParameter(parameter);

                #region 记录日志
                if (flag == true)
                    LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = "修改了公积金收费标准" });
                #endregion
            }

            ViewBag.Message = flag ? "更新成功" : "更新失败";
            return GetCostParameter(parameter.Status);
        }
    }
}