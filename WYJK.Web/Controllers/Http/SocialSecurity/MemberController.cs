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

namespace WYJK.Web.Controllers.Http
{
    /// <summary>
    /// 用户接口
    /// </summary>
    public class MemberController : ApiController
    {
        private readonly IMemberService _memberService = new MemberService();
        private readonly ISocialSecurityService _socialSecurityService = new SocialSecurityService();
        private readonly IParameterSettingService _parameterSettingService = new ParameterSettingService();

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public async Task<JsonResult<MemberRegisterModel>> RegisterMember(MemberRegisterModel entity)
        {
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
            Dictionary<bool, string> dic = await _memberService.ForgetPassword(entity);

            return new JsonResult<MemberForgetPasswordModel>
            {
                status = dic.First().Key,
                Message = dic.First().Value,
                Data = entity
            };
        }

        /// <summary>
        /// 获取用户信息 IsAuthentication:是否认证 0/未认证 1/已认证，IsComplete：是否信息补全 0/未补全 1/已补全,UserType 用户类型  0：普通用户、1：企业用户、2：个体用户
        /// </summary>
        /// <param name="MemberID">MemberID</param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public async Task<JsonResult<dynamic>> GetMemberInfo(int MemberID)
        {
            Members member = await _memberService.GetMemberInfo(MemberID);
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
        public JsonResult<dynamic> SubmitMemberInfo(int MemberID, string TrueName, string MemberName, string MemberPhone)
        {
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
            bool flag = await _memberService.CommitEnterpriseCertification(parameter);

            ////验证身份证
            //if (!Regex.IsMatch(parameter.EnterpriseLegalIdentityCardNo, @"(^\d{18}$)|(^\d{15}$)"))
            //    return new JsonResult<dynamic>
            //    {
            //        status = false,
            //        Message = "身份证号填写错误"
            //    };

            return new JsonResult<dynamic>
            {
                status = flag,
                Message = flag ? "企业认证成功" : "企业认证失败"
            };
        }

        /// <summary>
        /// 个体认证
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult<dynamic>> CommitPersonCertification(IndividualCertification parameter)
        {
            bool flag = await _memberService.CommitPersonCertification(parameter);

            return new JsonResult<dynamic>
            {
                status = flag,
                Message = flag ? "个体认证成功" : "个体认证失败"
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
            accountInfo.HeadPortrait = ConfigurationManager.AppSettings["ServerUrl"] + accountInfo.HeadPortrait;
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
                        //社保续费的人数*服务费
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
                        //社保待续费任务*服务费
                        AFServiceCost = _socialSecurityService.GetAccumulationFundRenewListByMemberID(MemberID).Count * Convert.ToDecimal(str1[2]);
                        break;
                    }

                }
            }

            //不管充几个月服务，都要加上这个服务费，然后减去账户金额
            decimal RenewMonthTotal = _socialSecurityService.GetRenewAmountByMemberID(MemberID);
            AccountInfo accountInfo = _memberService.GetAccountInfo(MemberID);
            TotalServiceCost = SSServiceCost + AFServiceCost;
            //获取该用户下所有参保人的所有待办金额之和
            decimal WaitingHandleTotal = _socialSecurityService.GetWaitingHandleTotalByMemberID(MemberID);

            Dictionary<int, decimal> dic = new Dictionary<int, decimal>();
            for (int i = 0; i < 12; i++)
            {
                dic.Add(i + 1, RenewMonthTotal * (i + 1) + TotalServiceCost - accountInfo.Account - WaitingHandleTotal);
            }

            return new JsonResult<List<KeyValuePair<int, decimal>>>
            {
                status = true,
                Message = "获取续费服务集合成功",
                Data = dic.ToList()
            };

            #region 作废
            ////所有服务月-账户余额，如果<0则去除
            //for (int i = 0; i < 12; i++)
            //{
            //    dic[i + 1] = dic[i + 1] - accountInfo.Account;
            //    if (dic[i + 1] <= 0)
            //        dic.Remove(i + 1);
            //}


            ////1号前无需服务费；1号后看账户余额，如果够缴纳一个月，则不需要交服务费，否则需交所有人一个月服务费(同下)
            ////账户余额够一个月，则无需交服务费，如果不够，则看1号前还是1号后，前不需要交，后需要交
            ////一个月服务
            //decimal MonthTotal = _socialSecurityService.GetMonthTotalAmountByMemberID(MemberID);
            //Dictionary<int, decimal> dic = new Dictionary<int, decimal>();
            //for (int i = 0; i < 12; i++)
            //{
            //    dic.Add(i + 1, MonthTotal * (i + 1));
            //}
            ////计算第一个月
            //decimal TotalServiceCost = 0;
            //decimal SSServiceCost = 0;
            //decimal AFServiceCost = 0;

            //AccountInfo accountInfo = _memberService.GetAccountInfo(MemberID);
            //if (accountInfo.Account < MonthTotal)
            //{
            //    int day = DateTime.Now.Day;
            //    //社保服务费
            //    CostParameterSetting SSParameter = _parameterSettingService.GetCostParameter((int)PayTypeEnum.SocialSecurity);
            //    if (SSParameter != null && !string.IsNullOrEmpty(SSParameter.RenewServiceCost))
            //    {
            //        string[] str = SSParameter.RenewServiceCost.Split(';');
            //        foreach (var item in str)
            //        {
            //            string[] str1 = item.Split(',');

            //            if (Convert.ToInt32(str1[0]) <= day && day <= Convert.ToInt32(str1[1]))
            //            {
            //                //社保待办与正常与续费的人数
            //                SSServiceCost = _socialSecurityService.GetSocialSecurityRenewListByMemberID(MemberID).Count * Convert.ToDecimal(str1[2]);
            //                break;
            //            }

            //        }
            //    }
            //    //公积金服务费
            //    CostParameterSetting AFParameter = _parameterSettingService.GetCostParameter((int)PayTypeEnum.AccumulationFund);
            //    if (AFParameter != null && !string.IsNullOrEmpty(AFParameter.RenewServiceCost))
            //    {
            //        string[] str = AFParameter.RenewServiceCost.Split(';');
            //        foreach (var item in str)
            //        {
            //            string[] str1 = item.Split(',');

            //            if (Convert.ToInt32(str1[0]) <= day && day <= Convert.ToInt32(str1[1]))
            //            {
            //                //社保待办与正常的人数
            //                AFServiceCost = _socialSecurityService.GetAccumulationFundRenewListByMemberID(MemberID).Count * Convert.ToDecimal(str1[2]);
            //                break;
            //            }

            //        }
            //    }

            //    //小于一个月的（现在处于小于一个月内），不管充几个月服务，都要加上这个服务费，然后减去账户金额
            //    TotalServiceCost = SSServiceCost + AFServiceCost;
            //    for (int i = 0; i < 12; i++)
            //    {
            //        dic[i + 1] = dic[i + 1] + TotalServiceCost;
            //    }
            //}

            ////所有服务月-账户余额，如果<0则去除
            //for (int i = 0; i < 12; i++)
            //{
            //    dic[i + 1] = dic[i + 1] - accountInfo.Account;
            //    if (dic[i + 1] <= 0)
            //        dic.Remove(i + 1);
            //}

            //return new JsonResult<List<KeyValuePair<int, decimal>>>
            //{
            //    status = true,
            //    Message = "获取集合成功",
            //    Data = dic.ToList()
            //};
            #endregion
        }

        ///// <summary>
        ///// 创建续费服务
        ///// </summary>
        ///// <param name="MemberID"></param>
        ///// <returns></returns>
        //private JsonResult<dynamic> CreateRenewalService(int MemberID)
        //{
        //    return new JsonResult<dynamic>
        //    {
        //        status = true,
        //        Message = "获取状态成功"
        //    };
        //}

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
                    //获取某用户下的所有待续费金额之和
                    decimal RenewMonthTotal = _socialSecurityService.GetRenewAmountByMemberID(parameter.MemberID);

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
                    //社保待续费人员列表
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
                    //服务费总数
                    TotalServiceCost = SSServiceCost + AFServiceCost;


                    sqlAccountRecord += $@"insert into AccountRecord(SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime)
values({DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random(Guid.NewGuid().GetHashCode()).Next(1000).ToString().PadLeft(3, '0')},{parameter.MemberID},'','','收入','{parameter.PayMethod}','{ShouNote}',{parameter.Amount},{accountInfo.Account + parameter.Amount},getdate());
                                       insert into AccountRecord(SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime) 
values({DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random(Guid.NewGuid().GetHashCode()).Next(1000).ToString().PadLeft(3, '0')},{parameter.MemberID},'','','支出','余额','{ZhiNote}',{TotalServiceCost},{accountInfo.Account + parameter.Amount - TotalServiceCost},getdate()); ";


                    //修改账户余额
                    decimal account = parameter.Amount - TotalServiceCost;
                    string sqlMember = $"update Members set Account=ISNULL(Account,0)+{account} where MemberID={parameter.MemberID}";
                    int updateResult = DbHelper.ExecuteSqlCommand(sqlMember, null);

                    //更新记录
                    DbHelper.ExecuteSqlCommand(sqlAccountRecord, null);

                    //将所有的待续费变成正常,并将剩余月数变成服务月数
                    _socialSecurityService.UpdateRenewToNormalByMemberID(parameter.MemberID, parameter.MonthCount);

                    #region 作废
                    //                    decimal MonthTotal = _socialSecurityService.GetMonthTotalAmountByMemberID(parameter.MemberID);
                    //                    //计算第一个月
                    //                    decimal TotalServiceCost = 0;
                    //                    decimal SSServiceCost = 0;//社保服务费
                    //                    decimal AFServiceCost = 0;//公积金服务费
                    //                    AccountInfo accountInfo = _memberService.GetAccountInfo(parameter.MemberID);

                    //                    string sqlAccountRecord = "";//记录
                    //                    if (accountInfo.Account < MonthTotal)
                    //                    {
                    //                        int day = DateTime.Now.Day;
                    //                        //社保服务费
                    //                        CostParameterSetting SSParameter = _parameterSettingService.GetCostParameter((int)PayTypeEnum.SocialSecurity);
                    //                        if (SSParameter != null && !string.IsNullOrEmpty(SSParameter.RenewServiceCost))
                    //                        {
                    //                            string[] str = SSParameter.RenewServiceCost.Split(';');
                    //                            foreach (var item in str)
                    //                            {
                    //                                string[] str1 = item.Split(',');

                    //                                if (Convert.ToInt32(str1[0]) <= day && day <= Convert.ToInt32(str1[1]))
                    //                                {
                    //                                    List<SocialSecurityPeople> SocialSecurityPeopleList = _socialSecurityService.GetSocialSecurityRenewListByMemberID(parameter.MemberID);
                    //                                    //社保待办与正常的人数
                    //                                    SSServiceCost = SocialSecurityPeopleList.Count * Convert.ToDecimal(str1[2]);
                    //                                    //记录支出
                    //                                    sqlAccountRecord += $@"insert into AccountRecord(MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,CreateTime) 
                    //values({parameter.MemberID},'','','支出','余额','社保服务费',{SSServiceCost},getdate());";
                    //                                    //                                    if (SocialSecurityPeopleList.Count > 0)
                    //                                    //                                    {
                    //                                    //                                        foreach (var item1 in SocialSecurityPeopleList)
                    //                                    //                                        {
                    //                                    //                                            sqlAccountRecord += $@"insert into AccountRecord(MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,CreateTime) 
                    //                                    //values({parameter.MemberID},{item1.SocialSecurityPeopleID},'{item1.SocialSecurityPeopleName}','支出','余额','社保服务费',{str1[2]},getdate());";
                    //                                    //                                        }
                    //                                    //                                    }
                    //                                    break;
                    //                                }

                    //                            }
                    //                        }
                    //                        //公积金服务费
                    //                        CostParameterSetting AFParameter = _parameterSettingService.GetCostParameter((int)PayTypeEnum.AccumulationFund);
                    //                        if (AFParameter != null && !string.IsNullOrEmpty(AFParameter.RenewServiceCost))
                    //                        {
                    //                            string[] str = AFParameter.RenewServiceCost.Split(';');
                    //                            foreach (var item in str)
                    //                            {
                    //                                string[] str1 = item.Split(',');

                    //                                if (Convert.ToInt32(str1[0]) <= day && day <= Convert.ToInt32(str1[1]))
                    //                                {
                    //                                    List<SocialSecurityPeople> SocialSecurityPeopleList = _socialSecurityService.GetAccumulationFundRenewListByMemberID(parameter.MemberID);
                    //                                    //社保待办与正常的人数
                    //                                    AFServiceCost = SocialSecurityPeopleList.Count * Convert.ToDecimal(str1[2]);
                    //                                    //记录支出
                    //                                    sqlAccountRecord += $@"insert into AccountRecord(MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,CreateTime) 
                    //values({parameter.MemberID},'','','支出','余额','公积金服务费',{AFServiceCost},getdate());";
                    //                                    //                                    if (SocialSecurityPeopleList.Count > 0)
                    //                                    //                                    {
                    //                                    //                                        foreach (var item1 in SocialSecurityPeopleList)
                    //                                    //                                        {
                    //                                    //                                            sqlAccountRecord += $@"insert into AccountRecord(MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,CreateTime) 
                    //                                    //values({parameter.MemberID},{item1.SocialSecurityPeopleID},'{item1.SocialSecurityPeopleName}','支出','余额','公积金服务费',{str1[2]},getdate());";
                    //                                    //                                        }
                    //                                    //                                    }
                    //                                    break;
                    //                                }
                    //                            }
                    //                        }
                    //                    }
                    //                    TotalServiceCost = SSServiceCost + AFServiceCost;
                    //                    //修改账户余额
                    //                    decimal account = parameter.Amount - TotalServiceCost;
                    //                    string sqlMember = $"update Members set Account=ISNULL(Account,0)+{account} where MemberID={parameter.MemberID}";
                    //                    int updateResult = DbHelper.ExecuteSqlCommand(sqlMember, null);
                    //                    if (!(updateResult > 0)) throw new Exception("更新个人账户失败");

                    //                    //记录收入
                    //                    sqlAccountRecord += $"insert into AccountRecord(MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,CreateTime) values({parameter.MemberID},'','','收入','{parameter.PayMethod}','续费',{parameter.Amount},getdate());";
                    //                    //更新记录
                    //                    DbHelper.ExecuteSqlCommand(sqlAccountRecord, null);

                    //                    //将所有的待续费变成正常,并将剩余月数变成服务月数  --待修改
                    //                    _socialSecurityService.UpdateRenewToNormalByMemberID(parameter.MemberID, parameter.MonthCount);
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
                    if (!_socialSecurityService.IsExistsRenew(parameter.MemberID))
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
                        //社保待续费人员列表
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
                        //公积金人员列表
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

                        //获取某用户下的所有待续费金额之和
                        decimal RenewMonthTotal = _socialSecurityService.GetRenewAmountByMemberID(parameter.MemberID);
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

                            //将所有的待续费变成正常,并将剩余月数变成服务月数
                            _socialSecurityService.UpdateRenewToNormalByMemberID(parameter.MemberID, 1);
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

                        transaction.Complete();
                    }
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
        public JsonResult<dynamic> TestTransaction()
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

    }
}