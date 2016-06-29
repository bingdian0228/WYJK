using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 服务协议实体
    /// </summary>
    public class ServiceProtocol
    {
        /// <summary>
        /// 服务协议内容
        /// </summary>
        [Required(ErrorMessage ="必填")]
        public string ServiceProtocolContent { get; set; }
    }
}
