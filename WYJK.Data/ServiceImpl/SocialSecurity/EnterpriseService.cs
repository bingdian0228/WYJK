using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Data.IService;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;

namespace WYJK.Data.ServiceImpl
{
    /// <summary>
    /// 签约企业实现类
    /// </summary>
    public class EnterpriseService : IEnterpriseService
    {

        /// <summary>
        /// 根据企业ID获取签约企业实体
        /// </summary>
        /// <param name="EnterpriseID"></param>
        /// <returns></returns>
        public EnterpriseSocialSecurity GetEnterpriseSocialSecurity(int EnterpriseID)
        {
            string sqlstr = $"select * from EnterpriseSocialSecurity where EnterpriseID={EnterpriseID}";
            EnterpriseSocialSecurity model = DbHelper.QuerySingle<EnterpriseSocialSecurity>(sqlstr);
            return model;
        }

        /// <summary>
        /// 获取参保企业列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public PagedResult<EnterpriseSocialSecurity> GetEnterpriseList(EnterpriseSocialSecurityParameter parameter)
        {
            string sqlstr = $"select * from (select ROW_NUMBER() OVER(ORDER BY s.EnterpriseID )AS Row,s.* from EnterpriseSocialSecurity s where EnterpriseName like '%{parameter.EnterpriseName}%') ss  WHERE ss.Row BETWEEN @StartIndex AND @EndIndex";

            List<EnterpriseSocialSecurity> modelList = DbHelper.Query<EnterpriseSocialSecurity>(sqlstr, new
            {
                StartIndex = parameter.SkipCount,
                EndIndex = parameter.TakeCount
            });

            int totalCount = DbHelper.QuerySingle<int>($"select count(0) from EnterpriseSocialSecurity  where EnterpriseName like '%{parameter.EnterpriseName}%'");

            return new PagedResult<EnterpriseSocialSecurity>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = totalCount,
                Items = modelList
            };
        }

        /// <summary>
        /// 添加企业
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool AddEnterprise(EnterpriseSocialSecurity model)
        {
            string sqlStr = "insert into EnterpriseSocialSecurity(EnterpriseName,EnterpriseArea,ContactUser,ContactTel,Fax,Email"
                                                                  + ",OfficeTel,OrgAddress,SocialAvgSalary,MinSocial"
                                                                  + ",MaxSocial,CompYangLao,CompYiLiao,CompShiYe,CompGongShang,CompShengYu"
                                                                  + ",PersonalYangLao,PersonalYiLiao,PersonalShiYeTown,PersonalShiYeRural,PersonalGongShang"
                                                                  + ",PersonalShengYu,MinAccumulationFund,MaxAccumulationFund,CompProportion"
                                                                  + ",PersonalProportion,IsDefault)"
                                                                  + " values(@EnterpriseName,@EnterpriseArea,@ContactUser,@ContactTel,@Fax,@Email"
                                                                  + ",@OfficeTel,@OrgAddress,@SocialAvgSalary,@MinSocial"
                                                                  + ",@MaxSocial,@CompYangLao,@CompYiLiao,@CompShiYe,@CompGongShang,@CompShengYu"
                                                                  + ",@PersonalYangLao,@PersonalYiLiao,@PersonalShiYeTown,@PersonalShiYeRural,@PersonalGongShang"
                                                                  + ",@PersonalShengYu,@MinAccumulationFund,@MaxAccumulationFund,@CompProportion"
                                                                  + ",@PersonalProportion,@IsDefault)";
            SqlParameter[] sqlparameters = new SqlParameter[] {
                new SqlParameter("EnterpriseName",model.EnterpriseName),
                new SqlParameter("EnterpriseArea",model.EnterpriseArea),
                new SqlParameter("ContactUser",model.ContactUser),
                new SqlParameter("ContactTel",model.ContactTel),
                new SqlParameter("Fax",model.Fax),
                new SqlParameter("Email",model.Email),
                new SqlParameter("OfficeTel",model.OfficeTel),
                //new SqlParameter("HouseholdProperty",model.HouseholdProperty),
                new SqlParameter("OrgAddress",model.OrgAddress),
                new SqlParameter("SocialAvgSalary",model.SocialAvgSalary),
                new SqlParameter("MinSocial",model.MinSocial),
                new SqlParameter("MaxSocial",model.MaxSocial),
                new SqlParameter("CompYangLao",model.CompYangLao),
                new SqlParameter("CompYiLiao",model.CompYiLiao),
                new SqlParameter("CompShiYe",model.CompShiYe),
                new SqlParameter("CompGongShang",model.CompGongShang),
                new SqlParameter("CompShengYu",model.CompShengYu),

                new SqlParameter("PersonalYangLao",model.PersonalYangLao),
                new SqlParameter("PersonalYiLiao",model.PersonalYiLiao),
                new SqlParameter("PersonalShiYeTown",model.PersonalShiYeTown),
                new SqlParameter("PersonalShiYeRural",model.PersonalShiYeRural),
                new SqlParameter("PersonalGongShang",model.PersonalGongShang),
                new SqlParameter("PersonalShengYu",model.PersonalShengYu),

                new SqlParameter("MinAccumulationFund",model.MinAccumulationFund),
                new SqlParameter("MaxAccumulationFund",model.MaxAccumulationFund),
                new SqlParameter("CompProportion",model.CompProportion),
                new SqlParameter("PersonalProportion",model.PersonalProportion),
                new SqlParameter("IsDefault",model.IsDefault)
            };

            int result = DbHelper.ExecuteSqlCommandScalar(sqlStr, sqlparameters);
            return result > 0;

        }

        /// <summary>
        /// 批量删除企业
        /// </summary>
        /// <param name="EnterpriseIDs"></param>
        /// <returns></returns>
        public bool BatchDeleteEnterprise(int[] EnterpriseIDs)
        {
            string EnterpriseIDstr = string.Join("','", EnterpriseIDs);

            string sqlstr = $"delete from EnterpriseSocialSecurity where EnterpriseID in('{EnterpriseIDstr}')";

            int result = DbHelper.ExecuteSqlCommand(sqlstr, null);
            return result > 0;
        }

        /// <summary>
        /// 是否存在该企业
        /// </summary>
        /// <param name="EnterpriseName"></param>
        /// <param name="EnterpriseID"></param>
        /// <returns></returns>
        public bool IsExistsEnterprise(string EnterpriseName, int EnterpriseID)
        {
            string sqlstr = $"select * from EnterpriseSocialSecurity where EnterpriseName = '{EnterpriseName}' and EnterpriseID <> {EnterpriseID}";

            int result = DbHelper.Query<EnterpriseSocialSecurity>(sqlstr).Count;

            return result > 0;
        }

        /// <summary>
        /// 更新企业
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool UpdateEnterprise(EnterpriseSocialSecurity model)
        {
            string sqlstr = "update EnterpriseSocialSecurity set EnterpriseName=@EnterpriseName,EnterpriseArea=@EnterpriseArea,ContactUser=@ContactUser,ContactTel=@ContactTel,Fax=@Fax,Email=@Email,OfficeTel=@OfficeTel,OrgAddress=@OrgAddress,SocialAvgSalary=@SocialAvgSalary,MinSocial=@MinSocial,MaxSocial=@MaxSocial,CompYangLao=@CompYangLao,CompYiLiao=@CompYiLiao,CompShiYe=@CompShiYe,CompGongShang=@CompGongShang,CompShengYu=@CompShengYu,PersonalYangLao=@PersonalYangLao,PersonalYiLiao=@PersonalYiLiao,PersonalShiYeTown=@PersonalShiYeTown,PersonalShiYeRural=@PersonalShiYeRural,PersonalGongShang=@PersonalGongShang,PersonalShengYu=@PersonalShengYu,MinAccumulationFund=@MinAccumulationFund,MaxAccumulationFund=@MaxAccumulationFund,CompProportion=@CompProportion,PersonalProportion=@PersonalProportion,IsDefault=@IsDefault where EnterpriseID=@EnterpriseID";


            int result = DbHelper.ExecuteSqlCommand(sqlstr, new
            {
                EnterpriseName = model.EnterpriseName,
                EnterpriseArea = model.EnterpriseArea,
                ContactUser = model.ContactUser,
                ContactTel = model.ContactTel,
                Fax = model.Fax,
                Email = model.Email,
                OfficeTel = model.OfficeTel,
                //HouseholdProperty = model.HouseholdProperty,
                OrgAddress = model.OrgAddress,
                SocialAvgSalary = model.SocialAvgSalary,
                MinSocial = model.MinSocial,
                MaxSocial = model.MaxSocial,
                CompYangLao = model.CompYangLao,
                CompYiLiao = model.CompYiLiao,
                CompShiYe = model.CompShiYe,
                CompGongShang = model.CompGongShang,
                CompShengYu = model.CompShengYu,

                PersonalYangLao = model.PersonalYangLao,
                PersonalYiLiao = model.PersonalYiLiao,
                PersonalShiYeTown = model.PersonalShiYeTown,
                PersonalShiYeRural = model.PersonalShiYeRural,
                PersonalGongShang = model.PersonalGongShang,
                PersonalShengYu = model.PersonalShengYu,

                MinAccumulationFund = model.MinAccumulationFund,
                MaxAccumulationFund = model.MaxAccumulationFund,
                CompProportion = model.CompProportion,
                PersonalProportion = model.PersonalProportion,
                IsDefault = model.IsDefault,
                EnterpriseID = model.EnterpriseID
            });

            return result > 0;
        }

        /// <summary>
        /// 根据地址更新企业默认值
        /// </summary>
        /// <param name="EnterpriseArea"></param>
        /// <param name="HouseholdProperty"></param>
        /// <returns></returns>
        public void UpdateEnterpriseDefault(string EnterpriseArea,int EnterpriseID)
        {
            string sqlHouseholdProperty = string.Empty;
            //switch (HouseholdProperty)
            //{
            //    case (int)HouseholdPropertyEnum.InRural:
            //        sqlHouseholdProperty = Convert.ToString((int)HouseholdPropertyEnum.InRural) +","+ Convert.ToString((int)HouseholdPropertyEnum.OutRural);
            //        break;
            //    case (int)HouseholdPropertyEnum.OutRural:
            //        sqlHouseholdProperty = Convert.ToString((int)HouseholdPropertyEnum.InRural) + "," + Convert.ToString((int)HouseholdPropertyEnum.OutRural);
            //        break;
            //    case (int)HouseholdPropertyEnum.InTown:
            //        sqlHouseholdProperty = Convert.ToString((int)HouseholdPropertyEnum.InTown) + "," + Convert.ToString((int)HouseholdPropertyEnum.OutTown);
            //        break;
            //    case (int)HouseholdPropertyEnum.OutTown:
            //        sqlHouseholdProperty = Convert.ToString((int)HouseholdPropertyEnum.InTown) + "," + Convert.ToString((int)HouseholdPropertyEnum.OutTown);
            //        break;
            //}

            string sqlstr = $"update EnterpriseSocialSecurity set IsDefault = 0 where EnterpriseArea like '%{EnterpriseArea}%' and EnterpriseID <>{EnterpriseID}";

            DbHelper.ExecuteSqlCommand(sqlstr, null);

        }

        /// <summary>
        /// 获取企业名称列表
        /// </summary>
        /// <param name="SocialSecurityPeopleIDs"></param>
        /// <returns></returns>
        public string GetEnterpriseNames(int[] EnterpriseIDs)
        {
            string EnterpriseIDStr = string.Join("','", EnterpriseIDs);
            string strsql = $@"declare @Name nvarchar(50)
                            select @Name = ISNULL(@Name + '，', '') + EnterpriseName from EnterpriseSocialSecurity where EnterpriseID in('{EnterpriseIDStr}');
                            select @Name";

            return DbHelper.QuerySingle<string>(strsql);
        }

        /// <summary>
        /// 城市社平管理列表
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public PagedResult<EnterpriseCity> GetEnterpriseCityList(PagedParameter parameter)
        {
            string sqlstr = $"select * from (select ROW_NUMBER() OVER(ORDER BY s.city )AS Row,s.* from (select distinct reverse(SUBSTRING(reverse(EnterpriseArea),charindex('|',reverse(EnterpriseArea))+1,LEN(EnterpriseArea))) city from EnterpriseSocialSecurity) s) ss  WHERE ss.Row BETWEEN @StartIndex AND @EndIndex";

            List<EnterpriseCity> modelList = DbHelper.Query<EnterpriseCity>(sqlstr, new
            {
                StartIndex = parameter.SkipCount,
                EndIndex = parameter.TakeCount
            });

            int totalCount = DbHelper.QuerySingle<int>($"select count(0) from (select distinct reverse(SUBSTRING(reverse(EnterpriseArea),charindex('|',reverse(EnterpriseArea))+1,LEN(EnterpriseArea))) city from EnterpriseSocialSecurity) s");

            return new PagedResult<EnterpriseCity>
            {
                PageIndex = parameter.PageIndex,
                PageSize = parameter.PageSize,
                TotalItemCount = totalCount,
                Items = modelList
            };
        }
    }
}
