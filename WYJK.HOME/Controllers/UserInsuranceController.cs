using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Data.IService;
using WYJK.Data.IServices;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.Captcha;
using WYJK.Framework.EnumHelper;
using WYJK.HOME.Models;
using WYJK.HOME.Service;

namespace WYJK.HOME.Controllers
{
    public class UserInsuranceController : BaseFilterController
    {
        ISocialSecurityService socialSv = new Data.ServiceImpl.SocialSecurityService();

        WYJK.HOME.Service.SocialSecurityService localSocialSv = new Service.SocialSecurityService();


        RegionService regionSv = new RegionService();

        #region 参保人列表

        /// <summary>
        /// 参保人列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public ActionResult Index(InsuranceQueryParamModel parameter)
        {

            Members m = (Members)this.Session["UserInfo"];

            string where = "";
            if (Convert.ToInt32(parameter.HouseholdProperty) > 0)
            {
                string hp = EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)(Int32.Parse(parameter.HouseholdProperty)));
                where += $@"and sp.HouseholdProperty='{hp}'";
            }
            if (parameter.InsuranceArea != null)
            {

                where += $@"and ss.InsuranceArea='{parameter.InsuranceArea}'";
            }
            if (parameter.SocialSecurityPeopleName != null)
            {
                where += $@"and sp.SocialSecurityPeopleName='{parameter.SocialSecurityPeopleName}'";
            }
            string sqlstr = $@" 
                SELECT 
	                sp.SocialSecurityPeopleID,sp.SocialSecurityPeopleName,sp.IdentityCard,
	                sp.HouseholdProperty,convert(varchar(10),ss.PayTime,111) PayTime,convert(varchar(10),ss.StopDate,111) StopDate,ss.SocialSecurityBase,ss.Status SocialSecurityStatus,
	                cast(round((ss.SocialSecurityBase*ss.PayProportion)/100,2) as numeric(12,2)) SocialSecurityAmount,
	                af.AccumulationFundBase,	
	                cast(round((af.AccumulationFundBase*af.PayProportion)/100,2) as numeric(12,2)) AccumulationFundAmount,	
	                af.Status AccumulationFundStatus	
                from SocialSecurityPeople sp
                left join SocialSecurity  ss on sp.SocialSecurityPeopleID=ss.SocialSecurityPeopleID
                left join AccumulationFund af on sp.SocialSecurityPeopleID=af.SocialSecurityPeopleID
            where sp.MemberID = {m.MemberID} {where} order by sp.SocialSecurityPeopleID desc  ";



            List<InsuranceListViewModel> SocialSecurityPeopleList = DbHelper.Query<InsuranceListViewModel>(sqlstr);

            var c = SocialSecurityPeopleList.Skip(parameter.SkipCount - 1).Take(parameter.PageSize);


            PagedResult<InsuranceListViewModel> page = new PagedResult<InsuranceListViewModel>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = SocialSecurityPeopleList.Count,
                Items = c
            };

            bulidHouseholdPropertyDropdown(parameter.HouseholdProperty);

            return View(page);
        }

        private void bulidHouseholdPropertyDropdown(string value)
        {
            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(HouseholdPropertyEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "户籍性质", Value = "" });

            ViewData["HouseholdProperty"] = new SelectList(UserTypeList, "Value", "Text");

        }



        #endregion



        #region 参保人添加

        [HttpGet]
        public ActionResult Add1()
        {
            bulidHouseholdPropertyDropdown("");
            return View();
        }

        /// <summary>
        /// 处理身份证上传
        /// </summary>
        /// <returns></returns>
        public ActionResult UploadIDCard()
        {
            var files = Request.Files;
            HttpPostedFileBase file = files[0];
            string fielName = DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(file.FileName);

            string path = Path.Combine(CommonHelper.BasePath, DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString(), fielName);
            //生成文件夹
            DirectoryInfo directory = new DirectoryInfo(Path.GetDirectoryName(path) ?? "");
            if (directory.Exists == false)
            {
                directory.Create();
            }

            file.SaveAs(path);

            return Json(path.Replace(CommonHelper.BasePath, "UploadFiles").Replace("\\", "/"));
        }


        [HttpPost]
        public ActionResult Add1(InsuranceAdd1ViewModel model)
        {
            if (ModelState.IsValid)
            {
                SocialSecurityPeople socialPeople = new SocialSecurityPeople();
                socialPeople.MemberID = CommonHelper.CurrentUser.MemberID;
                socialPeople.IdentityCard = model.IdentityCard;
                socialPeople.SocialSecurityPeopleName = model.SocialSecurityPeopleName;
                socialPeople.IdentityCardPhoto = model.IdentityCardPhoto.Substring(1);
                socialPeople.HouseholdProperty = EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)int.Parse(model.HouseholdProperty));

                //保持参保人到数据库,返回参保人ID
                int id = localSocialSv.AddSocialSecurityPeople(socialPeople);

                if (id > 0)
                {
                    socialPeople.SocialSecurityPeopleID = id;
                    //把参保人保存到session中
                    Session["SocialSecurityPeople"] = socialPeople;
                    return RedirectToAction("Add2");
                }
                else
                {
                    bulidHouseholdPropertyDropdown(model.HouseholdProperty);
                    return View(model);
                }
            }
            else
            {
                bulidHouseholdPropertyDropdown(model.HouseholdProperty);
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Add2()
        {
            //获取省份
            ViewBag.Provinces = CommonHelper.EntityListToSelctList(regionSv.GetProvince(), "请选择省份");
            return View();
        }
        [HttpPost]
        public ActionResult Add2(InsuranceAdd2ViewModel model)
        {
            if (ModelState.IsValid)
            {
                //保存数据到数据库
                int id = AddLocalSocialSecurity(model);

                if (id > 0)
                {
                    SocialSecurityPeople people = (SocialSecurityPeople)Session["SocialSecurityPeople"];
                    //跳转到确认页面
                    return Redirect("/UserOrder/Create/" + people.SocialSecurityPeopleID);
                }
                else
                {
                    return RedirectToAction("Add2");
                }


            }
            else
            {
                return RedirectToAction("Add2");
            }

        }

        /// <summary>
        /// 添加社保下一步
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Add2Next(InsuranceAdd2ViewModel model)
        {

            if (ModelState.IsValid)
            {

                int id = AddLocalSocialSecurity(model);

                if (id > 0)
                {
                    //保存数据到数据库
                    return RedirectToAction("Add3");
                }
                else
                {
                    return RedirectToAction("Add2");
                }
            }
            else
            {
                return RedirectToAction("Add2");
            }

        }


        public int AddLocalSocialSecurity(InsuranceAdd2ViewModel model)
        {
            SocialSecurity socialSecurity = new SocialSecurity();
            SocialSecurityPeople people = (SocialSecurityPeople)Session["SocialSecurityPeople"];
            socialSecurity.SocialSecurityPeopleID = people.SocialSecurityPeopleID;
            //socialSecurity.SocialSecurityPeopleID = 49;

            //socialSecurity.InsuranceArea = string.Format("{0}|{1}",Request["provinceText"], Request["city"]);
            socialSecurity.InsuranceArea = "山东省|青岛市|崂山区";
            socialSecurity.HouseholdProperty = people.HouseholdProperty;
            //socialSecurity.HouseholdProperty = "本市城镇";

            socialSecurity.SocialSecurityBase = model.SocialSecurityBase;
            socialSecurity.PayProportion = 0;
            socialSecurity.PayTime = model.PayTime;
            socialSecurity.PayMonthCount = model.PayMonthCount;
            socialSecurity.PayBeforeMonthCount = model.PayBeforeMonthCount;
            socialSecurity.BankPayMonth = model.BankPayMonth;
            socialSecurity.EnterprisePayMonth = model.EnterprisePayMonth;
            socialSecurity.Note = model.Note;
            socialSecurity.RelationEnterprise = 0;
            //保存数据到数据库
            int id = socialSv.AddSocialSecurity(socialSecurity);

            return id;
        }


        [HttpGet]
        public ActionResult Add3()
        {
            //获取省份
            ViewBag.Provinces = CommonHelper.EntityListToSelctList(regionSv.GetProvince(), "请选择省份");
            return View();
        }

        [HttpPost]
        public ActionResult Confirm(InsuranceAdd3ViewModel model)
        {
            if (ModelState.IsValid)
            {
                SocialSecurityPeople people = (SocialSecurityPeople)Session["SocialSecurityPeople"];

                AccumulationFund accumulationFund = new AccumulationFund();

                accumulationFund.SocialSecurityPeopleID = people.SocialSecurityPeopleID;
                //accumulationFund.SocialSecurityPeopleID = 49;
                //accumulationFund.AccumulationFundArea = string.Format("{0}|{1}", Request["provinceText"], Request["city"]);
                accumulationFund.AccumulationFundArea = "山东省|青岛市|崂山区";
                accumulationFund.AccumulationFundBase = model.AccumulationFundBase;
                accumulationFund.PayProportion = 0;
                accumulationFund.PayTime = model.PayTime;
                accumulationFund.PayMonthCount = model.PayMonthCount;
                accumulationFund.PayBeforeMonthCount = model.PayBeforeMonthCount;
                accumulationFund.Note = model.Note;
                accumulationFund.RelationEnterprise = 0;

                int id = socialSv.AddAccumulationFund(accumulationFund);

                if (id > 0)
                {
                    //跳转到确认页面
                    return Redirect("/UserOrder/Create/" + people.SocialSecurityPeopleID);
                }
                else
                {
                    return RedirectToAction("Add3");
                }
            }
            else
            {
                return RedirectToAction("Add3");
            }

        }

        #endregion


        #region 基数变更

        /// <summary>
        /// 社保基数变更
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangeSB()
        {
            return View();
        }

        /// <summary>
        /// 公积金基数变更
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangeFund()
        {
            return View();
        }

        /// <summary>
        /// 社保基数变更历史
        /// </summary>
        /// <returns></returns>
        public ActionResult RecSB(PagedParameter parameter)
        {
            List<SocialSecurityPeopleViewModel> list = localSocialSv.GetBaseAjustRecord(0, CommonHelper.CurrentUser.MemberID);

            var c = list.Skip(parameter.SkipCount - 1).Take(parameter.PageSize);


            PagedResult<SocialSecurityPeopleViewModel> page = new PagedResult<SocialSecurityPeopleViewModel>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = list.Count,
                Items = c
            };

            return View(page);
        }

        /// <summary>
        /// 公积金基数变更历史
        /// </summary>
        /// <returns></returns>
        public ActionResult RecFund(PagedParameter parameter)
        {
            List<SocialSecurityPeopleViewModel> list = localSocialSv.GetBaseAjustRecord(1, CommonHelper.CurrentUser.MemberID);

            var c = list.Skip(parameter.SkipCount - 1).Take(parameter.PageSize);


            PagedResult<SocialSecurityPeopleViewModel> page = new PagedResult<SocialSecurityPeopleViewModel>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = list.Count,
                Items = c
            };

            return View(page);
        }

        #endregion

        /// <summary>
        /// 停保
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult StopSocialSecurity()
        {
            if (Request["cbxSS"] != null)
            {
                string[] ids = Request["cbxSS"].Split(',');

                bool result = localSocialSv.ApplyTopSocialSecurity("个人原因", ids);

            }

            return RedirectToAction("Index");

        }

        /// <summary>
        /// 停公积金
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult StopAF()
        {
            if (Request["cbxSS"] != null)
            {
                string[] ids = Request["cbxSS"].Split(',');

                bool result = localSocialSv.ApplyTopAccumulationFund("", ids);

            }

            return RedirectToAction("Index");

        }

        private readonly ISocialSecurityService _socialSecurityService = new Data.ServiceImpl.SocialSecurityService();
        private readonly IAccumulationFundService _accumulationFundService = new AccumulationFundService();
        private readonly IMemberService _memberService = new MemberService();

        public ActionResult Edit(int? SocialSecurityPeopleID)
        {
            SocialSecurityPeople socialSecurityPeople = null;

            if (SocialSecurityPeopleID == null)
            {
                socialSecurityPeople = new SocialSecurityPeople() { socialSecurity = new SocialSecurity(), accumulationFund = new AccumulationFund() };
                SocialSecurityPeopleID = 0;
            }
            else
            {
                socialSecurityPeople = _socialSecurityService.GetSocialSecurityPeopleForAdmin(SocialSecurityPeopleID.Value);
            }

            if (SocialSecurityPeopleID != null && _socialSecurityService.GetSocialSecurityDetail(SocialSecurityPeopleID.Value) != null)
            {
                socialSecurityPeople.socialSecurity = _socialSecurityService.GetSocialSecurityDetail(SocialSecurityPeopleID.Value);
                //企业签约单位列表
                List<EnterpriseSocialSecurity> SSList = _socialSecurityService.GetEnterpriseSocialSecurityByAreaList(socialSecurityPeople.socialSecurity.InsuranceArea, socialSecurityPeople.HouseholdProperty);
                EnterpriseSocialSecurity SS = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(socialSecurityPeople.socialSecurity.InsuranceArea, socialSecurityPeople.HouseholdProperty);
                ViewData["SSEnterpriseList"] = new SelectList(SSList, "EnterpriseID", "EnterpriseName", socialSecurityPeople.socialSecurity.RelationEnterprise);
                ViewData["SSMaxBase"] = Math.Round(SS.SocialAvgSalary * SS.MaxSocial / 100);
                ViewData["SSMinBase"] = Math.Round(SS.SocialAvgSalary * SS.MinSocial / 100);
            }
            if (SocialSecurityPeopleID != null && _accumulationFundService.GetAccumulationFundDetail(SocialSecurityPeopleID.Value) != null)
            {
                socialSecurityPeople.accumulationFund = _accumulationFundService.GetAccumulationFundDetail(SocialSecurityPeopleID.Value);
                //企业签约单位列表
                List<EnterpriseSocialSecurity> AFList = _socialSecurityService.GetEnterpriseSocialSecurityByAreaList(socialSecurityPeople.accumulationFund.AccumulationFundArea, socialSecurityPeople.HouseholdProperty);
                EnterpriseSocialSecurity AF = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(socialSecurityPeople.accumulationFund.AccumulationFundArea, socialSecurityPeople.HouseholdProperty);
                ViewData["AFEnterpriseList"] = new SelectList(AFList, "EnterpriseID", "EnterpriseName", socialSecurityPeople.accumulationFund.RelationEnterprise);
                ViewData["AFMaxBase"] = AF.MaxAccumulationFund;
                ViewData["AFMinBase"] = AF.MinAccumulationFund;
            }

            //获取会员信息
            ViewData["member"] = _memberService.GetMemberInfoForAdmin(CommonHelper.CurrentUser.MemberID);

            List<AccountRecord> accountRecordList = _memberService.GetAccountRecordList(CommonHelper.CurrentUser.MemberID).OrderByDescending(n => n.ID).ToList();
            //获取账户列表
            ViewData["accountRecordList"] = accountRecordList;

            //获取社保缴费明细
            ViewData["SSAccountRecordList"] = accountRecordList.Where(n => n.SocialSecurityPeopleID == SocialSecurityPeopleID.Value && n.Type == "0").ToList();


            //获取公积金缴费明细
            ViewData["AFAccountRecordList"] = accountRecordList.Where(n => n.SocialSecurityPeopleID == SocialSecurityPeopleID.Value && n.Type == "1").ToList();

            //调整社平工资的缴费明细
            ViewData["SocialAvgSalaryRecordList"] = accountRecordList.Where(n => n.SocialSecurityPeopleID == SocialSecurityPeopleID.Value && n.Type == "2").ToList();


            #region 户口性质
            List<SelectListItem> list = EnumExt.GetSelectList(typeof(HouseholdPropertyEnum));
            list.Insert(0, new SelectListItem() { Text = "请选择", Selected = false, Value = "" });

            int householdType = 0;
            foreach (var item in list)
            {
                if (item.Text == socialSecurityPeople.HouseholdProperty)
                {
                    householdType = Convert.ToInt32(item.Value);
                    break;
                }
            }

            ViewData["HouseholdProperty"] = new SelectList(list, "value", "text", householdType);
            #endregion


            return View(socialSecurityPeople);
        }
        private readonly IUserService _userService = new UserService();

        private HttpClient client = new HttpClient();
        private string url = ConfigurationManager.AppSettings["ServerUrl"] + "/api";
        private JsonMediaTypeFormatter formatter = System.Web.Http.GlobalConfiguration.Configuration.Formatters.Where(f =>
        {
            return f.SupportedMediaTypes.Any(v => v.MediaType.Equals("application/json", StringComparison.CurrentCultureIgnoreCase));
        }).FirstOrDefault() as JsonMediaTypeFormatter;


        [HttpPost]
        public async Task<ActionResult> Edit(SocialSecurityPeopleDetail model, string IdentityCardPhotoPath1, string IdentityCardPhotoPath2, HttpPostedFileBase IdentityCardPhoto1, HttpPostedFileBase IdentityCardPhoto2)
        {
            SocialSecurityPeople socialSecurityPeople = new SocialSecurityPeople();
            socialSecurityPeople.IdentityCard = model.IdentityCard;
            socialSecurityPeople.SocialSecurityPeopleName = model.SocialSecurityPeopleName;
            var file1 = default(string);
            try
            {

                var bytes1 = new byte[IdentityCardPhoto1.ContentLength];
                IdentityCardPhoto1.InputStream.Read(bytes1, 0, IdentityCardPhoto1.ContentLength);
                var fileContent1 = new ByteArrayContent(bytes1);
                fileContent1.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = "IdentityCardPhoto1.jpg" };

                var requpload = await client.PostAsync(url + "/Upload/MultiUpload", new MultipartFormDataContent() { fileContent1 });
                var resultupload = await requpload.Content.ReadAsAsync<JsonResult<List<string>>>();
                file1 = resultupload.Data.FirstOrDefault();
            }
            catch (Exception ex)
            {
            }

            var file2 = default(string);
            try
            {

                var bytes2 = new byte[IdentityCardPhoto2.ContentLength];
                IdentityCardPhoto2.InputStream.Read(bytes2, 0, IdentityCardPhoto2.ContentLength);
                var fileContent2 = new ByteArrayContent(bytes2);
                fileContent2.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = "IdentityCardPhoto2.jpg" };

                var requpload = await client.PostAsync(url + "/Upload/MultiUpload", new MultipartFormDataContent() { fileContent2 });
                var resultupload = await requpload.Content.ReadAsAsync<JsonResult<List<string>>>();
                file2 = resultupload.Data.FirstOrDefault();
            }
            catch (Exception ex)
            {
            }

            socialSecurityPeople.IdentityCardPhoto = string.Join(";", new[] { file1 ?? IdentityCardPhotoPath1, file2 ?? IdentityCardPhotoPath2 }).Replace(ConfigurationManager.AppSettings["ServerUrl"], string.Empty);

            #region 户籍性质
            List<SelectListItem> list = EnumExt.GetSelectList(typeof(HouseholdPropertyEnum));

            foreach (var item in list)
            {
                if (item.Value == model.HouseholdProperty)
                {
                    socialSecurityPeople.HouseholdProperty = item.Text;
                    break;
                }
            }
            #endregion
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    //日志记录
                    string logStr = string.Empty;



                    #region 更新参保人

                    #region 参保人的日志记录
                    //获取参保人旧数据
                    SocialSecurityPeople oldSocialSecurityPeople = DbHelper.QuerySingle<SocialSecurityPeople>($"select * from SocialSecurityPeople where SocialSecurityPeopleID={model.SocialSecurityPeopleID}");
                    //比较新旧值是否一致
                    if (oldSocialSecurityPeople.SocialSecurityPeopleName != socialSecurityPeople.SocialSecurityPeopleName)
                    {
                        logStr += "客服修改了{1}的姓名,从" + oldSocialSecurityPeople.SocialSecurityPeopleName + "到{1};";
                    }
                    if (oldSocialSecurityPeople.IdentityCard != socialSecurityPeople.IdentityCard)
                    {
                        logStr += "客服修改了{1}的身份证号,从" + oldSocialSecurityPeople.IdentityCard + "到" + socialSecurityPeople.IdentityCard + ";";
                    }
                    if (oldSocialSecurityPeople.HouseholdProperty != socialSecurityPeople.HouseholdProperty)
                    {
                        logStr += "客服修改了{1}的户籍性质,从" + oldSocialSecurityPeople.HouseholdProperty + "到" + socialSecurityPeople.HouseholdProperty + ";";
                    }
                    if (oldSocialSecurityPeople.IdentityCardPhoto != socialSecurityPeople.IdentityCardPhoto)
                    {
                        logStr += "客服修改了{1}的身份证照;";
                    }
                    #endregion

                    //参保人新数据更新

                    DbHelper.ExecuteSqlCommand($"update SocialSecurityPeople set SocialSecurityPeopleName='{socialSecurityPeople.SocialSecurityPeopleName}', IdentityCard='{socialSecurityPeople.IdentityCard}',HouseholdProperty='{socialSecurityPeople.HouseholdProperty}',IdentityCardPhoto='{socialSecurityPeople.IdentityCardPhoto}' where SocialSecurityPeopleID={model.SocialSecurityPeopleID}", null);
                    #endregion

                    #region 更新用户
                    string inviteCode = string.Empty;
                    if (!string.IsNullOrEmpty(model.InviteCode))
                    {
                        inviteCode = _userService.GetUserInfoByUserID(model.InviteCode).InviteCode;
                    }
                    #region 注册用户的日志记录

                    string oldUserName = DbHelper.QuerySingle<string>($"select TrueName from Users where InviteCode=(select InviteCode from Members where MemberID={model.MemberID})") ?? "公司";

                    string newUserName = DbHelper.QuerySingle<string>($"select TrueName from Users where UserID='{model.InviteCode}'") ?? "公司";
                    if (oldUserName != newUserName)
                    {
                        logStr += "客服修改了{0}的业务专员,从" + oldUserName + "到" + newUserName + ";";
                    }
                    #endregion

                    DbHelper.ExecuteSqlCommand($"update Members set InviteCode='{inviteCode}' where MemberID={model.MemberID}", null);
                    #endregion

                    #region 生成子订单

                    //查看是否有未付款的订单，如果有，则更改原来的订单，如果没有
                    //然后查看参保人是否已经支付，如果已经支付则需要生成一个待支付的差额订单

                    //查找参保人对应的社保和公积金的交费金额
                    decimal OldSSAmount = 0, NewSSAmount = 0, OldAFAmount = 0, NewAFAmount = 0;
                    if (_socialSecurityService.GetSocialSecurityDetail(model.SocialSecurityPeopleID) != null)
                    {
                        OldSSAmount = Math.Round(DbHelper.QuerySingle<decimal>($"select SocialSecurityBase*PayProportion/100 from SocialSecurity where SocialSecurityPeopleID={ model.SocialSecurityPeopleID}"), 2);
                        NewSSAmount = Math.Round(Convert.ToDecimal(model.SocialSecurityBase) * Convert.ToDecimal(model.ssPayProportion.TrimEnd('%')) / 100, 2);//现在社保金额
                    }

                    if (_accumulationFundService.GetAccumulationFundDetail(model.SocialSecurityPeopleID) != null)
                    {
                        OldAFAmount = Math.Round(DbHelper.QuerySingle<decimal>($"select AccumulationFundBase*PayProportion/100 from AccumulationFund where SocialSecurityPeopleID={ model.SocialSecurityPeopleID}"), 2);
                        NewAFAmount = Math.Round(Convert.ToDecimal(model.AccumulationFundBase) * Convert.ToDecimal(model.afPayProportion.TrimEnd('%')) / 100, 2);//现在公积金金额
                    }
                    //如果新社保金额大于原来金额
                    if (NewSSAmount > OldSSAmount)
                    {
                        //查看是否存在社保未付款的订单
                        if (DbHelper.QuerySingle<int>($"select count(0) from [Order] left join OrderDetails on [Order].OrderCode =OrderDetails.OrderCode where SocialSecurityPeopleID={ model.SocialSecurityPeopleID} and OrderDetails.IsPaySocialSecurity=1 and [Order].Status=0") > 0)
                        {
                            DbHelper.ExecuteSqlCommand($"update OrderDetails set SocialSecurityAmount='{NewSSAmount}' where OrderDetailID=(select OrderDetails.OrderDetailID from [Order] left join OrderDetails on [Order].OrderCode =OrderDetails.OrderCode where SocialSecurityPeopleID={ model.SocialSecurityPeopleID} and OrderDetails.IsPaySocialSecurity=1 and [Order].Status=0)", null);
                        }
                        //查看参保人是否已经支付，如果已经支付则需要生成一个待支付的差额订单
                        else if (DbHelper.QuerySingle<bool>($"select IsPay from SocialSecurity where SocialSecurityPeopleID={ model.SocialSecurityPeopleID}"))
                        {
                            SocialSecurity socialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select * from SocialSecurity where  SocialSecurityPeopleID={ model.SocialSecurityPeopleID}");
                            string orderCode = DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random().Next(1000).ToString().PadLeft(3, '0');
                            DbHelper.ExecuteSqlCommand($@"insert into [Order](OrderCode,MemberID,GenerateDate,Status,IsNotCancel) 
values('{orderCode}',{model.MemberID},'{DateTime.Now}',0,1);
insert into OrderDetails(OrderCode,SocialSecurityPeopleID,SocialSecurityPeopleName,SSPayTime,SocialSecurityAmount,SocialSecuritypayMonth,IsPaySocialSecurity)
 values('{orderCode}',{model.SocialSecurityPeopleID},'{model.SocialSecurityPeopleName}','{socialSecurity.PayTime}',{NewSSAmount - OldSSAmount},{socialSecurity.PayMonthCount},1)", null);
                        }
                    }

                    //如果新公积金金额大于原来金额
                    if (NewAFAmount > OldAFAmount)
                    {
                        //查看是否存在公积金未付款订单
                        if (DbHelper.QuerySingle<int>($"select count(0) from [Order] left join OrderDetails on [Order].OrderCode =OrderDetails.OrderCode where SocialSecurityPeopleID={ model.SocialSecurityPeopleID} and OrderDetails.IsPayAccumulationFund=1 and [Order].Status=0") > 0)
                        {
                            DbHelper.ExecuteSqlCommand($"update OrderDetails set AccumulationFundAmount='{NewAFAmount}' where OrderDetailID=(select OrderDetails.OrderDetailID form [Order] left join OrderDetails on [Order].OrderCode =OrderDetails.OrderCode where SocialSecurityPeopleID={ model.SocialSecurityPeopleID} and OrderDetails.IsPayAccumulationFund=1 and [Order].Status=0)", null);
                        }
                        else if (DbHelper.QuerySingle<bool>($"select IsPay from AccumulationFund where SocialSecurityPeopleID={ model.SocialSecurityPeopleID}"))
                        {
                            AccumulationFund accumulationFund = DbHelper.QuerySingle<AccumulationFund>($"select * from AccumulationFund where  SocialSecurityPeopleID={ model.SocialSecurityPeopleID}");
                            string orderCode = DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random().Next(1000).ToString().PadLeft(3, '0');
                            DbHelper.ExecuteSqlCommand($@"insert into [Order](OrderCode,MemberID,GenerateDate,Status,IsNotCancel) 
values('{orderCode}',{model.MemberID},'{DateTime.Now}',0,1);
insert into OrderDetails(OrderCode,SocialSecurityPeopleID,SocialSecurityPeopleName,AFPayTime,AccumulationFundAmount,AccumulationFundpayMonth,IsPayAccumulationFund)
 values('{orderCode}',{model.SocialSecurityPeopleID},'{model.SocialSecurityPeopleName}','{accumulationFund.PayTime}',{NewAFAmount - OldAFAmount},{accumulationFund.PayMonthCount},1)", null);
                        }
                    }


                    #endregion


                    if (_socialSecurityService.GetSocialSecurityDetail(model.SocialSecurityPeopleID) != null)
                    {
                        #region 社保的日志记录
                        //参保原数据
                        SocialSecurity oldSocialSecurity = _socialSecurityService.GetSocialSecurityDetail(model.SocialSecurityPeopleID);
                        //客户社保号
                        if ((oldSocialSecurity.SocialSecurityNo ?? "") != (model.SocialSecurityNo ?? ""))
                        {
                            logStr += "客服修改了{1}的客户社保号,从" + oldSocialSecurity.SocialSecurityNo + "到" + model.SocialSecurityNo + ";";
                        }
                        //签约单位
                        if (oldSocialSecurity.RelationEnterprise != Convert.ToInt32(model.SSEnterpriseList))
                        {
                            string oldRelationEnterprise = DbHelper.QuerySingle<string>("select EnterpriseName from EnterpriseSocialSecurity where EnterpriseID=" + oldSocialSecurity.RelationEnterprise);
                            string newRelationEnterprise = DbHelper.QuerySingle<string>("select EnterpriseName from EnterpriseSocialSecurity where EnterpriseID=" + model.SSEnterpriseList);
                            logStr += "客服修改了{1}的签约单位,从" + oldRelationEnterprise + "到" + newRelationEnterprise + ";";
                        }
                        //基数
                        if (oldSocialSecurity.SocialSecurityBase != Convert.ToDecimal(model.SocialSecurityBase))
                        {
                            logStr += "客服修改了{1}的基数,从" + oldSocialSecurity.SocialSecurityBase + "到" + model.SocialSecurityBase + ";";
                        }

                        #endregion

                        #region 更新社保
                        DbHelper.ExecuteSqlCommand($"update SocialSecurity set SocialSecurityNo='{model.SocialSecurityNo}',SocialSecurityBase='{model.SocialSecurityBase}',RelationEnterprise='{model.SSEnterpriseList}',PayProportion='{model.ssPayProportion.TrimEnd('%')}' where SocialSecurityPeopleID={model.SocialSecurityPeopleID}", null);
                        #endregion
                    }
                    if (_accumulationFundService.GetAccumulationFundDetail(model.SocialSecurityPeopleID) != null)
                    {
                        #region 公积金的日志记录
                        //公积金原数据
                        AccumulationFund oldAccumulationFund = _accumulationFundService.GetAccumulationFundDetail(model.SocialSecurityPeopleID);
                        //客户公积金号
                        if ((oldAccumulationFund.AccumulationFundNo ?? "") != (model.AccumulationFundNo ?? ""))
                        {
                            logStr += "客服修改了{1}的客户公积金号,从" + oldAccumulationFund.AccumulationFundNo + "到" + model.AccumulationFundNo + ";";
                        }
                        //签约单位
                        if (oldAccumulationFund.RelationEnterprise != Convert.ToInt32(model.AFEnterpriseList))
                        {
                            string oldRelationEnterprise = DbHelper.QuerySingle<string>("select EnterpriseName from EnterpriseSocialSecurity where EnterpriseID=" + oldAccumulationFund.RelationEnterprise);
                            string newRelationEnterprise = DbHelper.QuerySingle<string>("select EnterpriseName from EnterpriseSocialSecurity where EnterpriseID=" + model.AFEnterpriseList);
                            logStr += "客服修改了{1}的签约单位,从" + oldRelationEnterprise + "到" + newRelationEnterprise + ";";
                        }
                        //基数
                        if (oldAccumulationFund.AccumulationFundBase != Convert.ToDecimal(model.AccumulationFundBase))
                        {
                            logStr += "客服修改了{1}的基数,从" + oldAccumulationFund.AccumulationFundBase + "到" + model.AccumulationFundBase + ";";
                        }

                        #endregion

                        #region 更新公积金
                        DbHelper.ExecuteSqlCommand($"update AccumulationFund set AccumulationFundNo='{model.AccumulationFundNo}',AccumulationFundBase='{model.AccumulationFundBase}',RelationEnterprise='{model.AFEnterpriseList}',PayProportion='{model.afPayProportion.TrimEnd('%')}' where SocialSecurityPeopleID={model.SocialSecurityPeopleID}", null);
                        #endregion
                    }

                    if (logStr != string.Empty)
                    {
                        LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = logStr, MemberID = model.MemberID, SocialSecurityPeopleID = model.SocialSecurityPeopleID });
                    }


                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "更新失败";
                    TempData["MessageType"] = false;

                    return RedirectToAction("GetCustomerServiceList");
                }
                finally
                {
                    transaction.Dispose();
                }
            }
            TempData["Message"] = "更新成功";
            TempData["MessageType"] = true;
            return RedirectToAction("Index");
        }

        private readonly IEnterpriseService _enterpriseService = new EnterpriseService();


        /// <summary>
        /// 获取企业列表
        /// </summary>
        /// <param name="area"></param>
        /// <param name="HouseHoldProperty"></param>
        /// <returns></returns>
        public ActionResult GetEnterpriseSocialSecurityByAreaList(string area, string HouseHoldProperty)
        {
            List<EnterpriseSocialSecurity> list = _socialSecurityService.GetEnterpriseSocialSecurityByAreaList(area, HouseHoldProperty);
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 根据签约单位ID获取签约单位信息
        /// </summary>
        /// <param name="EnterpriseID"></param>
        /// <returns></returns>
        public ActionResult GetSSEnterprise(int EnterpriseID, int HouseholdProperty)
        {
            EnterpriseSocialSecurity model = _enterpriseService.GetEnterpriseSocialSecurity(EnterpriseID);
            decimal SSMaxBase = Math.Round(model.SocialAvgSalary * model.MaxSocial / 100);
            decimal SSMinBase = Math.Round(model.SocialAvgSalary * model.MinSocial / 100);
            decimal value = 0;
            if (HouseholdProperty == (int)HouseholdPropertyEnum.ThisCityRural ||
                HouseholdProperty == (int)HouseholdPropertyEnum.ThisProvinceRural ||
                    HouseholdProperty == (int)HouseholdPropertyEnum.OtherProvinceRural)
            {
                value = model.PersonalShiYeRural;
            }
            else if (HouseholdProperty == (int)HouseholdPropertyEnum.ThisCityTown ||
                HouseholdProperty == (int)HouseholdPropertyEnum.ThisProvinceTown ||
                    HouseholdProperty == (int)HouseholdPropertyEnum.OtherProvinceTown)
            {
                value = model.PersonalShiYeTown;
            }
            decimal SSPayProportion = model.CompYangLao + model.CompYiLiao + model.CompShiYe + model.CompGongShang + model.CompShengYu
                + model.PersonalYangLao + model.PersonalYiLiao + value + model.PersonalGongShang + model.PersonalShengYu;
            decimal SSMonthAccount = Math.Round(Math.Round(Convert.ToDecimal(SSMinBase)) * Convert.ToDecimal(SSPayProportion) / 100, 2);

            return Json(new
            {
                SSMaxBase = SSMaxBase,
                SSMinBase = SSMinBase,
                SSPayProportion = SSPayProportion,
                SSMonthAccount = SSMonthAccount
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 根据签约单位ID获取签约单位信息
        /// </summary>
        /// <param name="EnterpriseID"></param>
        /// <returns></returns>
        public ActionResult GetAFEnterprise(int EnterpriseID)
        {
            EnterpriseSocialSecurity model = _enterpriseService.GetEnterpriseSocialSecurity(EnterpriseID);
            decimal AFMaxBase = model.MaxAccumulationFund;
            decimal AFMinBase = model.MinAccumulationFund;
            decimal AFPayProportion = model.CompProportion + model.PersonalProportion;
            decimal AFMonthAccount = Math.Round(AFMinBase * AFPayProportion / 100, 2);

            return Json(new
            {
                AFMaxBase = AFMaxBase,
                AFMinBase = AFMinBase,
                AFPayProportion = AFPayProportion,
                AFMonthAccount = AFMonthAccount
            }, JsonRequestBehavior.AllowGet);
        }
    }
}