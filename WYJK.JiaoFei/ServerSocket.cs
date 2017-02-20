using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.JiaoFei
{
    public class ServerSocket
    {
        private const int BufferSize = 8888;
        public void StartSocket()
        {
            Debug.WriteLine("****************************  Server is running ... ********************************");
            Console.WriteLine("Server is running ... ");
            //IPAddress ip = new IPAddress(new byte[] { 192, 168, 1, 7 });
            IPAddress ip = new IPAddress(new byte[] { 192, 168, 40, 2 });
            TcpListener listener = new TcpListener(ip, 8600);

            listener.Start();           // 开始侦听
            Console.WriteLine("Start Listening ...");

            while (true)
            {
                // 获取一个连接，同步方法，在此处中断
                TcpClient client = listener.AcceptTcpClient();

                //#region 记录连接日志
                //Debug.WriteLine("****************************  收到一个连接请求 ********************************");

                //LogManager logManager = new LogManager(AppDomain.CurrentDomain.BaseDirectory + "ConnectRecord.txt");

                //logManager.SaveLog(string.Format("Client Connected！{0} <-- {1},用户: {2}", client.Client.LocalEndPoint, client.Client.RemoteEndPoint, Encoding.UTF8.GetString(buffer, 0, aaa)), DateTime.Now);

                //#endregion

                RemoteClient wapper = new RemoteClient(client);
            }
        }
    }
}
