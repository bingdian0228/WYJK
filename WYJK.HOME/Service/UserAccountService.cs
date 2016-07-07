using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using WYJK.Data;
using WYJK.Data.IService;
using WYJK.Data.IServices;
using WYJK.Data.ServiceImpl;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;
using WYJK.HOME.Models;

namespace WYJK.HOME.Service
{
    public class UserAccountService
    {
        ISocialSecurityService _socialSecurityService = new Data.ServiceImpl.SocialSecurityService();
        private readonly IMemberService _memberService = new MemberService();
        private readonly IParameterSettingService _parameterSettingService = new ParameterSettingService();

        /// <summary>
        /// 提交充值
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool SubmitRechargeAmount(RechargeParameters parameter)
        {
            bool result = false;

            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    decimal MonthTotal = _socialSecurityService.GetMonthTotalAmountByMemberID(parameter.MemberID);
                    //计算第一个月                                                             
                    decimal TotalServiceCost = 0;
                    decimal SSServiceCost = 0;//社保服务费
                    decimal AFServiceCost = 0;//公积金服务费
                    AccountInfo accountInfo = _memberService.GetAccountInfo(parameter.MemberID);

                    string sqlAccountRecord = "";//记录
                    if (accountInfo.Account < MonthTotal)
                    {
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
                                    List<SocialSecurityPeople> SocialSecurityPeopleList = _socialSecurityService.GetSocialSecurityRenewListByMemberID(parameter.MemberID);
                                    //社保待办与正常的人数
                                    SSServiceCost = SocialSecurityPeopleList.Count * Convert.ToDecimal(str1[2]);
                                    //记录支出
                                    sqlAccountRecord += $@"insert into AccountRecord(MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,CreateTime) 
values({parameter.MemberID},'','','支出','余额','社保服务费',{SSServiceCost},getdate());";
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
                                    List<SocialSecurityPeople> SocialSecurityPeopleList = _socialSecurityService.GetAccumulationFundRenewListByMemberID(parameter.MemberID);
                                    //社保待办与正常的人数
                                    AFServiceCost = SocialSecurityPeopleList.Count * Convert.ToDecimal(str1[2]);
                                    //记录支出
                                    sqlAccountRecord += $@"insert into AccountRecord(MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,CreateTime) 
values({parameter.MemberID},'','','支出','余额','公积金服务费',{AFServiceCost},getdate());";
                                    break;
                                }
                            }
                        }
                    }
                    TotalServiceCost = SSServiceCost + AFServiceCost;
                    //修改账户余额
                    decimal account = parameter.Amount - TotalServiceCost;
                    string sqlMember = $"update Members set Account=ISNULL(Account,0)+{account} where MemberID={parameter.MemberID}";
                    int updateResult = DbHelper.ExecuteSqlCommand(sqlMember, null);
                    if (!(updateResult > 0)) throw new Exception("更新个人账户失败");

                    //记录收入
                    sqlAccountRecord += $"insert into AccountRecord(MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,CreateTime) values({parameter.MemberID},'','','收入','{parameter.PayMethod}','续费',{parameter.Amount},getdate());";
                    //更新记录
                    DbHelper.ExecuteSqlCommand(sqlAccountRecord, null);

                    //将所有的待续费变成正常,并将剩余月数变成服务月数  --待修改
                    string sqlstr = $@"update SocialSecurity set SocialSecurity.Status = {(int)SocialSecurityStatusEnum.Normal} where socialsecurity.SocialSecurityID in
  (select SocialSecurity.SocialSecurityID from SocialSecurity
left join SocialSecurityPeople on SocialSecurity.SocialSecurityPeopleID = SocialSecurityPeople.SocialSecurityPeopleID
  where SocialSecurityPeople.MemberID = {parameter.MemberID} and SocialSecurity.Status = {(int)SocialSecurityStatusEnum.Renew});
update AccumulationFund set AccumulationFund.Status = {(int)SocialSecurityStatusEnum.Normal} where AccumulationFund.AccumulationFundID in
  (select AccumulationFund.AccumulationFundID from AccumulationFund
left join SocialSecurityPeople on AccumulationFund.SocialSecurityPeopleID = SocialSecurityPeople.SocialSecurityPeopleID
  where SocialSecurityPeople.MemberID = {parameter.MemberID} and AccumulationFund.Status = {(int)SocialSecurityStatusEnum.Renew}) ";
                    DbHelper.ExecuteSqlCommand(sqlstr, null);

                    transaction.Complete();

                    result = true;
                }
                catch (Exception ex)
                {
                    result = false;
                }
                finally
                {
                    transaction.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 获取提现/申请列表
        /// </summary>
        /// <param name="drawCash"></param>
        /// <param name="applyStatus"></param>
        /// <returns></returns>
        public List<Models.DrawCash> GetDrawCashRecord(Models.DrawCash drawCash,int? applyStatus)
        {
            List<Models.DrawCash> list = new List<Models.DrawCash>();

            StringBuilder strBd = new StringBuilder();
            strBd.Append($@"select *from DrawCash where MemberId = {drawCash.MemberId} ");

            if (applyStatus != null)
            {
                strBd.Append($" and ApplyStatus = {applyStatus.Value}");
            }
            
            return DbHelper.Query<Models.DrawCash>(strBd.ToString());
        }

        /// <summary>
        /// 获取最新的提现申请
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public Models.DrawCash GetLastestDrawCash(int memberId)
        {
            string sql = $@"select 
	                            top 1 * 
                            from DrawCash
	                            where MemberId = {memberId} and ApplyStatus = 0
	                            order by ApplyTime desc";
            
            return DbHelper.QuerySingle<Models.DrawCash>(sql);
        }

        /// <summary>
        /// 提现申请
        /// </summary>
        /// <param name="drawCash"></param>
        public int DrawCash(Models.DrawCash drawCash)
        {
            int num = 0;

            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    string sql = $@"insert into DrawCash
	                                    (
	                                    MemberId,
	                                    Money,
	                                    ApplyTime,
	                                    ApplyStatus,
	                                    LeftAccount
	                                    )
                                    values
	                                    (
	                                    {drawCash.MemberId},
	                                    {drawCash.Money},
	                                    GETDATE(),
	                                    {drawCash.ApplyStatus},
	                                    {drawCash.LeftAccount}
	                                    )";

                    num = DbHelper.ExecuteSqlCommand(sql,null);
                    

                    transaction.Complete();
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    transaction.Dispose();
                }

            }

            return num;
        }

    }
}