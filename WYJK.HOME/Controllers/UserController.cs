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
using WYJK.Framework.Helpers;
using WYJK.HOME.Service;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace WYJK.HOME.Controllers
{
    public class UserController : BaseController
    {
        private readonly IMemberService _memberService = new MemberService();

        UserMemberService userSv = new UserMemberService();

     



        #region 登录、注册、找回密码

        // GET: User
        public ActionResult Login()
        {
            ViewBag.ReturnUrl = Request["returnUrl"];

            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.CheckCode.ToLower().Equals(Session["CheckCode"].ToString().ToLower()))
                {

                    //对密码进行加密
                    string pwd = SecurityHelper.HashPassword(model.Password,model.Password);

                    string sql = $"SELECT * FROM Members where (MemberName='{model.MemberName.Replace("'","").Trim()}' or MemberPhone='{model.MemberName}' or Email='{model.MemberName}') and Password='{pwd}' ";

                    Members users = DbHelper.QuerySingle<Members>(sql);

                    if (users != null)
                    {
                        this.Session["UserInfo"] = users;

                        //回调到原中页面

                        if (!string.IsNullOrEmpty(Request["returnUrl"].ToString()))
                        {
                            return Redirect(Request["returnUrl"].ToString()); ;
                        }

                        return Redirect("/Index/Index");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "用户名或密码错误";
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "验证码输入错误";
                }

            }
            return View(model);
        }

        public ActionResult LoginOut()
        {
            this.Session["UserInfo"] = null;
            return Redirect("/Index/Index");

        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Agreement)
                {
                    MemberRegisterModel m = new MemberRegisterModel()
                    {
                        InviteCode = model.InviteCode,
                        MemberName = model.MemberName,
                        MemberPhone = model.MemberPhone,
                        Password = model.Password,
                        VerificationCode = model.SMSCheckCode

                    };
                    string timespan = "30";
                    //短信验证
                    VerificationCode verificationCode = DbHelper.QuerySingle<VerificationCode>($"select * from VerificationCode where phone='{model.MemberPhone}' and Code='{model.SMSCheckCode}' and CurrentTime > DATEADD(n,-{timespan},getdate())");
                    if (verificationCode == null)
                    {
                        ViewBag.ErrorMessage = "验证码不正确或已失效!";
                        return View(model);
                    }
                        


                    Dictionary<bool, string> dic = await _memberService.RegisterMember(m);

                    if (dic.First().Key)
                    {
                        Members member = await DbHelper.QuerySingleAsync<Members>("select * from Members where MemberName=@MemberName and MemberPhone=@MemberPhone", new
                        {
                            MemberName = model.MemberName,
                            MemberPhone = model.MemberPhone
                        });
                        this.Session["UserInfo"] = member;
                        return Redirect("/User/Info");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = dic.First().Value;
                    }
                }else
                {
                    ViewBag.ErrorMessage = "请勾选 我已阅读并同意无忧数据服务协议";
                }
            }

            return View(model);
        }


        public ActionResult Password()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Password(PasswordViewModel pwd)
        {
            if (ModelState.IsValid)
            {
                Dictionary<bool,string> result = await _memberService.ForgetPassword(new MemberForgetPasswordModel { MemberName=pwd.MemberName, MemberPhone= pwd.MemberPhone, Password = pwd.Password });

                if (result.First().Key)//成功
                {
                    //跳转到登录页面
                    this.Session["UserInfo"] = userSv.Password(pwd);
                    return Redirect("/User/Info");

                }
                else
                {
                    ViewBag.ErrorMessage = "用户名或手机号错误！";
                }
            }

            return View();
        }

        #endregion

        #region 个人信息

        /// <summary>
        /// 个人信息
        /// </summary>
        /// <returns></returns>
        [UserLogin]
        public ActionResult Info()
        {
            return View(userSv.UserInfos(CommonHelper.CurrentUser.MemberID));
        }

        [UserLogin]
        public async Task<ActionResult> InfoChange()
        {
            ExtensionInformationParameter model = await _memberService.GetMemberExtensionInformation(CommonHelper.CurrentUser.MemberID);
            InfoChangeViewModel viewModel = new InfoChangeViewModel();
            model.CopyTo(viewModel);
            //获取下拉列表的数据
            buildSelectList(viewModel);
            return View(viewModel);
        }

        [UserLogin]
        [HttpPost]
        public async Task<ActionResult> InfoChange(InfoChangeViewModel viewModel)
        {

            ExtensionInformationParameter model = new ExtensionInformationParameter();
            viewModel.CopyTo(model);

            if (ModelState.IsValid)
            {
                bool flag = await _memberService.ModifyMemberExtensionInformation(model);
                assignMessage(flag ? "保存成功" : "保存失败", flag);

                #region 日志记录
                if (flag == true)
                {
                    LogService.WriteLogInfo(new Log { UserName = HttpContext.User.Identity.Name, Contents = string.Format("修改了用户{0}信息", (await _memberService.GetMemberInfo(model.MemberID)).MemberName) });
                }
                #endregion
            }

            buildSelectList(viewModel);
            return View(viewModel);
        }

        #endregion

        #region 企业认证

        private HttpClient client = new HttpClient();
        private string url = ConfigurationManager.AppSettings["ServerUrl"] + "/api";
        private JsonMediaTypeFormatter formatter = System.Web.Http.GlobalConfiguration.Configuration.Formatters.Where(f =>
        {
            return f.SupportedMediaTypes.Any(v => v.MediaType.Equals("application/json", StringComparison.CurrentCultureIgnoreCase));
        }).FirstOrDefault() as JsonMediaTypeFormatter;


        public async Task<ActionResult> CertificationAudit()
        {
            var req = await client.GetAsync(url + "/Member/GetEnterpriseCertificationDetails?memberID=" + CommonHelper.CurrentUser.MemberID);
            var model = (await req.Content.ReadAsAsync<JsonResult<CertificationAudit>>()).Data;
            var req1 = await client.GetAsync(url + "/Member/GetIndustryType");
            ViewBag.IndustryType = (await req1.Content.ReadAsAsync<JsonResult<List<string>>>()).Data;
            var req2 = await client.GetAsync(url + "/Member/GetEnterprisePeopleNum");
            ViewBag.EnterprisePeopleNum = (await req2.Content.ReadAsAsync<JsonResult<List<string>>>()).Data;

            return View(model ?? new Entity.CertificationAudit());
        }

        [HttpPost]
        public async Task<ActionResult> CertificationAudit(CertificationAudit model,string ProvinceCode, string CityCode, string CountyCode, HttpPostedFileBase EnterpriseBusinessLicenseImg)
        {
            #region 将编码变成名称
            string sqlstr = "select * from Region where RegionCode = '{0}'";
            string ProvinceName = DbHelper.QuerySingle<Region>(string.Format(sqlstr, ProvinceCode)).RegionName;
            string CityName = DbHelper.QuerySingle<Region>(string.Format(sqlstr, CityCode)).RegionName;
            //string CountyName = DbHelper.QuerySingle<Region>(string.Format(sqlstr, CountyCode)).RegionName;
            #endregion

            model.EnterpriseArea = ProvinceName + "|" + CityName;// + "|" + CountyName;
            string resultup = string.Empty;
            if (EnterpriseBusinessLicenseImg != null)
            {
                var bytes = new byte[EnterpriseBusinessLicenseImg.ContentLength];
                EnterpriseBusinessLicenseImg.InputStream.Read(bytes, 0, EnterpriseBusinessLicenseImg.ContentLength);

                var fileContent = new ByteArrayContent(bytes);
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = "EnterpriseBusinessLicense.jpg" };

                var requpload = await client.PostAsync(url + "/Upload/MultiUpload", new MultipartFormDataContent() { fileContent });
                var resultupload = await requpload.Content.ReadAsAsync<JsonResult<List<string>>>();
                resultup = resultupload.Data.FirstOrDefault();
            }
    

            var req = await client.PostAsync(url + "/Member/CommitEnterpriseCertification", new { CommonHelper.CurrentUser.MemberID, model.EnterpriseName, model.EnterpriseTax, model.EnterpriseType, model.EnterpriseArea, model.EnterpriseLegal, model.EnterpriseLegalIdentityCardNo, model.EnterprisePeopleNum, model.SocialSecurityCreditCode, EnterpriseBusinessLicense = resultup, model.EnterprisePositionName }, formatter);
            var result = await req.Content.ReadAsAsync<JsonResult<object>>();

            TempData["Message"] = result.Data;
            TempData["MessageType"] = true;

            var req1 = await client.GetAsync(url + "/Member/GetIndustryType");
            ViewBag.IndustryType = (await req1.Content.ReadAsAsync<JsonResult<List<string>>>()).Data;
            var req2 = await client.GetAsync(url + "/Member/GetEnterprisePeopleNum");
            ViewBag.EnterprisePeopleNum = (await req2.Content.ReadAsAsync<JsonResult<List<string>>>()).Data;
            return View(model ?? new Entity.CertificationAudit());
        }

        #endregion  

        #region 显示验证码
        /// <summary>
        /// 显示验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public FileContentResult Captcha()
        {
            CaptchaOptions options = new CaptchaOptions
            {
                GaussianDeviation = 0.4,
                Height = 35,
                Background = NoiseLevel.Low,
                Line = NoiseLevel.Low
            };
            using (ICapatcha capatch = new FluentCaptcha())
            {
                capatch.Options = options;
                CaptchaResult captchaResult = capatch.DrawBackgroud().DrawLine().DrawText().Atomized().DrawBroder().DrawImage();
                using (captchaResult)
                {
                    MemoryStream ms = new MemoryStream();
                    captchaResult.Bitmap.Save(ms, ImageFormat.Gif);
                    Session["CheckCode"] = captchaResult.Text;
                    return File(ms.ToArray(), "image/gif");
                }
            }
        }
        #endregion

        #region 基础信息获取

        private void buildSelectList(InfoChangeViewModel model)
        {

            #region 证件类型
            var CertificateTypeList = new List<string> { "请选择" }.Concat(GetCertificateType()).Select(
                                        item => new SelectListItem
                                        {
                                            Text = item,
                                            Value = item == "请选择" ? "" : item,
                                            Selected = item == model.CertificateType

                                        }).ToList();
            ViewData["CertificateType"] = new SelectList(CertificateTypeList, "Value", "Text", model.CertificateType);

            #endregion

            #region 政治面貌
            var PoliticalStatusList = new List<string> { "请选择" }.Concat(GetPoliticalStatus()).Select(
                                        item => new SelectListItem
                                        {
                                            Text = item,
                                            Value = item == "请选择" ? "" : item
                                        }).ToList();

            ViewData["PoliticalStatus"] = new SelectList(PoliticalStatusList, "Value", "Text", model.PoliticalStatus);
            #endregion

            #region 学历
            var EducationList = new List<string> { "请选择" }.Concat(GetEducation()).Select(
                                item => new SelectListItem
                                {
                                    Text = item,
                                    Value = item == "请选择" ? "" : item
                                }).ToList();

            ViewData["Education"] = new SelectList(EducationList, "Value", "Text", model.Education);
            #endregion

            #region 户口性质
            List<SelectListItem> UserTypeList = EnumExt.GetSelectList(typeof(HouseholdPropertyEnum));
            UserTypeList.Insert(0, new SelectListItem { Text = "请选择", Value = "" });


            ViewData["HouseholdType"] = new SelectList(UserTypeList, "Value", "Text", model.HouseholdType);
            #endregion
        }


        /// <summary>
        /// 获取证件类型
        /// </summary>
        /// <returns></returns>
        public List<string> GetCertificateType()
        {
            List<string> list = new List<string>() {
               "身份证","居住证","签证","护照","户口本","军人证","团员证","党员证","港澳通行证"
            };
            return list;
        }

        /// <summary>
        /// 获取政治面貌
        /// </summary>
        /// <returns></returns>
        public List<string> GetPoliticalStatus()
        {
            List<string> list = new List<string>() {
                "中共党员","共青团员","群众"
            };
            return list;
        }

        /// <summary>
        /// 获取学历
        /// </summary>
        /// <returns></returns>
        public List<string> GetEducation()
        {
            List<string> list = new List<string>() {
                "中专","高中","高职（大专）","本科","硕士","博士","博士后"
            };
            return list;
        }

        #endregion

        [UserLogin]
        public async Task<ActionResult> UserMessage()
        {
            var req1 = await client.GetAsync(url + "/Member/GetMessageList?memberID="+CommonHelper.CurrentUser.MemberID);
            List<Message> list = (await req1.Content.ReadAsAsync<JsonResult<List<Message>>>()).Data;
            ViewBag.UserMsg = list;
            ViewBag.HasMsg = (list.Count>0);
            return View();
        }

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <param name="MemberName"></param>
        /// <param name="MemberPhone"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public async Task<JsonResult<dynamic>> GetVerificationCode(string MemberName, string MemberPhone)
        {
            
            var req1 = await client.GetAsync(url + "/Member/GetVerificationCode?MemberName=" + MemberName+ "&MemberPhone="+ MemberPhone);
            object sendRegist = (await req1.Content.ReadAsAsync<JsonResult<object>>()).Data;
            //ViewBag.UserMsg = list;
            //ViewBag.HasMsg = (list.Count > 0);

            //VerificationCode verificationCode = DbHelper.Query<VerificationCode>($"select * from VerificationCode where Phone='{MemberPhone}'").FirstOrDefault();
            ////生成随机数
            //Random rdm = new Random();
            //string code = rdm.Next(0, 9999).ToString().PadLeft(4, '0');
            //if (verificationCode == null)
            //{
            //    DbHelper.ExecuteSqlCommand($"insert into VerificationCode(Phone,Code,CurrentTime) values('{MemberPhone}','{code}',getdate())", null);
            //}
            //else {
            //    DbHelper.ExecuteSqlCommand($"update VerificationCode set Code='{code}',CurrentTime=getdate() where phone='{MemberPhone}'", null);
            //}

            ////发送短信
            //SendSms(MemberPhone, MemberName, code);

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "发送成功"
            };

        }

    }
}