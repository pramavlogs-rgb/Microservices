using Microsoft.AspNetCore.Mvc;
using PostService.Models;
using PostService.Data;
using PostService.Dtos;
using Microsoft.AspNetCore.Authorization;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace PostService.Controllers;
[Authorize]
[ApiController]
[Route("[controller]")]
public class PostController : ControllerBase
{
    IDataContextDapper _dapper;
    IHttpClientFactory _httpClientFactory;
    ILogger<PostController> _logger;
    
    public PostController(IDataContextDapper dapper, IHttpClientFactory httpClientFactory, ILogger<PostController> logger)
    {
        _dapper = dapper;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }


    [HttpGet("GetPosts")]
    public IEnumerable<Post> GetPosts()
    {
        _logger.LogInformation("GetPosts endpoint called");
        try
        {
            string sql ="SELECT * FROM public.\"Posts\"";
            IEnumerable<Post> posts = _dapper.LoadData<Post>(sql);
            _logger.LogDebug("Retrieved {PostCount} posts from database", posts.Count());
            return posts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving posts from database");
            throw;
        }
    }

        [HttpGet("GetSinglePost/{postId}")]
  
    public Post GetSinglePost(int postId)
    {
        _logger.LogInformation("GetSinglePost endpoint called for postId: {PostId}", postId);
        try
        {
             string sql ="SELECT * FROM public.\"Posts\" WHERE \"PostId\"= " + postId.ToString();
            Post post = _dapper.LoadDataSingle<Post>(sql);
            _logger.LogDebug("Retrieved post data for postId: {PostId}", postId);
            return post;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving post {PostId} from database", postId);
            throw;
        }
    }

    [HttpPost("Post")]
        public IActionResult AddPost(PostToAddDto postToAdd)
        {
            _logger.LogInformation("AddPost endpoint called for userId: {UserId}, title: {PostTitle}", 
                int.Parse(this.User.FindFirst("userId")?.Value ?? "0"), postToAdd.PostTitle);
            string sql = @"INSERT INTO public.""Posts""(""UserId"", ""PostTitle"", ""PostContent"", ""PostCreated"", ""PostUpdated"") VALUES (@UserId, @PostTitle, @PostContent, NOW(), NOW())";
            
            int userId = int.Parse(this.User.FindFirst("userId")?.Value ?? "0");
            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@UserId", userId),
                new NpgsqlParameter("@PostTitle", postToAdd.PostTitle ?? (object)DBNull.Value),
                new NpgsqlParameter("@PostContent", postToAdd.PostContent ?? (object)DBNull.Value)
            };
            
            if (_dapper.ExecuteSqlWithParameters(sql, parameters))
            {
                _logger.LogInformation("Post created successfully for userId: {UserId}", userId);
                return Ok();
            }

            _logger.LogError("Failed to create new post for userId: {UserId}", userId);
            throw new Exception("Failed to create new post!");
        }

        [HttpPut("Put")]
        public IActionResult EditPost(PostToEditDto postToEdit)
        {
            _logger.LogInformation("EditPost endpoint called for postId: {PostId}, userId: {UserId}", postToEdit.PostId, postToEdit.UserId);
            string sql = @"UPDATE public.""Posts"" SET ""PostContent"" = @PostContent, ""PostTitle"" = @PostTitle, ""PostUpdated"" = NOW() WHERE ""PostId"" = @PostId AND ""UserId"" = @UserId";

            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@PostContent", postToEdit.PostContent ?? (object)DBNull.Value),
                new NpgsqlParameter("@PostTitle", postToEdit.PostTitle ?? (object)DBNull.Value),
                new NpgsqlParameter("@PostId", postToEdit.PostId),
                new NpgsqlParameter("@UserId", postToEdit.UserId)
            };

            if (_dapper.ExecuteSqlWithParameters(sql, parameters))
            {
                _logger.LogInformation("Post edited successfully for postId: {PostId}", postToEdit.PostId);
                return Ok();
            }

            _logger.LogError("Failed to edit post for postId: {PostId}", postToEdit.PostId);
            throw new Exception("Failed to edit post!");
        }

        [HttpDelete("Delete/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            _logger.LogInformation("DeletePost endpoint called for postId: {PostId}", postId);
            string sql = @"DELETE FROM public.""Posts"" WHERE ""PostId"" = @PostId";

            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@PostId", NpgsqlDbType.Integer) { Value = postId }
            };

            if (_dapper.ExecuteSqlWithParameters(sql, parameters))
            {
                _logger.LogInformation("Post deleted successfully for postId: {PostId}", postId);
                return Ok();
            }

            _logger.LogError("Failed to delete post for postId: {PostId}", postId);
            throw new Exception("Failed to delete post!");
        }

        [HttpGet("GetPostsWithUserInfo/{userId}")]
        public async Task<IActionResult> GetPostsWithUserInfo(int userId)
        {
            _logger.LogInformation("GetPostsWithUserInfo endpoint called for userId: {UserId}", userId);
            try
            {
                // Get all posts for the user
                string sql = @"SELECT * FROM public.""Posts"" WHERE ""UserId"" = @UserId";
                var parameters = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@UserId", NpgsqlDbType.Integer) { Value = userId }
                };
                
                IEnumerable<Post> posts = _dapper.LoadData<Post>(sql, parameters);
                _logger.LogDebug("Retrieved {PostCount} posts for userId: {UserId}", posts.Count(), userId);
                
                // Call UserService to get user's first name
                using (HttpClient client = _httpClientFactory.CreateClient())
                {
                    var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    
                    HttpResponseMessage response = await client.GetAsync($"http://localhost:5010/user/GetSingleUser/{userId}");
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                        {
                            string firstName = doc.RootElement.GetProperty("firstName").GetString() ?? "Unknown";
                            _logger.LogDebug("Retrieved user info for userId: {UserId}, firstName: {FirstName}", userId, firstName);
                            
                            return Ok(new 
                            { 
                                firstName = firstName,
                                posts = posts
                            });
                        }
                    }
                    _logger.LogWarning("Failed to get user information for userId: {UserId}, status: {StatusCode}", userId, response.StatusCode);
                    return StatusCode((int)response.StatusCode, "Failed to get user information");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPostsWithUserInfo for userId: {UserId}", userId);
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
}