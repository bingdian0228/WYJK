using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WYJK.Entity
{
    /// <summary>
    /// 信息
    /// </summary>
    public class Information
    {   /// <summary>
        /// ID
        /// </summary>		
        public int ID { get; set; }
        /// <summary>
        /// 标题
        /// </summary>		
        [Required(ErrorMessage = "标题不能为空")]
        public string Name { get; set; }
        /// <summary>
        /// 图片名称数组
        /// </summary>		
        public string[] ImgUrls { get; set; }
        /// <summary>
        /// 图片名称
        /// </summary>		
        public string ImgUrl { get; set; }

        /// <summary>
        /// 信息类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// StrContent
        /// </summary>		
        public string StrContent { get; set; }

        public string Type { get; set; }
    }

    /// <summary>
    /// 信息查询参数 (Web)
    /// </summary>
    public class InformationParameter : PagedParameter
    {
        /// <summary>
        /// 标题
        /// </summary>		
        public string Name { get; set; }

        public string Type { get; set; }
    }


}
