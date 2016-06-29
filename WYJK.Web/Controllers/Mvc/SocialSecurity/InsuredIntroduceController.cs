using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WYJK.Data.IService;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;

namespace WYJK.Web.Controllers
{
    /// <summary>
    /// 参保介绍
    /// </summary>
    public class InsuredIntroduceController : BaseController
    {
        private readonly IInsuredIntroduceService _insuredIntroduceService = new InsuredIntroduceService();

        public async Task<ActionResult> GetInsuredIntroduceList(PagedParameter parameter)
        {
            PagedResult<InsuredIntroduce> informationList = await _insuredIntroduceService.GetInsuredIntroduceList(parameter);
            return View(informationList);
        }

        /// <summary>
        /// 添加参保介绍
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult InsuredIntroduceAdd()
        {
            return View(new InsuredIntroduce());
        }

        /// <summary>
        /// 保存参保介绍
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> InsuredIntroduceAdd(InsuredIntroduce model)
        {
            if (!ModelState.IsValid) return View();

            bool flag = await _insuredIntroduceService.InsuredIntroduceAdd(model);

            TempData["Message"] = flag ==true ? "保存成功" : "保存失败";
            return RedirectToAction("GetInsuredIntroduceList");
        }


        /// <summary>
        /// 新建参保编辑
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> InsuredIntroduceEdit(int ID)
        {
            InsuredIntroduce model = await _insuredIntroduceService.GetInsuredIntroduceDetail(ID);

            return View(model);
        }

        /// <summary>
        /// 保存参保编辑
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> InsuredIntroduceEdit(InsuredIntroduce model)
        {
            bool flag =await _insuredIntroduceService.ModifyInsuredIntroduce(model);
            TempData["Message"]= flag==true ? "编辑成功" : "编辑失败";

            return RedirectToAction("GetInsuredIntroduceList");
        }


        /// <summary>
        /// 批量删除信息
        /// </summary>
        /// <param name="infoids"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult BatchDeleteInfos(int[] ids)
        {
            string idsStr = string.Join(",", ids);
            bool flag = _insuredIntroduceService.BatchDeleteInfos(idsStr);
            return Json(new { status = flag, message = flag ? "删除成功" : "删除失败" });
        }
    }
}