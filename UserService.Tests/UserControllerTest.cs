using Xunit;
using Moq;
using UserService.Controllers;
using UserService.Models;
using UserService.Dtos;
using UserService.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace UserService.Tests
{
    public class UserControllerTest
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ILogger<UserController>> _mockLogger;
        private readonly UserController _controller;

        public UserControllerTest()
        {
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<UserController>>();
            _controller = new UserController(_mockLogger.Object, _mockUserService.Object);
        }

        [Fact]
        public void GetUsers_ReturnsOkWithAllUsers_WhenUsersExist()
        {
            var users = new List<User>
            {
                new User { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Gender = "Male", Active = true },
                new User { UserId = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", Gender = "Female", Active = true }
            };
            _mockUserService.Setup(s => s.GetUsers()).Returns(users);

            var result = _controller.GetUsers();

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<User>>().Subject;
            returnedUsers.Should().HaveCount(2);
            returnedUsers.First().UserId.Should().Be(1);
            returnedUsers.Last().UserId.Should().Be(2);
            _mockUserService.Verify(s => s.GetUsers(), Times.Once);
        }

        [Fact]
        public void GetUsers_ReturnsOkWithEmptyList_WhenNoUsersExist()
        {
            _mockUserService.Setup(s => s.GetUsers()).Returns(new List<User>());

            var result = _controller.GetUsers();

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<User>>().Subject;
            returnedUsers.Should().BeEmpty();
        }

        [Fact]
        public void GetUsers_Returns503_WhenDatabaseUnavailable()
        {
            _mockUserService.Setup(s => s.GetUsers()).Throws<Npgsql.NpgsqlException>();

            var result = _controller.GetUsers();

            var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(503);
        }

        [Fact]
        public void GetUsers_Returns500_WhenUnexpectedErrorOccurs()
        {
            _mockUserService.Setup(s => s.GetUsers()).Throws<Exception>();

            var result = _controller.GetUsers();

            var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public void GetSingleUser_ReturnsOkWithUser_WhenUserExists()
        {
            var user = new User { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Gender = "Male", Active = true };
            _mockUserService.Setup(s => s.GetSingleUser(1)).Returns(user);

            var result = _controller.GetSingleUser(1);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedUser = okResult.Value.Should().BeOfType<User>().Subject;
            returnedUser.UserId.Should().Be(1);
            returnedUser.FirstName.Should().Be("John");
            _mockUserService.Verify(s => s.GetSingleUser(1), Times.Once);
        }

        [Fact]
        public void GetSingleUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            _mockUserService.Setup(s => s.GetSingleUser(99)).Returns((User?)null);

            var result = _controller.GetSingleUser(99);

            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public void GetSingleUser_Returns503_WhenDatabaseUnavailable()
        {
            _mockUserService.Setup(s => s.GetSingleUser(It.IsAny<int>())).Throws<Npgsql.NpgsqlException>();

            var result = _controller.GetSingleUser(1);

            var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(503);
        }

        [Fact]
        public void GetSingleUser_Returns500_WhenUnexpectedErrorOccurs()
        {
            _mockUserService.Setup(s => s.GetSingleUser(It.IsAny<int>())).Throws<Exception>();

            var result = _controller.GetSingleUser(1);

            var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public void AddUser_ReturnsOk_WhenUserAddedSuccessfully()
        {
            var userToAdd = new UserToAddDto { FirstName = "John", LastName = "Doe", Email = "john@example.com", Gender = "Male", Active = true };
            _mockUserService.Setup(s => s.AddUser(userToAdd)).Returns(true);

            var result = _controller.AddUser(userToAdd);

            result.Should().BeOfType<OkResult>();
            _mockUserService.Verify(s => s.AddUser(userToAdd), Times.Once);
        }

        [Fact]
        public void AddUser_Returns500_WhenUserAdditionFails()
        {
            var userToAdd = new UserToAddDto { FirstName = "John", LastName = "Doe", Email = "john@example.com", Gender = "Male", Active = true };
            _mockUserService.Setup(s => s.AddUser(userToAdd)).Returns(false);

            var result = _controller.AddUser(userToAdd);

            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public void AddUser_Returns503_WhenDatabaseUnavailable()
        {
            var userToAdd = new UserToAddDto { FirstName = "John", LastName = "Doe", Email = "john@example.com", Gender = "Male", Active = true };
            _mockUserService.Setup(s => s.AddUser(It.IsAny<UserToAddDto>())).Throws<Npgsql.NpgsqlException>();

            var result = _controller.AddUser(userToAdd);

            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(503);
        }

        [Fact]
        public void AddUser_Returns500_WhenUnexpectedErrorOccurs()
        {
            var userToAdd = new UserToAddDto { FirstName = "John", LastName = "Doe", Email = "john@example.com", Gender = "Male", Active = true };
            _mockUserService.Setup(s => s.AddUser(It.IsAny<UserToAddDto>())).Throws<Exception>();

            var result = _controller.AddUser(userToAdd);

            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public void EditUser_ReturnsOk_WhenUserEditedSuccessfully()
        {
            var user = new User { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Gender = "Male", Active = true };
            _mockUserService.Setup(s => s.EditUser(user)).Returns(true);

            var result = _controller.EditUser(user);

            result.Should().BeOfType<OkResult>();
            _mockUserService.Verify(s => s.EditUser(user), Times.Once);
        }

        [Fact]
        public void EditUser_Returns500_WhenUserEditFails()
        {
            var user = new User { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Gender = "Male", Active = true };
            _mockUserService.Setup(s => s.EditUser(user)).Returns(false);

            var result = _controller.EditUser(user);

            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public void EditUser_Returns503_WhenDatabaseUnavailable()
        {
            var user = new User { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Gender = "Male", Active = true };
            _mockUserService.Setup(s => s.EditUser(It.IsAny<User>())).Throws<Npgsql.NpgsqlException>();

            var result = _controller.EditUser(user);

            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(503);
        }

        [Fact]
        public void EditUser_Returns500_WhenUnexpectedErrorOccurs()
        {
            var user = new User { UserId = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", Gender = "Male", Active = true };
            _mockUserService.Setup(s => s.EditUser(It.IsAny<User>())).Throws<Exception>();

            var result = _controller.EditUser(user);

            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public void DeleteUser_ReturnsOk_WhenUserDeletedSuccessfully()
        {
            _mockUserService.Setup(s => s.DeleteUser(1)).Returns(true);

            var result = _controller.DeleteUser(1);

            result.Should().BeOfType<OkResult>();
            _mockUserService.Verify(s => s.DeleteUser(1), Times.Once);
        }

        [Fact]
        public void DeleteUser_Returns500_WhenUserDeletionFails()
        {
            _mockUserService.Setup(s => s.DeleteUser(1)).Returns(false);

            var result = _controller.DeleteUser(1);

            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public void DeleteUser_Returns503_WhenDatabaseUnavailable()
        {
            _mockUserService.Setup(s => s.DeleteUser(It.IsAny<int>())).Throws<Npgsql.NpgsqlException>();

            var result = _controller.DeleteUser(1);

            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(503);
        }

        [Fact]
        public void DeleteUser_Returns500_WhenUnexpectedErrorOccurs()
        {
            _mockUserService.Setup(s => s.DeleteUser(It.IsAny<int>())).Throws<Exception>();

            var result = _controller.DeleteUser(1);

            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
        }
    }
}