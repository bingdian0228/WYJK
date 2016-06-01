using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WYJK.Web.Models
{
    public class DrawCashModel
    {
        public string DrawCashIds { get; set; }
        [StringLength(30,ErrorMessage =("打款单号30字以内"))]
        [Required(ErrorMessage ="请填写打款单号")]
        public string OrderNo { get; set; }
    }
}