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
using System.Net.Http;
using System.Text;
using CMBCHINALib;

namespace WYJK.Web.Controllers.Http
{
    /// <summary>
    /// 社保接口 未参保-1，待办-2，正常-3，续费-4，待停-5，已停-6
    /// </summary>
    public class SocialSecurityController : BaseApiController
    {
        private readonly ISocialSecurityService _socialSecurityService = new SocialSecurityService();
        private readonly IAccumulationFundService _accumulationFundService = new AccumulationFundService();
        private readonly IParameterSettingService _parameterSettingService = new ParameterSettingService();
        private readonly IMemberService _memberService = new MemberService();
        private readonly IEnterpriseService _enterpriseService = new EnterpriseService();
        private readonly IInsuredIntroduceService _insuredIntroduceService = new InsuredIntroduceService();


        /// <summary>
        /// 获取省列表
        /// </summary>
        /// <returns></returns>
        public JsonResult<List<string>> GetProvinceList()
        {
            List<string> list = DbHelper.Query<string>("select SUBSTRING(EnterpriseArea,1,CHARINDEX('|',EnterpriseArea)-1) Province from EnterpriseSocialSecurity").Distinct().ToList();

            return new JsonResult<List<string>>
            {
                status = true,
                Message = "获取成功",
                Data = list
            };
        }

        /// <summary>
        /// 根据省份获取城市
        /// </summary>
        /// <param name="provinceName"></param>
        /// <returns></returns>
        public JsonResult<List<string>> GetCityListByProvince(string provinceName)
        {
            List<string> list = DbHelper.Query<string>($@"select distinct SUBSTRING(reverse(SUBSTRING(reverse(EnterpriseArea),charindex('|',reverse(EnterpriseArea))+1,LEN(EnterpriseArea))),CHARINDEX('|',EnterpriseArea)+1,LEN(EnterpriseArea)) city
                                                          from EnterpriseSocialSecurity
                                                          where EnterpriseArea like '%{provinceName}%'").ToList();

            return new JsonResult<List<string>>
            {
                status = true,
                Message = "获取成功",
                Data = list
            };

        }

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
            //如果存在订单，则不能删除
            int count = DbHelper.QuerySingle<int>($@"select count(0) from OrderDetails  
left join SocialSecurityPeople on SocialSecurityPeople.SocialSecurityPeopleID = OrderDetails.SocialSecurityPeopleID
where SocialSecurityPeople.SocialSecurityPeopleID = {SocialSecurityPeopleID}");
            if (count > 0)
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "此参保人不能删除"
                };

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

                if (item.IsPaySocialSecurity)
                {
                    #region 补差费
                    //每人每月补差费用
                    decimal FreezingAmount = DbHelper.QuerySingle<decimal>("select FreezingAmount from CostParameterSetting where Status = 0");

                    //参保月份、参保月数、签约单位ID
                    SocialSecurity socialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select InsuranceArea,PayTime,PayMonthCount,RelationEnterprise from SocialSecurity where SocialSecurityPeopleID ={item.SocialSecurityPeopleID}");
                    decimal BuchaAmount = 0;
                    if (socialSecurity != null)
                    {
                        int payMonth = socialSecurity.PayTime.Value.Month;
                        int monthCount = socialSecurity.PayMonthCount;
                        //相对应的签约单位城市是否已调差（社平工资）
                        EnterpriseSocialSecurity enterpriseSocialSecurity = _enterpriseService.GetEnterpriseSocialSecurity(socialSecurity.RelationEnterprise);//签约公司
                                                                                                                                                              //已调,当年以后知道年末都不需交，直到一月份开始交
                        if (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year == DateTime.Now.Year)
                        {
                            int freeBuchaMonthCount = 12 + 1 - payMonth;//免补差月数
                            int BuchaMonthCount = monthCount - freeBuchaMonthCount;
                            if (freeBuchaMonthCount < monthCount)
                            {
                                BuchaAmount = FreezingAmount * BuchaMonthCount;
                            }
                        }
                        //未调，往后每个月都需要交，许吧当年1月份到现在的都要交上
                        if (enterpriseSocialSecurity.AdjustDt == null || (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year != DateTime.Now.Year))
                        {
                            int BuchaMonthCount = payMonth - 1 + monthCount;
                            BuchaAmount = FreezingAmount * BuchaMonthCount;
                        }
                    }
                    item.Bucha = BuchaAmount;
                    #endregion
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
        public JsonResult<dynamic> GetStopSocialSecurityReason()
        {
            //13号到15号不允许点击停保
            if (DateTime.Now.Day >= 13 && DateTime.Now.Day <= 15)
            {
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "13号到15号不允许点击停保"
                };
            }
            //提示时间
            string dateTip = string.Empty;
            if (DateTime.Now.Day < 13)
            {
                dateTip = "缴费月数截止到" + DateTime.Now.Year + "年" + DateTime.Now.Month + "月";
            }
            if (DateTime.Now.Day > 15)
            {
                DateTime datetime = DateTime.Now.AddMonths(1);
                dateTip = "缴费月数截止到" + datetime.Year + "年" + datetime.Month + "月";
            }
            //材料收取方式
            List<Property> CollectTypeList = SelectListClass.GetSelectList(typeof(SocialSecurityCollectTypeEnum));

            string StopSocialSecurityReason = "劳动合同到期|企业解除劳动合同|企业经济性裁员|企业破产|企业撤销解散|个人申请解除劳动合同";
            List<string> list = StopSocialSecurityReason.Split('|').ToList();
            return new JsonResult<dynamic>
            {
                status = true,
                Message = "获取成功",
                Data = new { dateTip = dateTip, CollectTypeList = CollectTypeList, list = list }
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


            ////验证身份证
            //if (!Regex.IsMatch(socialSecurityPeople.IdentityCard, @"(^\d{18}$)|(^\d{15}$)"))
            //    return new JsonResult<dynamic>
            //    {
            //        status = false,
            //        Message = "身份证号填写错误"
            //    };

            //判断身份证是否已存在
            //if (_socialSecurityService.IsExistsSocialSecurityPeopleIdentityCard(socialSecurityPeople.IdentityCard))
            //    return new JsonResult<dynamic>
            //    {
            //        status = false,
            //        Message = "身份证已存在"
            //    };

            if (_socialSecurityService.IsExistsSocialSecurityPeopleIdentityCard(socialSecurityPeople.IdentityCard))
            {
                int PayFlag = 0;

                int sscount = DbHelper.QuerySingle<int>($@"select COUNT(0) from SocialSecurity 
  left join SocialSecurityPeople on SocialSecurityPeople.SocialSecurityPeopleID = socialsecurity.SocialSecurityPeopleID
  where SocialSecurityPeople.IdentityCard = '{socialSecurityPeople.IdentityCard}'");
                int afcount = DbHelper.QuerySingle<int>($@"select COUNT(0) from AccumulationFund 
  left join SocialSecurityPeople on SocialSecurityPeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID
  where SocialSecurityPeople.IdentityCard = '{socialSecurityPeople.IdentityCard}'");

                if (sscount == 1 && afcount == 0)
                {
                    if (socialSecurityPeople.accumulationFund.AccumulationFundID > 0)
                    {
                        DbHelper.ExecuteSqlCommand($"update SocialSecurityPeople set IsPayAccumulationFund=1 where IdentityCard='{socialSecurityPeople.IdentityCard}'", null);
                        int socialSecurityPeopleID = DbHelper.QuerySingle<int>($"select SocialSecurityPeopleID from SocialSecurityPeople where IdentityCard='{socialSecurityPeople.IdentityCard}'");
                        DbHelper.ExecuteSqlCommand($"update AccumulationFund set SocialSecurityPeopleID = {socialSecurityPeopleID} where AccumulationFundID = {socialSecurityPeople.accumulationFund.AccumulationFundID }", null);
                    }
                    else {

                        return new JsonResult<dynamic>
                        {
                            status = false,
                            Message = "请添加公积金方案"
                        };
                    }
                }
                else if (sscount == 0 && afcount == 1)
                {
                    if (socialSecurityPeople.socialSecurity.SocialSecurityID > 0)
                    {
                        DbHelper.ExecuteSqlCommand($"update SocialSecurityPeople set IsPaySocialSecurity=1 where IdentityCard='{socialSecurityPeople.IdentityCard}'", null);
                        int socialSecurityPeopleID = DbHelper.QuerySingle<int>($"select SocialSecurityPeopleID from SocialSecurityPeople where IdentityCard='{socialSecurityPeople.IdentityCard}'");
                        DbHelper.ExecuteSqlCommand($"update SocialSecurity set SocialSecurityPeopleID = {socialSecurityPeopleID} where SocialSecurityID = {socialSecurityPeople.socialSecurity.SocialSecurityID}", null);
                    }
                    else {

                        return new JsonResult<dynamic>
                        {
                            status = false,
                            Message = "请添加社保方案"
                        };
                    }
                }
                else if (sscount == 1 && afcount == 1)
                {
                    return new JsonResult<dynamic>
                    {
                        status = false,
                        Message = "您之前已经添加过社保和公积金"
                    };
                }



                return new JsonResult<dynamic>
                {
                    status = true,
                    Message = "添加成功"
                };
            }
            else
            {
                bool flag = await _socialSecurityService.AddSocialSecurityPeople(socialSecurityPeople);
                return new JsonResult<dynamic>
                {
                    status = flag,
                    Message = flag ? "添加成功" : "添加失败"
                };
            }


        }


        /// <summary>
        /// 点击选择参保方案
        /// 查看是否有这个身份证号，如果没有，前台需要提示填写户籍性质，跟之前的正常流程一样；如果有，则需要判断那项没交
        /// 没交过：0；交了社保：1；交了公积金：2；都交了：3   从data中获取
        /// </summary>
        /// <param name="IdentityCard"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<dynamic> SelectSocialSecurityScheme(string IdentityCard)
        {
            int PayFlag = 0;
            string HouseholdProperty = string.Empty;

            int sscount = DbHelper.QuerySingle<int>($@"select COUNT(0) from SocialSecurity 
  left join SocialSecurityPeople on SocialSecurityPeople.SocialSecurityPeopleID = socialsecurity.SocialSecurityPeopleID
  where SocialSecurityPeople.IdentityCard = '{IdentityCard}'");
            int afcount = DbHelper.QuerySingle<int>($@"select COUNT(0) from AccumulationFund 
  left join SocialSecurityPeople on SocialSecurityPeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID
  where SocialSecurityPeople.IdentityCard = '{IdentityCard}'");

            if (sscount == 0 && afcount == 0)
                PayFlag = 0;
            else if (sscount == 1 && afcount == 0)
            {
                HouseholdProperty = DbHelper.QuerySingle<string>($@"select SocialSecurityPeople.HouseholdProperty from SocialSecurity 
  left join SocialSecurityPeople on SocialSecurityPeople.SocialSecurityPeopleID = socialsecurity.SocialSecurityPeopleID
  where SocialSecurityPeople.IdentityCard = '{IdentityCard}'");
                PayFlag = 1;
            }
            else if (sscount == 0 && afcount == 1)
            {
                HouseholdProperty = DbHelper.QuerySingle<string>($@"select SocialSecurityPeople.HouseholdProperty from AccumulationFund 
  left join SocialSecurityPeople on SocialSecurityPeople.SocialSecurityPeopleID = AccumulationFund.SocialSecurityPeopleID
  where SocialSecurityPeople.IdentityCard = '{IdentityCard}'");
                PayFlag = 2;
            }
            else if (sscount == 1 && afcount == 1)
                PayFlag = 3;

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "获取成功",
                Data = new { HouseholdProperty = HouseholdProperty, PayFlag = PayFlag }
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
            decimal FreezingCharge = 0;
            decimal AccumulationFundAmount = 0;
            decimal AccumulationFundBacklogCost = 0;
            SocialSecurityPeopleDetail model = _socialSecurityService.GetSocialSecurityPeopleDetail(SocialSecurityPeopleID);
            SocialSecurity socialSecurity2 = DbHelper.QuerySingle<SocialSecurity>($"select * from SocialSecurity where SocialSecurityPeopleID = {model.SocialSecurityPeopleID} and IsPay=0");
            if (socialSecurity2 != null)
            {
                string sql = $"select ISNULL(SocialSecurityBase * PayProportion/100*PayMonthCount,0) from SocialSecurity where SocialSecurityPeopleID = {model.SocialSecurityPeopleID} and IsPay=0";
                SocialSecurityAmount = DbHelper.QuerySingle<decimal>(sql);
                SocialSecurityBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.SocialSecurity).BacklogCost;

                #region 补差费
                //每人每月补差费用
                decimal FreezingAmount = DbHelper.QuerySingle<decimal>("select FreezingAmount from CostParameterSetting where Status = 0");

                //参保月份、参保月数、签约单位ID
                SocialSecurity socialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select InsuranceArea,PayTime,PayMonthCount,RelationEnterprise from SocialSecurity where SocialSecurityPeopleID ={SocialSecurityPeopleID} and IsPay=0");
                decimal BuchaAmount = 0;
                if (socialSecurity != null)
                {
                    int payMonth = socialSecurity.PayTime.Value.Month;
                    int monthCount = socialSecurity.PayMonthCount;
                    //相对应的签约单位城市是否已调差（社平工资）
                    EnterpriseSocialSecurity enterpriseSocialSecurity = _enterpriseService.GetEnterpriseSocialSecurity(socialSecurity.RelationEnterprise);//签约公司
                                                                                                                                                          //已调,当年以后知道年末都不需交，直到一月份开始交
                    if (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year == DateTime.Now.Year)
                    {
                        int freeBuchaMonthCount = 12 + 1 - payMonth;//免补差月数
                        int BuchaMonthCount = monthCount - freeBuchaMonthCount;
                        if (freeBuchaMonthCount < monthCount)
                        {
                            FreezingCharge = FreezingAmount * BuchaMonthCount;
                        }
                    }
                    //未调，往后每个月都需要交，许吧当年1月份到现在的都要交上
                    if (enterpriseSocialSecurity.AdjustDt == null || (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year != DateTime.Now.Year))
                    {
                        int BuchaMonthCount = payMonth - 1 + monthCount;
                        FreezingCharge = FreezingAmount * BuchaMonthCount;
                    }
                }
                #endregion


            }

            AccumulationFund accumulationFund2 = DbHelper.QuerySingle<AccumulationFund>($"select * from AccumulationFund where SocialSecurityPeopleID = {model.SocialSecurityPeopleID} and IsPay=0");

            if (accumulationFund2 != null)
            {
                string sql = $"select AccumulationFundBase * PayProportion/100*PayMonthCount from AccumulationFund where SocialSecurityPeopleID = {model.SocialSecurityPeopleID} and IsPay=0";
                AccumulationFundAmount = DbHelper.QuerySingle<decimal>(sql);
                AccumulationFundBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.AccumulationFund).BacklogCost;
            }
            model.Amount = SocialSecurityAmount + AccumulationFundAmount + SocialSecurityBacklogCost + AccumulationFundBacklogCost + FreezingCharge;
            model.IdentityCardPhoto = ConfigurationManager.AppSettings["ServerUrl"] + model.IdentityCardPhoto.Replace(";", ";" + ConfigurationManager.AppSettings["ServerUrl"]);

            //检测该参保人下是否有订单  --是否修改参保人
            if (DbHelper.QuerySingle<int>($"select count(1) from OrderDetails where SocialSecurityPeopleID={SocialSecurityPeopleID}") > 0)
            {
                model.IsCanModify = false;
            }
            else {
                model.IsCanModify = true;
            }


            bool IsGenerateSSOrder = false;
            bool IsGenerateAFOrder = false;
            SocialSecurity socialSecurity1 = DbHelper.QuerySingle<SocialSecurity>($"select * from SocialSecurity where  SocialSecurityPeopleID={SocialSecurityPeopleID} and IsGenerateOrder=1 ");
            if (socialSecurity1 != null)
                IsGenerateSSOrder = true;
            AccumulationFund accumulationFund1 = DbHelper.QuerySingle<AccumulationFund>($"select * from AccumulationFund where  SocialSecurityPeopleID={SocialSecurityPeopleID} and IsGenerateOrder=1 ");
            if (accumulationFund1 != null)
                IsGenerateAFOrder = true;


            if (IsGenerateSSOrder == true && IsGenerateAFOrder == true)
            {
                model.IsDisplayModify = false;
            }
            else {
                model.IsDisplayModify = true;
            }

            return new JsonResult<SocialSecurityPeopleDetail>
            {
                status = true,
                Message = "获取成功",
                Data = model
            };
        }

        /// <summary>
        ///  获取参保方案信息
        /// </summary>
        /// <param name="SocialSecurityPeopleID"></param>
        /// <param name="ReApply">是否重新办理</param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<SocialSecurityPeople> GetSocialSecurityScheme(int SocialSecurityPeopleID, bool ReApply = false)
        {
            SocialSecurityPeople model = new SocialSecurityPeople();
            //获取参保人信息
            string sql = $"select *  from SocialSecurityPeople where SocialSecurityPeopleID={SocialSecurityPeopleID}";
            model = DbHelper.QuerySingle<SocialSecurityPeople>(sql);
            if (ReApply == true)
            {
                SocialSecurity socialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select *  from SocialSecurity where SocialSecurityPeopleID={SocialSecurityPeopleID} and Status = 6");
                if (socialSecurity != null)
                {
                    //可显示
                    //可修改
                    model.IsPaySocialSecurity = true;
                    model.IsModifySocialSecurity = true;
                    model.socialSecurity = socialSecurity;
                }
                else {
                    //不显示
                    //不修改
                    model.IsPaySocialSecurity = false;
                    model.IsModifySocialSecurity = false;
                }

                AccumulationFund accumulationFund = DbHelper.QuerySingle<AccumulationFund>($"select *  from AccumulationFund where SocialSecurityPeopleID={SocialSecurityPeopleID} and Status = 6");
                if (accumulationFund != null)
                {
                    model.IsPayAccumulationFund = true;
                    model.IsModifyAccumulationFund = true;
                    model.accumulationFund = accumulationFund;
                }
                else {
                    //不显示
                    //不修改
                    model.IsPayAccumulationFund = false;
                    model.IsModifyAccumulationFund = false;
                }
            }
            else
            {
                SocialSecurity socialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select *  from SocialSecurity where SocialSecurityPeopleID={SocialSecurityPeopleID} and Status = 1 and IsGenerateOrder=0");
                if (socialSecurity != null)
                {
                    //可显示
                    //可修改
                    model.IsPaySocialSecurity = true;
                    model.IsModifySocialSecurity = true;
                    model.socialSecurity = socialSecurity;
                }
                else {
                    //不显示
                    model.IsPaySocialSecurity = false;
                    SocialSecurity socialSecurity1 = DbHelper.QuerySingle<SocialSecurity>($"select *  from SocialSecurity where SocialSecurityPeopleID={SocialSecurityPeopleID}");
                    if (socialSecurity1 != null)
                    {
                        //不修改
                        model.IsModifySocialSecurity = false;
                    }
                    else {
                        //可修改
                        model.IsModifySocialSecurity = true;
                    }
                }

                AccumulationFund accumulationFund = DbHelper.QuerySingle<AccumulationFund>($"select *  from AccumulationFund where SocialSecurityPeopleID={SocialSecurityPeopleID} and Status = 1 and IsGenerateOrder=0");
                if (accumulationFund != null)
                {
                    model.IsPayAccumulationFund = true;
                    model.IsModifyAccumulationFund = true;
                    model.accumulationFund = accumulationFund;
                }
                else {
                    //不显示
                    model.IsPayAccumulationFund = false;
                    AccumulationFund accumulationFund1 = DbHelper.QuerySingle<AccumulationFund>($"select *  from AccumulationFund where SocialSecurityPeopleID={SocialSecurityPeopleID}");
                    if (accumulationFund1 != null)
                    {
                        //不修改
                        model.IsModifyAccumulationFund = false;
                    }
                    else {
                        //可修改
                        model.IsModifyAccumulationFund = true;
                    }
                }
            }

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
                if (Convert.ToDateTime(socialSecurityPeople.socialSecurity.PayTime.Value.ToString("yyyy-MM")) < Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM")) || (socialSecurityPeople.socialSecurity.PayTime.Value.Month == DateTime.Now.Month && DateTime.Now.Day > 13))
                {
                    return new JsonResult<dynamic>
                    {
                        status = false,
                        Message = "参保人日期已失效，请修改"
                    };
                }


                IsExistSocialSecurityCase = true;
                socialSecurityStartTime = socialSecurityPeople.socialSecurity.PayTime.Value;
                socialSecurityEndTime = socialSecurityPeople.socialSecurity.PayTime.Value.AddMonths(socialSecurityPeople.socialSecurity.PayMonthCount - 1);
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


                #region 补差费
                //每人每月补差费用
                decimal FreezingAmount = DbHelper.QuerySingle<decimal>("select FreezingAmount from CostParameterSetting where Status = 0");

                //参保月份、参保月数、签约单位ID
                //SocialSecurity socialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select InsuranceArea,PayTime,PayMonthCount,RelationEnterprise from SocialSecurity where SocialSecurityPeopleID ={item.SocialSecurityPeopleID}");

                int payMonth = socialSecurityPeople.socialSecurity.PayTime.Value.Month;
                int monthCount = socialSecurityPeople.socialSecurity.PayMonthCount;
                //相对应的签约单位城市是否已调差（社平工资）
                //EnterpriseSocialSecurity enterpriseSocialSecurity = _enterpriseService.GetEnterpriseSocialSecurity(socialSecurity.RelationEnterprise);//签约公司
                //已调,当年以后知道年末都不需交，直到一月份开始交
                if (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year == DateTime.Now.Year)
                {
                    int freeBuchaMonthCount = 12 + 1 - payMonth;//免补差月数
                    int BuchaMonthCount = monthCount - freeBuchaMonthCount;
                    if (freeBuchaMonthCount < monthCount)
                    {
                        FreezingCharge = FreezingAmount * BuchaMonthCount;
                    }
                }
                //未调，往后每个月都需要交，许吧当年1月份到现在的都要交上
                if (enterpriseSocialSecurity.AdjustDt == null || (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year != DateTime.Now.Year))
                {
                    int BuchaMonthCount = payMonth - 1 + monthCount;
                    FreezingCharge = FreezingAmount * BuchaMonthCount;
                }
                #endregion

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
                if (Convert.ToDateTime(socialSecurityPeople.accumulationFund.PayTime.Value.ToString("yyyy-MM")) < Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM")) || (socialSecurityPeople.accumulationFund.PayTime.Value.Month == DateTime.Now.Month && DateTime.Now.Day > 13))
                {
                    return new JsonResult<dynamic>
                    {
                        status = false,
                        Message = "参保人日期已失效，请修改"
                    };
                }

                IsExistaAccumulationFundCase = true;
                AccumulationFundStartTime = socialSecurityPeople.accumulationFund.PayTime.Value;
                AccumulationFundEndTime = socialSecurityPeople.accumulationFund.PayTime.Value.AddMonths(socialSecurityPeople.accumulationFund.PayMonthCount - 1);
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
        /// 更新参保方案   socialSecurityPeople下的SocialSecurityPeopleID也需要传   IsReApply 表明是否是重新办理
        /// 1、从已停过来的 2、从修改过来的（第一次办理的和重新办理的） 分情况考虑
        /// </summary>
        /// <param name="socialSecurityPeople"></param>
        /// <returns></returns>
        public JsonResult<dynamic> ModifySocialSecurityScheme(SocialSecurityPeople socialSecurityPeople)
        {
            int SocialSecurityID = 0;
            decimal SocialSecurityAmount = 0;
            int SocialSecurityMonthCount = 0;
            decimal SocialSecurityBacklogCost = 0;
            decimal FreezingCharge = 0;
            int AccumulationFundID = 0;
            decimal AccumulationFundAmount = 0;
            int AccumulationFundMonthCount = 0;
            decimal AccumulationFundBacklogCost = 0;

            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    //如果是从重新办理过来的
                    if (socialSecurityPeople.IsReApply)
                    {
                        //保存社保参保方案
                        if (socialSecurityPeople.socialSecurity != null)
                        {
                            #region 签约公司ID，缴费比例查询
                            socialSecurityPeople.socialSecurity.SocialSecurityPeopleID = socialSecurityPeople.SocialSecurityPeopleID;
                            string sql = $"select * from EnterpriseSocialSecurity where enterpriseArea  like '%{socialSecurityPeople.socialSecurity.InsuranceArea}%' and IsDefault = 1";
                            EnterpriseSocialSecurity model = DbHelper.QuerySingle<EnterpriseSocialSecurity>(sql);

                            decimal value = 0;
                            //model.PersonalShiYeTown
                            if (socialSecurityPeople.socialSecurity.HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.InRural)) ||
                    socialSecurityPeople.socialSecurity.HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.OutRural)))
                            {
                                value = model.PersonalShiYeRural;
                            }
                            else if (socialSecurityPeople.socialSecurity.HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.InTown)) ||
                              socialSecurityPeople.socialSecurity.HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.OutTown)))
                            {
                                value = model.PersonalShiYeTown;
                            }

                            decimal PayProportion = model.CompYangLao + model.CompYiLiao + model.CompShiYe + model.CompGongShang + model.CompShengYu
                                + model.PersonalYangLao + model.PersonalYiLiao + value + model.PersonalGongShang + model.PersonalShengYu;
                            #endregion



                            //复制到中间表备用
                            try
                            {
                                DbHelper.ExecuteSqlCommand($"insert into SocialSecurityTemp select * from SocialSecurity where SocialSecurityPeopleID={socialSecurityPeople.socialSecurity.SocialSecurityPeopleID}", null);
                            }
                            catch { }

                            //更新社保方案
                            DbHelper.ExecuteSqlCommand($@"update SocialSecurity set InsuranceArea='{socialSecurityPeople.socialSecurity.InsuranceArea}',SocialSecurityBase='{socialSecurityPeople.socialSecurity.SocialSecurityBase}',PayProportion='{PayProportion}',PayTime='{socialSecurityPeople.socialSecurity.PayTime}',PayMonthCount='{socialSecurityPeople.socialSecurity.PayMonthCount}',AlreadyPayMonthCount='{socialSecurityPeople.socialSecurity.AlreadyPayMonthCount}',Note='{socialSecurityPeople.socialSecurity.Note}',Status=1,RelationEnterprise='{model.EnterpriseID}',IsReApply=1,ReApplyNum=ISNULL(ReApplyNum,0)+1,IsPay=0,IsGenerateOrder=0
  where SocialSecurityPeopleID = '{socialSecurityPeople.socialSecurity.SocialSecurityPeopleID}'", null);


                            SocialSecurityID = DbHelper.QuerySingle<int>($"select SocialSecurityID from SocialSecurity where SocialSecurityPeopleID={socialSecurityPeople.socialSecurity.SocialSecurityPeopleID}");
                            //算费用
                            SocialSecurityAmount = _socialSecurityService.GetSocialSecurityAmount(DbHelper.QuerySingle<int>($"select SocialSecurityID from SocialSecurity where SocialSecurityPeopleID={socialSecurityPeople.socialSecurity.SocialSecurityPeopleID}"));
                            //查询社保月数
                            SocialSecurityMonthCount = _socialSecurityService.GetSocialSecurityMonthCount(DbHelper.QuerySingle<int>($"select SocialSecurityID from SocialSecurity where SocialSecurityPeopleID={socialSecurityPeople.socialSecurity.SocialSecurityPeopleID}"));
                            SocialSecurityBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.SocialSecurity).BacklogCost;

                            #region 补差费
                            EnterpriseSocialSecurity enterpriseSocialSecurity = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(socialSecurityPeople.socialSecurity.InsuranceArea, socialSecurityPeople.HouseholdProperty);
                            //每人每月补差费用
                            decimal FreezingAmount = DbHelper.QuerySingle<decimal>("select FreezingAmount from CostParameterSetting where Status = 0");

                            //参保月份、参保月数、签约单位ID
                            //SocialSecurity socialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select InsuranceArea,PayTime,PayMonthCount,RelationEnterprise from SocialSecurity where SocialSecurityPeopleID ={item.SocialSecurityPeopleID}");

                            int payMonth = socialSecurityPeople.socialSecurity.PayTime.Value.Month;
                            int monthCount = socialSecurityPeople.socialSecurity.PayMonthCount;
                            //相对应的签约单位城市是否已调差（社平工资）
                            //EnterpriseSocialSecurity enterpriseSocialSecurity = _enterpriseService.GetEnterpriseSocialSecurity(socialSecurity.RelationEnterprise);//签约公司
                            //已调,当年以后知道年末都不需交，直到一月份开始交
                            if (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year == DateTime.Now.Year)
                            {
                                int freeBuchaMonthCount = 12 + 1 - payMonth;//免补差月数
                                int BuchaMonthCount = monthCount - freeBuchaMonthCount;
                                if (freeBuchaMonthCount < monthCount)
                                {
                                    FreezingCharge = FreezingAmount * BuchaMonthCount;
                                }
                            }
                            //未调，往后每个月都需要交，许吧当年1月份到现在的都要交上
                            if (enterpriseSocialSecurity.AdjustDt == null || (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year != DateTime.Now.Year))
                            {
                                int BuchaMonthCount = payMonth - 1 + monthCount;
                                FreezingCharge = FreezingAmount * BuchaMonthCount;
                            }
                            #endregion


                        }
                        else {
                            SocialSecurity socialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select * from SocialSecurity where SocialSecurityPeopleID={socialSecurityPeople.SocialSecurityPeopleID }");
                            if (socialSecurity != null)
                                SocialSecurityID = socialSecurity.SocialSecurityID;
                        }
                        //保存公积金参保方案
                        if (socialSecurityPeople.accumulationFund != null)
                        {
                            socialSecurityPeople.accumulationFund.SocialSecurityPeopleID = socialSecurityPeople.SocialSecurityPeopleID;
                            string sql = $"select * from EnterpriseSocialSecurity where enterpriseArea  like '%{socialSecurityPeople.accumulationFund.AccumulationFundArea}%' and IsDefault = 1";
                            EnterpriseSocialSecurity model = DbHelper.QuerySingle<EnterpriseSocialSecurity>(sql);

                            decimal PayProportion = model.CompProportion + model.PersonalProportion;

                            //复制到中间表备用
                            try
                            {
                                DbHelper.ExecuteSqlCommand($"insert into AccumulationFundTemp select * from AccumulationFund where SocialSecurityPeopleID={socialSecurityPeople.accumulationFund.SocialSecurityPeopleID}", null);
                            }
                            catch { }


                            //更新公积金方案
                            DbHelper.ExecuteSqlCommand($@"update AccumulationFund set AccumulationFundArea='{socialSecurityPeople.accumulationFund.AccumulationFundArea}',AccumulationFundBase='{socialSecurityPeople.accumulationFund.AccumulationFundBase}',PayProportion='{PayProportion}',PayTime='{socialSecurityPeople.accumulationFund.PayTime}',PayMonthCount='{socialSecurityPeople.accumulationFund.PayMonthCount}',Note='{socialSecurityPeople.accumulationFund.Note}',Status=1,RelationEnterprise='{model.EnterpriseID}',IsReApply=1,ReApplyNum=ISNULL(ReApplyNum,0)+1,IsPay=0,IsGenerateOrder=0,AccumulationFundType='{socialSecurityPeople.accumulationFund.AccumulationFundType}'
  where SocialSecurityPeopleID = '{socialSecurityPeople.accumulationFund.SocialSecurityPeopleID}'", null);

                            AccumulationFundID = DbHelper.QuerySingle<int>($"select AccumulationFundID from AccumulationFund where SocialSecurityPeopleID={socialSecurityPeople.accumulationFund.SocialSecurityPeopleID}");

                            AccumulationFundAmount = _socialSecurityService.GetAccumulationFundAmount(DbHelper.QuerySingle<int>($"select AccumulationFundID from AccumulationFund where SocialSecurityPeopleID={socialSecurityPeople.accumulationFund.SocialSecurityPeopleID }"));
                            //查询公积金月数
                            AccumulationFundMonthCount = _socialSecurityService.GetAccumulationFundMonthCount(DbHelper.QuerySingle<int>($"select AccumulationFundID from AccumulationFund where SocialSecurityPeopleID={socialSecurityPeople.accumulationFund.SocialSecurityPeopleID }"));
                            AccumulationFundBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.AccumulationFund).BacklogCost;


                        }
                        else {
                            AccumulationFund accumulationFund = DbHelper.QuerySingle<AccumulationFund>($"select * from AccumulationFund where SocialSecurityPeopleID={socialSecurityPeople.SocialSecurityPeopleID }");
                            if (accumulationFund != null)
                                AccumulationFundID = accumulationFund.AccumulationFundID;
                        }
                    }
                    else {
                        //查看前台有没有保存社保，
                        //如果有，则查询参保人ID对应的社保信息，如果有社保信息，则判断是否是重新办理的用户，如果是重新办理的用户，则进行更新操作，如果不是则删除后添加；如果没有社保信息，则添加
                        //如果没有，则查询参保人ID对应的社保信息，如果有社保信息，则判断是否是重新办理的用户，如果是重新办理的用户，回滚（）变成停保，如果不是则删除原有记录；如果没有社保信息，则不操作

                        ////删除该参保人下的参保方案
                        //string sqlDel = $"delete from SocialSecurity where SocialSecurityPeopleID={socialSecurityPeople.SocialSecurityPeopleID};"
                        //           + $" delete from AccumulationFund where SocialSecurityPeopleID ={socialSecurityPeople.SocialSecurityPeopleID};";
                        //DbHelper.ExecuteSqlCommand(sqlDel, null);

                        //保存社保参保方案
                        if (socialSecurityPeople.socialSecurity != null)
                        {
                            #region 签约公司ID，缴费比例查询
                            socialSecurityPeople.socialSecurity.SocialSecurityPeopleID = socialSecurityPeople.SocialSecurityPeopleID;
                            SocialSecurity SocialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select * from SocialSecurity where SocialSecurityPeopleID={ socialSecurityPeople.socialSecurity.SocialSecurityPeopleID}");
                            if (SocialSecurity != null)
                            {
                                socialSecurityPeople.socialSecurity.SocialSecurityPeopleID = socialSecurityPeople.SocialSecurityPeopleID;
                                string sql = $"select * from EnterpriseSocialSecurity where enterpriseArea  like '%{socialSecurityPeople.socialSecurity.InsuranceArea}%' and IsDefault = 1";
                                EnterpriseSocialSecurity model = DbHelper.QuerySingle<EnterpriseSocialSecurity>(sql);

                                decimal value = 0;
                                //model.PersonalShiYeTown
                                if (socialSecurityPeople.socialSecurity.HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.InRural)) ||
                        socialSecurityPeople.socialSecurity.HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.OutRural)))
                                {
                                    value = model.PersonalShiYeRural;
                                }
                                else if (socialSecurityPeople.socialSecurity.HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.InTown)) ||
                                  socialSecurityPeople.socialSecurity.HouseholdProperty == EnumExt.GetEnumCustomDescription((HouseholdPropertyEnum)((int)HouseholdPropertyEnum.OutTown)))
                                {
                                    value = model.PersonalShiYeTown;
                                }

                                decimal PayProportion = model.CompYangLao + model.CompYiLiao + model.CompShiYe + model.CompGongShang + model.CompShengYu
                                    + model.PersonalYangLao + model.PersonalYiLiao + value + model.PersonalGongShang + model.PersonalShengYu;

                                #endregion

                                if (SocialSecurity.ReApplyNum > 0)
                                {
                                    //更新社保方案
                                    DbHelper.ExecuteSqlCommand($@"update SocialSecurity set InsuranceArea='{socialSecurityPeople.socialSecurity.InsuranceArea}',SocialSecurityBase='{socialSecurityPeople.socialSecurity.SocialSecurityBase}',PayProportion='{PayProportion}',PayTime='{socialSecurityPeople.socialSecurity.PayTime}',PayMonthCount='{socialSecurityPeople.socialSecurity.PayMonthCount}',Note='{socialSecurityPeople.socialSecurity.Note}',Status=1,RelationEnterprise='{model.EnterpriseID}'
  where SocialSecurityPeopleID = '{socialSecurityPeople.socialSecurity.SocialSecurityPeopleID}'", null);


                                    SocialSecurityID = SocialSecurity.SocialSecurityID;
                                    //查询社保金额
                                    SocialSecurityAmount = _socialSecurityService.GetSocialSecurityAmount(SocialSecurity.SocialSecurityID);
                                    //查询社保月数
                                    SocialSecurityMonthCount = _socialSecurityService.GetSocialSecurityMonthCount(SocialSecurity.SocialSecurityID);
                                    SocialSecurityBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.SocialSecurity).BacklogCost;

                                    #region 补差费
                                    EnterpriseSocialSecurity enterpriseSocialSecurity = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(socialSecurityPeople.socialSecurity.InsuranceArea, socialSecurityPeople.HouseholdProperty);
                                    //每人每月补差费用
                                    decimal FreezingAmount = DbHelper.QuerySingle<decimal>("select FreezingAmount from CostParameterSetting where Status = 0");

                                    //参保月份、参保月数、签约单位ID
                                    //SocialSecurity socialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select InsuranceArea,PayTime,PayMonthCount,RelationEnterprise from SocialSecurity where SocialSecurityPeopleID ={item.SocialSecurityPeopleID}");

                                    int payMonth = socialSecurityPeople.socialSecurity.PayTime.Value.Month;
                                    int monthCount = socialSecurityPeople.socialSecurity.PayMonthCount;
                                    //相对应的签约单位城市是否已调差（社平工资）
                                    //EnterpriseSocialSecurity enterpriseSocialSecurity = _enterpriseService.GetEnterpriseSocialSecurity(socialSecurity.RelationEnterprise);//签约公司
                                    //已调,当年以后知道年末都不需交，直到一月份开始交
                                    if (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year == DateTime.Now.Year)
                                    {
                                        int freeBuchaMonthCount = 12 + 1 - payMonth;//免补差月数
                                        int BuchaMonthCount = monthCount - freeBuchaMonthCount;
                                        if (freeBuchaMonthCount < monthCount)
                                        {
                                            FreezingCharge = FreezingAmount * BuchaMonthCount;
                                        }
                                    }
                                    //未调，往后每个月都需要交，许吧当年1月份到现在的都要交上
                                    if (enterpriseSocialSecurity.AdjustDt == null || (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year != DateTime.Now.Year))
                                    {
                                        int BuchaMonthCount = payMonth - 1 + monthCount;
                                        FreezingCharge = FreezingAmount * BuchaMonthCount;
                                    }
                                    #endregion

                                }

                                else {
                                    //删除参保方案
                                    DbHelper.ExecuteSqlCommand($"delete from SocialSecurity where SocialSecurityPeopleID={socialSecurityPeople.SocialSecurityPeopleID};", null);


                                    SocialSecurityID = _socialSecurityService.AddSocialSecurity(socialSecurityPeople.socialSecurity);
                                    //查询社保金额
                                    SocialSecurityAmount = _socialSecurityService.GetSocialSecurityAmount(SocialSecurityID);
                                    //查询社保月数
                                    SocialSecurityMonthCount = _socialSecurityService.GetSocialSecurityMonthCount(SocialSecurityID);
                                    SocialSecurityBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.SocialSecurity).BacklogCost;

                                    #region 补差费
                                    EnterpriseSocialSecurity enterpriseSocialSecurity = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(socialSecurityPeople.socialSecurity.InsuranceArea, socialSecurityPeople.HouseholdProperty);
                                    //每人每月补差费用
                                    decimal FreezingAmount = DbHelper.QuerySingle<decimal>("select FreezingAmount from CostParameterSetting where Status = 0");

                                    //参保月份、参保月数、签约单位ID
                                    //SocialSecurity socialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select InsuranceArea,PayTime,PayMonthCount,RelationEnterprise from SocialSecurity where SocialSecurityPeopleID ={item.SocialSecurityPeopleID}");

                                    int payMonth = socialSecurityPeople.socialSecurity.PayTime.Value.Month;
                                    int monthCount = socialSecurityPeople.socialSecurity.PayMonthCount;
                                    //相对应的签约单位城市是否已调差（社平工资）
                                    //EnterpriseSocialSecurity enterpriseSocialSecurity = _enterpriseService.GetEnterpriseSocialSecurity(socialSecurity.RelationEnterprise);//签约公司
                                    //已调,当年以后知道年末都不需交，直到一月份开始交
                                    if (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year == DateTime.Now.Year)
                                    {
                                        int freeBuchaMonthCount = 12 + 1 - payMonth;//免补差月数
                                        int BuchaMonthCount = monthCount - freeBuchaMonthCount;
                                        if (freeBuchaMonthCount < monthCount)
                                        {
                                            FreezingCharge = FreezingAmount * BuchaMonthCount;
                                        }
                                    }
                                    //未调，往后每个月都需要交，许吧当年1月份到现在的都要交上
                                    if (enterpriseSocialSecurity.AdjustDt == null || (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year != DateTime.Now.Year))
                                    {
                                        int BuchaMonthCount = payMonth - 1 + monthCount;
                                        FreezingCharge = FreezingAmount * BuchaMonthCount;
                                    }
                                    #endregion
                                }
                            }
                            else {

                                SocialSecurityID = _socialSecurityService.AddSocialSecurity(socialSecurityPeople.socialSecurity);
                                //查询社保金额
                                SocialSecurityAmount = _socialSecurityService.GetSocialSecurityAmount(SocialSecurityID);
                                //查询社保月数
                                SocialSecurityMonthCount = _socialSecurityService.GetSocialSecurityMonthCount(SocialSecurityID);
                                SocialSecurityBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.SocialSecurity).BacklogCost;

                                #region 补差费
                                EnterpriseSocialSecurity enterpriseSocialSecurity = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(socialSecurityPeople.socialSecurity.InsuranceArea, socialSecurityPeople.HouseholdProperty);
                                //每人每月补差费用
                                decimal FreezingAmount = DbHelper.QuerySingle<decimal>("select FreezingAmount from CostParameterSetting where Status = 0");

                                //参保月份、参保月数、签约单位ID
                                //SocialSecurity socialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select InsuranceArea,PayTime,PayMonthCount,RelationEnterprise from SocialSecurity where SocialSecurityPeopleID ={item.SocialSecurityPeopleID}");

                                int payMonth = socialSecurityPeople.socialSecurity.PayTime.Value.Month;
                                int monthCount = socialSecurityPeople.socialSecurity.PayMonthCount;
                                //相对应的签约单位城市是否已调差（社平工资）
                                //EnterpriseSocialSecurity enterpriseSocialSecurity = _enterpriseService.GetEnterpriseSocialSecurity(socialSecurity.RelationEnterprise);//签约公司
                                //已调,当年以后知道年末都不需交，直到一月份开始交
                                if (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year == DateTime.Now.Year)
                                {
                                    int freeBuchaMonthCount = 12 + 1 - payMonth;//免补差月数
                                    int BuchaMonthCount = monthCount - freeBuchaMonthCount;
                                    if (freeBuchaMonthCount < monthCount)
                                    {
                                        FreezingCharge = FreezingAmount * BuchaMonthCount;
                                    }
                                }
                                //未调，往后每个月都需要交，许吧当年1月份到现在的都要交上
                                if (enterpriseSocialSecurity.AdjustDt == null || (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year != DateTime.Now.Year))
                                {
                                    int BuchaMonthCount = payMonth - 1 + monthCount;
                                    FreezingCharge = FreezingAmount * BuchaMonthCount;
                                }
                                #endregion
                            }
                        }
                        else {
                            // socialSecurityPeople.socialSecurity.SocialSecurityPeopleID = socialSecurityPeople.SocialSecurityPeopleID;
                            SocialSecurity SocialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select * from SocialSecurity where SocialSecurityPeopleID={ socialSecurityPeople.SocialSecurityPeopleID}");
                            if (SocialSecurity != null)
                            {
                                if (SocialSecurity.ReApplyNum > 0 && SocialSecurity.Status == "1")
                                {
                                    SocialSecurityID = SocialSecurity.SocialSecurityID;

                                    //从中间表回滚社保表，并删除中间表记录
                                    SocialSecurity SocialSecurityTemp = DbHelper.QuerySingle<SocialSecurity>($"select * from SocialSecurityTemp where SocialSecurityPeopleID={  socialSecurityPeople.SocialSecurityPeopleID}");
                                    DbHelper.ExecuteSqlCommand($@"UPDATE SocialSecurity
                                                                   SET SocialSecurityPeopleID = '{SocialSecurityTemp.SocialSecurityPeopleID}'
                                                                      ,InsuranceArea = '{SocialSecurityTemp.InsuranceArea}'
                                                                      ,SocialSecurityBase = '{SocialSecurityTemp.SocialSecurityBase}'
                                                                      ,PayProportion = '{SocialSecurityTemp.PayProportion}'
                                                                      ,PayTime = '{SocialSecurityTemp.PayTime}'
                                                                      ,PayMonthCount = '{SocialSecurityTemp.PayMonthCount}'
                                                                      ,AlreadyPayMonthCount = '{SocialSecurityTemp.AlreadyPayMonthCount}'
                                                                      ,PayBeforeMonthCount = '{SocialSecurityTemp.PayBeforeMonthCount}'
                                                                      ,BankPayMonth = '{SocialSecurityTemp.BankPayMonth}'
                                                                      ,EnterprisePayMonth ='{SocialSecurityTemp.EnterprisePayMonth}'
                                                                      ,Note = '{SocialSecurityTemp.Note}'
                                                                      ,Status ='{SocialSecurityTemp.Status}'
                                                                      ,RelationEnterprise ='{SocialSecurityTemp.RelationEnterprise}'
                                                                      ,PayedMonthCount = '{SocialSecurityTemp.PayedMonthCount}'
                                                                      ,StopMethod = '{SocialSecurityTemp.StopMethod}'
                                                                      ,StopReason = '{SocialSecurityTemp.StopReason}'
                                                                      ,ApplyStopDate = '{SocialSecurityTemp.ApplyStopDate}'
                                                                      ,StopDate = '{SocialSecurityTemp.StopDate}'
                                                                      ,SocialSecurityNo = '{SocialSecurityTemp.SocialSecurityNo}'
                                                                      ,SocialSecurityException = '{SocialSecurityTemp.SocialSecurityException}'
                                                                      ,HandleDate ='{SocialSecurityTemp.HandleDate}'
                                                                      ,MailAddress = '{SocialSecurityTemp.MailAddress}'
                                                                      ,ContactsPhone = '{SocialSecurityTemp.ContactsPhone}'
                                                                      ,ContactsUser = '{SocialSecurityTemp.ContactsUser}'
                                                                      ,CollectType = '{SocialSecurityTemp.CollectType}'
                                                                      ,MailOrder = '{SocialSecurityTemp.MailOrder}'
                                                                      ,ExpressCompany = '{SocialSecurityTemp.ExpressCompany}'
                                                                      ,IsPay ='{SocialSecurityTemp.IsPay}'
                                                                      ,CustomerServiceAuditStatus = '{SocialSecurityTemp.CustomerServiceAuditStatus}'
                                                                      ,IsReApply = '{SocialSecurityTemp.IsReApply}'
                                                                      ,ReApplyNum = '{SocialSecurityTemp.ReApplyNum}'
                                                                      ,IsGenerateOrder='{SocialSecurityTemp.IsGenerateOrder}'
                                                                 WHERE SocialSecurityID={SocialSecurityTemp.SocialSecurityID}", null);
                                    //删除社保临时表
                                    DbHelper.ExecuteSqlCommand($"delete from SocialSecurityTemp where SocialSecurityPeopleID={  socialSecurityPeople.SocialSecurityPeopleID}", null);
                                }
                                if (SocialSecurity.ReApplyNum == 0 && SocialSecurity.Status == "1")
                                {
                                    //删除参保方案
                                    DbHelper.ExecuteSqlCommand($"delete from SocialSecurity where SocialSecurityPeopleID={socialSecurityPeople.SocialSecurityPeopleID};", null);

                                }
                            }
                        }

                        //保存公积金参保方案
                        if (socialSecurityPeople.accumulationFund != null)
                        {
                            socialSecurityPeople.accumulationFund.SocialSecurityPeopleID = socialSecurityPeople.SocialSecurityPeopleID;
                            AccumulationFund accumulationFund = DbHelper.QuerySingle<AccumulationFund>($"select * from AccumulationFund where SocialSecurityPeopleID={ socialSecurityPeople.accumulationFund.SocialSecurityPeopleID}");

                            if (accumulationFund != null)
                            {
                                #region 签约公司ID，缴费比例查询
                                EnterpriseSocialSecurity model = DbHelper.QuerySingle<EnterpriseSocialSecurity>($"select * from EnterpriseSocialSecurity where enterpriseArea  like '%{socialSecurityPeople.accumulationFund.AccumulationFundArea}%' and IsDefault = 1");

                                decimal PayProportion = model.CompProportion + model.PersonalProportion;
                                #endregion
                                if (accumulationFund.ReApplyNum > 0)
                                {
                                    //更新公积金方案
                                    DbHelper.ExecuteSqlCommand($@"update AccumulationFund set AccumulationFundArea='{socialSecurityPeople.accumulationFund.AccumulationFundArea}',AccumulationFundBase='{socialSecurityPeople.accumulationFund.AccumulationFundBase}',PayProportion='{PayProportion}',PayTime='{socialSecurityPeople.accumulationFund.PayTime}',PayMonthCount='{socialSecurityPeople.accumulationFund.PayMonthCount}',Note='{socialSecurityPeople.accumulationFund.Note}',Status=1,RelationEnterprise='{model.EnterpriseID}',AccumulationFundType='{socialSecurityPeople.accumulationFund.AccumulationFundType}'
  where SocialSecurityPeopleID = '{socialSecurityPeople.accumulationFund.SocialSecurityPeopleID}'", null);

                                    AccumulationFundID = accumulationFund.AccumulationFundID;
                                    //查询公积金金额
                                    AccumulationFundAmount = _socialSecurityService.GetAccumulationFundAmount(accumulationFund.AccumulationFundID);
                                    //查询公积金月数
                                    AccumulationFundMonthCount = _socialSecurityService.GetAccumulationFundMonthCount(accumulationFund.AccumulationFundID);
                                    AccumulationFundBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.AccumulationFund).BacklogCost;
                                    /**********************/
                                }
                                else {
                                    //删除公积金方案
                                    DbHelper.ExecuteSqlCommand($"delete from AccumulationFund where SocialSecurityPeopleID={socialSecurityPeople.SocialSecurityPeopleID};", null);

                                    AccumulationFundID = _socialSecurityService.AddAccumulationFund(socialSecurityPeople.accumulationFund);
                                    //查询公积金金额
                                    AccumulationFundAmount = _socialSecurityService.GetAccumulationFundAmount(AccumulationFundID);
                                    //查询公积金月数
                                    AccumulationFundMonthCount = _socialSecurityService.GetAccumulationFundMonthCount(AccumulationFundID);
                                    AccumulationFundBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.AccumulationFund).BacklogCost;
                                }
                            }
                            else {
                                AccumulationFundID = _socialSecurityService.AddAccumulationFund(socialSecurityPeople.accumulationFund);
                                //查询公积金金额
                                AccumulationFundAmount = _socialSecurityService.GetAccumulationFundAmount(AccumulationFundID);
                                //查询公积金月数
                                AccumulationFundMonthCount = _socialSecurityService.GetAccumulationFundMonthCount(AccumulationFundID);
                                AccumulationFundBacklogCost = _parameterSettingService.GetCostParameter((int)PayTypeEnum.AccumulationFund).BacklogCost;
                            }
                        }
                        else {
                            //socialSecurityPeople.accumulationFund.SocialSecurityPeopleID = socialSecurityPeople.SocialSecurityPeopleID;
                            AccumulationFund accumulationFund = DbHelper.QuerySingle<AccumulationFund>($"select * from AccumulationFund where SocialSecurityPeopleID={ socialSecurityPeople.SocialSecurityPeopleID}");
                            if (accumulationFund != null)
                            {
                                if (accumulationFund.ReApplyNum > 0 && accumulationFund.Status == "1")
                                {
                                    AccumulationFundID = accumulationFund.AccumulationFundID;


                                    ////从中间表回滚社保表，并删除中间表记录
                                    AccumulationFund AccumulationFundTemp = DbHelper.QuerySingle<AccumulationFund>($"select * from AccumulationFundTemp where SocialSecurityPeopleID={ socialSecurityPeople.SocialSecurityPeopleID}");
                                    DbHelper.ExecuteSqlCommand($@"UPDATE WYJK.dbo.AccumulationFund
                                                               SET SocialSecurityPeopleID = '{AccumulationFundTemp.SocialSecurityPeopleID}'
                                                                  ,AccumulationFundArea = '{AccumulationFundTemp.AccumulationFundArea}'
                                                                  ,AccumulationFundBase = '{AccumulationFundTemp.AccumulationFundBase}'
                                                                  ,PayProportion = '{AccumulationFundTemp.PayProportion}'
                                                                  ,PayTime = '{AccumulationFundTemp.PayTime}'
                                                                  ,PayMonthCount = '{AccumulationFundTemp.PayMonthCount}'
                                                                  ,AlreadyPayMonthCount = '{AccumulationFundTemp.AlreadyPayMonthCount}'
                                                                  ,PayBeforeMonthCount = '{AccumulationFundTemp.PayBeforeMonthCount}'
                                                                  ,Note = '{AccumulationFundTemp.Note}'
                                                                  ,Status = '{AccumulationFundTemp.Status}'
                                                                  ,RelationEnterprise = '{AccumulationFundTemp.RelationEnterprise}'
                                                                  ,PayedMonthCount = '{AccumulationFundTemp.PayedMonthCount}'
                                                                  ,StopMethod = '{AccumulationFundTemp.StopMethod}'
                                                                  ,ApplyStopDate = '{AccumulationFundTemp.ApplyStopDate}'
                                                                  ,StopDate = '{AccumulationFundTemp.StopDate}'
                                                                  ,AccumulationFundNo = '{AccumulationFundTemp.AccumulationFundNo}'
                                                                  ,AccumulationFundException = '{AccumulationFundTemp.AccumulationFundException}'
                                                                  ,HandleDate ='{AccumulationFundTemp.HandleDate}'
                                                                  ,AccumulationFundType = '{AccumulationFundTemp.AccumulationFundType}'
                                                                  ,CompanyName = '{AccumulationFundTemp.CompanyName}'
                                                                  ,CompanyAccumulationFundCode = '{AccumulationFundTemp.CompanyAccumulationFundCode}'
                                                                  ,AccumulationFundTopType = '{AccumulationFundTemp.AccumulationFundTopType}'
                                                                  ,IsPay ='{AccumulationFundTemp.IsPay}'
                                                                  ,CustomerServiceAuditStatus ='{AccumulationFundTemp.CustomerServiceAuditStatus}'
                                                                  ,IsReApply = '{AccumulationFundTemp.IsReApply}'
                                                                  ,ReApplyNum ='{AccumulationFundTemp.ReApplyNum}'
                                                                  ,IsGenerateOrder='{AccumulationFundTemp.IsGenerateOrder}'
                                                                 WHERE AccumulationFundID={AccumulationFundTemp.AccumulationFundID}", null);
                                    //删除社保临时表
                                    DbHelper.ExecuteSqlCommand($"delete from AccumulationFundTemp where SocialSecurityPeopleID={socialSecurityPeople.SocialSecurityPeopleID}", null);
                                }
                                if (accumulationFund.ReApplyNum == 0 && accumulationFund.Status == "1")
                                {
                                    //删除参保方案
                                    DbHelper.ExecuteSqlCommand($"delete from AccumulationFund where SocialSecurityPeopleID={socialSecurityPeople.SocialSecurityPeopleID};", null);

                                }
                            }
                        }
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
                    Amount = SocialSecurityAmount * SocialSecurityMonthCount + AccumulationFundAmount * AccumulationFundMonthCount + SocialSecurityBacklogCost + AccumulationFundBacklogCost + FreezingCharge
                }
            };
        }


        /// <summary>
        /// 获取公积金办理类型
        /// </summary>
        /// <returns></returns>
        public JsonResult<dynamic> GetAccumulationFundTypeList()
        {
            List<Property> list = SelectListClass.GetSelectList(typeof(AccumulationFundTypeEnum));
            return new JsonResult<dynamic>
            {
                status = true,
                Message = "获取成功",
                Data = list
            };
        }

        /// <summary>
        /// 选择公积金转移时获取企业公积金编号与企业名称
        /// </summary>
        /// <param name="area"></param>
        /// <param name="HouseholdProperty"></param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public JsonResult<dynamic> AccumulationFundTransferShow(string area, string HouseholdProperty)
        {
            EnterpriseSocialSecurity model = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(area, HouseholdProperty);

            return new JsonResult<dynamic>
            {
                status = true,
                Message = "获取成功",
                Data = new
                {
                    EnterpriseName = model.EnterpriseName,
                    AccumulationFundCode = model.AccumulationFundCode
                }
            };
        }

        /// <summary>
        /// 获取公积金办停类型
        /// </summary>
        /// <returns></returns>
        public JsonResult<dynamic> GetAccumulationFundTopTypeList()
        {
            //13号到15号不允许点击停保
            if (DateTime.Now.Day >= 13 && DateTime.Now.Day <= 15)
            {
                return new JsonResult<dynamic>
                {
                    status = false,
                    Message = "13号到15号不允许点击停公积金"
                };
            }
            //提示时间
            string dateTip = string.Empty;
            if (DateTime.Now.Day < 13)
            {
                dateTip = "缴费月数截止到" + DateTime.Now.Year + "年" + DateTime.Now.Month + "月";
            }
            if (DateTime.Now.Day > 15)
            {
                DateTime datetime = DateTime.Now.AddMonths(1);
                dateTip = "缴费月数截止到" + datetime.Year + "年" + datetime.Month + "月";
            }

            List<Property> list = SelectListClass.GetSelectList(typeof(AccumulationFundTopTypeEnum));
            return new JsonResult<dynamic>
            {
                status = true,
                Message = "获取成功",
                Data = new { dateTip = dateTip, list = list }
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
            decimal FreezingCharge = 0;
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

                        #region 补差费
                        EnterpriseSocialSecurity enterpriseSocialSecurity = _socialSecurityService.GetDefaultEnterpriseSocialSecurityByArea(socialSecurityPeople.socialSecurity.InsuranceArea, socialSecurityPeople.HouseholdProperty);
                        //每人每月补差费用
                        decimal FreezingAmount = DbHelper.QuerySingle<decimal>("select FreezingAmount from CostParameterSetting where Status = 0");

                        //参保月份、参保月数、签约单位ID
                        //SocialSecurity socialSecurity = DbHelper.QuerySingle<SocialSecurity>($"select InsuranceArea,PayTime,PayMonthCount,RelationEnterprise from SocialSecurity where SocialSecurityPeopleID ={item.SocialSecurityPeopleID}");

                        int payMonth = socialSecurityPeople.socialSecurity.PayTime.Value.Month;
                        int monthCount = socialSecurityPeople.socialSecurity.PayMonthCount;
                        //相对应的签约单位城市是否已调差（社平工资）
                        //EnterpriseSocialSecurity enterpriseSocialSecurity = _enterpriseService.GetEnterpriseSocialSecurity(socialSecurity.RelationEnterprise);//签约公司
                        //已调,当年以后知道年末都不需交，直到一月份开始交
                        if (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year == DateTime.Now.Year)
                        {
                            int freeBuchaMonthCount = 12 + 1 - payMonth;//免补差月数
                            int BuchaMonthCount = monthCount - freeBuchaMonthCount;
                            if (freeBuchaMonthCount < monthCount)
                            {
                                FreezingCharge = FreezingAmount * BuchaMonthCount;
                            }
                        }
                        //未调，往后每个月都需要交，许吧当年1月份到现在的都要交上
                        if (enterpriseSocialSecurity.AdjustDt == null || (enterpriseSocialSecurity.AdjustDt != null && enterpriseSocialSecurity.AdjustDt.Value.Year != DateTime.Now.Year))
                        {
                            int BuchaMonthCount = payMonth - 1 + monthCount;
                            FreezingCharge = FreezingAmount * BuchaMonthCount;
                        }
                        #endregion



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
                    Amount = SocialSecurityAmount * SocialSecurityMonthCount + AccumulationFundAmount * AccumulationFundMonthCount + SocialSecurityBacklogCost + AccumulationFundBacklogCost + FreezingCharge
                }
            };
        }

        /// <summary>
        /// 获取待办理列表    状态为 1和2的都显示为待办,当判断未参保1的时候，还需要判断ssIsPay afIsPay 是否支付过，只有支付过才显示
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public JsonResult<List<SocialSecurityPeoples>> GetWaitingHandleListByStatus(int MemberID)
        {

            string sql = "select ssp.SocialSecurityPeopleID,ssp.SocialSecurityPeopleName,ss.IsPay ssIsPay, ss.PayTime SSPayTime,ISNULL(ss.AlreadyPayMonthCount,0) SSAlreadyPayMonthCount,ss.Status SSStatus,ss.PayMonthCount SSRemainingMonthCount,af.IsPay afIsPay, af.PayTime AFPayTime,ISNULL(af.AlreadyPayMonthCount,0) AFAlreadyPayMonthCount,af.Status AFStatus,af.PayMonthCount AFRemainingMonthCount"
            + " from SocialSecurityPeople ssp"
            + " left join SocialSecurity ss on ssp.SocialSecurityPeopleID = ss.SocialSecurityPeopleID"
            + " left join AccumulationFund af on ssp.SocialSecurityPeopleID = af.SocialSecurityPeopleID"
            + $" where (ss.Status = {(int)SocialSecurityStatusEnum.WaitingHandle} or af.Status = {(int)SocialSecurityStatusEnum.WaitingHandle} or (ss.Status = {(int)SocialSecurityStatusEnum.UnInsured} and ss.IsPay=1) or (af.Status = {(int)SocialSecurityStatusEnum.UnInsured} and af.IsPay =1)) and ssp.MemberID = {MemberID}";
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

            if (model.SSStatus != null && model.SSStatus == 3)
                model.IsSocialSecurity = true;
            if (model.AFStatus != null && model.AFStatus == 3)
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
        /// 获取已停列表  1、自取 2、邮寄
        /// </summary>
        /// <param name="MemberID"></param>
        /// <returns></returns>
        public JsonResult<List<TopSocialSecurityPeoples>> GetAlreadyStop(int MemberID)
        {
            string sql = "select ssp.SocialSecurityPeopleID,ssp.SocialSecurityPeopleName,ss.CollectType,ss.MailOrder,ss.ExpressCompany, ss.PayTime SSPayTime,ISNULL(ss.AlreadyPayMonthCount,0) SSAlreadyPayMonthCount,ss.Status SSStatus,ss.StopReason SSStopReason, ss.ApplyStopDate SSApplyStopDate,ss.StopDate SSStopDate,af.StopDate AFStopDate, ss.PayMonthCount SSRemainingMonthCount, af.PayTime AFPayTime,ISNULL(af.AlreadyPayMonthCount,0) AFAlreadyPayMonthCount,af.Status AFStatus,af.ApplyStopDate AFApplyStopDate,af.PayMonthCount AFRemainingMonthCount"
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
        /// 调整基数提交    PlatType:  Web/0;Mobile/1
        /// </summary>
        /// <returns></returns>
        public JsonResult<dynamic> PostAdjustingBase(AdjustingBaseParameter parameter)
        {
            int orderID = _socialSecurityService.AddAdjustingBase(parameter);

            if (orderID > 0)
            {
                BaseOrders baseOrders = DbHelper.QuerySingle<BaseOrders>($"select * from BaseOrders where OrderID={orderID}");

                string BranchID = "0532";
                string CoNo = "019387";
                string BillNo = orderID.ToString().PadLeft(10, '0');
                string Amount = Convert.ToDecimal(baseOrders.SSBaseServiceCharge + baseOrders.AFBaseServiceCharge).ToString();
                string Date = DateTime.Now.ToString("yyyyMMdd");
                string MerchantUrl = ConfigurationManager.AppSettings["ServerUrl"] + "api/socialsecurity/AdjustingBase_Return";
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
        /// 调基回调
        /// </summary>
        /// <param name="Succeed"></param>
        /// <param name="BillNo"></param>
        /// <param name="Amount"></param>
        /// <param name="Date"></param>
        /// <param name="Msg"></param>
        /// <param name="Signature"></param>
        [System.Web.Http.HttpGet]
        public void AdjustingBase_Return(string Succeed, string BillNo, string Amount, string Date, string Msg, string Signature)
        {
            #region 招行提供
            /*
             * 必须验证返回数据的有效性防止订单信息支付过程中被篡改
             * 先判断是否支付成功
             * 验证支付成功还要验证支付金额是否和订单的金额一致
             */
            string ReturnInfo = "Succeed=" + Succeed + "&BillNo=" + BillNo + "&Amount=" + Amount + "&Date=" + Date + "&Msg=" + Msg + "&Signature=" + Signature;
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
                    throw new Exception(err);
                    //Response.Write("<script>alert('" + err + "')</script>");
                    //return;
                }
                if (Succeed.Trim() != "Y")//验证支付结果是否成功
                {
                    throw new Exception("支付失败！");
                    //Response.Write("<script>alert('支付失败！')</script>");
                    //return;
                }
                decimal payMoney = 5M;  //订单的金额也就是CMBChina_PayMoney.aspx页面中输入的金额 这里只是简单的测试实际运用中请使用实际支付值 
                if (payMoney != Convert.ToDecimal(Amount))//验证银行实际收到与支付金额是否相等
                {
                    throw new Exception("支付金额与订单金额不一致！");
                    //Response.Write("<script>alert('支付金额与订单金额不一致！')</script>");
                    //return;
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
                string orderID = BillNo.TrimStart('0');
                BaseOrders baseOrders = DbHelper.QuerySingle<BaseOrders>($"select * from BaseOrders where OrderID='{orderID}'");
                if (baseOrders.Status == "0")
                {
                    //更新订单
                    DbHelper.ExecuteSqlCommand($"update BaseOrders set Status=1 where OrderID='{orderID}'", null);

                    decimal account = DbHelper.QuerySingle<decimal>($"select Account from Members where MemberID={baseOrders.MemberID}");

                    //生成日志
                    string sqlAccountRecord = $@"insert into AccountRecord(SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime)
    values({DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random(Guid.NewGuid().GetHashCode()).Next(1000).ToString().PadLeft(3, '0')},{baseOrders.MemberID},{baseOrders.SocialSecurityPeopleID},'','收入','银行卡','调基费',{baseOrders.SSBaseServiceCharge + baseOrders.AFBaseServiceCharge},{account + baseOrders.SSBaseServiceCharge + baseOrders.AFBaseServiceCharge},getdate());";

                    sqlAccountRecord += $@"insert into AccountRecord(SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime)
    values({DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random(Guid.NewGuid().GetHashCode()).Next(1000).ToString().PadLeft(3, '0')},{baseOrders.MemberID},{baseOrders.SocialSecurityPeopleID},'','支出','余额','调基费',{baseOrders.SSBaseServiceCharge + baseOrders.AFBaseServiceCharge},{account },getdate());";

                    DbHelper.ExecuteSqlCommand(sqlAccountRecord, null);
                }
            }
        }



        #region 参保介绍列表
        /// <summary>
        /// 获取参保介绍列表
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult<List<InsuredIntroduce>>> GetInsuredIntroduceList()
        {
            List<InsuredIntroduce> insuredIntroduceList = await _insuredIntroduceService.GetInsuredIntroduceList();

            return new JsonResult<List<InsuredIntroduce>>
            {
                status = true,
                Message = "获取成功",
                Data = insuredIntroduceList
            };
        }
        #endregion

        #region 服务协议
        ///// <summary>
        ///// 检测用户是否已经同意服务协议
        ///// </summary>
        ///// <param name="memberID"></param>
        ///// <returns></returns>
        //[System.Web.Http.HttpGet]
        //public JsonResult<dynamic> IsAgreeProtocol(int memberID)
        //{
        //    bool isAgreeProtocol = false;
        //    Members members = DbHelper.QuerySingle<Members>($"select IsAgreeProtocol from Members where MemberID={memberID}");
        //    if (members != null)
        //        isAgreeProtocol = members.IsAgreeProtocol;

        //    return new JsonResult<dynamic>
        //    {
        //        status = true,
        //        Message = "获取成功",
        //        Data = isAgreeProtocol
        //    };
        //}
        /// <summary>
        /// 获取服务协议内容
        /// </summary>
        /// <returns></returns>
        public JsonResult<dynamic> GetServiceProtocol()
        {
            ServiceProtocol serviceProtocol = DbHelper.QuerySingle<ServiceProtocol>("select ServiceProtocolContent from ServiceProtocol");
            return new JsonResult<dynamic>
            {
                status = true,
                Message = "获取成功",
                Data = serviceProtocol.ServiceProtocolContent
            };
        }

        ///// <summary>
        ///// 同意服务协议
        ///// </summary>
        ///// <returns></returns>
        //[System.Web.Http.HttpGet]
        //public JsonResult<dynamic> AgreeServiceProtocol(int memberID)
        //{
        //    DbHelper.ExecuteSqlCommand($"update Members set IsAgreeProtocol=1 where MemberID={memberID}", null);
        //    return new JsonResult<dynamic>
        //    {
        //        status = true,
        //        Message = "获取成功"
        //    };

        //}

        #endregion

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