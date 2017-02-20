using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WYJK.Entity;
using WYJK.Web.Controllers.Http;


namespace WYJK.JiaoFei
{
    public class JiaoFeiService
    {
        HttpClient httpClient = new HttpClient();
        private string url = ConfigurationManager.AppSettings["ServerUrl"] + "/api/Member/";

        /// <summary>
        /// 用户查询
        /// </summary>
        /// <param name="memberQuery"></param>
        /// <returns></returns>
        public async Task<MemberQueryReturn> JiaoFeiMemberQuery(MemberQuery memberQuery)
        {
            var req = await httpClient.GetAsync(url + "GetJiaoFeiMemberQuery?businessCode=" + memberQuery.BusinessCode + "&bankCode=" + memberQuery.BankCode + "&memberPhone=" + memberQuery.MemberPhone);

            return await req.Content.ReadAsAsync<MemberQueryReturn>();
        }

        /// <summary>
        /// 缴费
        /// </summary>
        /// <param name="jiaoFeiSubmit"></param>
        /// <returns></returns>
        public async Task<JiaoFeiSubmitReturn> PostJiaoFeiSubmit(JiaoFeiSubmit jiaoFeiSubmit)
        {
            var req = await httpClient.PostAsJsonAsync(url + "PostJiaoFeiSubmit", jiaoFeiSubmit);

            return await req.Content.ReadAsAsync<JiaoFeiSubmitReturn>();
        }

        /// <summary>
        /// 冲正
        /// </summary>
        /// <param name="chongZheng"></param>
        /// <returns></returns>
        public async Task<ChongZhengReturn> PostChongZheng(ChongZheng chongZheng) {
            var req = await httpClient.PostAsJsonAsync(url + "PostChongZheng", chongZheng);

            return await req.Content.ReadAsAsync<ChongZhengReturn>();
        }

        /// <summary>
        /// 日终对账
        /// </summary>
        /// <param name="riZhongDuiZhang"></param>
        /// <returns></returns>
        
        public RiZhongDuiZhangReturn PostRiZhongDuiZhang(RiZhongDuiZhang riZhongDuiZhang)
        {
            //var req = await httpClient.PostAsJsonAsync(url + "PostRiZhongDuiZhang", riZhongDuiZhang);

            //return await req.Content.ReadAsAsync<RiZhongDuiZhangReturn>();

            MemberController memberController = new MemberController();
            return memberController.PostRiZhongDuiZhang(riZhongDuiZhang);
        }

    }
}
