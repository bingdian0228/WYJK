using WYJK.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.ModelBinding;
using System.Web.Mvc;
using WYJK.Data.IServices;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Web.Filters;
using System.Text.RegularExpressions;
using WYJK.Data;
using WYJK.Framework.EnumHelper;
using WYJK.Data.IService;
using System.Transactions;
using System.Configuration;
using System.Threading;
using System.IO;
using System.Text;
using WYJK.Framework.Helpers;
using CMBCHINALib;
using System.Data.Common;
using System.Collections.Specialized;

namespace WYJK.Web.Controllers.Http
{
    /// <summary>
    /// 用户接口
    /// </summary>
    public class MemberController : BaseApiController
    {
        private readonly IMemberService _memberService = new MemberService();
        private readonly ISocialSecurityService _socialSecurityService = new SocialSecurityService();
        private readonly IParameterSettingService _parameterSettingService = new ParameterSettingService();
        private readonly IEnterpriseService _enterpriseService = new EnterpriseService();

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public async Task<JsonResult<MemberRegisterModel>> RegisterMember(MemberRegisterModel entity)
        {
            string timespan = ConfigurationManager.AppSettings["timespan"].ToString();
            //短信验证
            VerificationCode verificationCode = DbHelper.QuerySingle<VerificationCode>($"select * from VerificationCode where phone='{entity.MemberPhone}' and Code='{entity.VerificationCode}' and CurrentTime > DATEADD(n,-{timespan},getdate())");
            if (verificationCode == null)
                return new JsonResult<MemberRegisterModel>
                {
                    status = false,
                    Message = "验证码不正确或已失效！"
                };


            Dictionary<bool, string> dic = await _memberService.RegisterMember(entity);

            if (dic.First().Key)
            {
                //根据用户名和手机号获取MemberID

                Members member = await DbHelper.QuerySingleAsync<Members>("select * from Members where MemberName=@MemberName and MemberPhone=@MemberPhone", new
                {
                    MemberName = entity.MemberName,
                    MemberPhone = entity.MemberPhone
                });
                entity.MemberID = member.MemberID;
            }

            return new JsonResult<MemberRegisterModel>
            {
                status = dic.First().Key,
                Message = dic.First().Value,
                Data = entity
            };
        }

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <param name="MemberName"></param>
        /// <param name="MemberPhone"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<dynamic> GetVerificationCode(string MemberName, string MemberPhone)
        {
            VerificationCode verificationCode = DbHelper.Query<VerificationCode>($"select * from VerificationCode where Phone='{MemberPhone}'").FirstOrDefault();
            //生成随机数
            Random rdm = new Random();
            string code = rdm.Next(0, 9999).ToString().PadLeft(4, '0');
            if (verificationCode == null)
            {
                DbHelper.ExecuteSqlCommand($"insert into VerificationCode(Phone,Code,CurrentTime) values('{MemberPhone}','{code}',getdate())", null);
            }
            else {
                DbHelper.ExecuteSqlCommand($"update VerificationCode set Code='{code}',CurrentTime=getdate() where phone='{MemberPhone}'", null);
            }

            //发送短信
            SendSms(MemberPhone, MemberName, code);

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "发送成功"
            };

        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public string SendSms(string mobile, string userName, string code)
        {
            string timespan = ConfigurationManager.AppSettings["timespan"].ToString();
            string apikey = ConfigurationManager.AppSettings["apikey"].ToString();
            string contentFormat = ConfigurationManager.AppSettings["SMSVerifContentFormat"].ToString();
            string content = string.Format(contentFormat, userName, code, timespan);
            return Sms.sendSms(apikey, content, mobile);
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public async Task<JsonResult<MemberLoginModel>> LoginMember(MemberLoginModel entity)
        {

            Dictionary<bool, string> dic = await _memberService.LoginMember(entity);

            if (dic.First().Key)
            {
                //根据用户名和手机号获取MemberID

                Members member = await DbHelper.QuerySingleAsync<Members>("select * from Members where MemberName=@MemberName or MemberPhone=@MemberPhone", new
                {
                    MemberName = entity.Account,
                    MemberPhone = entity.Account
                });
                entity.MemberID = member.MemberID;
                entity.MemberName = member.MemberName;
                entity.MemberPhone = member.MemberPhone;
            }


            return new JsonResult<MemberLoginModel>
            {
                status = dic.First().Key,
                Message = dic.First().Value,
                Data = entity
            };
        }

        /// <summary>
        /// 用户密码找回
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public async Task<JsonResult<MemberForgetPasswordModel>> ForgetPassword(MemberForgetPasswordModel entity)
        {
            string timespan = ConfigurationManager.AppSettings["timespan"].ToString();
            //短信验证
            VerificationCode verificationCode = DbHelper.QuerySingle<VerificationCode>($"select * from VerificationCode where phone='{entity.MemberPhone}' and Code='{entity.VerificationCode}' and CurrentTime > DATEADD(n,-{timespan},getdate())");
            if (verificationCode == null)
                return new JsonResult<MemberForgetPasswordModel>
                {
                    status = false,
                    Message = "验证码不正确或已失效！"
                };

            Dictionary<bool, string> dic = await _memberService.ForgetPassword(entity);

            return new JsonResult<MemberForgetPasswordModel>
            {
                status = dic.First().Key,
                Message = dic.First().Value,
                Data = entity
            };
        }

        /// <summary>
        /// 获取用户信息 IsAuthentication:是否认证 0/未认证 1/已认证 2/认证中，IsComplete：是否信息补全 0/未补全 1/已补全,UserType 用户类型  0：普通用户、1：企业用户、2：个体用户
        /// </summary>
        /// <param name="MemberID">MemberID</param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public async Task<JsonResult<dynamic>> GetMemberInfo(int MemberID)
        {
            Members member = await _memberService.GetMemberInfo(MemberID);
            //member.UserType = DbHelper.QuerySingle<CertificationAudit>($"select * from CertificationAudit where MemberID={MemberID}").UserType;

            if (member.IsAuthentication == "2")
            {
                member.UserType = DbHelper.Query<CertificationAudit>($"select UserType from CertificationAudit where MemberID ={MemberID}").FirstOrDefault().UserType;
            }

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "获取成功",
                Data = new
                {
                    TrueName = member.TrueName,
                    MemberName = member.MemberName,
                    MemberPhone = member.MemberPhone,
                    IsAuthentication = member.IsAuthentication,
                    IsComplete = member.IsComplete,
                    UserType = member.UserType
                }
            };
        }

        /// <summary>
        /// 信息编辑提交
        /// </summary>
        /// <param name="MemberID"></param>
        /// <param name="TrueName"></param>
        /// <param name="MemberName"></param>
        /// <param name="MemberPhone"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<dynamic> SubmitMemberInfo(int MemberID, string TrueName, string MemberName, string MemberPhone, string VerificationCode)
        {
            string timespan = ConfigurationManager.AppSettings["timespan"].ToString();
            //如果新手机号与原手机号不同，则需要验证码
            string oldMemberPhone = DbHelper.QuerySingle<string>($"select MemberPhone from Members where MemberID={MemberID}");
            string compPhone = "";
            if (MemberPhone != oldMemberPhone)
            {
                compPhone = oldMemberPhone;
            }
            else
            {
                compPhone = MemberPhone;
            }
            //短信验证    
            VerificationCode verificationCode = DbHelper.QuerySingle<VerificationCode>($"select * from VerificationCode where phone='{compPhone}' and Code='{VerificationCode}' and CurrentTime > DATEADD(n,-{timespan},getdate())");
            if (verificationCode == null)
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "验证码不正确或已失效！"
                };

            //如果用户名重复
            if (DbHelper.QuerySingle<int>($"select count(1) from Members where MemberName='{MemberName}' and MemberID<>{MemberID}") > 0)
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "用户名重复"
                };
            //如果手机号重复
            if (DbHelper.QuerySingle<int>($"select count(1) from Members where MemberPhone='{MemberPhone}' and MemberID<>{MemberID}") > 0)
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "手机号重复"
                };

            string sqlstr = $"update Members set TrueName ='{TrueName}',MemberName='{MemberName}',MemberPhone='{MemberPhone}' where MemberID={MemberID}";
            int result = DbHelper.ExecuteSqlCommand(sqlstr, null);
            return new JsonResult<dynamic>
            {
                status = result > 0,
                Message = result > 0 ? "更新成功" : "更新失败"
            };
        }

        /// <summary>
        /// 修改密码 根据MemberID修改密码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public async Task<JsonResult<dynamic>> ModifyPassword(MemberMidifyPassword model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return new JsonResult<dynamic>
            //    {
            //        status = false,
            //        Message = ModelState.FirstModelStateError()
            //    };
            //}

            bool isTrue = await _memberService.IsTrueOldPassword(model);
            if (!isTrue)
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "原密码错误"
                };
            bool flag = await _memberService.ModifyPassword(model);

            return new JsonResult<dynamic>
            {
                status = flag,
                Message = flag ? "修改成功" : "修改失败"
            };
        }

        /// <summary>
        /// 获取行业类型
        /// </summary>
        /// <returns></returns>
        public JsonResult<List<string>> GetIndustryType()
        {
            List<string> list = new List<string>();
            list.Add("软件行业");
            list.Add("硬件行业");

            return new JsonResult<List<string>>
            {
                status = true,
                Message = "获取成功",
                Data = list
            };
        }

        /// <summary>
        /// 获取公司规模
        /// </summary>
        /// <returns></returns>
        public JsonResult<List<string>> GetEnterprisePeopleNum()
        {
            List<string> list = new List<string>();
            list.Add("20人以下");
            list.Add("21-50");
            list.Add("50-100");

            return new JsonResult<List<string>>
            {
                status = true,
                Message = "获取成功",
                Data = list
            };
        }

        /// <summary>
        /// 企业资质认证
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public async Task<JsonResult<dynamic>> CommitEnterpriseCertification(EnterpriseCertification parameter)
        {
            //删除个体认证
            DbHelper.ExecuteSqlCommand($"delete from CertificationAudit where MemberID = {parameter.MemberID}", null);

            await _memberService.CommitEnterpriseCertification(parameter);

            DbHelper.ExecuteSqlCommand($"update Members set IsAuthentication=2 where MemberID={parameter.MemberID}", null);

            ////验证身份证
            //if (!Regex.IsMatch(parameter.EnterpriseLegalIdentityCardNo, @"(^\d{18}$)|(^\d{15}$)"))
            //    return new JsonResult<dynamic>
            //    {
            //        status = false,
            //        Message = "身份证号填写错误"
            //    };

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "企业认证中"
            };
        }

        /// <summary>
        /// 获取企业认证详情
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<dynamic> GetEnterpriseCertificationDetails(int memberID)
        {
            CertificationAudit model = DbHelper.QuerySingle<CertificationAudit>($"select * from CertificationAudit where MemberID={memberID}");

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "获取企业认证成功",
                Data = model

            };
        }

        /// <summary>
        /// 更新企业资质认证
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<JsonResult<dynamic>> UpdateEnterpriseCertification(EnterpriseCertification parameter)
        {
            //删除认证审核
            DbHelper.ExecuteSqlCommand($"delete from CertificationAudit where MemberID={parameter.MemberID}", null);

            await _memberService.CommitEnterpriseCertification(parameter);

            DbHelper.ExecuteSqlCommand($"update Members set IsAuthentication=2 where MemberID={parameter.MemberID}", null);

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "企业认证中"

            };
        }




        /// <summary>
        /// 个体认证
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult<dynamic>> CommitPersonCertification(IndividualCertification parameter)
        {
            //删除企业认证
            DbHelper.ExecuteSqlCommand($"delete from CertificationAudit where MemberID = {parameter.MemberID}", null);

            await _memberService.CommitPersonCertification(parameter);

            DbHelper.ExecuteSqlCommand($"update Members set IsAuthentication=2 where MemberID={parameter.MemberID}", null);

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "个体认证中"
            };
        }

        /// <summary>
        /// 获取个人认证详情
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<dynamic> GetPersonCertificationDetails(int memberID)
        {
            CertificationAudit model = DbHelper.QuerySingle<CertificationAudit>($"select * from CertificationAudit where MemberID={memberID}");

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "获取个体认证成功",
                Data = model

            };
        }

        /// <summary>
        /// 更新个体资质认证
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<JsonResult<dynamic>> UpdatePersonCertification(IndividualCertification parameter)
        {
            //删除认证审核
            DbHelper.ExecuteSqlCommand($"delete from CertificationAudit where MemberID={parameter.MemberID}", null);

            await _memberService.CommitPersonCertification(parameter);

            DbHelper.ExecuteSqlCommand($"update Members set IsAuthentication=2 where MemberID={parameter.MemberID}", null);

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "个体认证中"
            };
        }


        /// <summary>
        /// 补充信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public async Task<JsonResult<dynamic>> CommitExtensionInformation(ExtensionInformationParameter model)
        {
            //int MemberID = Convert.ToInt32(HttpContext.Current.Request.Form["MemberID"]);
            bool flag = await _memberService.ModifyMemberExtensionInformation(model);

            return new JsonResult<dynamic>
            {
                status = flag,
                Message = flag ? "补充信息成功" : "补充信息失败"
            };
        }

        /// <summary>
        /// 获取补充信息
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public async Task<JsonResult<ExtensionInformation>> GetExtensionInformation(int MemberID)
        {
            //获取补充信息
            ExtensionInformation model = await _memberService.GetExtensionInformation(MemberID);

            return new JsonResult<ExtensionInformation>
            {
                status = true,
                Message = "获取成功",
                Data = model
            };
        }

        /// <summary>
        /// 获取证件类型
        /// </summary>
        /// <returns></returns>
        public JsonResult<List<string>> GetCertificateType()
        {
            List<string> list = new List<string>() {
               "身份证","居住证","签证","护照","户口本","军人证","团员证","党员证","港澳通行证"
            };

            return new JsonResult<List<string>>
            {
                status = true,
                Message = "获取成功",
                Data = list
            };
        }

        /// <summary>
        /// 获取政治面貌
        /// </summary>
        /// <returns></returns>
        public JsonResult<List<string>> GetPoliticalStatus()
        {

            List<string> list = new List<string>() {
                "中共党员","共青团员","群众"
            };
            return new JsonResult<List<string>>
            {
                status = true,
                Message = "获取成功",
                Data = list
            };
        }

        /// <summary>
        /// 获取学历
        /// </summary>
        /// <returns></returns>
        public JsonResult<List<string>> GetEducation()
        {
            List<string> list = new List<string>() {
                "中专","高中","高职（大专）","本科","硕士","博士","博士后"
            };
            return new JsonResult<List<string>>
            {
                status = true,
                Message = "获取成功",
                Data = list
            };
        }


        /// <summary>
        /// 获取账单简单信息
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public JsonResult<AccountInfo> GetAccountInfo(int MemberID)
        {
            AccountInfo accountInfo = _memberService.GetAccountInfo(MemberID);
            accountInfo.HeadPortrait = ConfigurationManager.AppSettings["ServerUrl2"] + accountInfo.HeadPortrait;

            return new JsonResult<AccountInfo>
            {
                status = true,
                Message = "获取账单信息成功",
                Data = accountInfo
            };
        }

        /// <summary>
        /// 获取账单列表 类型：收入/0,支出/1   简单页面只需传MemberID、PageIndex、PageSize   显示属性如：OperationType，CostDisplay，CreateTime
        /// </summary>
        /// <param name="MemberID"></param>
        /// <param name="ShouZhiType"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        public async Task<JsonResult<PagedResult<AccountRecord>>> GetAccountRecordList(int MemberID, string ShouZhiType = "", DateTime? StartTime = null, DateTime? EndTime = null, int PageIndex = 1, int PageSize = 1)
        {
            PagedResult<AccountRecord> list = await _memberService.GetAccountRecordList(MemberID, ShouZhiType, StartTime, EndTime, new PagedParameter { PageIndex = PageIndex, PageSize = PageSize });
            list.Items.ToList().ForEach(n =>
            {
                if (n.ShouZhiType.Trim() == "收入")
                    n.CostDisplay = "+" + Convert.ToString(n.Cost);
                else if (n.ShouZhiType.Trim() == "支出")
                    n.CostDisplay = "-" + Convert.ToString(n.Cost);
            });

            return new JsonResult<PagedResult<AccountRecord>>
            {
                status = true,
                Message = "获取账单列表成功",
                Data = list
            };
        }

        /// <summary>
        /// 账单记录   显示属性：ShouZhiType，LaiYuan，OperationType，CostDisplay，CreateTime
        /// </summary>
        /// <param name="ID">账单ID</param>
        /// <returns></returns>
        public async Task<JsonResult<AccountRecord>> GetAccountRecord(int ID)
        {
            AccountRecord model = await _memberService.GetAccountRecord(ID);
            if (model.ShouZhiType.Trim() == "收入")
                model.CostDisplay = "+" + Convert.ToString(model.Cost);
            else if (model.ShouZhiType.Trim() == "支出")
                model.CostDisplay = "-" + Convert.ToString(model.Cost);

            return new JsonResult<AccountRecord>
            {
                status = true,
                Message = "获取账单记录成功",
                Data = model
            };
        }

        /// <summary>
        /// 获取账户状态   待续费/正常
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public JsonResult<string> GetAccountStatus(int MemberID)
        {
            string status = string.Empty;
            bool flag = _memberService.GetAccountStatus(MemberID);
            if (flag == true)
                status = EnumExt.GetEnumCustomDescription((SocialSecurityStatusEnum)Convert.ToInt32(SocialSecurityStatusEnum.Renew));
            else
                status = EnumExt.GetEnumCustomDescription((SocialSecurityStatusEnum)Convert.ToInt32(SocialSecurityStatusEnum.Normal));

            return new JsonResult<string>
            {
                status = true,
                Message = "获取状态成功",
                Data = status
            };
        }



        /// <summary>
        /// 获取续费服务集合 只有需要续费的才能进入
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public JsonResult<List<KeyValuePair<int, decimal>>> GetRenewalServiceList(int MemberID)
        {
            //只有账户状态为待续费状态下才能进行续费服务，付费提供1-12个月的服务，1号前无需缴纳服务费，其它需按设置缴纳，不管续几个月，只缴纳一个月服务费

            //计算第一个月
            decimal TotalServiceCost = 0;
            decimal SSServiceCost = 0;
            decimal AFServiceCost = 0;

            int day = DateTime.Now.Day;
            //社保服务费
            CostParameterSetting SSParameter = _parameterSettingService.GetCostParameter((int)PayTypeEnum.SocialSecurity);
            if (SSParameter != null && !string.IsNullOrEmpty(SSParameter.RenewServiceCost))
            {
                string[] str = SSParameter.RenewServiceCost.Split(';');
                foreach (var item in str)
                {
                    string[] str1 = item.Split(',');

                    if (Convert.ToInt32(str1[0]) <= day && day <= Convert.ToInt32(str1[1]))
                    {
                        //社保续费（包括未续费待停的）的人数*服务费
                        SSServiceCost = _socialSecurityService.GetSocialSecurityRenewListByMemberID(MemberID).Count * Convert.ToDecimal(str1[2]);
                        break;
                    }
                }
            }
            //公积金服务费
            CostParameterSetting AFParameter = _parameterSettingService.GetCostParameter((int)PayTypeEnum.AccumulationFund);
            if (AFParameter != null && !string.IsNullOrEmpty(AFParameter.RenewServiceCost))
            {
                string[] str = AFParameter.RenewServiceCost.Split(';');
                foreach (var item in str)
                {
                    string[] str1 = item.Split(',');

                    if (Convert.ToInt32(str1[0]) <= day && day <= Convert.ToInt32(str1[1]))
                    {
                        //社保待续费（包括未续费待停的）人数*服务费
                        AFServiceCost = _socialSecurityService.GetAccumulationFundRenewListByMemberID(MemberID).Count * Convert.ToDecimal(str1[2]);
                        break;
                    }
                }
            }

            //不管充几个月服务，都要加上这个服务费，然后减去账户金额和待办状态金额
            decimal Renew_WaitingTopMonthTotal = _socialSecurityService.GetRenew_WaitingTopAmountByMemberID(MemberID);
            //待续费和未续费待停保社保列表
            List<SocialSecurity> Renew_WaitingTopList = DbHelper.Query<SocialSecurity>($@"select SocialSecurity.* from SocialSecurity 
  left join SocialSecurityPeople on SocialSecurity.SocialSecurityPeopleID = SocialSecurityPeople.SocialSecurityPeopleID
  where SocialSecurityPeople.MemberID = {MemberID} and (SocialSecurity.Status =4 or (SocialSecurity.Status=5 and stopmethod=1))");
            //账户信息
            AccountInfo accountInfo = _memberService.GetAccountInfo(MemberID);
            TotalServiceCost = SSServiceCost + AFServiceCost;
            //获取该用户下所有参保人的所有待办金额之和
            decimal WaitingHandleTotal = _socialSecurityService.GetWaitingHandleTotalByMemberID(MemberID);

            #region 补差费
            //每人每月补差费用
            decimal FreezingAmount = DbHelper.QuerySingle<decimal>("select FreezingAmount from CostParameterSetting where Status = 0");
            //更新订单补差费用
            string BuchaSqlStr = string.Empty;
            List<decimal> Buchalist = new List<decimal>();//12个月的补差费用

            for (int i = 1; i <= 12; i++)
            {
                decimal BuchaAmountTotal = 0;
                //遍历订单下的所有子订单
                foreach (var SocialSecurityPeopleID in Renew_WaitingTopList.Select(m => m.SocialSecurityPeopleID))
                {
                    //参保月份、参保月数、签约单位ID
                    SocialSecurity socialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select InsuranceArea,PayTime,PayMonthCount,RelationEnterprise from SocialSecurity where SocialSecurityPeopleID ={SocialSecurityPeopleID}");
                    decimal BuchaAmount = 0;
                    if (socialSecurity != null)
                    {
                        //int payMonth = socialSecurity.PayTime.Month;
                        int monthCount = socialSecurity.PayMonthCount;
                        //相对应的签约单位城市是否已调差（社平工资）
                        EnterpriseSocialSecurity enterpriseSocialSecurity = _enterpriseService.GetEnterpriseSocialSecurity(socialSecurity.RelationEnterprise);//签约公司
                        //已调,当年以后直到年末都不需交，直到一月份开始交
                        if (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year == DateTime.Now.Year)
                        {
                            int freeBuchaMonthCount = 0;//免补差月数
                            if (DateTime.Now.Day > 15)
                                freeBuchaMonthCount = 12 - DateTime.Now.Month;
                            else
                                freeBuchaMonthCount = 12 + 1 - DateTime.Now.Month;

                            //续费月数>参保人剩余月数
                            if (i > freeBuchaMonthCount && monthCount > freeBuchaMonthCount && i > monthCount)
                            {
                                BuchaAmount = (i - monthCount) * FreezingAmount;
                            }
                        }
                        //未调，往后每个月都需要交
                        if (enterpriseSocialSecurity.AdjustDt == null || (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year != DateTime.Now.Year))
                        {
                            if (i > monthCount)
                            {
                                BuchaAmount = FreezingAmount * (i - monthCount);
                            }
                        }
                    }
                    BuchaAmountTotal += BuchaAmount;
                }

                Buchalist.Add(BuchaAmountTotal);
            }
            #endregion

            Dictionary<int, decimal> dic = new Dictionary<int, decimal>();
            for (int i = 0; i < 12; i++)
            {
                dic.Add(i + 1, Renew_WaitingTopMonthTotal * (i + 1) + TotalServiceCost - (accountInfo.Account - WaitingHandleTotal) + Buchalist[i]);
            }

            return new JsonResult<List<KeyValuePair<int, decimal>>>
            {
                status = true,
                Message = "获取续费服务集合成功",
                Data = dic.ToList()
            };
        }

        /// <summary>
        /// 提交续费订单
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public JsonResult<dynamic> SubmitRenewalServiceOrder(RenewalServiceParameters parameter)
        {
            string orderCode = DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random().Next(1000).ToString().PadLeft(3, '0');
            int orderID = DbHelper.ExecuteSqlCommandScalar($@"insert into RenewOrders(OrderCode,MemberID,PaymentMethod,GenerateDate,Status,Money,MonthCount) 
                values('{orderCode}',{parameter.MemberID},'银行卡',getdate(),0,'{parameter.Amount}',{parameter.MonthCount})", new DbParameter[] { });

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "生成续费订单成功",
                Data = new { OrderID = orderID }
            };
        }

        /// <summary>
        /// 续费调取支付页面
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public JsonResult<dynamic> SubmitRenewalServiceOrderPayment(RenewalServicePayment parameter)
        {
            //判断账户是否冻结，如果冻结，则不能进行支付
            bool isFrozen = DbHelper.QuerySingle<bool>($@"select ISNULL(IsFrozen,0) from Members
  where MemberID = (select MemberID from[Order] where OrderID = {parameter.OrderID})");
            if (isFrozen)
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "该账户被冻结，不允许支付"
                };

            int orderID = parameter.OrderID;
            if (orderID > 0)
            {
                RenewOrders renewOrders = DbHelper.QuerySingle<RenewOrders>($"select * from RenewOrders where OrderID={orderID}");

                string BranchID = "0532";
                string CoNo = "019387";
                string BillNo = orderID.ToString().PadLeft(10, '0');
                string Amount = Convert.ToDecimal(renewOrders.Money).ToString();
                string Date = DateTime.Now.ToString("yyyyMMdd");
                string MerchantUrl = ConfigurationManager.AppSettings["ServerUrl"] + "api/Member/SubmitRenewalServiceOrderPayment_Return";
                if (parameter.PlatType == "1")
                {
                    #region 移动端
                    string uri = "https://netpay.cmbchina.com/netpayment/BaseHttp.dll?MfcISAPICommand=PrePayWAP&BranchID=" + BranchID + "&CoNo=" + CoNo + "&BillNo=" + BillNo + "&Amount=" + Amount + "&Date=" + Date + "&ExpireTimeSpan=30&MerchantUrl=" + MerchantUrl + "&MerchantPara=";

                    return new JsonResult<dynamic>
                    {
                        status = true,
                        Message = "提交成功",
                        Data = new { url = uri }
                    };
                    #endregion
                }
                else if (parameter.PlatType == "2")
                {
                    #region 移动端
                    string Url = $@"https://netpay.cmbchina.com/netpayment/BaseHttp.dll?PrePayC1?BranchID={BranchID}&CoNo={CoNo}&BillNo={BillNo}&Amount={Amount}&Date={Date}&ExpireTimeSpan=30&MerchantUrl=" + MerchantUrl + "&MerchantPara=";
                    //string uri = "https://netpay.cmbchina.com/netpayment/BaseHttp.dll?MfcISAPICommand=PrePayWAP&BranchID=" + BranchID + "&CoNo=" + CoNo + "&BillNo=" + BillNo + "&Amount=" + "0.01" + "&Date=" + Date + "&ExpireTimeSpan=30&MerchantUrl=" + MerchantUrl + "&MerchantPara=";

                    return new JsonResult<dynamic>
                    {
                        status = true,
                        Message = "提交成功",
                        Data = new { url = Url }
                    };
                    #endregion
                }
                else {
                    #region PC端
                    string m_Action = "https://netpay.cmbchina.com/netpayment/BaseHttp.dll?PrePayC2";//订单提交地址
                    FirmClientClass fm = new FirmClientClass();
                    string dtime = DateTime.Now.ToString("yyyyMMdd");
                    var result = fm.exGenMerchantCode("Superman19810928", dtime, BranchID, CoNo, BillNo, Convert.ToDecimal(Amount).ToString("f"), "", MerchantUrl, "1231", "1231230", "58.56.179.142", "54011600", "");
                    string responseText = "<form name='cbanksubmit' method='post' action='" + m_Action + "'>"
    + "<input type='hidden' name='BranchID' value=" + BranchID + ">"
    + "<input type='hidden' name='CoNo' value=" + CoNo + ">"
    + "<input type='hidden' name='BillNo' value=" + BillNo + ">"
    + "<input type='hidden' name='Amount' value=" + Convert.ToDecimal(Amount).ToString("f") + ">"
    + "<input type='hidden' name='Date' value=" + dtime + ">"
    + "<input type='hidden' name='MerchantUrl' value=" + MerchantUrl + ">"
    + "<input type='hidden' name='MerchantCode' value='" + result.ToString() + "'>"
    + "</form>"
    + "<script>"
    + "document.cbanksubmit.submit()"
    + "</script>";
                    return new JsonResult<dynamic>
                    {
                        status = true,
                        Message = "获取成功",
                        Data = responseText
                    };
                    #endregion
                }

            }
            else
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "提交失败"
                };
        }

        private static object locker = new object();
        /// <summary>
        /// 续费支付回调
        /// </summary>
        /// <param name="Succeed"></param>
        /// <param name="BillNo"></param>
        /// <param name="Amount"></param>
        /// <param name="Date"></param>
        /// <param name="Msg"></param>
        /// <param name="Signature"></param>
        [System.Web.Http.HttpGet]
        public void SubmitRenewalServiceOrderPayment_Return(string Succeed, string CoNo, string BillNo, string Amount, string Date, string MerchantPara, string Msg, string Signature)
        {
            string orderID = BillNo.TrimStart('0');
            RenewOrders model = DbHelper.QuerySingle<RenewOrders>($"select * from RenewOrders where OrderID='{orderID}'");
            #region 招行提供
            /*
             * 必须验证返回数据的有效性防止订单信息支付过程中被篡改
             * 先判断是否支付成功
             * 验证支付成功还要验证支付金额是否和订单的金额一致
             */
            string ReturnInfo = "Succeed=" + Succeed + "&CoNo=" + CoNo + "&BillNo=" + BillNo + "&Amount=" + Amount + "&Date=" + Date + "&MerchantPara=" + MerchantPara + "&Msg=" + Msg + "&Signature=" + Signature;
            //ReturnInfo = "Succeed=Y&BillNo=001000&Amount=0.01&Date=20160629&Msg=05320193872016062916262934500000001150&Signature=17|14|68|103|5|51|240|207|114|143|173|141|239|172|246|168|116|14|187|166|230|236|195|150|243|90|239|216|233|75|239|171|246|55|182|214|203|96|212|124|184|55|250|3|169|126|210|61|204|152|108|213|216|199|200|188|92|180|241|210|253|149|186|27|";
            StringBuilder str = new StringBuilder();
            string upLoadPath = HttpContext.Current.Server.MapPath("~/log/");
            if (!System.IO.Directory.Exists(upLoadPath))
            {
                System.IO.Directory.CreateDirectory(upLoadPath);
            }
            str.Append("\r\n" + DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"));
            str.Append("\r\n\t请求信息：" + HttpContext.Current.Request.Form);
            str.Append("\r\n\t参数：" + ReturnInfo);
            str.Append("\r\n--------------------------------------------------------------------------------------------------");


            try
            {
                string Key_Path = HttpContext.Current.Server.MapPath("~/") + @"key\public.key";//银行公用Key地址
                FirmClientClass cbmBank = new FirmClientClass();
                short key = cbmBank.exCheckInfoFromBank(Key_Path, ReturnInfo);
                str.Append("\r\n" + DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"));
                str.Append("\r\n\tkey：" + key);
                str.Append("\r\n--------------------------------------------------------------------------------------------------");

                if (key != 0)//验证银行返回数据是否合法
                {
                    string err = cbmBank.exGetLastErr(key);
                    //throw new Exception(err);
                    HttpContext.Current.Response.Write("<script>alert('" + err + "')</script>");
                    return;
                }
                if (Succeed.Trim() != "Y")//验证支付结果是否成功
                {
                    //throw new Exception("支付失败！");
                    HttpContext.Current.Response.Write("<script>alert('支付失败！')</script>");
                    return;
                }
                decimal payMoney = 0.01M;  //订单的金额也就是CMBChina_PayMoney.aspx页面中输入的金额 这里只是简单的测试实际运用中请使用实际支付值 
                if (model.Money != Convert.ToDecimal(Amount))//验证银行实际收到与支付金额是否相等
                {
                    //throw new Exception("支付金额与订单金额不一致！");
                    HttpContext.Current.Response.Write("<script>alert('支付金额与订单金额不一致！')</script>");
                    return;
                }
            }
            catch (Exception ex)
            {

                str.Append("\r\n" + DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"));
                str.Append("\r\n\t错误：" + ex);
                str.Append("\r\n--------------------------------------------------------------------------------------------------");

            }
            System.IO.File.AppendAllText(upLoadPath + DateTime.Now.ToString("yyyy.MM.dd") + ".log", str.ToString(), System.Text.Encoding.UTF8);

            #endregion

            lock (locker)
            {

                if (model.Status == "0")
                {

                    RenewalServiceParameters parameter = new RenewalServiceParameters()
                    {
                        MemberID = model.MemberID,
                        PayMethod = model.PaymentMethod,
                        Amount = model.Money,
                        MonthCount = model.MonthCount
                    };
                    SubmitRenewalService(parameter);
                    DbHelper.ExecuteSqlCommand($"update RenewOrders set status=1 where OrderID={orderID}", null);


                    HttpContext.Current.Response.StatusCode = 200;


                }
                HttpContext.Current.Response.Redirect(ConfigurationManager.AppSettings["ServerUrl2"] + $"html5/user-billIndex.html?MemberID={model.MemberID}");

            }



        }

        /// <summary>
        /// 续费
        /// </summary>
        /// <returns></returns>
        public JsonResult<dynamic> SubmitRenewalService(RenewalServiceParameters parameter)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    //获取某用户下的所有待续费(包括未续费待停)金额之和
                    decimal RenewWaitStopMonthTotal = _socialSecurityService.GetRenew_WaitingTopAmountByMemberID(parameter.MemberID);

                    //计算第一个月
                    decimal TotalServiceCost = 0;
                    decimal SSServiceCost = 0;//社保服务费
                    decimal AFServiceCost = 0;//公积金服务费
                    AccountInfo accountInfo = _memberService.GetAccountInfo(parameter.MemberID);

                    string sqlAccountRecord = "";//记录
                    sqlAccountRecord = "";
                    string ShouNote = "续费：";//收入备注
                    string ZhiNote = "";//支出备注
                    int day = DateTime.Now.Day;
                    //社保待续费（包括未续费待停保）人员列表
                    List<SocialSecurityPeople> SocialSecurityPeopleList = _socialSecurityService.GetSocialSecurityRenewListByMemberID(parameter.MemberID);
                    //收入
                    foreach (var socialSecurityPeople in SocialSecurityPeopleList)
                    {
                        ShouNote += string.Format("{0}:{1}个月社保;", socialSecurityPeople.SocialSecurityPeopleName, parameter.MonthCount);
                    }
                    //社保服务费
                    CostParameterSetting SSParameter = _parameterSettingService.GetCostParameter((int)PayTypeEnum.SocialSecurity);
                    if (SSParameter != null && !string.IsNullOrEmpty(SSParameter.RenewServiceCost))
                    {
                        string[] str = SSParameter.RenewServiceCost.Split(';');
                        foreach (var item in str)
                        {
                            string[] str1 = item.Split(',');

                            if (Convert.ToInt32(str1[0]) <= day && day <= Convert.ToInt32(str1[1]))
                            {
                                //社保待续费的人数
                                SSServiceCost = SocialSecurityPeopleList.Count * Convert.ToDecimal(str1[2]);
                                //收入
                                foreach (var socialSecurityPeople in SocialSecurityPeopleList)
                                {
                                    ShouNote += string.Format("{0}:社保服务费:{1};", socialSecurityPeople.SocialSecurityPeopleName, str1[2]);
                                    ZhiNote += string.Format("{0}:社保服务费:{1};", socialSecurityPeople.SocialSecurityPeopleName, str1[2]);
                                }
                                break;
                            }

                        }
                    }
                    //公积金人员列表
                    List<SocialSecurityPeople> SocialSecurityPeopleList1 = _socialSecurityService.GetAccumulationFundRenewListByMemberID(parameter.MemberID);
                    //收入
                    foreach (var socialSecurityPeople in SocialSecurityPeopleList1)
                    {
                        ShouNote += string.Format("{0}:{1}个月公积金;", socialSecurityPeople.SocialSecurityPeopleName, parameter.MonthCount);
                    }
                    //公积金服务费
                    CostParameterSetting AFParameter = _parameterSettingService.GetCostParameter((int)PayTypeEnum.AccumulationFund);
                    if (AFParameter != null && !string.IsNullOrEmpty(AFParameter.RenewServiceCost))
                    {
                        string[] str = AFParameter.RenewServiceCost.Split(';');
                        foreach (var item in str)
                        {
                            string[] str1 = item.Split(',');

                            if (Convert.ToInt32(str1[0]) <= day && day <= Convert.ToInt32(str1[1]))
                            {

                                //社保待续费的人数*金额
                                AFServiceCost = SocialSecurityPeopleList1.Count * Convert.ToDecimal(str1[2]);
                                //收入
                                foreach (var socialSecurityPeople in SocialSecurityPeopleList1)
                                {
                                    ShouNote += string.Format("{0}:公积金服务费:{1};", socialSecurityPeople.SocialSecurityPeopleName, str1[2]);
                                    ZhiNote += string.Format("{0}:公积金服务费:{1};", socialSecurityPeople.SocialSecurityPeopleName, str1[2]);
                                }
                                break;
                            }
                        }
                    }


                    #region 补差费
                    //待续费社保列表
                    List<SocialSecurity> RenewWaitingTopList = DbHelper.Query<SocialSecurity>($@"select SocialSecurity.*,SocialSecurityPeople.SocialSecurityPeopleName from SocialSecurity 
  left join SocialSecurityPeople on SocialSecurity.SocialSecurityPeopleID = SocialSecurityPeople.SocialSecurityPeopleID
  where SocialSecurityPeople.MemberID = {parameter.MemberID} and (SocialSecurity.Status =4  or (SocialSecurity.Status=5 and stopmethod=1))");
                    //每人每月补差费用
                    decimal FreezingAmount = DbHelper.QuerySingle<decimal>("select FreezingAmount from CostParameterSetting where Status = 0");
                    //更新订单补差费用
                    string BuchaSqlStr = string.Empty;


                    decimal BuchaAmountTotal = 0;
                    //遍历订单下的所有子订单
                    foreach (var socialSecurity1 in RenewWaitingTopList)
                    {
                        //参保月份、参保月数、签约单位ID
                        SocialSecurity socialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select InsuranceArea,PayTime,PayMonthCount,RelationEnterprise from SocialSecurity where SocialSecurityPeopleID ={socialSecurity1.SocialSecurityPeopleID}");
                        decimal BuchaAmount = 0;
                        if (socialSecurity != null)
                        {
                            //int payMonth = socialSecurity.PayTime.Month;
                            int monthCount = socialSecurity.PayMonthCount;
                            //相对应的签约单位城市是否已调差（社平工资）
                            EnterpriseSocialSecurity enterpriseSocialSecurity = _enterpriseService.GetEnterpriseSocialSecurity(socialSecurity.RelationEnterprise);//签约公司
                                                                                                                                                                  //已调,当年以后直到年末都不需交，直到一月份开始交
                            if (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year == DateTime.Now.Year)
                            {
                                int freeBuchaMonthCount = 0;//免补差月数
                                if (DateTime.Now.Day > 15)
                                    freeBuchaMonthCount = 12 - DateTime.Now.Month;
                                else
                                    freeBuchaMonthCount = 12 + 1 - DateTime.Now.Month;

                                //续费月数>参保人剩余月数
                                if (parameter.MonthCount > freeBuchaMonthCount && monthCount > freeBuchaMonthCount && parameter.MonthCount > monthCount)
                                {
                                    BuchaAmount = (parameter.MonthCount - monthCount) * FreezingAmount;
                                    ShouNote += string.Format("{0}:补差费:{1};", socialSecurity1.SocialSecurityPeopleName, BuchaAmount);
                                    ZhiNote += string.Format("{0}:补差费:{1};", socialSecurity1.SocialSecurityPeopleName, BuchaAmount);
                                }
                            }
                            //未调，往后每个月都需要交
                            if (enterpriseSocialSecurity.AdjustDt == null || (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year != DateTime.Now.Year))
                            {
                                if (parameter.MonthCount > monthCount)
                                {
                                    BuchaAmount = FreezingAmount * (parameter.MonthCount - monthCount);
                                    ShouNote += string.Format("{0}:补差费:{1};", socialSecurity1.SocialSecurityPeopleName, BuchaAmount);
                                    ZhiNote += string.Format("{0}:补差费:{1};", socialSecurity1.SocialSecurityPeopleName, BuchaAmount);
                                }
                            }
                        }
                        BuchaAmountTotal += BuchaAmount;
                    }

                    #endregion

                    //服务费总数
                    TotalServiceCost = SSServiceCost + AFServiceCost;
                    //续费人数
                    int PeopleCount = DbHelper.QuerySingle<int>($@"select count(1) from SocialSecurityPeople
  left join SocialSecurity on SocialSecurity.SocialSecurityPeopleID = SocialSecurityPeople.SocialSecurityPeopleID
  left join AccumulationFund on AccumulationFund.SocialSecurityPeopleID= SocialSecurityPeople.SocialSecurityPeopleID
  where SocialSecurityPeople.MemberID ={parameter.MemberID}
            and(SocialSecurity.Status in({(int)SocialSecurityStatusEnum.Renew}) or AccumulationFund.Status in({(int)SocialSecurityStatusEnum.Renew}))");

                    sqlAccountRecord += $@"insert into AccountRecord(SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime,PeopleCount)
values({DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random(Guid.NewGuid().GetHashCode()).Next(1000).ToString().PadLeft(3, '0')},{parameter.MemberID},'','','收入','{parameter.PayMethod}','{ShouNote}',{parameter.Amount},{accountInfo.Account + parameter.Amount},getdate(),{PeopleCount});";
                    if (!string.IsNullOrWhiteSpace(ZhiNote))
                        sqlAccountRecord += $@"insert into AccountRecord(SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime) 
values({DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random(Guid.NewGuid().GetHashCode()).Next(1000).ToString().PadLeft(3, '0')},{parameter.MemberID},'','','支出','余额','{ZhiNote}',{TotalServiceCost},{accountInfo.Account + parameter.Amount - TotalServiceCost - BuchaAmountTotal},getdate()); ";

                    //修改账户余额
                    decimal account = parameter.Amount - TotalServiceCost;
                    string sqlMember = $"update Members set Account=ISNULL(Account,0)+{account},Bucha+={BuchaAmountTotal} where MemberID={parameter.MemberID}";
                    int updateResult = DbHelper.ExecuteSqlCommand(sqlMember, null);

                    //更新记录
                    DbHelper.ExecuteSqlCommand(sqlAccountRecord, null);

                    //将所有的待续费(包括未续费待停保)变成正常,并将剩余月数变成服务月数
                    _socialSecurityService.UpdateRenew_WaitingTopToNormalByMemberID(parameter.MemberID, parameter.MonthCount);

                    #region 针对银行接口做回滚



                    #endregion

                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    return new JsonResult<dynamic>
                    {
                        status = false,
                        Message = "续费失败"
                    };
                }
                finally
                {
                    transaction.Dispose();
                }
            }

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "续费成功"
            };
        }

        /// <summary>
        /// 生成充值订单
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public JsonResult<dynamic> SubmitRechargeAmountOrder(RechargeParameters parameter)
        {
            string orderCode = DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random().Next(1000).ToString().PadLeft(3, '0');
            int orderID = DbHelper.ExecuteSqlCommandScalar($@"insert into RechargeOrders(OrderCode,MemberID,PaymentMethod,GenerateDate,Status,Money) 
                values('{orderCode}',{parameter.MemberID},'银行卡',getdate(),0,'{parameter.Amount}')", new DbParameter[] { });

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "生成充值订单成功",
                Data = new { OrderID = orderID }
            };
        }

        /// <summary>
        /// 调取支付页面
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public JsonResult<dynamic> SubmitRechargeAmountPayment(OrderPayParameter parameter)
        {
            //判断账户是否冻结，如果冻结，则不能进行支付
            bool isFrozen = DbHelper.QuerySingle<bool>($@"select ISNULL(IsFrozen,0) from Members
  where MemberID = (select MemberID from[Order] where OrderID = {parameter.OrderID})");
            if (isFrozen)
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "该账户被冻结，不允许支付"
                };

            int orderID = parameter.OrderID;
            if (orderID > 0)
            {
                RechargeOrders rechargeOrders = DbHelper.QuerySingle<RechargeOrders>($"select * from RechargeOrders where OrderID={orderID}");

                string BranchID = "0532";
                string CoNo = "019387";
                string BillNo = orderID.ToString().PadLeft(10, '0');
                string Amount = Convert.ToDecimal(rechargeOrders.Money).ToString();
                string Date = DateTime.Now.ToString("yyyyMMdd");
                string MerchantUrl = ConfigurationManager.AppSettings["ServerUrl"] + "api/Member/SubmitRechargeAmountPayment_Return";
                if (parameter.PlatType == "1")
                {
                    #region 移动端
                    string uri = "https://netpay.cmbchina.com/netpayment/BaseHttp.dll?MfcISAPICommand=PrePayWAP&BranchID=" + BranchID + "&CoNo=" + CoNo + "&BillNo=" + BillNo + "&Amount=" +  Amount + "&Date=" + Date + "&ExpireTimeSpan=30&MerchantUrl=" + MerchantUrl + "&MerchantPara=";

                    return new JsonResult<dynamic>
                    {
                        status = true,
                        Message = "提交成功",
                        Data = new { url = uri }
                    };
                    #endregion
                }
                else if (parameter.PlatType == "2")
                {
                    #region 移动端
                    string Url = $@"https://netpay.cmbchina.com/netpayment/BaseHttp.dll?PrePayC1?BranchID={BranchID}&CoNo={CoNo}&BillNo={BillNo}&Amount={Amount}&Date={Date}&ExpireTimeSpan=30&MerchantUrl=" +MerchantUrl + "&MerchantPara=";
                    //string uri = "https://netpay.cmbchina.com/netpayment/BaseHttp.dll?MfcISAPICommand=PrePayWAP&BranchID=" + BranchID + "&CoNo=" + CoNo + "&BillNo=" + BillNo + "&Amount=" + "0.01" + "&Date=" + Date + "&ExpireTimeSpan=30&MerchantUrl=" + MerchantUrl + "&MerchantPara=";

                    return new JsonResult<dynamic>
                    {
                        status = true,
                        Message = "提交成功",
                        Data = new { url = Url }
                    };
                    #endregion
                }
                else {
                    #region PC端
                    string m_Action = "https://netpay.cmbchina.com/netpayment/BaseHttp.dll?PrePayC2";//订单提交地址
                    FirmClientClass fm = new FirmClientClass();
                    string dtime = DateTime.Now.ToString("yyyyMMdd");
                    var result = fm.exGenMerchantCode("Superman19810928", dtime, BranchID, CoNo, BillNo, Convert.ToDecimal(Amount).ToString("f"), "", MerchantUrl, "1231", "1231230", "58.56.179.142", "54011600", "");
                    string responseText = "<form name='cbanksubmit' method='post' action='" + m_Action + "'>"
    + "<input type='hidden' name='BranchID' value=" + BranchID + ">"
    + "<input type='hidden' name='CoNo' value=" + CoNo + ">"
    + "<input type='hidden' name='BillNo' value=" + BillNo + ">"
    + "<input type='hidden' name='Amount' value=" + Convert.ToDecimal(Amount).ToString("f") + ">"
    + "<input type='hidden' name='Date' value=" + dtime + ">"
    + "<input type='hidden' name='MerchantUrl' value=" + MerchantUrl + ">"
    + "<input type='hidden' name='MerchantCode' value='" + result.ToString() + "'>"
    + "</form>"
    + "<script>"
    + "document.cbanksubmit.submit()"
    + "</script>";
                    return new JsonResult<dynamic>
                    {
                        status = true,
                        Message = "获取成功",
                        Data = responseText
                    };
                    #endregion
                }

            }
            else
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "提交失败"
                };
        }

        private static object locker1 = new object();
        /// <summary>
        /// 充值支付回调
        /// </summary>
        /// <param name="Succeed"></param>
        /// <param name="BillNo"></param>
        /// <param name="Amount"></param>
        /// <param name="Date"></param>
        /// <param name="Msg"></param>
        /// <param name="Signature"></param>
        [System.Web.Http.HttpGet]
        public void SubmitRechargeAmountPayment_Return(string Succeed, string CoNo, string BillNo, string Amount, string Date, string MerchantPara, string Msg, string Signature)
        {
            string orderID = BillNo.TrimStart('0');
            RechargeOrders model = DbHelper.QuerySingle<RechargeOrders>($"select * from RechargeOrders where OrderID='{orderID}'");
            #region 招行提供
            /*
             * 必须验证返回数据的有效性防止订单信息支付过程中被篡改
             * 先判断是否支付成功
             * 验证支付成功还要验证支付金额是否和订单的金额一致
             */
            string ReturnInfo = "Succeed=" + Succeed + "&CoNo=" + CoNo + "&BillNo=" + BillNo + "&Amount=" + Amount + "&Date=" + Date + "&MerchantPara=" + MerchantPara + "&Msg=" + Msg + "&Signature=" + Signature;
            //ReturnInfo = "Succeed=Y&BillNo=001000&Amount=0.01&Date=20160629&Msg=05320193872016062916262934500000001150&Signature=17|14|68|103|5|51|240|207|114|143|173|141|239|172|246|168|116|14|187|166|230|236|195|150|243|90|239|216|233|75|239|171|246|55|182|214|203|96|212|124|184|55|250|3|169|126|210|61|204|152|108|213|216|199|200|188|92|180|241|210|253|149|186|27|";
            StringBuilder str = new StringBuilder();
            string upLoadPath = HttpContext.Current.Server.MapPath("~/log/");
            if (!System.IO.Directory.Exists(upLoadPath))
            {
                System.IO.Directory.CreateDirectory(upLoadPath);
            }
            str.Append("\r\n" + DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"));
            str.Append("\r\n\t请求信息：" + HttpContext.Current.Request.Form);
            str.Append("\r\n\t参数：" + ReturnInfo);
            str.Append("\r\n--------------------------------------------------------------------------------------------------");


            try
            {
                string Key_Path = HttpContext.Current.Server.MapPath("~/") + @"key\public.key";//银行公用Key地址
                FirmClientClass cbmBank = new FirmClientClass();
                short key = cbmBank.exCheckInfoFromBank(Key_Path, ReturnInfo);
                str.Append("\r\n" + DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"));
                str.Append("\r\n\tkey：" + key);
                str.Append("\r\n--------------------------------------------------------------------------------------------------");

                if (key != 0)//验证银行返回数据是否合法
                {
                    string err = cbmBank.exGetLastErr(key);
                    //throw new Exception(err);
                    HttpContext.Current.Response.Write("<script>alert('" + err + "')</script>");
                    return;
                }
                if (Succeed.Trim() != "Y")//验证支付结果是否成功
                {
                    //throw new Exception("支付失败！");
                    HttpContext.Current.Response.Write("<script>alert('支付失败！')</script>");
                    return;
                }
                decimal payMoney = 0.01M;  //订单的金额也就是CMBChina_PayMoney.aspx页面中输入的金额 这里只是简单的测试实际运用中请使用实际支付值 
                if (model.Money != Convert.ToDecimal(Amount))//验证银行实际收到与支付金额是否相等
                {
                    //throw new Exception("支付金额与订单金额不一致！");
                    HttpContext.Current.Response.Write("<script>alert('支付金额与订单金额不一致！')</script>");
                    return;
                }
            }
            catch (Exception ex)
            {

                str.Append("\r\n" + DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"));
                str.Append("\r\n\t错误：" + ex);
                str.Append("\r\n--------------------------------------------------------------------------------------------------");

            }
            System.IO.File.AppendAllText(upLoadPath + DateTime.Now.ToString("yyyy.MM.dd") + ".log", str.ToString(), System.Text.Encoding.UTF8);

            #endregion
            lock (locker)
            {


                if (model.Status == "0")
                {
                    RechargeParameters parameter = new RechargeParameters()
                    {
                        MemberID = model.MemberID,
                        PayMethod = model.PaymentMethod,
                        Amount = model.Money
                    };
                    SubmitRechargeAmount(parameter);
                    DbHelper.ExecuteSqlCommand($"update RechargeOrders set status=1 where OrderID={orderID}", null);

                    HttpContext.Current.Response.StatusCode = 200;
                }

                string nextUrl = ConfigurationManager.AppSettings["ServerUrl2"] + $"html5/user-billIndex.html?MemberID={model.MemberID}";
                //跳转页面
                HttpContext.Current.Response.Redirect(nextUrl);

                //返回Html
                //HttpClient httpClient = new HttpClient();
                //string str_html = httpClient.GetStringAsync(nextUrl).Result;

                //return nextUrl;

            }
        }

        /// <summary>
        /// 提交充值
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public JsonResult<dynamic> SubmitRechargeAmount(RechargeParameters parameter)
        {
            //如果账户状态为正常，则随便充值；如果账户状态为待续费，则看充值时间段，1)充值金额 >=待续费金额+服务费,2)充值金额 <待续费金额+服务费
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {

                    AccountInfo info = _memberService.GetAccountInfo(parameter.MemberID);
                    //检查账户状态
                    if (!_socialSecurityService.IsExistsRenew_WaitingTop(parameter.MemberID))
                    {
                        //账户记录
                        DbHelper.ExecuteSqlCommand($@"insert into AccountRecord(SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime)
                                                values({DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random(Guid.NewGuid().GetHashCode()).Next(1000).ToString().PadLeft(3, '0')},{parameter.MemberID},'','','收入','{parameter.PayMethod}','充值',{parameter.Amount},{info.Account + parameter.Amount},getdate())", null);
                        //修改账户余额
                        DbHelper.ExecuteSqlCommand($@"update Members set Account = ISNULL(Account, 0) +{ parameter.Amount}
                                                where MemberID = { parameter.MemberID }", null);
                    }
                    else {
                        //计算第一个月
                        decimal TotalServiceCost = 0;
                        decimal SSServiceCost = 0;//社保服务费
                        decimal AFServiceCost = 0;//公积金服务费

                        string sqlAccountRecord = "";//记录
                        string ShouNote = "充值：";//收入备注
                        string ZhiNote = "";//支出备注
                        int day = DateTime.Now.Day;
                        //社保待续费(包括未续费待停保的)人员列表
                        List<SocialSecurityPeople> SocialSecurityPeopleList = _socialSecurityService.GetSocialSecurityRenewListByMemberID(parameter.MemberID);

                        //社保服务费
                        CostParameterSetting SSParameter = _parameterSettingService.GetCostParameter((int)PayTypeEnum.SocialSecurity);
                        if (SSParameter != null && !string.IsNullOrEmpty(SSParameter.RenewServiceCost))
                        {
                            string[] str = SSParameter.RenewServiceCost.Split(';');
                            foreach (var item in str)
                            {
                                string[] str1 = item.Split(',');

                                if (Convert.ToInt32(str1[0]) <= day && day <= Convert.ToInt32(str1[1]))
                                {
                                    //社保待续费的人数
                                    SSServiceCost = SocialSecurityPeopleList.Count * Convert.ToDecimal(str1[2]);

                                    break;
                                }

                            }
                        }
                        //公积金人员（包括未续费待停保）列表
                        List<SocialSecurityPeople> SocialSecurityPeopleList1 = _socialSecurityService.GetAccumulationFundRenewListByMemberID(parameter.MemberID);

                        //公积金服务费
                        CostParameterSetting AFParameter = _parameterSettingService.GetCostParameter((int)PayTypeEnum.AccumulationFund);
                        if (AFParameter != null && !string.IsNullOrEmpty(AFParameter.RenewServiceCost))
                        {
                            string[] str = AFParameter.RenewServiceCost.Split(';');
                            foreach (var item in str)
                            {
                                string[] str1 = item.Split(',');

                                if (Convert.ToInt32(str1[0]) <= day && day <= Convert.ToInt32(str1[1]))
                                {

                                    //社保待续费的人数*金额
                                    AFServiceCost = SocialSecurityPeopleList1.Count * Convert.ToDecimal(str1[2]);

                                    break;
                                }
                            }
                        }

                        //服务费总数
                        TotalServiceCost = SSServiceCost + AFServiceCost;

                        //获取某用户下的所有待续费(包括未续费代办停)金额之和
                        decimal RenewMonthTotal = _socialSecurityService.GetRenew_WaitingTopAmountByMemberID(parameter.MemberID);
                        //获取该用户下所有参保人的所有待办金额之和
                        decimal WaitingHandleTotal = _socialSecurityService.GetWaitingHandleTotalByMemberID(parameter.MemberID);



                        if (parameter.Amount >= RenewMonthTotal - (info.Account - WaitingHandleTotal) + TotalServiceCost)
                        {
                            //交服务费
                            //账户记录
                            DbHelper.ExecuteSqlCommand($@"insert into AccountRecord(SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime)
                                                values({DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random(Guid.NewGuid().GetHashCode()).Next(1000).ToString().PadLeft(3, '0')},{parameter.MemberID},'','','收入','{parameter.PayMethod}','充值',{parameter.Amount},{info.Account + parameter.Amount},getdate());
                                                        insert into AccountRecord(SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime) 
                                                 values({DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random(Guid.NewGuid().GetHashCode()).Next(1000).ToString().PadLeft(3, '0')},{parameter.MemberID},'','','支出','余额','服务费',{TotalServiceCost},{info.Account + parameter.Amount - TotalServiceCost},getdate()); ", null);
                            //修改账户余额
                            DbHelper.ExecuteSqlCommand($@"update Members set Account = ISNULL(Account, 0) +{ parameter.Amount - TotalServiceCost}
                                                where MemberID = { parameter.MemberID }", null);

                            //将所有的待续费（包括未续费代办停）变成正常,并将剩余月数变成服务月数
                            _socialSecurityService.UpdateRenew_WaitingTopToNormalByMemberID(parameter.MemberID, 1);
                        }
                        else {
                            //不交服务费
                            //账户记录
                            DbHelper.ExecuteSqlCommand($@"insert into AccountRecord(SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime)
                                                values({DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random(Guid.NewGuid().GetHashCode()).Next(1000).ToString().PadLeft(3, '0')},{parameter.MemberID},'','','收入','{parameter.PayMethod}','充值',{parameter.Amount},{info.Account + parameter.Amount},getdate())", null);
                            //修改账户余额
                            DbHelper.ExecuteSqlCommand($@"update Members set Account = ISNULL(Account, 0) +{ parameter.Amount}
                                                where MemberID = { parameter.MemberID }", null);
                        }


                    }
                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    return new JsonResult<dynamic>
                    {
                        status = false,
                        Message = "充值失败"
                    };
                }
                finally
                {
                    transaction.Dispose();
                }
            }

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "充值成功"
            };
        }




        /// <summary>
        /// 检测账户是否已冻结  提现，续费，充值，基数调整，支付的按钮都需要加这个判断
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<dynamic> CheckAccountIsFrozen(int MemberID)
        {
            bool IsFrozen = DbHelper.QuerySingle<bool>($"select ISNULL(IsFrozen,0) from Members where MemberID={MemberID}");
            if (IsFrozen == true)
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "不可以进行此操作"
                };
            else
                return new JsonResult<dynamic>
                {
                    status = true,
                    Message = "可以操作"
                };
        }

        /// <summary>
        /// 获取提现信息
        /// </summary>
        /// <param name="memberID"></param>
        /// <returns></returns>

        [System.Web.Http.HttpGet]
        public JsonResult<dynamic> DrawCash(int memberID)
        {
            Members member = DbHelper.QuerySingle<Members>($"select * from Members where MemberID={memberID}");
            decimal DongJie = DbHelper.QuerySingle<decimal>($"select ISNULL(SUM(Money),0) from DrawCash where MemberID={memberID}  and ApplyStatus=1");
            return new JsonResult<dynamic>
            {
                status = true,
                Message = "获取成功",
                Data = new
                {
                    Account = member.Account,
                    DongJie = DongJie
                }
            };
        }

        /// <summary>
        /// 提现审核   提交信息：MemberID、Money、LeftAccount
        /// </summary>
        /// <param name="drawCash"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JsonResult<dynamic> DrawCash(DrawCash drawCash)
        {
            //检测提现金额不能大于可提取余额
            decimal Account = DbHelper.QuerySingle<decimal>($"select  ISNULL(Account,0) from Members where MemberID={drawCash.MemberID}");
            decimal DongJie = DbHelper.QuerySingle<decimal>($"select ISNULL(SUM(Money),0) from DrawCash where MemberID={drawCash.MemberID} and ApplyStatus=1");
            if (drawCash.Money > Account - DongJie)
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "提现金额不能大于可提取余额"
                };

            DbHelper.ExecuteSqlCommand($"insert into DrawCash(MemberId,Money,ApplyTime,ApplyStatus,LeftAccount) values({drawCash.MemberID},'{drawCash.Money}','{DateTime.Now}',1,'{drawCash.LeftAccount}')", null);
            return new JsonResult<dynamic>
            {
                status = true,
                Message = "审核中"
            };
        }

        /// <summary>
        /// 是否已经绑定账户   看data数据  
        /// </summary>
        /// <param name="memberID"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<dynamic> IsBoundAccount(int memberID)
        {
            bool isBound;
            //获取注册人银行账号 BankCardNo
            string BankCardNo = DbHelper.QuerySingle<string>($"select BankCardNo from Members where MemberId={memberID}");
            if (!string.IsNullOrWhiteSpace(BankCardNo))
                isBound = true;
            else
                isBound = false;
            return new JsonResult<dynamic>
            {
                status = true,
                Message = "获取成功",
                Data = isBound
            };
        }

        /// <summary>
        /// 绑定账户
        /// </summary>
        /// <param name="boundAccount"></param>
        /// <returns></returns>
        public JsonResult<dynamic> BoundAccount(BoundAccount boundAccount)
        {
            DbHelper.ExecuteSqlCommand($"update Members set BankCardNo='{boundAccount.BankCardNo}' where MemberID={boundAccount.MemberID}", null);
            return new JsonResult<dynamic>
            {
                status = true,
                Message = "绑定成功"
            };
        }


        /// <summary>
        /// 获取冻结金额说明
        /// </summary>
        /// <returns></returns>
        public JsonResult<dynamic> GetFreezingAmountInstruction()
        {
            string note = _memberService.GetFreezingAmountInstruction();
            return new JsonResult<dynamic>
            {
                status = true,
                Message = "获取成功",
                Data = note
            };
        }

        /// <summary>
        /// 保存头像
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JsonResult<dynamic> SaveHeadPortrait(HeadPortraitParameters parameter)
        {
            bool flag = _memberService.SaveHeadPortrait(parameter.MemberID, parameter.HeadPortrait);
            return new JsonResult<dynamic>
            {
                status = flag,
                Message = flag ? "上传成功" : "上传失败"
            };

        }

        [System.Web.Http.HttpGet]
        private JsonResult<dynamic> TestTransaction()
        {
            string url = "http://localhost:47565/api/member/GetRenewalServiceList?MemberID";

            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();

            //Stream receiveStream = myHttpWebResponse.GetResponseStream();//得到回写的字节流
            StreamReader stream = new StreamReader(myHttpWebResponse.GetResponseStream(), Encoding.UTF8);
            return new JsonResult<dynamic>
            {
                status = true,
                Message = "测试成功",
                Data = stream.ReadToEnd()
            };
        }


        #region 缴费明细

        /// <summary>
        /// 根据用户ID获取参保人列表
        /// </summary>
        /// <param name="memberID"></param>
        /// <returns></returns>
        public JsonResult<dynamic> GetSocialSecurityPeopleList(int memberID, string socialSecurityPeopleName)
        {
            string sqlstr = $"select * from SocialSecurityPeople where MemberID={memberID} and SocialSecurityPeopleName like '%{socialSecurityPeopleName}%'";
            List<SocialSecurityPeople> list = DbHelper.Query<SocialSecurityPeople>(sqlstr);

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "获取成功",
                Data = list.SelectMany(item =>
                {
                    List<dynamic> newList = new List<dynamic>();
                    newList.Add(new
                    {
                        SocialSecurityPeopleID = item.SocialSecurityPeopleID,
                        IdentityCard = item.IdentityCard,
                        SocialSecurityPeopleName = item.SocialSecurityPeopleName
                    });
                    return newList;
                })
            };
        }

        /// <summary>
        /// 获取缴费列表  type:0/社保,1/公积金
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public JsonResult<dynamic> GetPaymentList(int socialSecurityPeopleID, int year, int type = 0)
        {
            //年份列表

            List<int> dateList = new List<int> { DateTime.Now.Year }.Concat(DbHelper.Query<DateTime>($"select CreateTime from AccountRecord where SocialSecurityPeopleID ={socialSecurityPeopleID} and Type={type}").Select(date => date.Year).ToList()).Distinct().ToList();
            dateList.Sort();

            //流水记录
            List<AccountRecord> accountRecordList = DbHelper.Query<AccountRecord>($"select * from AccountRecord where SocialSecurityPeopleID={socialSecurityPeopleID} and Type ={type} and YEAR(CreateTime) = {year}");

            //组织查询列表
            List<dynamic> payList = new List<dynamic>();
            for (int i = 1; i <= 12; i++)
            {
                bool flag = false;
                foreach (var accountRecord in accountRecordList)
                {
                    if (accountRecord.CreateTime.Month == i)
                    {
                        flag = true;
                        payList.Add(new { PayMonth = i + "月", Cost = accountRecord.Cost, IsPay = "已购买" });
                        break;
                    }

                }

                if (flag == false)
                    payList.Add(new { PayMonth = i + "月", Cost = 0, IsPay = "未购买" });

            }

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "获取成功",
                Data = new
                {
                    dateList = dateList,
                    payList = payList
                }
            };
        }

        #endregion


        /// <summary>
        /// 获取消息列表
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<List<Message>> GetMessageList(int memberID)
        {
            List<Message> messageList = DbHelper.Query<Message>($"select * from Message where MemberID={memberID} order by Dt desc");
            return new JsonResult<List<Message>>
            {
                status = true,
                Message = "获取成功",
                Data = messageList
            };
        }

        ///// <summary>
        ///// 获取多个错误
        ///// </summary>
        ///// <returns></returns>
        //private string AllModelStateErrors()
        //{
        //    List<string> sb = new List<string>();
        //    //获取所有错误的Key
        //    List<string> Keys = ModelState.Keys.ToList();
        //    //获取每一个key对应的ModelStateDictionary
        //    foreach (var key in Keys)
        //    {
        //        var errors = ModelState[key].Errors.ToList();
        //        //将错误描述添加到sb中
        //        foreach (var error in errors)
        //        {
        //            sb.Add(error.ErrorMessage);
        //        }
        //    }
        //    return string.Join("\n", sb);
        //}

        [System.Web.Http.HttpGet]
        public void Test()
        {
            try
            {


                //HttpContext.Current.Response.Status = "200";
                HttpContext.Current.Response.StatusCode = 200;
                HttpContext.Current.Response.Redirect(ConfigurationManager.AppSettings["ServerUrl2"] + "html5/user-billIndex.html");
            }
            catch (Exception ex)
            {
                throw;
            }


        }

        #region 银行接口对接
        #region 用户查询
        /// <summary>
        /// 根据手机号获取续费服务列表
        /// </summary>
        /// <param name="memberPhone"></param>
        [System.Web.Http.HttpGet]
        public MemberQueryReturn GetJiaoFeiMemberQuery(string businessCode, string bankCode, string memberPhone)
        {
            MemberQueryReturn memberQueryReturn = new MemberQueryReturn()
            {
                BusinessCode = businessCode,
                BankCode = bankCode,
                MemberPhone = memberPhone
            };
            try
            {
                int memberId = DbHelper.QuerySingle<int>($"select MemberID from Members where MemberPhone='{memberPhone}'");
                if (memberId == 0)
                    //无此用户编号
                    memberQueryReturn.ReturnCode = "0002";
                else {
                    memberQueryReturn.TrueName = DbHelper.QuerySingle<string>($"select TrueName from Members where MemberPhone='{memberPhone}'");
                    //检验是否需要续费
                    bool flag = _memberService.GetAccountStatus(memberId);
                    if (flag == false)
                        memberQueryReturn.ReturnCode = "0003";
                    else {
                        memberQueryReturn.ReturnCode = "0000";
                        foreach (var item in GetRenewalServiceList(memberId).Data)
                        {
                            memberQueryReturn.MoneyArray.Add(item.Value);
                        }
                    }
                }

                return memberQueryReturn;
            }
            catch (Exception)
            {
                memberQueryReturn.ReturnCode = "0001";
                return memberQueryReturn;
            }
        }

        #endregion

        #region 缴费
        /// <summary>
        /// 提交缴费,需要记录日志
        /// </summary>
        /// <param name="jiaoFeiSubmit"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JiaoFeiSubmitReturn PostJiaoFeiSubmit(JiaoFeiSubmit jiaoFeiSubmit)
        {
            JiaoFeiSubmitReturn jiaoFeiSubmitReturn = new JiaoFeiSubmitReturn()
            {
                BusinessCode = jiaoFeiSubmit.BusinessCode,
                BankCode = jiaoFeiSubmit.BankCode,
                MemberPhone = jiaoFeiSubmit.MemberPhone,
                SerialNumber = jiaoFeiSubmit.SerialNumber
            };

            try
            {
                RenewalServiceParameters parameter = new RenewalServiceParameters()
                {
                    MemberID = DbHelper.QuerySingle<int>($"select MemberID from Members where MemberPhone='{jiaoFeiSubmit.MemberPhone}'"),
                    PayMethod = "招行支付",
                    Amount = jiaoFeiSubmit.Money,
                    MonthCount = jiaoFeiSubmit.MonthCount
                };

                if (SubmitRenewalService(parameter).status)
                {
                    jiaoFeiSubmitReturn.ReturnCode = "0000";
                    DbHelper.ExecuteSqlCommand($"insert into JiaoFei_ZS(BusinessCode,BankCode,MemberPhone,Money,MonthCount,BusinessDate,BusinessTime,SerialNumber) values('{jiaoFeiSubmit.BusinessCode}','{jiaoFeiSubmit.BankCode}','{jiaoFeiSubmit.MemberPhone}','{jiaoFeiSubmit.Money}','{jiaoFeiSubmit.MonthCount}','{jiaoFeiSubmit.BusinessDate}','{jiaoFeiSubmit.BusinessTime}','{jiaoFeiSubmit.SerialNumber}')", null);
                }
                else {
                    jiaoFeiSubmitReturn.ReturnCode = "0001";
                }

                return jiaoFeiSubmitReturn;
            }
            catch (Exception)
            {
                jiaoFeiSubmitReturn.ReturnCode = "0001";
                return jiaoFeiSubmitReturn;
            }
        }

        #endregion

        #region 冲正
        /// <summary>
        /// 冲正请求
        /// </summary>
        /// <param name="chongZheng"></param>
        /// <returns></returns>
        public ChongZhengReturn PostChongZheng(ChongZheng chongZheng)
        {
            ChongZhengReturn chongZhengReturn = new ChongZhengReturn()
            {
                BusinessCode = chongZheng.BusinessCode,
                BankCode = chongZheng.BankCode,
                MemberPhone = chongZheng.MemberPhone,
                SerialNumber = chongZheng.SerialNumber
            };
            try
            {
                DbHelper.ExecuteSqlCommand($"update JiaoFei_ZS set Status = '1003' where SerialNumber='{chongZheng.SerialNumber}'", null);
                //缴费回滚


                chongZhengReturn.ReturnCode = "0000";
                return chongZhengReturn;
            }
            catch (Exception)
            {
                chongZhengReturn.ReturnCode = "0001";
                return chongZhengReturn;
            }



        }
        #endregion

        #region 日终对账
        public RiZhongDuiZhangReturn PostRiZhongDuiZhang(RiZhongDuiZhang riZhongDuiZhang)
        {
            RiZhongDuiZhangReturn chongZhengReturn = new RiZhongDuiZhangReturn()
            {
                BusinessCode = riZhongDuiZhang.BusinessCode,
                BankCode = riZhongDuiZhang.BankCode
            };

            try
            {
                List<FileContent> fileContentList = new List<FileContent>();
                List<FileContent> fileErrorList = new List<FileContent>();
                //查出招商银行当天所有的缴费记录
                List<JiaoFei_ZS> jiaoFei_ZSList = DbHelper.Query<JiaoFei_ZS>($"select * from JiaoFei_ZS where BankCode ='{riZhongDuiZhang.BankCode}' and BusinessDate='{riZhongDuiZhang.BusinessDate}'");
                #region 根据文件名从指定目录中读取文件内容到对象集合中

                //读取文件
                //string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"FileUpload\{riZhongDuiZhang.FileName.Trim()}");
                StreamReader sr = new StreamReader(riZhongDuiZhang.FullFilename, Encoding.GetEncoding("GBK"));
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    string recordLine = line.ToString();
                    FileContent fileContent = new FileContent()
                    {
                        MemberPhone = recordLine.Substring(0, 11),
                        Money = Convert.ToDecimal(recordLine.Substring(11, 10)),
                        MonthCount = Convert.ToInt32(recordLine.Substring(21, 2)),
                        BusinessDate = recordLine.Substring(23, 8),
                        BusinessTime = recordLine.Substring(31, 8),
                        SerialNumber = recordLine.Substring(39, 16),
                        BankCode = recordLine.Substring(55, 12)
                    };

                    fileContentList.Add(fileContent);
                }


                foreach (var fileContent in fileContentList)
                {
                    bool flag = false;
                    foreach (var jiaoFei_ZS in jiaoFei_ZSList)
                    {
                        if (fileContent.SerialNumber == jiaoFei_ZS.SerialNumber)
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (flag == false)
                    {
                        fileErrorList.Add(fileContent);
                    }
                }

                bool isSuccess = true;
                foreach (var fileError in fileErrorList)
                {
                    //针对未记录的重新缴纳一遍
                    JiaoFeiSubmit jiaoFeiSubmit = new JiaoFeiSubmit()
                    {
                        BusinessCode = "1004",
                        BankCode = fileError.BankCode,
                        MemberPhone = fileError.MemberPhone,
                        Money = fileError.Money,
                        MonthCount = fileError.MonthCount,
                        BusinessDate = fileError.BusinessDate,
                        BusinessTime = fileError.BusinessTime,
                        SerialNumber = fileError.SerialNumber
                    };
                    JiaoFeiSubmitReturn jiaoFeiSubmitReturn = PostJiaoFeiSubmit(jiaoFeiSubmit);
                    if (jiaoFeiSubmitReturn.ReturnCode == "0001")
                    {
                        isSuccess = false;
                        break;
                    }

                }

                #endregion
                if (isSuccess)
                    chongZhengReturn.ReturnCode = "0000";
                else
                    chongZhengReturn.ReturnCode = "0001";

                return chongZhengReturn;
            }
            catch (Exception ex)
            {
                chongZhengReturn.ReturnCode = "0001";
                return chongZhengReturn;
            }
        }

        #endregion

        #endregion
    }
}