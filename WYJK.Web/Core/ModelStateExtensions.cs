using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;
using WYJK.Entity;

namespace WYJK.Web
{
    /// <summary>
    /// ModelState 的扩展
    /// </summary>
    public static class ModelStateExtensions
    {
        /// <summary>
        ///  获取第一个错误信息
        /// </summary>
        /// <param name="modelState"></param>
        /// <returns></returns>
        public static string FirstModelStateError(this ModelStateDictionary modelState)
        {
            return modelState[modelState.Keys.ToList()[0]].Errors[0].ErrorMessage;
        }
    }
}