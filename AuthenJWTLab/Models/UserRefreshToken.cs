using System;
using System.Collections.Generic;

namespace AuthenJWTLab.Models
{
    public partial class UserRefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string RefreshToken { get; set; } = null!;
    }
}
