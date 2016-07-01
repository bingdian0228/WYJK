using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using WYJK.Data.IService;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;

namespace WYJK.Web.Controllers.Http
{
    /// <summary>
    /// 信息展示
    /// </summary>
    public class InformationController : ApiController
    {
        IInformationService _informationService = new InformationService();

        /// <summary>
        /// 获取新闻通知
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult<dynamic>> GetInformationList(string Type, int PageIndex, int PageSize)
        {
            PagedResult<Information> informationList = await _informationService.GetNewNoticeList(new InformationParameter { PageIndex = PageIndex, PageSize = PageSize, Type = Type, IsDescending = true });

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "成功",
                Data = informationList
            };
        }

        /// <summary>
        /// 添加保存
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public async Task<JsonResult<dynamic>> InformationAdd(Information model)
        {
            bool flag = await _informationService.InformationAdd(model);

            if (model.ImgUrls != null)
            {
                model.ImgUrl = string.Join(";", model.ImgUrls).Replace(ConfigurationManager.AppSettings["ServerUrl"], string.Empty);
            }

            return new JsonResult<dynamic>
            {
                status = flag,
                Message = flag ? "保存成功" : "保存失败",
            };
        }

        /// <summary>
        /// 保存编辑
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public async Task<JsonResult<dynamic>> InformationEdit(Information model)
        {
            if (model.ImgUrls != null)
            {
                model.ImgUrl = string.Join(";", model.ImgUrls).Replace(ConfigurationManager.AppSettings["ServerUrl"], string.Empty);
                model.CreateTime = DateTime.Now;
            }

            bool flag = await _informationService.ModifyInformation(model);
            return new JsonResult<dynamic>
            {
                status = flag,
                Message = flag ? "编辑成功" : "编辑失败",
            };
        }

        /// <summary>
        /// 批量删除信息
        /// </summary>
        /// <param name="infoids"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JsonResult<dynamic> BatchDeleteInfos(int[] infoids)
        {
            string infoidsStr = string.Join(",", infoids);
            bool flag = _informationService.BatchDeleteInfos(infoidsStr);
            return new JsonResult<dynamic>
            {
                status = flag,
                Message = flag ? "删除成功" : "删除失败",
            };
        }
    }
}