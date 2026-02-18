using Microsoft.AspNetCore.Mvc;
using PostService.Models;
using PostService.Data;
using PostService.Dtos;
using Microsoft.AspNetCore.Authorization;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;
using System.Diagnostics;

namespace PostService.Controllers;
[Authorize]
[ApiController]
[Route("[controller]")]
public class PostController : ControllerBase
{
    IDataContextDapper _dapper;
    IHttpClientFactory _httpClientFactory;
    
    public PostController(IDataContextDapper dapper, IHttpClientFactory httpClientFactory)
    {
        _dapper = dapper;
        _httpClientFactory = httpClientFactory;
    }


    [HttpGet("GetPosts")]
    public IEnumerable<Post> GetPosts()
    {
        string sql ="SELECT * FROM public.\"Posts\"";
        IEnumerable<Post> posts = _dapper.LoadData<Post>(sql);
        return posts;
    }

        [HttpGet("GetSinglePost/{postId}")]
  
    public Post GetSinglePost(int postId)
    {

         string sql ="SELECT * FROM public.\"Posts\" WHERE \"PostId\"= " + postId.ToString();
        Post post = _dapper.LoadDataSingle<Post>(sql);
        return post;
    }

    [HttpPost("Post")]
        public IActionResult AddPost(PostToAddDto postToAdd)
        {
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
                return Ok();
            }

            throw new Exception("Failed to create new post!");
        }

        [HttpPut("Put")]
        public IActionResult EditPost(PostToEditDto postToEdit)
        {
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
                return Ok();
            }

            throw new Exception("Failed to edit post!");
        }

        [HttpDelete("Delete/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @"DELETE FROM public.""Posts"" WHERE ""PostId"" = @PostId";

            var parameters = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@PostId", NpgsqlDbType.Integer) { Value = postId }
            };

            if (_dapper.ExecuteSqlWithParameters(sql, parameters))
            {
                return Ok();
            }

            throw new Exception("Failed to delete post!");
        }

        [HttpGet("GetPostsWithUserInfo/{userId}")]
        public async Task<IActionResult> GetPostsWithUserInfo(int userId)
        {
            try
            {
                // Get all posts for the user
                string sql = @"SELECT * FROM public.""Posts"" WHERE ""UserId"" = @UserId";
                var parameters = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@UserId", NpgsqlDbType.Integer) { Value = userId }
                };
                
                IEnumerable<Post> posts = _dapper.LoadData<Post>(sql, parameters);
                
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
                            
                            return Ok(new 
                            { 
                                firstName = firstName,
                                posts = posts
                            });
                        }
                    }
                    return StatusCode((int)response.StatusCode, "Failed to get user information");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
}