using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Owin;
using Owin;
using System;
using System.Diagnostics;

[assembly: OwinStartupAttribute(typeof(WYJK.Web.Startup))]
namespace WYJK.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //指定Hangfire使用内存存储后台任务信息
            GlobalConfiguration.Configuration.UseMemoryStorage();
            //启用HangfireServer这个中间件（它会自动释放）
            app.UseHangfireServer();
            //启用Hangfire的仪表盘（可以看到任务的状态，进度等信息）
            app.UseHangfireDashboard();

            ConfigureAuth(app);

            Hangfire.RecurringJob.AddOrUpdate(() => Test(), Hangfire.Cron.Minutely);
        }

        public void Test()
        {
            Debug.WriteLine("Hangfire !" + DateTime.Now);
        }
    }
}
