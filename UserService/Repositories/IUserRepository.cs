using UserService.Dtos;
using UserService.Models;

namespace UserService.Repositories;

public interface IUserRepository
{
    IEnumerable<User> GetUsers();
    User? GetSingleUser(int userId);
    bool EditUser(User user);
    bool AddUser(UserToAddDto user);
    bool DeleteUser(int userId);
}
