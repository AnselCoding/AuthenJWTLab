using AuthenJWTLab.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenJWTLab.Repository
{
    public class JWTManagerRepository : IJWTManagerRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IRepository<UserRefreshToken> _userRefreshTokenDb;
        private readonly LabDBContext _context;
        public JWTManagerRepository(IConfiguration configuration, IRepository<UserRefreshToken> repository, LabDBContext context)
        {
            _configuration = configuration;
            _userRefreshTokenDb = repository;
            _context = context;
        }

        public async Task<Token> GenerateJWTTokens(User user)
        {
            //_ = int.TryParse(_configuration["JWT:AccessExpireMin"], out var accessExpireMin);
            //_ = int.TryParse(_configuration["JWT:RefreshExpireMin"], out var refreshExpireMin);

            // 讀取組態並轉型
            var accessExpireMin = _configuration.GetSection("JWT:AccessExpireMin").Get<int>();
            var refreshExpireMin = _configuration.GetSection("JWT:RefreshExpireMin").Get<int>();

            var accessToken = GenerateToken(user, accessExpireMin, false); // 使用Expires參數生成 accessToken
            var refreshToken = GenerateToken(user, refreshExpireMin, true); // 使用Expires參數生成 refreshToken
            await ManageRefreshToken(user, refreshToken); // 儲存 refreshToken 到 DB
            return new Token
            {
                Access_Token = accessToken,
                Refresh_Token = refreshToken
            };
        }

        /// <summary>
        /// 生成 JWT Token
        /// </summary>
        /// <param name="user">用戶</param>
        /// <param name="expMin">設置過期時間</param>
        /// <param name="isRefreshToken">是否生成 refreshToken</param>
        /// <returns>string of JWT Token</returns>
        private String GenerateToken(User user, int expMin, bool isRefreshToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);
            var subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, user.Role),
		            // 添加其他需要的身份信息
		        });

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = subject,
                Expires = DateTime.Now.AddMinutes(expMin), // AddJwtBearer 中 ValidateLifetime 必須啟用
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["JWT:Issuer"], // 如果有啟用 Issuer 驗證，需要攜帶 Issuer 資訊
                Audience = _configuration["JWT:Audience"] // 如果有啟用 Audience 驗證，需要攜帶 Audience 資訊
            };
            if (isRefreshToken)
            {
                // RefreshToken 不需要夾帶身分訊息，只需要確認是否驗證通過與是否未過期
                tokenDescriptor.Subject = default;
            }
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token); // 取得 token
        }

        /// <summary>
        /// UserRefreshToken 不存在就加入，已存在就更新。
        /// </summary>
        /// <param name="user"></param>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        private async Task ManageRefreshToken(User user, string refreshToken)
        {

            var dbUserRefreshToken = _context.UserRefreshTokens.FirstOrDefault(x => x.UserId == user.Id);
            if (dbUserRefreshToken == null)
            {
                UserRefreshToken userRefreshToken = new UserRefreshToken
                {
                    Id = default,
                    UserId = user.Id,
                    RefreshToken = refreshToken
                };
                await _userRefreshTokenDb.CreateAsync(userRefreshToken);
            }
            else
            {
                dbUserRefreshToken.RefreshToken = refreshToken;
                await _userRefreshTokenDb.UpdateAsync(dbUserRefreshToken);
            }
        }


        /// <summary>
        /// 使用過期 access token 取得使用者資料。
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                //驗證金鑰、發行者與接收者。
                ValidateIssuer = true, // 如果設為 false，這表示不驗證此項目。(下面其他項目也是一樣概念)
                ValidateAudience = true,
                ValidIssuer = _configuration["JWT:Issuer"],
                ValidAudience = _configuration["JWT:Audience"],
                ValidateLifetime = false, // 是從過期的 token 取資料，所以不驗證期限
                ValidateIssuerSigningKey = true, // 令牌的金鑰。
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"])) // 設置令牌的金鑰。
            };
            ClaimsPrincipal principal = JwtSecurityToken(token, tokenValidationParameters);
            return principal;
        }


        /// <summary>
        /// refresh token 是否通過驗證(期限、金鑰、Issuer、Audience)
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public bool IsRefreshTokenValidate(string refreshToken)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, // 如果設為 false，這表示不驗證此項目。(下面其他項目也是一樣概念)
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = _configuration["JWT:Issuer"],
                ValidAudience = _configuration["JWT:Audience"],
                ValidateIssuerSigningKey = true, // 令牌的金鑰。
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"])) // 設置令牌的金鑰。
            };
            try
            {
                JwtSecurityToken(refreshToken, tokenValidationParameters);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static ClaimsPrincipal JwtSecurityToken(string token, TokenValidationParameters tokenValidationParameters)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            return principal;
        }

    }
}
