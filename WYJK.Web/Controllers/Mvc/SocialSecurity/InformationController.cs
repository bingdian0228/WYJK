using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WYJK.Data.IService;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;

namespace WYJK.Web.Controllers.Mvc
{
    /// <summary>
    /// 信息展示
    /// </summary>
    [Authorize]
    public class InformationController : Controller
    {
        IInformationService _informationService = new InformationService();

        /// <summary>
        /// 获取新闻通知
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> GetInformationList(InformationParameter parameter)
        {
            PagedResult<Information> informationList = await _informationService.GetNewNoticeList(parameter);
            return View(informationList);
        }


        public ActionResult InformationAdd(string type)
        {
            return View(new Information { Type = type });
        }

        /// <summary>
        /// 添加保存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> InformationAdd(Information model)
        {
            if (model.ImgUrls != null)
            {
                model.ImgUrl = string.Join(";", model.ImgUrls).Replace(ConfigurationManager.AppSettings["ServerUrl"], string.Empty);
                
            }
            model.CreateTime = DateTime.Now;

            bool flag = await _informationService.InformationAdd(model);
            ViewBag.ErrorMessage = flag ? "保存成功" : "保存失败";
            return InformationAdd(model.Type);
        }

        

        /// <summary>
        /// 新建信息编辑
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpGet]
        [ValidateInput(false)]
        public async Task<ActionResult> InformationEdit(int ID)
        {
            Information model = await _informationService.GetInfomationDetail(ID);
            if (!string.IsNullOrWhiteSpace(model.ImgUrl))
            {
                model.ImgUrl = ConfigurationManager.AppSettings["ServerUrl"] + model.ImgUrl.Replace(";", ";" + ConfigurationManager.AppSettings["ServerUrl"]);
                model.ImgUrls = model.ImgUrl.Split(';');
            }
            return View(model);
        }

        /// <summary>
        /// 保存编辑
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> InformationEdit(Information model)
        {
            if (model.ImgUrls != null)
            {
                model.ImgUrl = string.Join(";", model.ImgUrls).Replace(ConfigurationManager.AppSettings["ServerUrl"], string.Empty);
            }

            bool flag = await _informationService.ModifyInformation(model);
            ViewBag.ErrorMessage = flag ? "编辑成功" : "编辑失败";
            return await InformationEdit(model.ID);
        }

        /// <summary>
        /// 批量删除信息
        /// </summary>
        /// <param name="infoids"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult BatchDeleteInfos(int[] infoids)
        {
            string infoidsStr = string.Join(",", infoids);
            bool flag = _informationService.BatchDeleteInfos(infoidsStr);
            return Json(new { status = flag, message = flag ? "删除成功" : "删除失败" });
        }

    }
}