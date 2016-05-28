using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Transactions;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WYJK.Data;
using WYJK.Entity;
using WYJK.Framework.EnumHelper;
using WYJK.Web.Filters;

namespace WYJK.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configuration.Filters.Add(new ErrorAttribute());
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings =
               new JsonSerializerSettings
               {
                   DateFormatString = "yyyy-MM-dd HH:mm:ss"
               };

            #region 定时任务
            Timer myTimer = new Timer();
            myTimer.Elapsed += new ElapsedEventHandler(theout);
            myTimer.Interval = 1000;
            myTimer.AutoReset = true;
            myTimer.Enabled = true;

            #endregion
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void theout(object source, System.Timers.ElapsedEventArgs e)
        {
            int CurrentDay = DateTime.Now.Day;
            int CurrentHour = DateTime.Now.Hour;
            int CurrentMinute = DateTime.Now.Minute;
            int CurrentSecond = DateTime.Now.Second;

            //定制时间 每月15号 00：00：00 开始执行
            int CustomDay = 15;
            int CustomHour = 00;
            int CustomMinute = 00;
            int CustomSecond = 00;

            Debug.WriteLine(DateTime.Now);

            if (CurrentDay == CustomDay && CurrentHour == CustomHour
                && CurrentMinute == CustomMinute && CurrentSecond == CustomSecond)
            {
                Console.WriteLine("每月15号 00：00：00 开始执行");

                using (TransactionScope transaction = new TransactionScope())
                {
                    try
                    {
                        #region 每个用户下的所有正常的参保人进行扣款,并将已投月数+1
                        //查询所有用户
                        string sqlMember = "select * from Members";
                        List<Members> memberList = DbHelper.Query<Members>(sqlMember);
                        string sqlStr = string.Empty;

                        foreach (Members member in memberList)
                        {
                            //查询该用户下的所有参保人
                            string sqlSocialSecurityPeople = $"select * from SocialSecurityPeople where MemberID={member.MemberID}";
                            List<SocialSecurityPeople> SocialSecurityPeopleList = DbHelper.Query<SocialSecurityPeople>(sqlSocialSecurityPeople);
                            string SocialSecurityPeopleIDStr = string.Join("','", SocialSecurityPeopleList.Select(n => n.SocialSecurityPeopleID));

                            //查询该用户下的所有正常参保方案
                            string sqlSocialSecurity = $"select * from SocialSecurity where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}' and Status={(int)SocialSecurityStatusEnum.Normal})";
                            List<SocialSecurity> SocialSecurityList = DbHelper.Query<SocialSecurity>(sqlSocialSecurity);
                            foreach (SocialSecurity socialSecurity in SocialSecurityList)
                            {
                                //社保单月金额
                                decimal account = socialSecurity.SocialSecurityBase * socialSecurity.PayProportion / 100;
                                //余额减
                                sqlStr += $"update Members set Account=Account-{account} where MemberID={member.MemberID};";
                                //社保流水账
                                sqlStr += $"insert into AccountRecord(MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,CreateTime)"
                                           + $" values({member.MemberID},{socialSecurity.SocialSecurityPeopleID},{socialSecurity.SocialSecurityPeopleName},'支出','余额','社保费',{account},getdate());";
                                //已投月数+1
                                sqlStr += $"update SocialSecurity set AlreadyPayMonthCount=ISNULL(AlreadyPayMonthCount,0)+1 where SocialSecurityPeopleID={socialSecurity.SocialSecurityPeopleID};";
                            }

                            //查询该用户下的所有正常参公积金方案
                            string sqlAccumulationFund = $"select * from AccumulationFund where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}' and Status={(int)SocialSecurityStatusEnum.Normal})";
                            List<AccumulationFund> AccumulationFundList = DbHelper.Query<AccumulationFund>(sqlAccumulationFund);
                            foreach (AccumulationFund accumulationFund in AccumulationFundList)
                            {
                                //公积金单月金额
                                decimal account = accumulationFund.AccumulationFundBase * accumulationFund.PayProportion / 100;
                                //余额减
                                sqlStr += $"update Members set Account=Account-{account} where MemberID={member.MemberID};";
                                //公积金流水账
                                sqlStr += $"insert into AccountRecord(MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,CreateTime)"
                                           + $" values({member.MemberID},{accumulationFund.SocialSecurityPeopleID},{accumulationFund.SocialSecurityPeopleName},'支出','余额','公积金费',{account},getdate());";
                                //已投月数+1
                                sqlStr += $"update AccumulationFund set AlreadyPayMonthCount=ISNULL(AlreadyPayMonthCount,0)+1 where SocialSecurityPeopleID={accumulationFund.SocialSecurityPeopleID};";
                            }
                        }

                        DbHelper.ExecuteSqlCommand(sqlStr, null);

                        #endregion

                        #region 检测下月余额是否够用，不够用则状态变为待续费
                        string sqlStr2 = string.Empty;
                        foreach (Members member in memberList)
                        {
                            #region 查询每个用户余额
                            decimal totalAccount = 0;
                            //查询该用户下的所有参保人
                            string sqlSocialSecurityPeople = $"select * from SocialSecurityPeople where MemberID={member.MemberID}";
                            List<SocialSecurityPeople> SocialSecurityPeopleList = DbHelper.Query<SocialSecurityPeople>(sqlSocialSecurityPeople);
                            string SocialSecurityPeopleIDStr = string.Join("','", SocialSecurityPeopleList.Select(n => n.SocialSecurityPeopleID));

                            //查询该用户下的所有正常参保方案
                            string sqlSocialSecurity = $"select * from SocialSecurity where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}' and Status={(int)SocialSecurityStatusEnum.Normal})";
                            List<SocialSecurity> SocialSecurityList = DbHelper.Query<SocialSecurity>(sqlSocialSecurity);
                            foreach (SocialSecurity socialSecurity in SocialSecurityList)
                            {
                                //社保单月金额
                                decimal account = socialSecurity.SocialSecurityBase * socialSecurity.PayProportion / 100;
                                totalAccount += account;
                            }

                            //查询该用户下的所有正常参公积金方案
                            string sqlAccumulationFund = $"select * from AccumulationFund where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}' and Status={(int)SocialSecurityStatusEnum.Normal})";
                            List<AccumulationFund> AccumulationFundList = DbHelper.Query<AccumulationFund>(sqlAccumulationFund);
                            foreach (AccumulationFund accumulationFund in AccumulationFundList)
                            {
                                //公积金单月金额
                                decimal account = accumulationFund.AccumulationFundBase * accumulationFund.PayProportion / 100;
                                totalAccount += account;
                            }
                            #endregion

                            #region 查询下月余额是否够用,若不够用，则变为待续费
                            if (member.Account < totalAccount)
                            {
                                //该用户下的正常社保变为待续费
                                foreach (SocialSecurity socialSecurity in SocialSecurityList)
                                {
                                    sqlStr2 += $"update SocialSecurity set Status ={(int)SocialSecurityStatusEnum.Renew} where SocialSecurityPeopleID={socialSecurity.SocialSecurityPeopleID};";
                                }
                                //该用户下的正常公积金变为待续费
                                foreach (AccumulationFund accumulationFund in AccumulationFundList)
                                {
                                    sqlStr2 += $"update AccumulationFund set Status={(int)SocialSecurityStatusEnum.Renew} where SocialSecurityPeopleID ={accumulationFund.SocialSecurityPeopleID};";
                                }

                                //发送消息提醒
                                DbHelper.ExecuteSqlCommand($"insert into Message(MemberID,ContentStr) values({message.MemberID},{message.ContentStr})", null);
                            }

                            #endregion

                            DbHelper.ExecuteSqlCommand(sqlStr2, null);
                        }
                        #endregion

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
            }
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ErrorAttribute());
        }

    }
}
