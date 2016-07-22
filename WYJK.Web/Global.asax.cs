using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
using WYJK.Framework.Helpers;
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



            #region 参保人与客服的匹配
            Timer onlineUserTimer = new Timer();
            onlineUserTimer.Elapsed += new ElapsedEventHandler(currentOnlineUser);
            onlineUserTimer.Interval = 60000;
            onlineUserTimer.AutoReset = true;
            onlineUserTimer.Enabled = true;

            #endregion


            #region 定时任务 15号
            Timer Timer15 = new Timer();
            Timer15.Elapsed += new ElapsedEventHandler(business15);
            Timer15.Interval = 1000;
            Timer15.AutoReset = true;
            Timer15.Enabled = true;

            #endregion

            #region 13号
            Timer Timer13 = new Timer();
            Timer13.Elapsed += new ElapsedEventHandler(business13);
            Timer13.Interval = 1000;
            Timer13.AutoReset = true;
            Timer13.Enabled = true;
            #endregion
        }

        protected void Application_End(object sender, EventArgs e)

        {

            //下面的代码是关键，可解决IIS应用程序池自动回收的问题  

            System.Threading.Thread.Sleep(1000);

            //这里设置你的web地址，可以随便指向你的任意一个aspx页面甚至不存在的页面，目的是要激发Application_Start  

            string url = "http://www.shaoqun.com";

            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();

            Stream receiveStream = myHttpWebResponse.GetResponseStream();//得到回写的字节流  

        }

        /// <summary>
        /// 客服匹配
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void currentOnlineUser(object source, System.Timers.ElapsedEventArgs e)
        {
            List<OnlineUser> currentOnlineUserList = new List<OnlineUser>();
            //List<OnlineUser> onlineUserList = HttpContext.Current.Cache["CurrentUser"] as List<OnlineUser>;
            List<OnlineUser> onlineUserList = HttpRuntime.Cache["CurrentUser"] as List<OnlineUser>;
            if (onlineUserList != null && onlineUserList.Count > 0)
            {
                foreach (var onlineUser in onlineUserList)
                {
                    //用户活跃时间在1分钟之内的保留
                    if (DateTime.Now - onlineUser.ActiveTime <= new TimeSpan(0, 1, 0))
                    {
                        currentOnlineUserList.Add(onlineUser);
                    }
                }

                //更新用户在线活跃度
                //HttpContext.Current.Cache.Insert("CurrentUser", currentOnlineUserList);
                HttpRuntime.Cache.Insert("CurrentUser", currentOnlineUserList);
            }
            else
                return;

            if (currentOnlineUserList.Count == 0)
                return;

            //查询没有分配客服的参保人
            string sqlstr = string.Empty;
            List<SocialSecurityPeople> socialSecurityPeopleList = DbHelper.Query<SocialSecurityPeople>("select * from SocialSecurityPeople where ISNULL(CustomerServiceUserName,'') =''");
            if (socialSecurityPeopleList != null && socialSecurityPeopleList.Count > 0)
            {
                int j = 0;
                for (int i = 0; i < socialSecurityPeopleList.Count; i++)
                {

                    for (; j < currentOnlineUserList.Count;)
                    {
                        sqlstr += $"update SocialSecurityPeople set CustomerServiceUserName='{currentOnlineUserList[j].UserName}' where SocialSecurityPeopleID={socialSecurityPeopleList[i].SocialSecurityPeopleID};";
                        j++;
                        if (j == currentOnlineUserList.Count)
                            j = 0;
                        break;
                    }
                }

                DbHelper.ExecuteSqlCommand(sqlstr, null);
            }
        }

        /// <summary>
        /// 13触发事件
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void business13(object source, System.Timers.ElapsedEventArgs e)
        {
            int CurrentDay = DateTime.Now.Day;
            int CurrentHour = DateTime.Now.Hour;
            int CurrentMinute = DateTime.Now.Minute;
            int CurrentSecond = DateTime.Now.Second;

            //定制时间 每月13号 00：00：00 开始执行
            int CustomDay = 13;
            int CustomHour = 00;
            int CustomMinute = 00;
            int CustomSecond = 00;

            Debug.WriteLine(DateTime.Now);

            if (CurrentDay == CustomDay && CurrentHour == CustomHour
                && CurrentMinute == CustomMinute && CurrentSecond == CustomSecond)
            {
                Console.WriteLine("每月13号 00：00：00 开始执行");

                TransactionOptions transactionOption = new TransactionOptions();
                //设置事务隔离级别
                transactionOption.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
                // 设置事务超时时间为60秒
                transactionOption.Timeout = new TimeSpan(0, 0, 60);

                using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, transactionOption))
                {
                    try
                    {
                        string sqlMember = "select * from Members where ISNULL(IsFrozen,0) = 0";
                        List<Members> memberList = DbHelper.Query<Members>(sqlMember);

                        #region 将所有待续费变成待停保
                        string sqlStr3 = string.Empty;
                        foreach (Members member in memberList)
                        {
                            //查询该用户下的所有参保人
                            string sqlSocialSecurityPeople = $"select * from SocialSecurityPeople where MemberID={member.MemberID}";
                            List<SocialSecurityPeople> SocialSecurityPeopleList = DbHelper.Query<SocialSecurityPeople>(sqlSocialSecurityPeople);
                            string SocialSecurityPeopleIDStr = string.Join("','", SocialSecurityPeopleList.Select(n => n.SocialSecurityPeopleID));

                            //查询该用户下的所有待停保参保方案
                            string sqlSocialSecurity = $"select * from SocialSecurity where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}') and Status={(int)SocialSecurityStatusEnum.Renew}";
                            List<SocialSecurity> SocialSecurityList = DbHelper.Query<SocialSecurity>(sqlSocialSecurity);
                            foreach (SocialSecurity socialSecurity in SocialSecurityList)
                            {
                                sqlStr3 += $"update SocialSecurity set Status ={(int)SocialSecurityStatusEnum.WaitingStop},ApplyStopDate=getdate() where SocialSecurityPeopleID={socialSecurity.SocialSecurityPeopleID};";
                            }

                            //查询该用户下的所有待停保参公积金方案
                            string sqlAccumulationFund = $"select * from AccumulationFund where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}') and Status={(int)SocialSecurityStatusEnum.Renew}";
                            List<AccumulationFund> AccumulationFundList = DbHelper.Query<AccumulationFund>(sqlAccumulationFund);
                            foreach (AccumulationFund accumulationFund in AccumulationFundList)
                            {
                                sqlStr3 += $"update AccumulationFund set Status ={(int)SocialSecurityStatusEnum.WaitingStop},ApplyStopDate=getdate() where SocialSecurityPeopleID={accumulationFund.SocialSecurityPeopleID};";
                            }
                        }
                        if (sqlStr3.Trim() != string.Empty)
                            DbHelper.ExecuteSqlCommand(sqlStr3, null);
                        #endregion
                        transaction.Complete();

                    }
                    catch (Exception ex) { }
                }
            }
        }


        /// <summary>
        /// 15触发事件
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void business15(object source, System.Timers.ElapsedEventArgs e)
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
                        #region 每个用户下的所有正常的参保人进行扣款,并将已投月数+1,剩余月数-1
                        //查询所有用户
                        string sqlMember = "select * from Members where ISNULL(IsFrozen,0) = 0";//此用户必须是非冻结的账户
                        List<Members> memberList = DbHelper.Query<Members>(sqlMember);
                        string sqlStr = string.Empty;

                        foreach (Members member in memberList)
                        {
                            decimal yue = DbHelper.QuerySingle<decimal>($"select ISNULL(Account,0) from Members where MemberID={member.MemberID}");
                            //查询该用户下的所有参保人
                            string sqlSocialSecurityPeople = $"select * from SocialSecurityPeople where MemberID={member.MemberID}";
                            List<SocialSecurityPeople> SocialSecurityPeopleList = DbHelper.Query<SocialSecurityPeople>(sqlSocialSecurityPeople);
                            string SocialSecurityPeopleIDStr = string.Join("','", SocialSecurityPeopleList.Select(n => n.SocialSecurityPeopleID));

                            //查询该用户下的所有正常参保方案
                            string sqlSocialSecurity = $"select * from SocialSecurity where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}') and Status={(int)SocialSecurityStatusEnum.Normal}";
                            List<SocialSecurity> SocialSecurityList = DbHelper.Query<SocialSecurity>(sqlSocialSecurity);
                            foreach (SocialSecurity socialSecurity in SocialSecurityList)
                            {
                                //社保单月金额
                                decimal account = socialSecurity.SocialSecurityBase * socialSecurity.PayProportion / 100;
                                //余额减
                                sqlStr += $"update Members set Account=Account-{account} where MemberID={member.MemberID};";

                                yue -= account;
                                //社保流水账
                                sqlStr += $"insert into AccountRecord(Type,SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime)"
                                           + $" values(0,{DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random(Guid.NewGuid().GetHashCode()).Next(1000).ToString().PadLeft(3, '0')},{member.MemberID},{socialSecurity.SocialSecurityPeopleID},'{socialSecurity.SocialSecurityPeopleName}','支出','余额','社保费',{account},{yue},getdate());";
                                //已投月数+1,剩余月数-1
                                sqlStr += $"update SocialSecurity set AlreadyPayMonthCount=ISNULL(AlreadyPayMonthCount,0)+1,PayMonthCount=ISNULL(PayMonthCount,0)-1  where SocialSecurityPeopleID={socialSecurity.SocialSecurityPeopleID};";
                            }

                            //查询该用户下的所有正常参公积金方案
                            string sqlAccumulationFund = $"select * from AccumulationFund where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}') and Status={(int)SocialSecurityStatusEnum.Normal}";
                            List<AccumulationFund> AccumulationFundList = DbHelper.Query<AccumulationFund>(sqlAccumulationFund);
                            foreach (AccumulationFund accumulationFund in AccumulationFundList)
                            {
                                //公积金单月金额
                                decimal account = accumulationFund.AccumulationFundBase * accumulationFund.PayProportion / 100;
                                //余额减
                                sqlStr += $"update Members set Account=Account-{account} where MemberID={member.MemberID};";

                                yue -= account;
                                //公积金流水账
                                sqlStr += $"insert into AccountRecord(Type,SerialNum,MemberID,SocialSecurityPeopleID,SocialSecurityPeopleName,ShouZhiType,LaiYuan,OperationType,Cost,Balance,CreateTime)"
                                           + $" values(1,{DateTime.Now.ToString("yyyyMMddHHmmssfff") + new Random(Guid.NewGuid().GetHashCode()).Next(1000).ToString().PadLeft(3, '0')},{member.MemberID},{accumulationFund.SocialSecurityPeopleID},'{accumulationFund.SocialSecurityPeopleName}','支出','余额','公积金费',{account},{yue},getdate());";
                                //已投月数+1,剩余月数-1
                                sqlStr += $"update AccumulationFund set AlreadyPayMonthCount=ISNULL(AlreadyPayMonthCount,0)+1,PayMonthCount=ISNULL(PayMonthCount,0)-1 where SocialSecurityPeopleID={accumulationFund.SocialSecurityPeopleID};";
                            }
                        }
                        if (sqlStr.Trim() != string.Empty)
                        {
                            DbHelper.ExecuteSqlCommand(sqlStr, null);
                        }

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
                            string sqlSocialSecurity = $"select * from SocialSecurity where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}') and Status={(int)SocialSecurityStatusEnum.Normal}";
                            List<SocialSecurity> SocialSecurityList = DbHelper.Query<SocialSecurity>(sqlSocialSecurity);
                            foreach (SocialSecurity socialSecurity in SocialSecurityList)
                            {
                                //社保单月金额
                                decimal account = socialSecurity.SocialSecurityBase * socialSecurity.PayProportion / 100;
                                totalAccount += account;
                            }

                            //查询该用户下的所有正常参公积金方案
                            string sqlAccumulationFund = $"select * from AccumulationFund where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}') and Status={(int)SocialSecurityStatusEnum.Normal}";
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

                                Message message = new Message();
                                message.MemberID = member.MemberID;
                                message.ContentStr = "您的账户余额已不足抵扣下个月社保、公积金.请及时充值.";

                                //发送消息提醒
                                DbHelper.ExecuteSqlCommand($"insert into Message(MemberID,ContentStr) values({message.MemberID},'{message.ContentStr}')", null);

                                #region 发送短信
                                string content = string.Format(contentFormat, member.MemberPhone.Substring(member.MemberPhone.Length - 4), member.Account, DateTime.Now.AddMonths(1).Month, totalAccount, totalAccount - member.Account);

                                Sms.sendSms(apikey, content, member.MemberPhone);
                                #endregion
                            }

                            #endregion
                        }
                        if (sqlStr2.Trim() != string.Empty)
                            DbHelper.ExecuteSqlCommand(sqlStr2, null);
                        #endregion


                        #region 待停变停保
                        string sqlStr3 = string.Empty;
                        foreach (Members member in memberList)
                        {
                            //查询该用户下的所有参保人
                            string sqlSocialSecurityPeople = $"select * from SocialSecurityPeople where MemberID={member.MemberID}";
                            List<SocialSecurityPeople> SocialSecurityPeopleList = DbHelper.Query<SocialSecurityPeople>(sqlSocialSecurityPeople);
                            string SocialSecurityPeopleIDStr = string.Join("','", SocialSecurityPeopleList.Select(n => n.SocialSecurityPeopleID));

                            //查询该用户下的所有待停保参保方案
                            string sqlSocialSecurity = $"select * from SocialSecurity where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}') and Status={(int)SocialSecurityStatusEnum.WaitingStop}";
                            List<SocialSecurity> SocialSecurityList = DbHelper.Query<SocialSecurity>(sqlSocialSecurity);
                            foreach (SocialSecurity socialSecurity in SocialSecurityList)
                            {
                                sqlStr3 += $"update SocialSecurity set Status ={(int)SocialSecurityStatusEnum.AlreadyStop},StopDate=getdate() where SocialSecurityPeopleID={socialSecurity.SocialSecurityPeopleID};";
                            }

                            //查询该用户下的所有待停保参公积金方案
                            string sqlAccumulationFund = $"select * from AccumulationFund where SocialSecurityPeopleID in('{SocialSecurityPeopleIDStr}') and Status={(int)SocialSecurityStatusEnum.WaitingStop}";
                            List<AccumulationFund> AccumulationFundList = DbHelper.Query<AccumulationFund>(sqlAccumulationFund);
                            foreach (AccumulationFund accumulationFund in AccumulationFundList)
                            {
                                sqlStr3 += $"update AccumulationFund set Status ={(int)SocialSecurityStatusEnum.AlreadyStop},StopDate=getdate() where SocialSecurityPeopleID={accumulationFund.SocialSecurityPeopleID};";
                            }
                        }
                        if (sqlStr3.Trim() != string.Empty)
                            DbHelper.ExecuteSqlCommand(sqlStr3, null);
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

        private static string apikey = ConfigurationManager.AppSettings["apikey"].ToString();
        private static string contentFormat = ConfigurationManager.AppSettings["SMS15Warn"].ToString();



        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ErrorAttribute());
        }

    }
}
