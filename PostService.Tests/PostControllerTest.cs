using Xunit;
using Moq;
using Moq.Protected;
using Microsoft.Extensions.Logging;
using PostService.Controllers;
using PostService.Models;
using PostService.Data;
using PostService.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace PostService.Tests
{
    public class PostControllerTest
    {
        #region GetSinglePost Tests

        [Fact]
        public void GetSinglePost_ReturnsPost_WhenPostExists()
        {
            var post = new Post
            {
                PostId = 1,
                UserId = 5,
                PostTitle = "Understanding Microservices",
                PostContent = "Microservices architecture allows independent service deployment...",
                PostCreated = new DateTime(2024, 1, 15, 10, 30, 0),
                PostUpdated = new DateTime(2024, 1, 20, 14, 45, 0)
            };

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.LoadDataSingle<Post>(It.IsAny<string>()))
                .Returns(post);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var controller = new PostController(mockDataContext.Object, mockHttpClientFactory.Object, new Mock<ILogger<PostController>>().Object);

            var result = controller.GetSinglePost(1);
            result.Should().NotBeNull();
            result.Should().BeOfType<Post>();
            result.PostId.Should().Be(1);
            result.UserId.Should().Be(5);
            result.PostTitle.Should().Be("Understanding Microservices");
            result.PostContent.Should().Contain("Microservices architecture");
            result.PostCreated.Should().Be(new DateTime(2024, 1, 15, 10, 30, 0));
            result.PostUpdated.Should().Be(new DateTime(2024, 1, 20, 14, 45, 0));
            
            mockDataContext.Verify(d => d.LoadDataSingle<Post>(It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region GetPosts Tests

        [Fact]
        public void GetPosts_ReturnsAllPosts_WhenPostsExist()
        {
            var posts = new List<Post>
            {
                new Post
                {
                    PostId = 1,
                    UserId = 5,
                    PostTitle = "First Post",
                    PostContent = "Content of first post",
                    PostCreated = new DateTime(2024, 1, 15, 10, 30, 0),
                    PostUpdated = new DateTime(2024, 1, 20, 14, 45, 0)
                },
                new Post
                {
                    PostId = 2,
                    UserId = 7,
                    PostTitle = "Second Post",
                    PostContent = "Content of second post",
                    PostCreated = new DateTime(2024, 1, 16, 11, 45, 0),
                    PostUpdated = new DateTime(2024, 1, 21, 15, 30, 0)
                }
            };

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.LoadData<Post>(It.IsAny<string>()))
                .Returns(posts);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var controller = new PostController(mockDataContext.Object, mockHttpClientFactory.Object, new Mock<ILogger<PostController>>().Object);

            var result = controller.GetPosts();
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().PostId.Should().Be(1);
            result.First().PostTitle.Should().Be("First Post");
            result.Last().PostId.Should().Be(2);
            result.Last().PostTitle.Should().Be("Second Post");
            
            mockDataContext.Verify(d => d.LoadData<Post>(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void GetPosts_ReturnsEmptyList_WhenNoPosts()
        {
            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.LoadData<Post>(It.IsAny<string>()))
                .Returns(new List<Post>());

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var controller = new PostController(mockDataContext.Object, mockHttpClientFactory.Object, new Mock<ILogger<PostController>>().Object);

            var result = controller.GetPosts();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
            mockDataContext.Verify(d => d.LoadData<Post>(It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region AddPost Tests

        [Fact]
        public void AddPost_ReturnsOk_WhenPostAddedSuccessfully()
        {
            var postToAdd = new PostToAddDto
            {
                PostTitle = "New Post Title",
                PostContent = "New post content here"
            };

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(true);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var controller = new PostController(mockDataContext.Object, mockHttpClientFactory.Object, new Mock<ILogger<PostController>>().Object);

            // Setup user context
            var claims = new List<Claim> { new Claim("userId", "5") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var result = controller.AddPost(postToAdd);
            result.Should().NotBeNull();
            result.Should().BeOfType<OkResult>();
            
            mockDataContext.Verify(
                d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()),
                Times.Once);
        }

        [Fact]
        public void AddPost_ThrowsException_WhenPostAdditionFails()
        {
            var postToAdd = new PostToAddDto
            {
                PostTitle = "New Post",
                PostContent = "Content"
            };

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(false);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var controller = new PostController(mockDataContext.Object, mockHttpClientFactory.Object, new Mock<ILogger<PostController>>().Object);

            // Setup user context
            var claims = new List<Claim> { new Claim("userId", "5") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var action = () => controller.AddPost(postToAdd);
            action.Should().Throw<Exception>().WithMessage("Failed to create new post!");
        }

        #endregion

        #region EditPost Tests

        [Fact]
        public void EditPost_ReturnsOk_WhenPostEditedSuccessfully()
        {
            var postToEdit = new PostToEditDto
            {
                PostId = 1,
                UserId = 5,
                PostTitle = "Updated Title",
                PostContent = "Updated content here"
            };

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(true);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var controller = new PostController(mockDataContext.Object, mockHttpClientFactory.Object, new Mock<ILogger<PostController>>().Object);

            // Setup user context
            var claims = new List<Claim> { new Claim("userId", "5") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var result = controller.EditPost(postToEdit);
            result.Should().NotBeNull();
            result.Should().BeOfType<OkResult>();
            
            mockDataContext.Verify(
                d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()),
                Times.Once);
        }

        [Fact]
        public void EditPost_ThrowsException_WhenPostEditFails()
        {
            var postToEdit = new PostToEditDto
            {
                PostId = 1,
                UserId = 5,
                PostTitle = "Updated Title",
                PostContent = "Updated content"
            };

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(false);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var controller = new PostController(mockDataContext.Object, mockHttpClientFactory.Object, new Mock<ILogger<PostController>>().Object);

            // Setup user context
            var claims = new List<Claim> { new Claim("userId", "5") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var action = () => controller.EditPost(postToEdit);
            action.Should().Throw<Exception>().WithMessage("Failed to edit post!");
        }

        #endregion

        #region DeletePost Tests

        [Fact]
        public void DeletePost_ReturnsOk_WhenPostDeletedSuccessfully()
        {
            int postIdToDelete = 1;

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(true);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var controller = new PostController(mockDataContext.Object, mockHttpClientFactory.Object, new Mock<ILogger<PostController>>().Object);

            // Setup user context
            var claims = new List<Claim> { new Claim("userId", "5") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var result = controller.DeletePost(postIdToDelete);
            result.Should().NotBeNull();
            result.Should().BeOfType<OkResult>();
            
            mockDataContext.Verify(
                d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()),
                Times.Once);
        }

        [Fact]
        public void DeletePost_ThrowsException_WhenPostDeletionFails()
        {
            int postIdToDelete = 1;

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.ExecuteSqlWithParameters(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(false);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var controller = new PostController(mockDataContext.Object, mockHttpClientFactory.Object, new Mock<ILogger<PostController>>().Object);

            // Setup user context
            var claims = new List<Claim> { new Claim("userId", "5") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var action = () => controller.DeletePost(postIdToDelete);
            action.Should().Throw<Exception>().WithMessage("Failed to delete post!");
        }

        #endregion

        #region GetPostsWithUserInfo Tests

        [Fact]
        public async Task GetPostsWithUserInfo_ReturnsObjectResult_WhenCalled()
        {
            int userId = 5;
            var posts = new List<Post>
            {
                new Post
                {
                    PostId = 1,
                    UserId = userId,
                    PostTitle = "User's First Post",
                    PostContent = "Content of user's post",
                    PostCreated = new DateTime(2024, 1, 15, 10, 30, 0),
                    PostUpdated = new DateTime(2024, 1, 20, 14, 45, 0)
                }
            };

            var mockDataContext = new Mock<IDataContextDapper>();
            mockDataContext.Setup(d => d.LoadData<Post>(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()))
                .Returns(posts);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            var controller = new PostController(mockDataContext.Object, mockHttpClientFactory.Object, new Mock<ILogger<PostController>>().Object);

            // Setup user context
            var claims = new List<Claim> { new Claim("userId", userId.ToString()) };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(r => r.Headers["Authorization"])
                .Returns("Bearer test-token");

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
            mockHttpContext.Setup(c => c.User).Returns(principal);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            var result = await controller.GetPostsWithUserInfo(userId);
            result.Should().NotBeNull();
            result.Should().BeOfType<ObjectResult>();
            
            mockDataContext.Verify(
                d => d.LoadData<Post>(It.IsAny<string>(), It.IsAny<List<Npgsql.NpgsqlParameter>>()),
                Times.Once);
        }

        #endregion
    }
}
