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
using Microsoft.Extensions.Logging;

namespace AuthService.Controllers
{   
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IDataContextDapper _dapper;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IDataContextDapper dapper, IConfiguration config, ILogger<AuthController> logger)
        {
            _dapper = dapper;
            _config = config;
            _logger = logger;
        }

        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            _logger.LogInformation("Register endpoint called for email: {Email}", userForRegistration.Email);
            
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
                    _logger.LogDebug("Email {Email} is available for registration", userForRegistration.Email);
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
                        _logger.LogDebug("Auth record created successfully for email: {Email}", userForRegistration.Email);
                        
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
                            _logger.LogInformation("User registration successful for email: {Email}", userForRegistration.Email);
                            return Ok();
                        }
                        _logger.LogError("Failed to add user record during registration for email: {Email}", userForRegistration.Email);
                        throw new Exception("Failed to add user.");
                    }
                    _logger.LogError("Failed to create auth record during registration for email: {Email}", userForRegistration.Email);
                    throw new Exception("Failed to register user.");
                }
                _logger.LogWarning("Registration attempt with existing email: {Email}", userForRegistration.Email);
                throw new Exception("User with this email already exists!");
            }
            _logger.LogWarning("Registration attempt with mismatched passwords for email: {Email}", userForRegistration.Email);
            throw new Exception("Passwords do not match!");
        }

        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            _logger.LogInformation("Login endpoint called for email: {Email}", userForLogin.Email);
            string sqlForHashAndSalt = @"select ""PasswordHash"",""PasswordSalt"" from public.""Auth"" where ""Email""='" +
                userForLogin.Email + "'";

            UserForLoginConfirmationDto userForConfirmation = _dapper
                .LoadDataSingle<UserForLoginConfirmationDto>(sqlForHashAndSalt);

            byte[] passwordHash = GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userForConfirmation.PasswordHash[index]){
                    _logger.LogWarning("Login attempt with incorrect password for email: {Email}", userForLogin.Email);
                    return StatusCode(401, "Incorrect password!");
                }
            }

            string userIdSql = @"SELECT ""UserId"" FROM public.""Users"" WHERE ""Email"" = '" +
                userForLogin.Email + "'";

            int userId = _dapper.LoadDataSingle<int>(userIdSql);
            
            string token = CreateToken(userId);
            _logger.LogInformation("Login successful for email: {Email}, userId: {UserId}", userForLogin.Email, userId);
            
            return Ok(new Dictionary<string, string> {
                {"token", token}
            });
        }
        
        [HttpGet("RefreshToken")]
        public string RefreshToken()
        {
            _logger.LogInformation("RefreshToken endpoint called");
            string userIdSql = @"SELECT ""UserId"" FROM public.""Users"" WHERE ""UserId"" = @UserId";
            
            List<NpgsqlParameter> refreshParams = new()
            {
                new NpgsqlParameter("@UserId", int.Parse(User.FindFirst("userId")?.Value ?? "0"))
            };

            int userId = _dapper.LoadDataSingle<int>(userIdSql, refreshParams);
            string newToken = CreateToken(userId);
            
            _logger.LogInformation("Token refreshed successfully for userId: {UserId}", userId);
            return newToken;
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
