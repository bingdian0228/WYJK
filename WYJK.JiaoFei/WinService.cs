using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;

namespace WYJK.JiaoFei
{
    public class WinService
    {
        public static void Main()
        {
            HostFactory.Run(x =>                                 
            {
                x.Service<Socket>(s =>                        
                {
                    s.ConstructUsing(name => new Socket());     
                    s.WhenStarted(tc => tc.Start());             
                    s.WhenStopped(tc => tc.Stop());              
                });
                x.RunAsLocalSystem();                            

                x.SetDescription("中威无忧银行缴费通信");        
                x.SetDisplayName("中威无忧银行缴费通信");        
                x.SetServiceName("中威无忧银行缴费通信");        
            });                                                  
            Console.Read();
        }
    }

    public class Socket
    {
        Task task = null;
        CancellationTokenSource source = null;

        public Socket()
        {
            source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            ServerSocket serverSocket = new ServerSocket();

            task = new Task(() =>
            {
                serverSocket.StartSocket();

            }, token);

        }
        public void Start() { task.Start(); }
        public void Stop() { source.Cancel(); }
    }
}
