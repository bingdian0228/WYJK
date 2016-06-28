using WYJK.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Mvc;

namespace WYJK.Web.Filters
{
    public class ErrorAttribute : System.Web.Mvc.HandleErrorAttribute, System.Web.Http.Filters.IExceptionFilter
    {
        public bool AllowMultiple = true;

        public Task ExecuteExceptionFilterAsync(HttpActionExecutedContext context, CancellationToken cancellationToken)
        {
            LogManager logManager = new LogManager(HttpContext.Current.Server.MapPath("~/HttpException.txt"));

            logManager.SaveLog(context.Exception.StackTrace, DateTime.Now);

            return Task.Run(() =>
            {
                context.Response = GetResponseMessage(context.Exception.Message, context.Exception);
            });

        }

        public override void OnException(ExceptionContext filterContext)
        {
            LogManager logManager = new LogManager(filterContext.HttpContext.Server.MapPath("~/MvcException.txt"));

            logManager.SaveLog(filterContext.Exception.Message, DateTime.Now);

            base.OnException(filterContext);

        }


        private HttpResponseMessage GetResponseMessage(string msg, Exception exception)
        {
            MediaTypeFormatter formatter = new JsonMediaTypeFormatter();

            JsonResult<string> result = new JsonResult<string>
            {
                status = false,
                Message = msg,
                Data = null
            };

            HttpResponseMessage response = new HttpResponseMessage
            {
               Content = new ObjectContent(typeof(JsonResult<string>), result, formatter)
               
            };

            return response;
        }

    }


    public class LogManager
    {

        private string logFilePath = string.Empty;

        public LogManager(string logFilePath)
        {

            this.logFilePath = logFilePath;

            FileInfo file = new FileInfo(logFilePath);

            if (!file.Exists)
            {

                file.Create().Close();

            }


        }

        public void SaveLog(string message, DateTime writerTime)
        {

            string log = writerTime.ToString() + ":" + message;

            StreamWriter sw = new StreamWriter(logFilePath, true);

            sw.WriteLine(log);

            sw.Close();

        }
    }
}