using Xunit;
using Moq;
using UserService.Controllers;
using UserService.Models;
using UserService.Data;
using UserService.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace UserService.Tests
{
    public class UserControllerTest
    {


        [Fact]
        public void GetSingleUser_ReturnsUser_WhenUserExists()
        {
            var user = new User
            {
                UserId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Gender = "Male",
                Active = true
            };

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.LoadDataSingle<User>(It.IsAny<string>()))
                .Returns(user);

            var controller = new UserController(mockDataContext.Object);

            var result = controller.GetSingleUser(1);

            result.Should().NotBeNull();
            result.Should().BeOfType<User>();
            result.UserId.Should().Be(1);
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.Email.Should().Be("john.doe@example.com");
            result.Gender.Should().Be("Male");
            result.Active.Should().BeTrue();
            
            mockDataContext.Verify(d => d.LoadDataSingle<User>(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void GetUsers_ReturnsAllUsers_WhenUsersExist()
        {
            var users = new List<User>
            {
                new User
                {
                    UserId = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    Gender = "Male",
                    Active = true
                },
                new User
                {
                    UserId = 2,
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@example.com",
                    Gender = "Female",
                    Active = true
                }
            };

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.LoadData<User>(It.IsAny<string>()))
                .Returns(users);

            var controller = new UserController(mockDataContext.Object);

            var result = controller.GetUsers();
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().UserId.Should().Be(1);
            result.First().FirstName.Should().Be("John");
            result.Last().UserId.Should().Be(2);
            result.Last().FirstName.Should().Be("Jane");
            
            mockDataContext.Verify(d => d.LoadData<User>(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void GetUsers_ReturnsEmptyList_WhenNoUsers()
        {
            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.LoadData<User>(It.IsAny<string>()))
                .Returns(new List<User>());

            var controller = new UserController(mockDataContext.Object);

            var result = controller.GetUsers();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
            mockDataContext.Verify(d => d.LoadData<User>(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void AddUser_ReturnsOk_WhenUserAddedSuccessfully()
        {
            var userToAdd = new UserToAddDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Gender = "Male",
                Active = true
            };

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(true);

            var controller = new UserController(mockDataContext.Object);

            var claims = new List<Claim> { new Claim("userId", "1") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var result = controller.AddUser(userToAdd);
            result.Should().NotBeNull();
            result.Should().BeOfType<OkResult>();
            
            mockDataContext.Verify(
                d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()),
                Times.Once);
        }

        [Fact]
        public void AddUser_ThrowsException_WhenUserAdditionFails()
        {
            var userToAdd = new UserToAddDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Gender = "Male",
                Active = true
            };

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(false);

            var controller = new UserController(mockDataContext.Object);

            var claims = new List<Claim> { new Claim("userId", "1") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var action = () => controller.AddUser(userToAdd);
            action.Should().Throw<Exception>().WithMessage("Failed to Add User");
        }

        [Fact]
        public void EditUser_ReturnsOk_WhenUserEditedSuccessfully()
        {
            var userToEdit = new User
            {
                UserId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Gender = "Male",
                Active = true
            };

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(true);

            var controller = new UserController(mockDataContext.Object);

            var claims = new List<Claim> { new Claim("userId", "1") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var result = controller.EditUser(userToEdit);
            result.Should().NotBeNull();
            result.Should().BeOfType<OkResult>();
            
            mockDataContext.Verify(
                d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()),
                Times.Once);
        }

        [Fact]
        public void EditUser_ThrowsException_WhenUserEditFails()
        {
            var userToEdit = new User
            {
                UserId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Gender = "Male",
                Active = true
            };

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(false);

            var controller = new UserController(mockDataContext.Object);

            var claims = new List<Claim> { new Claim("userId", "1") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var action = () => controller.EditUser(userToEdit);
            action.Should().Throw<Exception>().WithMessage("Failed to Update User");
        }

        [Fact]
        public void DeleteUser_ReturnsOk_WhenUserDeletedSuccessfully()
        {
            int userIdToDelete = 1;

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(true);

            var controller = new UserController(mockDataContext.Object);

            var claims = new List<Claim> { new Claim("userId", "1") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var result = controller.DeleteUser(userIdToDelete);
            result.Should().NotBeNull();
            result.Should().BeOfType<OkResult>();
            
            mockDataContext.Verify(
                d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()),
                Times.Once);
        }

        [Fact]
        public void DeleteUser_ThrowsException_WhenUserDeletionFails()
        {
            int userIdToDelete = 1;

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(false);

            var controller = new UserController(mockDataContext.Object);

            var claims = new List<Claim> { new Claim("userId", "1") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var action = () => controller.DeleteUser(userIdToDelete);
            action.Should().Throw<Exception>().WithMessage("Failed to Delete User");
        }

    }
}
