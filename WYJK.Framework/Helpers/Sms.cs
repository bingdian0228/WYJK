using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WYJK.Framework.Helpers

{
    public class Sms
    {
        /**
                * 服务http地址
                */
        private static string BASE_URI = "http://yunpian.com";
        /**
        * 服务版本号
        */
        private static string VERSION = "v1";
        /**
        * 查账户信息的http地址
        */
        private static string URI_GET_USER_INFO = BASE_URI + "/" + VERSION + "/user/get.json";
        /**
        * 智能匹配模版接口发短信的http地址
        */
        private static string URI_SEND_SMS = BASE_URI + "/" + VERSION + "/sms/send.json";
        /**
        * 模板接口短信接口的http地址
        */
        private static string URI_TPL_SEND_SMS = BASE_URI + "/" + VERSION + "/sms/tpl_send.json";

        /**
        * 取账户信息
        * @return json格式字符串
        */
        public static string getUserInfo(string apikey)
        {
            System.Net.WebRequest req = System.Net.WebRequest.Create(URI_GET_USER_INFO + "?apikey=" + apikey);
            req.Method = "POST";
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            return sr.ReadToEnd().Trim();
        }
        /**
        * 智能匹配模版接口发短信
        * @param text　短信内容　
        * @param mobile　接受的手机号
        * @return json格式字符串
        */
        public static string sendSms(string apikey, string text, string mobile)
        {
            //注意：参数必须进行Uri.EscapeDataString编码。以免&#%=等特殊符号无法正常提交
            string parameter = "apikey=" + apikey + "&text=" + Uri.EscapeDataString(text) + "&mobile=" + mobile;
            System.Net.WebRequest req = System.Net.WebRequest.Create(URI_SEND_SMS);
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(parameter);//这里编码设置为utf8
            req.ContentLength = bytes.Length;
            System.IO.Stream os = req.GetRequestStream();
            os.Write(bytes, 0, bytes.Length);
            os.Close();
            System.Net.WebResponse resp = req.GetResponse();
            if (resp == null) return null;
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            return sr.ReadToEnd().Trim();
        }

        /**
        * 模板接口发短信
        * @param tpl_id 模板id
        * @param tpl_value 模板变量值
        * @param mobile　接受的手机号
        * @return json格式字符串
        */
        public static string tplSendSms(string apikey, long tpl_id, string tpl_value, string mobile)
        {
            string encodedTplValue = Uri.EscapeDataString(tpl_value);
            string parameter = "apikey=" + apikey + "&tpl_id=" + tpl_id + "&tpl_value=" + encodedTplValue + "&mobile=" + mobile;
            System.Net.WebRequest req = System.Net.WebRequest.Create(URI_TPL_SEND_SMS);
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(parameter);//这里编码设置为utf8
            req.ContentLength = bytes.Length;
            System.IO.Stream os = req.GetRequestStream();
            os.Write(bytes, 0, bytes.Length);
            os.Close();
            System.Net.WebResponse resp = req.GetResponse();
            if (resp == null) return null;
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            return sr.ReadToEnd().Trim();
        }

    }
}