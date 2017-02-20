using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using WYJK.Entity;

namespace WYJK.JiaoFei
{
    public class RemoteClient
    {
        private TcpClient client;
        private NetworkStream streamToClient;
        private const int BufferSize = 8192;
        private byte[] buffer;
        private RequestHandler handler;

        JiaoFeiService jiaoFeiService = new JiaoFeiService();//缴费业务逻辑
        LogManager logManager = new LogManager(AppDomain.CurrentDomain.BaseDirectory + "JiaoFeiRecord.txt");

        public RemoteClient(TcpClient client)
        {
            this.client = client;

            // 打印连接到的客户端信息
            Console.WriteLine("\nClient Connected！{0} <-- {1}",
                client.Client.LocalEndPoint, client.Client.RemoteEndPoint);

            // 获得流
            streamToClient = client.GetStream();
            buffer = new byte[BufferSize];

            // 设置RequestHandler
            handler = new RequestHandler();
            // 在构造函数中就开始准备读取
            //Console.WriteLine("第一个线程:" + Thread.CurrentThread.ManagedThreadId);
            AsyncCallback callBack = new AsyncCallback(ReadComplete);
            streamToClient.BeginRead(buffer, 0, BufferSize, callBack, null);
        }

        // 再读取完成时进行回调
        private void ReadComplete(IAsyncResult ar)
        {
            int bytesRead = 0;
            try
            {
                lock (streamToClient)
                {
                    //Console.WriteLine("中间线程" + Thread.CurrentThread.ManagedThreadId);
                    bytesRead = streamToClient.EndRead(ar);
                    Console.WriteLine("Reading data, {0} bytes ...", bytesRead);
                }

                if (bytesRead == 0) throw new Exception("读取到0字节");

                string msg = Encoding.GetEncoding("GBK").GetString(buffer, 0, bytesRead);
                Array.Clear(buffer, 0, buffer.Length);        // 清空缓存，避免脏读

                #region 记录日志
                logManager.SaveLog(msg, DateTime.Now);
                #endregion

                #region 处理消息
                //判断是何种交易类型
                //报文无分隔符，文件内容有分隔符
                string businessCode = msg.Substring(4, 4);
                string returnString = string.Empty;
                switch (businessCode)
                {
                    #region 用户查询
                    case "1001":
                        //报文格式如：0027100110000000000115762279525
                        MemberQuery memberQuery = new MemberQuery()
                        {
                            BusinessCode = businessCode,
                            BankCode = msg.Substring(8, 12),
                            MemberPhone = msg.Substring(20, 11)

                        };
                        MemberQueryReturn memberQueryReturn = jiaoFeiService.JiaoFeiMemberQuery(memberQuery).Result;

                        returnString = "{0}"
                           + memberQueryReturn.BusinessCode
                           + memberQueryReturn.BankCode
                           + memberQueryReturn.MemberPhone
                           + memberQueryReturn.ReturnCode;

                        if (memberQueryReturn.ReturnCode == ReturnCode.Success)
                        {
                            returnString = string.Format(returnString, "0199");
                            returnString += (memberQueryReturn.TrueName ?? string.Empty).PadRight(48, ' ');

                            foreach (var item in memberQueryReturn.MoneyArray)
                            {
                                returnString += Math.Round(item, 2).ToString().PadRight(10, ' ');
                            }
                        }
                        else if (memberQueryReturn.ReturnCode == ReturnCode.Error || memberQueryReturn.ReturnCode == ReturnCode.NoExist)
                        {
                            returnString = string.Format(returnString, "0031");
                        }
                        else if (memberQueryReturn.ReturnCode == ReturnCode.NoNeed)
                        {
                            returnString = string.Format(returnString, "0079");
                            returnString += (memberQueryReturn.TrueName ?? string.Empty).PadRight(48, ' ');
                        }
                        break;
                    #endregion

                    #region 缴费
                    case "1002":
                        //组装对象
                        JiaoFeiSubmit jiaoFeiSubmit = new JiaoFeiSubmit()
                        {
                            BusinessCode = businessCode,
                            BankCode = msg.Substring(8, 12),
                            MemberPhone = msg.Substring(20, 11),
                            Money = Convert.ToDecimal(msg.Substring(31, 10).Trim()),
                            MonthCount = Convert.ToInt32(msg.Substring(41, 2)),
                            BusinessDate = msg.Substring(43, 8).ToString(),
                            BusinessTime = msg.Substring(51, 8).ToString(),
                            SerialNumber = msg.Substring(59, 16)
                        };
                        //缴费提交
                        JiaoFeiSubmitReturn jiaoFeiSubmitReturn = jiaoFeiService.PostJiaoFeiSubmit(jiaoFeiSubmit).Result;

                        //缴费返回
                        returnString = "0047" + jiaoFeiSubmitReturn.BusinessCode + jiaoFeiSubmitReturn.BankCode + jiaoFeiSubmitReturn.MemberPhone + jiaoFeiSubmitReturn.SerialNumber + jiaoFeiSubmitReturn.ReturnCode;

                        break;
                    #endregion

                    #region 冲正
                    case "1003":
                        //组装对象
                        ChongZheng chongZheng = new ChongZheng()
                        {
                            BusinessCode = businessCode,
                            BankCode = msg.Substring(8, 12),
                            MemberPhone = msg.Substring(20, 11),
                            SerialNumber = msg.Substring(31, 16),
                            BusinessDate = msg.Substring(47, 8),
                            Money = Convert.ToDecimal(msg.Substring(55, 10).Trim()),
                            MonthCount = Convert.ToInt32(msg.Substring(65, 2).Trim())
                        };

                        //查看系统内有无相应记录，如果有，则回滚；正常处理则返回正常
                        ChongZhengReturn chongZhengReturn = jiaoFeiService.PostChongZheng(chongZheng).Result;

                        //冲正返回
                        returnString = "0047" + chongZhengReturn.BusinessCode + chongZhengReturn.BankCode + chongZhengReturn.MemberPhone + chongZhengReturn.SerialNumber + chongZhengReturn.ReturnCode;

                        break;
                    #endregion

                    #region 日终对账
                    case "1004":
                        //组装对象
                        RiZhongDuiZhang riZhongDuiZhang = new RiZhongDuiZhang()
                        {
                            BusinessCode = businessCode,
                            BankCode = msg.Substring(8, 12),
                            BusinessDate = msg.Substring(20, 8),
                            FileName = msg.Substring(28, 18)
                        };
                        riZhongDuiZhang.FullFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"FileUpload\{riZhongDuiZhang.FileName.Trim()}");

                        //业务处理 读取文件
                        RiZhongDuiZhangReturn riZhongDuiZhangReturn = jiaoFeiService.PostRiZhongDuiZhang(riZhongDuiZhang);



                        //对账返回
                        returnString = "0020" + riZhongDuiZhangReturn.BusinessCode + riZhongDuiZhangReturn.BankCode + riZhongDuiZhangReturn.ReturnCode;

                        break;

                    #endregion

                    default:
                        break;
                }

                byte[] bufferRet = Encoding.GetEncoding("GBK").GetBytes(returnString);     // 获得缓存
                streamToClient.Write(bufferRet, 0, bufferRet.Length);     // 发往服务器
                Console.WriteLine(returnString);
                logManager.SaveLog(returnString, DateTime.Now);

                #endregion

                #region 关闭流和连接
                if (streamToClient != null)
                    streamToClient.Dispose();
                client.Close();
                #endregion


            }
            catch (Exception ex)
            {
                if (streamToClient != null)
                    streamToClient.Dispose();
                client.Close();
                Console.WriteLine(ex.Message);      // 捕获异常时退出程序              
            }
        }
    }
}
