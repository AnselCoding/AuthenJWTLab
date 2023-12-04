using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthenJWTLab.Controllers.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JwtController : ControllerBase
    {
        private readonly JwtBearerOptions _jwtOptions;

        public JwtController(IOptionsMonitor<JwtBearerOptions> jwtOptions)
        {
            //_jwtOptions = jwtBearerOptions.Get("Path2Scheme");

            // JwtBearerDefaults.AuthenticationScheme 是 ASP.NET Core 中默認的 JWT Bearer 身份驗證方案。這個方案的名稱通常是 "Bearer"。
            _jwtOptions = jwtOptions.Get("Bearer");
        }


        [HttpGet]
        public IActionResult GetJwtKey()
        {
            // 獲取 IssuerSigningKey
            var issuerSigningKey = _jwtOptions.TokenValidationParameters.IssuerSigningKey;

            // 在這裡使用 issuerSigningKey
            // ...
            return Ok(issuerSigningKey);
        }
    }
}
