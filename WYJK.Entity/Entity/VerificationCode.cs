using System;

namespace WYJK.Entity
{
    public class VerificationCode
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public string Code { get; set; }
        public DateTime CurrentTime { get; set; }
    }
}
