using Npgsql;
using UserService.Dtos;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Services;

public class UserDataService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserDataService> _logger;

    public UserDataService(IUserRepository userRepository, ILogger<UserDataService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public IEnumerable<User> GetUsers()
    {
        _logger.LogInformation("GetUsers service called");
        try
        {
            return _userRepository.GetUsers();
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Database error while retrieving users");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving users");
            throw;
        }
    }

    public User? GetSingleUser(int userId)
    {
        _logger.LogInformation("GetSingleUser service called for userId: {UserId}", userId);
        try
        {
            return _userRepository.GetSingleUser(userId);
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Database error while retrieving user {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving user {UserId}", userId);
            throw;
        }
    }

    public bool EditUser(User user)
    {
        _logger.LogInformation("EditUser service called for userId: {UserId}", user.UserId);
        try
        {
            return _userRepository.EditUser(user);
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Database error while editing user {UserId}", user.UserId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error editing user {UserId}", user.UserId);
            throw;
        }
    }

    public bool AddUser(UserToAddDto user)
    {
        _logger.LogInformation("AddUser service called for email: {Email}", user.Email);
        try
        {
            return _userRepository.AddUser(user);
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Database error while adding user {Email}", user.Email);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error adding user {Email}", user.Email);
            throw;
        }
    }

    public bool DeleteUser(int userId)
    {
        _logger.LogInformation("DeleteUser service called for userId: {UserId}", userId);
        try
        {
            return _userRepository.DeleteUser(userId);
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Database error while deleting user {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting user {UserId}", userId);
            throw;
        }
    }
}
