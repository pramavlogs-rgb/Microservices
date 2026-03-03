using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Data;
using UserService.Dtos;
using Microsoft.AspNetCore.Authorization;
using Npgsql;
using NpgsqlTypes;
using Microsoft.Extensions.Logging;

namespace UserService.Controllers;
[Authorize]
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    IDataContextDapper _dapper;
    ILogger<UserController> _logger;
    
    public UserController(IDataContextDapper dapper, ILogger<UserController> logger)
    {
        _dapper = dapper;
        _logger = logger;
    }

     [HttpGet("GetUsers")]
    // public IEnumerable<User> GetUsers()
    public IEnumerable<User> GetUsers()
    {
        _logger.LogInformation("GetUsers endpoint called");
        try
        {
            string sql ="SELECT * FROM public.\"Users\"";
            IEnumerable<User> users = _dapper.LoadData<User>(sql);
            _logger.LogDebug("Retrieved {UserCount} users from database", users.Count());
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users from database");
            throw;
        }
    }

        [HttpGet("GetSingleUser/{userId}")]
    // public IEnumerable<User> GetUsers()
    public User GetSingleUser(int userId)
    {
        _logger.LogInformation("GetSingleUser endpoint called for userId: {UserId}", userId);
        try
        {
             string sql ="SELECT * FROM public.\"Users\" WHERE \"UserId\"= " + userId.ToString();
            User user = _dapper.LoadDataSingle<User>(sql);
            _logger.LogDebug("Retrieved user data for userId: {UserId}", userId);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId} from database", userId);
            throw;
        }
    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        _logger.LogInformation("EditUser endpoint called for userId: {UserId}, email: {Email}", user.UserId, user.Email);
        string sql = @"UPDATE public.""Users"" SET ""FirstName"" = @FirstName, ""LastName"" = @LastName, ""Email"" = @Email, ""Gender"" = @Gender, ""Active"" = @Active WHERE ""UserId"" = @UserId";
        
        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@FirstName", NpgsqlDbType.Varchar) { Value = user.FirstName ?? (object)DBNull.Value },
            new NpgsqlParameter("@LastName", NpgsqlDbType.Varchar) { Value = user.LastName ?? (object)DBNull.Value },
            new NpgsqlParameter("@Email", NpgsqlDbType.Varchar) { Value = user.Email ?? (object)DBNull.Value },
            new NpgsqlParameter("@Gender", NpgsqlDbType.Varchar) { Value = user.Gender ?? (object)DBNull.Value },
            new NpgsqlParameter("@Active", NpgsqlDbType.Boolean) { Value = user.Active },
            new NpgsqlParameter("@UserId", NpgsqlDbType.Integer) { Value = user.UserId }
        };
        
        if (_dapper.ExecuteSqlWithParameters(sql, parameters))
        {
            _logger.LogInformation("User updated successfully for userId: {UserId}", user.UserId);
            return Ok();
        } 

        _logger.LogError("Failed to update user for userId: {UserId}", user.UserId);
        throw new Exception("Failed to Update User");
    }


    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        _logger.LogInformation("AddUser endpoint called for email: {Email}", user.Email);
        string sql = @"
            INSERT INTO public.""Users""(
                ""FirstName"",
                ""LastName"",
                ""Email"",
                ""Gender"",
                ""Active""
            ) VALUES (
                @FirstName,
                @LastName,
                @Email,
                @Gender,
                @Active
            )";
        
        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@FirstName", NpgsqlDbType.Varchar) { Value = user.FirstName ?? (object)DBNull.Value },
            new NpgsqlParameter("@LastName", NpgsqlDbType.Varchar) { Value = user.LastName ?? (object)DBNull.Value },
            new NpgsqlParameter("@Email", NpgsqlDbType.Varchar) { Value = user.Email ?? (object)DBNull.Value },
            new NpgsqlParameter("@Gender", NpgsqlDbType.Varchar) { Value = user.Gender ?? (object)DBNull.Value },
            new NpgsqlParameter("@Active", NpgsqlDbType.Boolean) { Value = user.Active }
        };

        if (_dapper.ExecuteSqlWithParameters(sql, parameters))
        {
            _logger.LogInformation("New user created successfully with email: {Email}", user.Email);
            return Ok();
        } 

        _logger.LogError("Failed to add new user with email: {Email}", user.Email);
        throw new Exception("Failed to Add User");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        _logger.LogInformation("DeleteUser endpoint called for userId: {UserId}", userId);
        string sql = @"
            DELETE FROM public.""Users"" 
                WHERE ""UserId"" = @UserId";
        
        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@UserId", NpgsqlDbType.Integer) { Value = userId }
        };

        if (_dapper.ExecuteSqlWithParameters(sql, parameters))
        {
            _logger.LogInformation("User deleted successfully for userId: {UserId}", userId);
            return Ok();
        } 

        _logger.LogError("Failed to delete user for userId: {UserId}", userId);
        throw new Exception("Failed to Delete User");
    }
}