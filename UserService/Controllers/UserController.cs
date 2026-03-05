using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Dtos;
using UserService.Services;
using Microsoft.AspNetCore.Authorization;
using Npgsql;
using Microsoft.Extensions.Logging;

namespace UserService.Controllers;
[Authorize]
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;

    public UserController(ILogger<UserController> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [HttpGet("GetUsers")]
    public ActionResult<IEnumerable<User>> GetUsers()
    {
        _logger.LogInformation("GetUsers endpoint called");
        try
        {
            IEnumerable<User> users = _userService.GetUsers();
            return Ok(users);
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Database error while retrieving users");
            return StatusCode(503, "Database unavailable. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving users from database");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpGet("GetSingleUser/{userId}")]
    public ActionResult<User> GetSingleUser(int userId)
    {
        _logger.LogInformation("GetSingleUser endpoint called for userId: {UserId}", userId);
        try
        {
            User user = _userService.GetSingleUser(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for userId: {UserId}", userId);
                return NotFound($"User with ID {userId} not found.");
            }
            _logger.LogDebug("Retrieved user data for userId: {UserId}", userId);
            return Ok(user);
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Database error while retrieving user {UserId}", userId);
            return StatusCode(503, "Database unavailable. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving user {UserId} from database", userId);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        _logger.LogInformation("EditUser endpoint called for userId: {UserId}", user.UserId);
        try
        {
            if (_userService.EditUser(user))
            {
                _logger.LogInformation("User updated successfully for userId: {UserId}", user.UserId);
                return Ok();
            }
            _logger.LogError("Failed to update user for userId: {UserId}", user.UserId);
            return StatusCode(500, "Failed to Update User");
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Database error while editing user {UserId}", user.UserId);
            return StatusCode(503, "Database unavailable. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error editing user {UserId}", user.UserId);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        _logger.LogInformation("AddUser endpoint called for email: {Email}", user.Email);
        try
        {
            if (_userService.AddUser(user))
            {
                _logger.LogInformation("New user created successfully with email: {Email}", user.Email);
                return Ok();
            }
            _logger.LogError("Failed to add user with email: {Email}", user.Email);
            return StatusCode(500, "Failed to Add User");
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Database error while adding user {Email}", user.Email);
            return StatusCode(503, "Database unavailable. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error adding user {Email}", user.Email);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        _logger.LogInformation("DeleteUser endpoint called for userId: {UserId}", userId);
        try
        {
            if (_userService.DeleteUser(userId))
            {
                _logger.LogInformation("User deleted successfully for userId: {UserId}", userId);
                return Ok();
            }
            _logger.LogError("Failed to delete user for userId: {UserId}", userId);
            return StatusCode(500, "Failed to Delete User");
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Database error while deleting user {UserId}", userId);
            return StatusCode(503, "Database unavailable. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting user {UserId}", userId);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }
}
