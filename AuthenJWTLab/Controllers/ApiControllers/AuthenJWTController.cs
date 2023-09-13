using AuthenJWTLab.Middleware;
using AuthenJWTLab.Models;
using AuthenJWTLab.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthenJWTLab.Controllers.ApiControllers
{
    [AllowAnonymous]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthenJWTController : ControllerBase
    {
        private readonly LabDBContext _context;
        private readonly IJWTManagerRepository _jWTManager;
        public AuthenJWTController(LabDBContext context,IJWTManagerRepository jWTManager)
        {
            _context = context;
            _jWTManager = jWTManager;
        }


        /// <summary>
        /// 使用帳密取得 JWT Token (登入時)。
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        // POST: api/AuthenJWT/Authenticate
        [HttpPost]
        public async Task<IActionResult> AuthenticateAsync(User user)
        {
            // 確認帳密，驗證沒通過 return Unauthorized("Incorrect username or password!");
            var dbUser = await _context.Users.FirstOrDefaultAsync(x => x.Name == user.Name);
            if (dbUser == null || dbUser.Password != user.Password)
            {
                return NotFound("Incorrect username or password!");
            }

            // 通過，執行生成 JWT token 物件
            var tokenObj = await _jWTManager.GenerateJWTTokens(dbUser);

            // 回傳 token 物件 
            return Ok(new { User=dbUser, Token=tokenObj});
        }

        /// <summary>
        /// 刷新 JWT Token，如果 refresh token 未過期 (欲保持登入)。
        /// </summary>
        /// <param name="token">Token 物件</param>
        /// <returns></returns>
        // POST: api/AuthenJWT/RefreshJWTToken
        [HttpPost]
        [TypeFilter(typeof(JwtSecurityExceptionFilter))]
        public async Task<IActionResult> RefreshJWTToken(Token token)
        {
            if (!_jWTManager.IsRefreshTokenValidate(token.Refresh_Token))
            {
                return Unauthorized("Refresh Token Invalid!");
            }
            var principal = _jWTManager.GetPrincipalFromExpiredToken(token.Access_Token);
            var userIdStr = principal.FindFirstValue("Id");
            Int32.TryParse(userIdStr, out var userId);

            //Retrieve the saved refresh token from database
            var savedRefreshToken = _context.UserRefreshTokens.FirstOrDefault(x => x.UserId == userId);
            if (savedRefreshToken == null || savedRefreshToken.RefreshToken != token.Refresh_Token)
            {
                return Unauthorized("Invalid attempt!");
            }

            // 通過，執行生成 JWT token 物件
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);
            var newJwtToken = await _jWTManager.GenerateJWTTokens(user);

            return Ok(newJwtToken); // 回傳 token 物件
        }
    }
}
