using WYJK.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using WYJK.Data;
using WYJK.Data.IServices;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;
using System.Transactions;
using WYJK.Data.IService;
using System.Text.RegularExpressions;
using System.Configuration;

namespace WYJK.Web.Controllers.Http
{
    /// <summary>
    /// 社保接口 未参保-1，待办-2，正常-3，续费-4，待停-5，已停-6
    /// </summary>
    public class SocialSecurityController : ApiController
    {
        private readonly ISocialSecurityService _socialSecurityService = new SocialSecurityService();
        private readonly IAccumulationFundService _accumulationFundService = new AccumulationFundService();
        private readonly IParameterSettingService _parameterSettingService = new ParameterSettingService();
        private readonly IMemberService _memberService = new MemberService();
        /// <summary>
        /// 获取户口性质
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<List<string>> GetHouseholdPropertyList()
        {
            List<string> list = new List<string>();
            list.Add("本市农村");
            list.Add("本市城镇");
            list.Add("外市农村");
            list.Add("外市城镇");

            return new JsonResult<List<string>>
            {
                status = true,
                Message = "获取成功",
                Data = list
            };

            //List<HouseholdProperty> HouseholdPropertyList = HouseholdPropertyClass.GetList(typeof(HouseholdPropertyEnum));

            //    return new JsonResult<List<HouseholdProperty>>
            //    {
            //        status = true,
            //        Message = "获取成功",
            //        Data = HouseholdPropertyList
            //    };
        }

        /// <summary>
        /// 删除未参保人
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public async Task<JsonResult<dynamic>> DeleteUninsuredPeople(int SocialSecurityPeopleID)
        {
            bool flag = await _socialSecurityService.DeleteUninsuredPeople(SocialSecurityPeopleID);

            return new JsonResult<dynamic>
            {
                status = flag,
                Message = flag ? "删除成功" : "删除失败"
            };

        }


        /// <summary>
        /// 根据参保地获取社保基数范围 社保基数范围：minBase：最小基数，maxBase：最大基数
        /// </summary>
        /// <param name="area">区域:省市名称之间用|隔开，如:山东省|青岛市</param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<dynamic> GetSocialSecurityBase(string area, string HouseholdProperty)
        {
            EnterpriseSocialSecurity model = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(area, HouseholdProperty);
            if (model == null)
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "该区域不能进行缴费操作，请选择其他区域"
                };
            decimal minBase = Math.Round(model.SocialAvgSalary * (model.MinSocial / 100));
            decimal maxBase = Math.Round(model.SocialAvgSalary * (model.MaxSocial / 100));
            return new JsonResult<dynamic>
            {
                status = true,
                Message = "获取成功",
                Data = new
                {
                    minBase = minBase,
                    maxBase = maxBase
                }
            };
        }

        /// <summary>
        /// 根据参公积金地获取社保基数范围 公积金基数范围：minBase：最小基数，maxBase：最大基数
        /// </summary>
        /// <param name="area">区域:省市名称之间用|隔开，如:山东省|青岛市</param>
        /// <returns></returns>
        public JsonResult<dynamic> GetAccumulationFundBase(string area, string HouseholdProperty)
        {
            EnterpriseSocialSecurity model = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(area, HouseholdProperty);
            if (model == null)
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "该区域不能进行缴费操作，请选择其他区域"
                };
            decimal minBase = model.MinAccumulationFund;
            decimal maxBase = model.MaxAccumulationFund;
            return new JsonResult<dynamic>
            {
                status = true,
                Message = "获取成功",
                Data = new
                {
                    minBase = minBase,
                    maxBase = maxBase
                }
            };
        }


        /// <summary>
        /// 获取未参保列表
        /// </summary>
        /// <param name="memberID">用户ID</param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public async Task<JsonResult<List<UnInsuredPeople>>> GetUnInsuredPeopleList(int memberID)
        {
            //未参保状态
            int status = (int)SocialSecurityStatusEnum.UnInsured;

            List<UnInsuredPeople> list = await _socialSecurityService.GetUnInsuredPeopleList(memberID, status);


            list.ForEach(item =>
            {
                if (item.IsPaySocialSecurity)
                {
                    item.SocialSecurityAmount = item.SocialSecurityBase * item.SSPayProportion / 100 * item.SSPayMonthCount;
                    item.socialSecurityFirstBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.SocialSecurity).BacklogCost;
                }
                if (item.IsPayAccumulationFund)
                {
                    item.AccumulationFundAmount = item.AccumulationFundBase * item.AFPayProportion / 100 * item.AFPayMonthCount;
                    item.AccumulationFundFirstBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.AccumulationFund).BacklogCost;
                }

                if (item.SSStatus != (int)SocialSecurityStatusEnum.UnInsured)
                {
                    item.SSStatus = 0;
                }

                if (item.AFStatus != (int)SocialSecurityStatusEnum.UnInsured)
                {
                    item.AFStatus = 0;
                }

            });
            return new JsonResult<List<UnInsuredPeople>>
            {
                status = true,
                Message = "获取成功",
                Data = list
            };
        }

        /// <summary>
        /// 选择停保原因
        /// </summary>
        /// <returns></returns>
        public JsonResult<List<string>> GetStopSocialSecurityReason()
        {
            string StopSocialSecurityReason = "劳动合同到期|企业解除劳动合同|企业经济性裁员|企业破产|企业撤销解散|个人申请解除劳动合同";
            List<string> list = StopSocialSecurityReason.Split('|').ToList();
            return new JsonResult<List<string>>
            {
                status = true,
                Message = "获取成功",
                Data = list
            };
        }


        /// <summary>
        /// 添加参保人  SocialSecurityID:社保ID，AccumulationFundID：公积金ID
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public async Task<JsonResult<dynamic>> AddSocialSecurityPeople(SocialSecurityPeople socialSecurityPeople)
        {
            if (socialSecurityPeople.socialSecurity.SocialSecurityID == 0 && socialSecurityPeople.accumulationFund.AccumulationFundID == 0)
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "必须选择参保方案"
                };


            //验证身份证
            if (!Regex.IsMatch(socialSecurityPeople.IdentityCard, @"(^\d{18}$)|(^\d{15}$)"))
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "身份证号填写错误"
                };

            //判断身份证是否已存在
            if (_socialSecurityService.IsExistsSocialSecurityPeopleIdentityCard(socialSecurityPeople.IdentityCard))
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "身份证已存在"
                };

            bool flag = await _socialSecurityService.AddSocialSecurityPeople(socialSecurityPeople);
            return new JsonResult<dynamic>
            {
                status = flag,
                Message = flag ? "添加成功" : "添加失败"
            };
        }


        /// <summary>
        /// 修改提交参保人  需要传参：参保人ID，社保ID，公积金ID
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public async Task<JsonResult<dynamic>> ModifySocialSecurityPeople(SocialSecurityPeople socialSecurityPeople)
        {
            //只修改参保人主页面
            if (socialSecurityPeople.socialSecurity == null && socialSecurityPeople.accumulationFund == null)
            {
                string sql = $"update SocialSecurityPeople set SocialSecurityPeopleName='{socialSecurityPeople.SocialSecurityPeopleName}',IdentityCard='{socialSecurityPeople.IdentityCard}',IdentityCardPhoto='{socialSecurityPeople.IdentityCardPhoto}',HouseholdProperty='{socialSecurityPeople.HouseholdProperty}' where SocialSecurityPeopleID={socialSecurityPeople.SocialSecurityPeopleID}";

                bool flag1 = DbHelper.ExecuteSqlCommand(sql, null) > 0;

                return new JsonResult<dynamic>
                {
                    status = flag1,
                    Message = flag1 ? "修改成功" : "修改失败"
                };
            }

            if (socialSecurityPeople.socialSecurity.SocialSecurityID == 0 && socialSecurityPeople.accumulationFund.AccumulationFundID == 0)
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "必须选择参保方案"
                };


            ////验证身份证
            //if (!Regex.IsMatch(socialSecurityPeople.IdentityCard, @"(^\d{18}$)|(^\d{15}$)"))
            //    return new JsonResult<dynamic>
            //    {
            //        status = false,
            //        Message = "身份证号填写错误"
            //    };

            ////判断身份证是否已存在 应排除该条对应的 --todo
            //if (_socialSecurityService.IsExistsSocialSecurityPeopleIdentityCard(socialSecurityPeople.IdentityCard, socialSecurityPeople.SocialSecurityPeopleID))
            //    return new JsonResult<dynamic>
            //    {
            //        status = false,
            //        Message = "身份证已存在"
            //    };

            bool flag = await _socialSecurityService.ModifySocialSecurityPeople(socialSecurityPeople);
            return new JsonResult<dynamic>
            {
                status = flag,
                Message = flag ? "修改成功" : "修改失败"
            };
        }



        /// <summary>
        /// 获取参保人详情
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<SocialSecurityPeopleDetail> GetSocialSecurityPeopleDetail(int SocialSecurityPeopleID)
        {
            decimal SocialSecurityAmount = 0;
            decimal SocialSecurityBacklogCost = 0;
            decimal AccumulationFundAmount = 0;
            decimal AccumulationFundBacklogCost = 0;
            SocialSecurityPeopleDetail model = _socialSecurityService.GetSocialSecurityPeopleDetail(SocialSecurityPeopleID);
            if (model.IsPaySocialSecurity)
            {
                string sql = $"select SocialSecurityBase * PayProportion/100*PayMonthCount from SocialSecurity where SocialSecurityPeopleID = {model.SocialSecurityPeopleID}";
                SocialSecurityAmount = DbHelper.QuerySingle<decimal>(sql);
                SocialSecurityBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.SocialSecurity).BacklogCost;
            }

            if (model.IsPayAccumulationFund)
            {
                string sql = $"select AccumulationFundBase * PayProportion/100*PayMonthCount from AccumulationFund where SocialSecurityPeopleID = {model.SocialSecurityPeopleID}";
                AccumulationFundAmount = DbHelper.QuerySingle<decimal>(sql);
                AccumulationFundBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.AccumulationFund).BacklogCost;
            }
            model.Amount = SocialSecurityAmount + AccumulationFundAmount + SocialSecurityBacklogCost + AccumulationFundBacklogCost;
            model.IdentityCardPhoto = ConfigurationManager.AppSettings["ServerUrl"] + model.IdentityCardPhoto.Replace(";", ";" + ConfigurationManager.AppSettings["ServerUrl"]);

            return new JsonResult<SocialSecurityPeopleDetail>
            {
                status = true,
                Message = "获取成功",
                Data = model
            };
        }

        /// <summary>
        /// 获取参保方案信息
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<SocialSecurityPeople> GetSocialSecurityScheme(int SocialSecurityPeopleID)
        {
            SocialSecurityPeople model = new SocialSecurityPeople();
            //获取参保人信息
            string sql = $"select *  from SocialSecurityPeople where SocialSecurityPeopleID={SocialSecurityPeopleID}";
            model = DbHelper.QuerySingle<SocialSecurityPeople>(sql);
            model.IdentityCardPhoto = ConfigurationManager.AppSettings["ServerUrl"] + model.IdentityCardPhoto.Replace(";", ";" + ConfigurationManager.AppSettings["ServerUrl"]);
            //获取社保信息
            if (model.IsPaySocialSecurity)
                model.socialSecurity = _socialSecurityService.GetSocialSecurityDetail(SocialSecurityPeopleID);
            //获取公积金信息
            if (model.IsPayAccumulationFund)
                model.accumulationFund = _accumulationFundService.GetAccumulationFundDetail(SocialSecurityPeopleID);
            return new JsonResult<SocialSecurityPeople>
            {
                status = true,
                Message = "获取成功",
                Data = model
            };
        }




        /// <summary>
        /// 确认社保方案并返回参保信息进行确认 根据以下两个字段来判断是否添加过社保或公积金  IsExistSocialSecurityCase：是否添加社保方案，IsExistaAccumulationFundCase：是否添加公积金方案，socialSecurityFirstBacklogCost：社保第一次代办费，SocialSecurityBase：社保基数，FreezingCharge：冻结金额
        /// </summary>
        /// <param name="socialSecurityPeople"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JsonResult<dynamic> ConfirmSocialSecurityScheme(SocialSecurityPeople socialSecurityPeople)
        {
            //社保
            bool IsExistSocialSecurityCase = false;
            DateTime socialSecurityStartTime = DateTime.MinValue;
            DateTime socialSecurityEndTime = DateTime.MinValue;
            int socialSecuritypayMonth = 0;
            decimal socialSecurityFirstBacklogCost = 0;
            decimal SocialSecurityBase = 0;
            decimal FreezingCharge = 0;
            decimal SocialSecurityAmount = 0;
            if (socialSecurityPeople.socialSecurity != null)
            {
                IsExistSocialSecurityCase = true;
                socialSecurityStartTime = socialSecurityPeople.socialSecurity.PayTime;
                socialSecurityEndTime = socialSecurityPeople.socialSecurity.PayTime.AddMonths(socialSecurityPeople.socialSecurity.PayMonthCount - 1);
                socialSecuritypayMonth = socialSecurityPeople.socialSecurity.PayMonthCount;
                socialSecurityFirstBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.SocialSecurity).BacklogCost;
                SocialSecurityBase = socialSecurityPeople.socialSecurity.SocialSecurityBase;
                FreezingCharge = 0;

                EnterpriseSocialSecurity enterpriseSocialSecurity = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(socialSecurityPeople.socialSecurity.InsuranceArea, socialSecurityPeople.HouseholdProperty);
                if (enterpriseSocialSecurity == null)
                    return new JsonResult<dynamic>
                    {
                        status = false,
                        Message = "该区域不能进行缴费操作，请选择其他区域"
                    };

                decimal value = 0;
                //model.PersonalShiYeTown
                if (socialSecurityPeople.HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.InRural)) ||
        socialSecurityPeople.HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.OutRural)))
                {
                    value = enterpriseSocialSecurity.PersonalShiYeRural;
                }
                else if (socialSecurityPeople.HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.InTown)) ||
                   socialSecurityPeople.HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.OutTown)))
                {
                    value = enterpriseSocialSecurity.PersonalShiYeTown;
                }

                decimal PayProportion = enterpriseSocialSecurity.CompYangLao + enterpriseSocialSecurity.CompYiLiao + enterpriseSocialSecurity.CompShiYe + enterpriseSocialSecurity.CompGongShang + enterpriseSocialSecurity.CompShengYu
                    + enterpriseSocialSecurity.PersonalYangLao + enterpriseSocialSecurity.PersonalYiLiao + value + enterpriseSocialSecurity.PersonalGongShang + enterpriseSocialSecurity.PersonalShengYu;
                SocialSecurityAmount = PayProportion * socialSecurityPeople.socialSecurity.SocialSecurityBase / 100;
            }
            //公积金
            bool IsExistaAccumulationFundCase = false;
            DateTime AccumulationFundStartTime = DateTime.MinValue;
            DateTime AccumulationFundEndTime = DateTime.MinValue;
            int AccumulationFundpayMonth = 0;
            decimal AccumulationFundFirstBacklogCost = 0;
            decimal AccumulationFundBase = 0;
            decimal AccumulationFundAmount = 0;
            if (socialSecurityPeople.accumulationFund != null)
            {
                IsExistaAccumulationFundCase = true;
                AccumulationFundStartTime = socialSecurityPeople.accumulationFund.PayTime;
                AccumulationFundEndTime = socialSecurityPeople.accumulationFund.PayTime.AddMonths(socialSecurityPeople.accumulationFund.PayMonthCount - 1);
                AccumulationFundpayMonth = socialSecurityPeople.accumulationFund.PayMonthCount;
                AccumulationFundFirstBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.AccumulationFund).BacklogCost;
                AccumulationFundBase = socialSecurityPeople.accumulationFund.AccumulationFundBase;

                EnterpriseSocialSecurity enterpriseSocialSecurity2 = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(socialSecurityPeople.accumulationFund.AccumulationFundArea, socialSecurityPeople.HouseholdProperty);
                if (enterpriseSocialSecurity2 == null)
                    return new JsonResult<dynamic>
                    {
                        status = false,
                        Message = "该区域不能进行缴费操作，请选择其他区域"
                    };


                decimal PayProportion2 = enterpriseSocialSecurity2.CompProportion + enterpriseSocialSecurity2.PersonalProportion;
                AccumulationFundAmount = PayProportion2 * socialSecurityPeople.accumulationFund.AccumulationFundBase / 100;
            }

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "获取成功",
                Data = new
                {
                    IsExistSocialSecurityCase = IsExistSocialSecurityCase,
                    socialSecurityStartTime = socialSecurityStartTime.ToString("yyyy-MM"),
                    socialSecurityEndTime = socialSecurityEndTime.ToString("yyyy-MM"),
                    socialSecuritypayMonth = socialSecuritypayMonth,
                    socialSecurityFirstBacklogCost = socialSecurityFirstBacklogCost,
                    SocialSecurityBase = SocialSecurityBase,
                    FreezingCharge = FreezingCharge,
                    SocialSecurityAmount = SocialSecurityAmount,
                    IsExistaAccumulationFundCase = IsExistaAccumulationFundCase,
                    AccumulationFundStartTime = AccumulationFundStartTime.ToString("yyyy-MM"),
                    AccumulationFundEndTime = AccumulationFundEndTime.ToString("yyyy-MM"),
                    AccumulationFundpayMonth = AccumulationFundpayMonth,
                    AccumulationFundFirstBacklogCost = AccumulationFundFirstBacklogCost,
                    AccumulationFundBase = AccumulationFundBase,
                    AccumulationFundAmount = AccumulationFundAmount
                }
            };
        }

        /// <summary>
        /// 更新参保方案   socialSecurityPeople下的SocialSecurityPeopleID也需要传
        /// </summary>
        /// <param name="socialSecurityPeople"></param>
        /// <returns></returns>
        public JsonResult<dynamic> ModifySocialSecurityScheme(SocialSecurityPeople socialSecurityPeople)
        {
            int SocialSecurityID = 0;
            decimal SocialSecurityAmount = 0;
            int SocialSecurityMonthCount = 0;
            decimal SocialSecurityBacklogCost = 0;
            int AccumulationFundID = 0;
            decimal AccumulationFundAmount = 0;
            int AccumulationFundMonthCount = 0;
            decimal AccumulationFundBacklogCost = 0;


            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    //删除该参保人下的参保方案
                    string sqlDel = $"delete from SocialSecurity where SocialSecurityPeopleID={socialSecurityPeople.SocialSecurityPeopleID};"
                               + $" delete from AccumulationFund where SocialSecurityPeopleID ={socialSecurityPeople.SocialSecurityPeopleID};";
                    DbHelper.ExecuteSqlCommand(sqlDel, null);

                    //保存社保参保方案
                    if (socialSecurityPeople.socialSecurity != null)
                    {
                        socialSecurityPeople.socialSecurity.SocialSecurityPeopleID = socialSecurityPeople.SocialSecurityPeopleID;
                        //if (socialSecurityPeople.socialSecurity.PayTime.Day > 14)
                        //    socialSecurityPeople.socialSecurity.PayTime.AddMonths(1);
                        SocialSecurityID = _socialSecurityService.AddSocialSecurity(socialSecurityPeople.socialSecurity);
                        //查询社保金额
                        SocialSecurityAmount = _socialSecurityService.GetSocialSecurityAmount(SocialSecurityID);
                        //查询社保月数
                        SocialSecurityMonthCount = _socialSecurityService.GetSocialSecurityMonthCount(SocialSecurityID);
                        SocialSecurityBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.SocialSecurity).BacklogCost;
                    }

                    //保存公积金参保方案
                    if (socialSecurityPeople.accumulationFund != null)
                    {
                        socialSecurityPeople.accumulationFund.SocialSecurityPeopleID = socialSecurityPeople.SocialSecurityPeopleID;
                        //if (socialSecurityPeople.accumulationFund.PayTime.Day > 14)
                        //    socialSecurityPeople.accumulationFund.PayTime.AddMonths(1);
                        AccumulationFundID = _socialSecurityService.AddAccumulationFund(socialSecurityPeople.accumulationFund);
                        //查询公积金金额
                        AccumulationFundAmount = _socialSecurityService.GetAccumulationFundAmount(AccumulationFundID);
                        //查询公积金月数
                        AccumulationFundMonthCount = _socialSecurityService.GetAccumulationFundMonthCount(AccumulationFundID);
                        AccumulationFundBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.AccumulationFund).BacklogCost;
                    }
                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    return new JsonResult<dynamic>
                    {
                        status = false,
                        Message = "更新社保方案失败"
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
                Message = "更新社保方案成功",
                Data = new
                {
                    SocialSecurityID = SocialSecurityID,
                    AccumulationFundID = AccumulationFundID,
                    Amount = SocialSecurityAmount * SocialSecurityMonthCount + AccumulationFundAmount * AccumulationFundMonthCount + SocialSecurityBacklogCost + AccumulationFundBacklogCost
                }
            };
        }


        /// <summary>
        /// 添加社保方案 返回社保ID:SocialSecurityID,公积金ID:AccumulationFundID，总金额:Amount
        /// </summary>
        /// <param name="socialSecurityPeople"></param>
        /// <returns></returns>
        public JsonResult<dynamic> AddSocialSecurityScheme(SocialSecurityPeople socialSecurityPeople)
        {
            int SocialSecurityID = 0;
            decimal SocialSecurityAmount = 0;
            int SocialSecurityMonthCount = 0;
            decimal SocialSecurityBacklogCost = 0;
            int AccumulationFundID = 0;
            decimal AccumulationFundAmount = 0;
            int AccumulationFundMonthCount = 0;
            decimal AccumulationFundBacklogCost = 0;

            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    //保存社保参保方案
                    if (socialSecurityPeople.socialSecurity != null)
                    {
                        //if (socialSecurityPeople.socialSecurity.PayTime.Day > 14)
                        //    socialSecurityPeople.socialSecurity.PayTime.AddMonths(1);

                        //返回社保ID
                        SocialSecurityID = _socialSecurityService.AddSocialSecurity(socialSecurityPeople.socialSecurity);
                        //查询社保金额
                        SocialSecurityAmount = _socialSecurityService.GetSocialSecurityAmount(SocialSecurityID);
                        //查询社保月数
                        SocialSecurityMonthCount = _socialSecurityService.GetSocialSecurityMonthCount(SocialSecurityID);
                        SocialSecurityBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.SocialSecurity).BacklogCost;
                    }

                    //保存公积金参保方案
                    if (socialSecurityPeople.accumulationFund != null)
                    {
                        //if (socialSecurityPeople.accumulationFund.PayTime.Day > 14)
                        //    socialSecurityPeople.accumulationFund.PayTime.AddMonths(1);
                        //返回公积金ID
                        AccumulationFundID = _socialSecurityService.AddAccumulationFund(socialSecurityPeople.accumulationFund);
                        //查询公积金金额
                        AccumulationFundAmount = _socialSecurityService.GetAccumulationFundAmount(AccumulationFundID);
                        //查询公积金月数
                        AccumulationFundMonthCount = _socialSecurityService.GetAccumulationFundMonthCount(AccumulationFundID);
                        AccumulationFundBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.AccumulationFund).BacklogCost;
                    }
                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    return new JsonResult<dynamic>
                    {
                        status = false,
                        Message = "添加社保方案失败"
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
                Message = "添加社保方案成功",
                Data = new
                {
                    SocialSecurityID = SocialSecurityID,
                    AccumulationFundID = AccumulationFundID,
                    Amount = SocialSecurityAmount * SocialSecurityMonthCount + AccumulationFundAmount * AccumulationFundMonthCount + SocialSecurityBacklogCost + AccumulationFundBacklogCost
                }
            };
        }

        /// <summary>
        /// 获取待办理列表    状态为 1和2的都显示为待办
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public JsonResult<List<SocialSecurityPeoples>> GetWaitingHandleListByStatus(int MemberID)
        {

            string sql = "select ssp.SocialSecurityPeopleID,ssp.SocialSecurityPeopleName,ss.PayTime SSPayTime,ISNULL(ss.AlreadyPayMonthCount,0) SSAlreadyPayMonthCount,ss.Status SSStatus,ss.PayMonthCount SSRemainingMonthCount, af.PayTime AFPayTime,ISNULL(af.AlreadyPayMonthCount,0) AFAlreadyPayMonthCount,af.Status AFStatus,af.PayMonthCount AFRemainingMonthCount"
            + " from SocialSecurityPeople ssp"
            + " left join SocialSecurity ss on ssp.SocialSecurityPeopleID = ss.SocialSecurityPeopleID"
            + " left join AccumulationFund af on ssp.SocialSecurityPeopleID = af.SocialSecurityPeopleID"
            + $" where (ss.Status = {(int)SocialSecurityStatusEnum.WaitingHandle} or af.Status = {(int)SocialSecurityStatusEnum.WaitingHandle}) or((ss.Status = {(int)SocialSecurityStatusEnum.UnInsured} or af.Status = {(int)SocialSecurityStatusEnum.UnInsured}) and ssp.IsPay =1) and ssp.MemberID = {MemberID}";
            List<SocialSecurityPeoples> socialSecurityPeopleList = DbHelper.Query<SocialSecurityPeoples>(sql);

            socialSecurityPeopleList.ForEach(item =>
            {
                if (item.SSStatus == null)
                    item.SSStatus = 0;
                if (item.AFStatus == null)
                    item.AFStatus = 0;
            });

            return new JsonResult<List<SocialSecurityPeoples>>
            {
                status = true,
                Message = "获取成功",
                Data = socialSecurityPeopleList
            };
        }

        /// <summary>
        /// 获取参保人列表 正常、待续费  状态说明如下：正常=3/待续费=4
        /// </summary>
        /// <param name="Status">状态</param>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public JsonResult<List<SocialSecurityPeoples>> GetInsuredPeopleListByStatus(int Status, int MemberID)
        {
            List<SocialSecurityPeoples> socialSecurityPeopleList = _socialSecurityService.GetSocialSecurityPeopleList(Status, MemberID);

            //剩余月数计算 --ToDo
            //int monthCount = _socialSecurityService.GetRemainingMonth(MemberID);

            //查询剩余月数
            socialSecurityPeopleList.ForEach(n =>
            {
                //n.SSRemainingMonthCount = monthCount;//待修改
                //n.AFRemainingMonthCount = monthCount;//待修改
                if (n.SSStatus != Status)
                {
                    n.SSStatus = 0;
                    n.SSPayTime = null;
                    n.SSAlreadyPayMonthCount = null;
                    n.SSRemainingMonthCount = null;
                }
                if (n.AFStatus != Status)
                {
                    n.AFStatus = 0;
                    n.AFPayTime = null;
                    n.AFAlreadyPayMonthCount = null;
                    n.AFRemainingMonthCount = null;
                }

            });

            return new JsonResult<List<SocialSecurityPeoples>>
            {
                status = true,
                Message = "获取成功",
                Data = socialSecurityPeopleList
            };
        }

        /// <summary>
        /// 获取待停列表  状态说明如下：申请=0/未续费=1  
        /// </summary>
        /// <param name="Status"></param>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public JsonResult<List<TopSocialSecurityPeoples>> GetWaitingTopList(int Status, int MemberID)
        {
            string sql = "select ssp.SocialSecurityPeopleID,ssp.SocialSecurityPeopleName,ss.PayTime SSPayTime,ISNULL(ss.AlreadyPayMonthCount,0) SSAlreadyPayMonthCount,ss.Status SSStatus,ss.StopReason SSStopReason, ss.ApplyStopDate SSApplyStopDate,ss.PayMonthCount SSRemainingMonthCount,af.PayTime AFPayTime,ISNULL(af.AlreadyPayMonthCount,0) AFAlreadyPayMonthCount,af.Status AFStatus,af.ApplyStopDate AFApplyStopDate,af.PayMonthCount AFRemainingMonthCount"
            + " from SocialSecurityPeople ssp"
            + " left join SocialSecurity ss on ssp.SocialSecurityPeopleID = ss.SocialSecurityPeopleID"
            + " left join AccumulationFund af on ssp.SocialSecurityPeopleID = af.SocialSecurityPeopleID"
            + $" where ((ss.Status = {(int)SocialSecurityStatusEnum.WaitingStop} and ss.StopMethod= {Status}) or (af.Status = {(int)SocialSecurityStatusEnum.WaitingStop} and af.StopMethod= {Status})) and ssp.MemberID = {MemberID}";

            List<TopSocialSecurityPeoples> socialSecurityPeopleList = DbHelper.Query<TopSocialSecurityPeoples>(sql);


            //剩余月数计算 --ToDo
            //int monthCount = _socialSecurityService.GetRemainingMonth(MemberID);



            //查询剩余月数
            socialSecurityPeopleList.ForEach(n =>
            {
                //n.SSRemainingMonthCount = monthCount;
                //n.AFRemainingMonthCount = monthCount;

                if (n.SSStatus != (int)SocialSecurityStatusEnum.WaitingStop)
                {
                    n.SSStatus = 0;
                }
                if (n.AFStatus != (int)SocialSecurityStatusEnum.WaitingStop)
                {
                    n.AFStatus = 0;
                }
            });

            return new JsonResult<List<TopSocialSecurityPeoples>>
            {
                status = true,
                Message = "获取成功",
                Data = socialSecurityPeopleList
            };
        }

        /// <summary>
        /// 获取社保与公积金正常参保详情
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public JsonResult<SocialSecurityDetail> GetSocialSecurityDetail(int SocialSecurityPeopleID, int MemberID)
        {

            SocialSecurityDetail model = _socialSecurityService.GetSocialSecurityAndAccumulationFundDetail(SocialSecurityPeopleID);
            //剩余月数
            if (!string.IsNullOrEmpty(model.InsuranceArea))
                model.InsuranceArea = model.InsuranceArea.Replace("|", "");
            if (!string.IsNullOrEmpty(model.AccumulationFundArea))
                model.AccumulationFundArea = model.AccumulationFundArea.Replace("|", "");
            //model.SSRemainingMonths = _socialSecurityService.GetRemainingMonth(MemberID);
            //model.AFRemainingMonths = _socialSecurityService.GetRemainingMonth(MemberID);

            if (!string.IsNullOrEmpty(model.SocialSecurityBase))
                model.IsSocialSecurity = true;
            if (!string.IsNullOrEmpty(model.AccumulationFundBase))
                model.IsAccumulationFund = true;

            return new JsonResult<SocialSecurityDetail>
            {
                status = true,
                Message = "获取成功",
                Data = model
            };
        }

        ///// <summary>
        /////  获取公积金正常参保详情
        ///// </summary>
        ///// <param name="SocialSecurityPeopleID"></param>
        ///// <param name="MemberID"></param>
        ///// <returns></returns>
        //public JsonResult<AccumulationFund> GetAccumulationFundDetail(int SocialSecurityPeopleID, int MemberID)
        //{
        //    AccumulationFund model = _accumulationFundService.GetAccumulationFundDetail(SocialSecurityPeopleID);
        //    //剩余月数
        //    model.RemainingMonths = _socialSecurityService.GetRemainingMonth(MemberID);

        //    return new JsonResult<AccumulationFund>
        //    {
        //        status = true,
        //        Message = "获取成功",
        //        Data = model
        //    };

        //}

        /// <summary>
        /// 获取申请办停列表 
        /// </summary>
        /// <param name="SocialSecurityPeopleName"></param>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public JsonResult<List<SocialSecurityPeoples>> GetApplyTopList(string SocialSecurityPeopleName, int MemberID)
        {
            //剩余月数计算 --ToDo
            //int monthCount = _socialSecurityService.GetRemainingMonth(MemberID);

            string sql = "select ssp.SocialSecurityPeopleID,ssp.SocialSecurityPeopleName,ss.PayTime SSPayTime,ISNULL(ss.AlreadyPayMonthCount,0) SSAlreadyPayMonthCount,ss.Status SSStatus,ss.PayMonthCount SSRemainingMonthCount,af.PayTime AFPayTime,ISNULL(af.AlreadyPayMonthCount,0) AFAlreadyPayMonthCount,af.Status AFStatus,af.PayMonthCount AFRemainingMonthCount"
            + " from SocialSecurityPeople ssp"
            + " left join SocialSecurity ss on ssp.SocialSecurityPeopleID = ss.SocialSecurityPeopleID"
            + " left join AccumulationFund af on ssp.SocialSecurityPeopleID = af.SocialSecurityPeopleID"
            + $" where (ss.Status in({(int)SocialSecurityStatusEnum.Normal},{(int)SocialSecurityStatusEnum.Renew}) or af.Status in({(int)SocialSecurityStatusEnum.Normal},{(int)SocialSecurityStatusEnum.Renew})) and ssp.MemberID = {MemberID} and ssp.SocialSecurityPeopleName like '%{SocialSecurityPeopleName}%'";

            List<SocialSecurityPeoples> socialSecurityPeopleList = DbHelper.Query<SocialSecurityPeoples>(sql);

            socialSecurityPeopleList.ForEach(n =>
            {

                if (n.SSStatus != (int)SocialSecurityStatusEnum.Normal && n.SSStatus != (int)SocialSecurityStatusEnum.Renew)
                {
                    n.SSStatus = 0;
                }
                if (n.AFStatus != (int)SocialSecurityStatusEnum.Normal && n.AFStatus != (int)SocialSecurityStatusEnum.Renew)
                {
                    n.AFStatus = 0;
                }
            });


            return new JsonResult<List<SocialSecurityPeoples>>
            {
                status = true,
                Message = "获取成功",
                Data = socialSecurityPeopleList
            };
        }

        /// <summary>
        /// 申请停社保
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public JsonResult<dynamic> ApplyStopSocialSecurity(StopSocialSecurityParameter parameter)
        {
            bool flag = _socialSecurityService.ApplyTopSocialSecurity(parameter);

            //查看是否处于待续费状态，如果是，则判断余额-待办是否够交待续费的钱，如果够则将待续费变成正常
            int memberID = DbHelper.QuerySingle<int>($"select MemberID from SocialSecurityPeople where SocialSecurityPeopleID={parameter.SocialSecurityPeopleID}");
            if (_socialSecurityService.IsExistsRenew(memberID)) {
                AccountInfo accountInfo = _memberService.GetAccountInfo(memberID);
                //获取该用户下所有参保人的所有待办金额之和
                decimal WaitingHandleTotal = _socialSecurityService.GetWaitingHandleTotalByMemberID(memberID);
                //获取某用户下的所有待续费金额之和
                decimal RenewMonthTotal = _socialSecurityService.GetRenewAmountByMemberID(memberID);
                if (accountInfo.Account - WaitingHandleTotal >= RenewMonthTotal) {
                    //将所有的待续费变成正常,并将剩余月数变成服务月数
                    _socialSecurityService.UpdateRenewToNormalByMemberID(memberID, 1);
                }
            }

            return new JsonResult<dynamic>
            {
                status = flag,
                Message = flag ? "申请成功" : "申请失败"
            };
        }



        /// <summary>
        /// 申请停公积金
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public JsonResult<dynamic> ApplyStopAccumulationFund(StopAccumulationFundParameter parameter)
        {
            bool flag = _socialSecurityService.ApplyTopAccumulationFund(parameter);

            //查看是否处于待续费状态，如果是，则判断余额-待办是否够交待续费的钱，如果够则将待续费变成正常
            int memberID = DbHelper.QuerySingle<int>($"select MemberID from SocialSecurityPeople where SocialSecurityPeopleID={parameter.SocialSecurityPeopleID}");
            if (_socialSecurityService.IsExistsRenew(memberID))
            {
                AccountInfo accountInfo = _memberService.GetAccountInfo(memberID);
                //获取该用户下所有参保人的所有待办金额之和
                decimal WaitingHandleTotal = _socialSecurityService.GetWaitingHandleTotalByMemberID(memberID);
                //获取某用户下的所有待续费金额之和
                decimal RenewMonthTotal = _socialSecurityService.GetRenewAmountByMemberID(memberID);
                if (accountInfo.Account - WaitingHandleTotal >= RenewMonthTotal)
                {
                    //将所有的待续费变成正常,并将剩余月数变成服务月数
                    _socialSecurityService.UpdateRenewToNormalByMemberID(memberID, 1);
                }
            }

            return new JsonResult<dynamic>
            {
                status = flag,
                Message = flag ? "申请成功" : "申请失败"
            };
        }

        /// <summary>
        /// 获取已停列表
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public JsonResult<List<TopSocialSecurityPeoples>> GetAlreadyStop(int MemberID)
        {
            string sql = "select ssp.SocialSecurityPeopleID,ssp.SocialSecurityPeopleName,ss.PayTime SSPayTime,ISNULL(ss.AlreadyPayMonthCount,0) SSAlreadyPayMonthCount,ss.Status SSStatus,ss.ApplyStopDate SSApplyStopDate,ss.PayMonthCount SSRemainingMonthCount, af.PayTime AFPayTime,ISNULL(af.AlreadyPayMonthCount,0) AFAlreadyPayMonthCount,af.Status AFStatus,af.ApplyStopDate AFApplyStopDate,af.PayMonthCount AFRemainingMonthCount"
            + " from SocialSecurityPeople ssp"
            + " left join SocialSecurity ss on ssp.SocialSecurityPeopleID = ss.SocialSecurityPeopleID"
            + " left join AccumulationFund af on ssp.SocialSecurityPeopleID = af.SocialSecurityPeopleID"
            + $" where (ss.Status = {(int)SocialSecurityStatusEnum.AlreadyStop} or af.Status = {(int)SocialSecurityStatusEnum.AlreadyStop}) and ssp.MemberID = {MemberID}";

            List<TopSocialSecurityPeoples> socialSecurityPeopleList = DbHelper.Query<TopSocialSecurityPeoples>(sql);

            //查询剩余月数
            socialSecurityPeopleList.ForEach(n =>
            {
                //n.SSRemainingMonthCount = 0;//待修改
                //n.AFRemainingMonthCount = 0;//待修改
                if (n.SSStatus != (int)SocialSecurityStatusEnum.AlreadyStop)
                {
                    n.SSStatus = 0;
                    n.SSPayTime = null;
                    n.SSAlreadyPayMonthCount = null;
                    n.SSRemainingMonthCount = null;
                }
                if (n.AFStatus != (int)SocialSecurityStatusEnum.AlreadyStop)
                {
                    n.AFStatus = 0;
                    n.AFPayTime = null;
                    n.AFAlreadyPayMonthCount = null;
                    n.AFRemainingMonthCount = null;
                }

            });

            return new JsonResult<List<TopSocialSecurityPeoples>>
            {
                status = true,
                Message = "获取成功",
                Data = socialSecurityPeopleList
            };
        }

        /// <summary>
        /// 获取社保计算结果
        /// </summary>
        /// <param name="InsuranceArea"></param>
        /// <param name="HouseholdProperty"></param>
        /// <param name="SocialSecurityBase"></param>
        /// <param name="AccountRecordBase"></param>
        /// <returns></returns>
        public JsonResult<SocialSecurityCalculation> GetSocialSecurityCalculation(string InsuranceArea, string HouseholdProperty, decimal SocialSecurityBase, decimal AccountRecordBase)
        {
            SocialSecurityCalculation model = _socialSecurityService.GetSocialSecurityCalculationResult(InsuranceArea, HouseholdProperty, SocialSecurityBase, AccountRecordBase);
            return new JsonResult<SocialSecurityCalculation>
            {
                status = true,
                Message = "获取成功",
                Data = model
            };
        }

        /// <summary>
        /// 根据用户ID获取所有参保人列表
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public JsonResult<List<SocialSecurityPeoples>> GetSocialSecurityListByMemberID(int MemberID)
        {
            string sqlstr = $@"select SocialSecurityPeople.SocialSecurityPeopleID,
SocialSecurityPeople.SocialSecurityPeopleName,
SocialSecurity.PayTime SSPayTime,
SocialSecurity.AlreadyPayMonthCount SSAlreadyPayMonthCount,
SocialSecurity.PayMonthCount SSRemainingMonthCount,
SocialSecurity.Status SSStatus,
AccumulationFund.PayTime AFPayTime,
AccumulationFund.AlreadyPayMonthCount AFAlreadyPayMonthCount,
AccumulationFund.PayMonthCount AFRemainingMonthCount,
AccumulationFund.Status AFStatus
from SocialSecurityPeople
left join SocialSecurity on SocialSecurityPeople.SocialSecurityPeopleID = SocialSecurity.SocialSecurityPeopleID
left
join AccumulationFund on SocialSecurityPeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID
where SocialSecurityPeople.MemberID = {MemberID}";
            List<SocialSecurityPeoples> SocialSecurityPeopleList = DbHelper.Query<SocialSecurityPeoples>(sqlstr);
            return new JsonResult<List<SocialSecurityPeoples>>
            {
                status = true,
                Message = "获取成功",
                Data = SocialSecurityPeopleList
            };
        }



        /// <summary>
        /// 调整基数创建
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<AdjustingBase> CreateAdjustingBase(int SocialSecurityPeopleID)
        {
            AdjustingBase model = _socialSecurityService.GetCurrentBase(SocialSecurityPeopleID);

            return new JsonResult<AdjustingBase>
            {
                status = true,
                Message = "获取成功",
                Data = model
            };

        }

        /// <summary>
        /// 调整基数提交
        /// </summary>
        /// <returns></returns>
        public JsonResult<dynamic> PostAdjustingBase(AdjustingBaseParameter parameter)
        {
            bool flag = _socialSecurityService.AddAdjustingBase(parameter);

            return new JsonResult<dynamic>
            {
                status = flag,
                Message = flag ? "提交成功" : "提交失败"
            };
        }


        ///// <summary>
        ///// 是否存在续费
        ///// </summary>
        ///// <param name="MemberID"></param>
        ///// <returns></returns>
        //[System.Web.Http.HttpGet]
        //public JsonResult<dynamic> IsExistsRenew(int MemberID)
        //{
        //    bool flag = _socialSecurityService.IsExistsRenew(MemberID);
        //    return new JsonResult<dynamic>
        //    {
        //        status = flag,
        //        Message = flag ? "存在" : "不存在"
        //    };
        //}
    }
}