using Xunit;
using Moq;
using AuthService.Controllers;
using AuthService.Data;
using AuthService.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthService.Tests
{
    public class AuthControllerTest
    {
        private IConfiguration GetMockConfiguration()
        {
            var configDict = new Dictionary<string, string>
            {
                {"AppSettings:TokenKey", "test-key-for-jwt-token-generation-must-be-long-enough-for-512-bits"},
                {"AppSettings:PasswordKey", "test-password-key"}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();
            return configuration;
        }

        #region Register Tests

        [Fact]
        public void Register_ReturnsOk_WhenPasswordsMatch()
        {
            var userForRegistration = new UserForRegistrationDto
            {
                Email = "newuser@example.com",
                FirstName = "John",
                LastName = "Doe",
                Gender = "Male",
                Password = "Test123!",
                PasswordConfirm = "Test123!"
            };

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.LoadData<string>(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(new List<string>());
            mockDataContext.Setup(d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(true);

            var config = GetMockConfiguration();
            var controller = new AuthController(mockDataContext.Object, config);

            var result = controller.Register(userForRegistration);

            result.Should().NotBeNull();
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public void Register_ThrowsException_WhenPasswordsDontMatch()
        {
            var userForRegistration = new UserForRegistrationDto
            {
                Email = "newuser@example.com",
                FirstName = "John",
                LastName = "Doe",
                Gender = "Male",
                Password = "Test123!",
                PasswordConfirm = "DifferentPassword"
            };

            var mockDataContext = new Mock<IDataContextDapper>();
            var config = GetMockConfiguration();
            var controller = new AuthController(mockDataContext.Object, config);

            var action = () => controller.Register(userForRegistration);

            action.Should().Throw<Exception>().WithMessage("Passwords do not match!");
        }

        [Fact]
        public void Register_ThrowsException_WhenUserAlreadyExists()
        {
            var userForRegistration = new UserForRegistrationDto
            {
                Email = "existing@example.com",
                FirstName = "John",
                LastName = "Doe",
                Gender = "Male",
                Password = "Test123!",
                PasswordConfirm = "Test123!"
            };

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.LoadData<string>(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(new List<string> { "existing@example.com" });

            var config = GetMockConfiguration();
            var controller = new AuthController(mockDataContext.Object, config);

            var action = () => controller.Register(userForRegistration);

            action.Should().Throw<Exception>().WithMessage("User with this email already exists!");
        }

        [Fact]
        public void Register_ThrowsException_WhenAuthInsertFails()
        {
            var userForRegistration = new UserForRegistrationDto
            {
                Email = "newuser@example.com",
                FirstName = "John",
                LastName = "Doe",
                Gender = "Male",
                Password = "Test123!",
                PasswordConfirm = "Test123!"
            };

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.LoadData<string>(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(new List<string>());
            mockDataContext.Setup(d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(false);

            var config = GetMockConfiguration();
            var controller = new AuthController(mockDataContext.Object, config);

            var action = () => controller.Register(userForRegistration);

            action.Should().Throw<Exception>().WithMessage("Failed to register user.");
        }

        [Fact]
        public void Register_ThrowsException_WhenUserInsertFails()
        {
            var userForRegistration = new UserForRegistrationDto
            {
                Email = "newuser@example.com",
                FirstName = "John",
                LastName = "Doe",
                Gender = "Male",
                Password = "Test123!",
                PasswordConfirm = "Test123!"
            };

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.LoadData<string>(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(new List<string>());

            var callCount = 0;
            mockDataContext.Setup(d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(() =>
                {
                    callCount++;
                    return callCount == 1;
                });

            var config = GetMockConfiguration();
            var controller = new AuthController(mockDataContext.Object, config);

            var action = () => controller.Register(userForRegistration);

            action.Should().Throw<Exception>().WithMessage("Failed to add user.");
        }

        #endregion

        #region Login Tests

        [Fact]
        public void Login_ReturnsOkWithToken_WhenCredentialsAreValid()
        {
            var userForLogin = new UserForLoginDto
            {
                Email = "user@example.com",
                Password = "Test123!"
            };

            var userForConfirmation = new UserForLoginConfirmationDto
            {
                PasswordHash = new byte[] { 1, 2, 3, 4, 5 },
                PasswordSalt = System.Text.Encoding.UTF8.GetBytes("salt")
            };

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.LoadDataSingle<UserForLoginConfirmationDto>(It.IsAny<string>()))
                .Returns(userForConfirmation);
            mockDataContext.Setup(d => d.LoadDataSingle<int>(It.IsAny<string>()))
                .Returns(1);

            var config = GetMockConfiguration();
            var controller = new AuthController(mockDataContext.Object, config);

            var claims = new List<Claim> { new Claim("userId", "1") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var result = controller.Login(userForLogin);

            result.Should().NotBeNull();
        }

        #endregion

        #region RefreshToken Tests

        [Fact]
        public void RefreshToken_ReturnsToken_WhenCalled()
        {
            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.LoadDataSingle<int>(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(1);

            var config = GetMockConfiguration();
            var controller = new AuthController(mockDataContext.Object, config);

            var claims = new List<Claim> { new Claim("userId", "1") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var result = controller.RefreshToken();

            result.Should().NotBeNullOrEmpty();
            result.Should().BeOfType<string>();
            
            mockDataContext.Verify(
                d => d.LoadDataSingle<int>(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()),
                Times.Once);
        }

        #endregion
    }
}
