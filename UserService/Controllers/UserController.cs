using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Data;
using UserService.Dtos;
using Microsoft.AspNetCore.Authorization;
using Npgsql;
using NpgsqlTypes;

namespace UserService.Controllers;
[Authorize]
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    DataContextDapper _dapper;
    public UserController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }

     [HttpGet("GetUsers")]
    // public IEnumerable<User> GetUsers()
    public IEnumerable<User> GetUsers()
    {
        string sql ="SELECT * FROM public.\"Users\"";
        IEnumerable<User> users = _dapper.LoadData<User>(sql);
        return users;
    }

        [HttpGet("GetSingleUser/{userId}")]
    // public IEnumerable<User> GetUsers()
    public User GetSingleUser(int userId)
    {
         string sql ="SELECT * FROM public.\"Users\" WHERE \"UserId\"= " + userId.ToString();
        User user = _dapper.LoadDataSingle<User>(sql);
        return user;
    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
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
            return Ok();
        } 

        throw new Exception("Failed to Update User");
    }


    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
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
            return Ok();
        } 

        throw new Exception("Failed to Add User");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = @"
            DELETE FROM public.""Users"" 
                WHERE ""UserId"" = @UserId";
        
        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@UserId", NpgsqlDbType.Integer) { Value = userId }
        };

        if (_dapper.ExecuteSqlWithParameters(sql, parameters))
        {
            return Ok();
        } 

        throw new Exception("Failed to Delete User");
    }
}