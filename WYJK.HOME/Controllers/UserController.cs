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
using WYJK.HOME.Models;

namespace WYJK.HOME.Controllers
{
    public class UserController : BaseController
    {
        private readonly IMemberService _memberService = new MemberService();

        // GET: User
        public ActionResult Login()
        {
            if (this.Session["UserInfo"]!=null)
            {
                return Redirect("/User/Info");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {

            if (ModelState.IsValid)
            {
                if (model.CheckCode.ToLower().Equals(Session["CheckCode"].ToString().ToLower()))
                {
                    string sql = $"SELECT * FROM Members where MemberName='{model.Email}' and Password='{model.Password}' ";

                    Members users = DbHelper.QuerySingle<Members>(sql);

                    if (users != null)
                    {
                        this.Session["UserInfo"] = users;
                        return Redirect("/User/Info");
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

        public ActionResult LoginOut(LoginViewModel model)
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
                        Password = model.Password

                    };

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
                    ViewBag.ErrorMessage = "请勾选 我已阅读并同意无忧借款服务协议";
                }
            }

            return View(model);
        }

        [NeedLogin]
        public async Task<ActionResult> Info()
        {
            Members m = (Members)this.Session["UserInfo"];

            string sql = "select * from Members where MemberID=@MemberID";

            Members member = await DbHelper.QuerySingleAsync<Members>(sql, new { MemberID = m.MemberID });

            return View(member);
        }


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
    }
}