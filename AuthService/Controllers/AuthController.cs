using System.Data;
using Npgsql;
using System.Security.Cryptography;
using System.Text;
using AuthService.Data;
using AuthService.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace AuthService.Controllers
{   
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IDataContextDapper _dapper;
        private readonly IConfiguration _config;

        public AuthController(IDataContextDapper dapper, IConfiguration config)
        {
            _dapper = dapper;
            _config = config;
        }

        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                string sqlCheckUserExists = "SELECT \"Email\" FROM public.\"Auth\" WHERE \"Email\" = @Email";

                List<NpgsqlParameter> checkUserParams = new()
                {
                    new NpgsqlParameter("@Email", userForRegistration.Email)
                };

                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists, checkUserParams);
                if (existingUsers.Count() == 0)
                {
                    byte[] passwordSalt = new byte[128 / 8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }

                    byte[] passwordHash = GetPasswordHash(userForRegistration.Password, passwordSalt);

                    string sqlAddAuth = @"
                                    INSERT INTO public.""Auth"" (""Email"", ""PasswordHash"", ""PasswordSalt"") 
                                    VALUES (@Email, @PasswordHash, @PasswordSalt)";
                    List<NpgsqlParameter> sqlParameters = new()
                    {
                        new NpgsqlParameter("@Email", userForRegistration.Email),
                        new NpgsqlParameter("@PasswordSalt", NpgsqlTypes.NpgsqlDbType.Bytea) { Value = passwordSalt },
                        new NpgsqlParameter("@PasswordHash", NpgsqlTypes.NpgsqlDbType.Bytea) { Value = passwordHash }
                    };

                    if (_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters))
                    {
                        
                        string sqlAddUser = @"
    INSERT INTO public.""Users"" (""FirstName"", ""LastName"", ""Email"", ""Gender"", ""Active"")
    VALUES (@FirstName, @LastName, @Email, @Gender, true)";
                        
                        List<NpgsqlParameter> userParameters = new()
                        {
                            new NpgsqlParameter("@FirstName", userForRegistration.FirstName),
                            new NpgsqlParameter("@LastName", userForRegistration.LastName),
                            new NpgsqlParameter("@Email", userForRegistration.Email),
                            new NpgsqlParameter("@Gender", userForRegistration.Gender)
                        };

                        if (_dapper.ExecuteSqlWithParameters(sqlAddUser, userParameters))
                        {
                            return Ok();
                        }
                        throw new Exception("Failed to add user.");
                    }
                    throw new Exception("Failed to register user.");
                }
                throw new Exception("User with this email already exists!");
            }
            throw new Exception("Passwords do not match!");
        }

        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            string sqlForHashAndSalt = @"select ""PasswordHash"",""PasswordSalt"" from public.""Auth"" where ""Email""='" +
                userForLogin.Email + "'";

            UserForLoginConfirmationDto userForConfirmation = _dapper
                .LoadDataSingle<UserForLoginConfirmationDto>(sqlForHashAndSalt);

            byte[] passwordHash = GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userForConfirmation.PasswordHash[index]){
                    return StatusCode(401, "Incorrect password!");
                }
            }

            string userIdSql = @"SELECT ""UserId"" FROM public.""Users"" WHERE ""Email"" = '" +
                userForLogin.Email + "'";

            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            return Ok(new Dictionary<string, string> {
                {"token", CreateToken(userId)}
            });
        }
        
        [HttpGet("RefreshToken")]
        public string RefreshToken()
        {
            string userIdSql = @"SELECT ""UserId"" FROM public.""Users"" WHERE ""UserId"" = @UserId";
            
            List<NpgsqlParameter> refreshParams = new()
            {
                new NpgsqlParameter("@UserId", int.Parse(User.FindFirst("userId")?.Value ?? "0"))
            };

            int userId = _dapper.LoadDataSingle<int>(userIdSql, refreshParams);

            return CreateToken(userId);
        }

        private byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value +
                Convert.ToBase64String(passwordSalt);

            return KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 1000000,
                numBytesRequested: 256 / 8
            );
        }

        private string CreateToken(int userId)
        {
            Claim[] claims = new Claim[] {
                new Claim("userId", userId.ToString())
            };

            string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;
            
            if (string.IsNullOrEmpty(tokenKeyString))
                throw new Exception("TokenKey is not configured in appsettings.json");

            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(tokenKeyString)
                );

            SigningCredentials credentials = new SigningCredentials(
                    tokenKey,
                    SecurityAlgorithms.HmacSha512Signature
                );

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddDays(1)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
