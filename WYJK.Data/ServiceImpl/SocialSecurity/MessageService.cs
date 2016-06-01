using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WYJK.Data.IService;
using WYJK.Entity;

namespace WYJK.Data.ServiceImpl
{
    /// <summary>
    /// 消息实现类
    /// </summary>
    public class MessageService : IMessageService
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(Message message)
        {
            string sqlstr = $"insert into Message(MemberID,ContentStr) values({message.MemberID},{message.ContentStr})";
            DbHelper.ExecuteSqlCommand(sqlstr, null);
        }
    }
}
