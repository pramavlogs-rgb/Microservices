using Npgsql;
using NpgsqlTypes;
using UserService.Data;
using UserService.Dtos;
using UserService.Models;

namespace UserService.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDataContextDapper _dapper;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IDataContextDapper dapper, ILogger<UserRepository> logger)
    {
        _dapper = dapper;
        _logger = logger;
    }

    public IEnumerable<User> GetUsers()
    {
        _logger.LogDebug("Executing GetUsers query");
        string sql = "SELECT * FROM public.\"Users\"";
        IEnumerable<User> users = _dapper.LoadData<User>(sql);
        _logger.LogDebug("Retrieved {UserCount} users from database", users.Count());
        return users;
    }

    public User? GetSingleUser(int userId)
    {
        _logger.LogDebug("Executing GetSingleUser query for userId: {UserId}", userId);
        string sql = "SELECT * FROM public.\"Users\" WHERE \"UserId\" = @UserId";
        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@UserId", NpgsqlDbType.Integer) { Value = userId }
        };
        return _dapper.LoadDataSingle<User>(sql, parameters);
    }

    public bool EditUser(User user)
    {
        _logger.LogDebug("Executing EditUser query for userId: {UserId}", user.UserId);
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
        return _dapper.ExecuteSqlWithParameters(sql, parameters);
    }

    public bool AddUser(UserToAddDto user)
    {
        _logger.LogDebug("Executing AddUser query for email: {Email}", user.Email);
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
        return _dapper.ExecuteSqlWithParameters(sql, parameters);
    }

    public bool DeleteUser(int userId)
    {
        _logger.LogDebug("Executing DeleteUser query for userId: {UserId}", userId);
        string sql = "DELETE FROM public.\"Users\" WHERE \"UserId\" = @UserId";
        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@UserId", NpgsqlDbType.Integer) { Value = userId }
        };
        return _dapper.ExecuteSqlWithParameters(sql, parameters);
    }
}
