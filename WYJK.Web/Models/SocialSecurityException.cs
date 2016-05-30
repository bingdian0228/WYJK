using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WYJK.Web.Models
{
    public class SocialSecurityException
    {
        public string PeopleIds { get; set; }
        [StringLength(200,ErrorMessage =("异常信息200字以内"))]
        [Required(ErrorMessage ="请填写异常信息")]
        public string Exception { get; set; }
    }
}