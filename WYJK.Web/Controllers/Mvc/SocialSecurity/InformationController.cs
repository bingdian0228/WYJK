using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WYJK.Data;
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
        //IInformationService _informationService = new InformationService();

        ///// <summary>
        ///// 获取新闻通知
        ///// </summary>
        ///// <returns></returns>
        //public async Task<ActionResult> GetInsuredIntroduceList(InformationParameter parameter)
        //{
        //    PagedResult<Information> informationList = await _informationService.GetInformationList(parameter);
        //    return View(informationList);
        //}


        ///// <summary>
        ///// 添加参保介绍
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public ActionResult InsuredIntroduceAdd()
        //{
        //    return View(new Information());
        //}

        ///// <summary>
        ///// 保存参保介绍
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost]
        //[ValidateInput(false)]
        //public async Task<ActionResult> InsuredIntroduceAdd(Information model)
        //{
        //    if (!ModelState.IsValid) return View();

        //    string sql = $"insert into Information(Name,StrContent,Type) values('{model.Name}','{model.StrContent}',{model.Type})";
        //    int result = await DbHelper.ExecuteSqlCommandAsync(sql);

        //    ViewBag.ErrorMessage = result > 0 ? "保存成功" : "保存失败";
        //    return InformationAdd();
        //}


        ///// <summary>
        ///// 新建信息编辑
        ///// </summary>
        ///// <param name="ID"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<ActionResult> InsuredIntroduceEdit(int ID)
        //{
        //    Information model = await _informationService.GetInfomationDetail(ID);

        //    return View(model);
        //}

        ///// <summary>
        ///// 保存编辑
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<ActionResult> InsuredIntroduceEdit(Information model)
        //{
        //    string sql = $"update Information set Name={model.Name},StrContent={model.StrContent} where ID=@ID";

        //    int result = await DbHelper.ExecuteSqlCommandAsync(sql);

        //    ViewBag.ErrorMessage = result > 0 ? "编辑成功" : "编辑失败";

        //    return await InformationEdit(model.ID);
        //}



        ///// <summary>
        ///// 新建添加
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public ActionResult InformationAdd()
        //{
        //    return View(new Information());
        //}

        ///// <summary>
        ///// 添加保存
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost]
        //[ValidateInput(false)]
        //public async Task<ActionResult> InformationAdd(Information model)
        //{
        //    if (!ModelState.IsValid) return View();

        //    bool flag = await _informationService.InformationAdd(model);
        //    ViewBag.ErrorMessage = flag ? "保存成功" : "保存失败";
        //    return InformationAdd();
        //}






        ///// <summary>
        ///// 新建信息编辑
        ///// </summary>
        ///// <param name="ID"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<ActionResult> InformationEdit(int ID)
        //{
        //    Information model = await _informationService.GetInfomationDetail(ID);
        //    if (!string.IsNullOrWhiteSpace(model.ImgUrl))
        //    {
        //        model.ImgUrl = ConfigurationManager.AppSettings["ServerUrl"] + model.ImgUrl.Replace(";", ";" + ConfigurationManager.AppSettings["ServerUrl"]);
        //        model.ImgUrls = model.ImgUrl.Split(';');
        //    }

        //    return View(model);
        //}

        ///// <summary>
        ///// 保存编辑
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<ActionResult> InformationEdit(Information model)
        //{
        //    if (model.ImgUrls != null)
        //    {
        //        model.ImgUrl = string.Join(";", model.ImgUrls).Replace(ConfigurationManager.AppSettings["ServerUrl"], string.Empty);

        //    }

        //    bool flag = await _informationService.ModifyInformation(model);
        //    ViewBag.ErrorMessage = flag ? "编辑成功" : "编辑失败";
        //    return await InformationEdit(model.ID);
        //}

        ///// <summary>
        ///// 批量删除信息
        ///// </summary>
        ///// <param name="infoids"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public JsonResult BatchDeleteInfos(int[] infoids)
        //{
        //    string infoidsStr = string.Join(",", infoids);
        //    bool flag = _informationService.BatchDeleteInfos(infoidsStr);
        //    return Json(new { status = flag, message = flag ? "删除成功" : "删除失败" });
        //}

    }
}