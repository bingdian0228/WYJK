using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Framework.Setting
{
    public static class WebConfigurationManager
    {
        /// <summary>
        /// 获取配置节点项
        /// </summary>
        public static NameValueCollection AppSettings
        {
            get { return System.Configuration.ConfigurationManager.AppSettings; }
        }
        /// <summary>
        /// 默认数据库连接字符串
        /// </summary>
        public static string DefaultConnectionString
        {
            get { return System.Configuration.ConfigurationManager.ConnectionStrings["WYJKConnectionString"].ConnectionString.Replace("******","qingdao!10"); }
        }

        /// <summary>
        /// 日志配置路径
        /// </summary>
        public static string LogConfigurationFile
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"App_Data", "log4net.config"); }
        }
        /// <summary>
        /// 默认附加数据的结构
        /// </summary>
        public static string DefaultAdditional { get; } = "<root></root>";
        /// <summary>
        /// 百度密钥配置
        /// </summary>
        public static BaiduSetting BaiduSetting { get; } = new BaiduSetting
        {
            AppKey = ConfigurationManager.AppSettings["Baidu.AppKey"],
            SecurityKey = ConfigurationManager.AppSettings["Baidu.SecurityKey"]
        };
        /// <summary>
        /// 极光推送配置
        /// </summary>
        public static JPushSetting JPushSetting { get; } = new JPushSetting
        {
            AppKey = ConfigurationManager.AppSettings["JPush.AppKey"],
            MasterSecret = ConfigurationManager.AppSettings["JPush.MasterSecret"]
        };
       
        /// <summary>
        /// 主站域名
        /// </summary>
        public static string Domain { get; } = ConfigurationManager.AppSettings["Domain.WebApi"];
        /// <summary>
        /// 图片域名
        /// </summary>
        public static string ImageDomain { get; } = ConfigurationManager.AppSettings["Domain.Images"];

        
    }
}
