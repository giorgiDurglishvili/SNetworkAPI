using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        public PostController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        [HttpGet("Posts/{postId}/{userId}/{searchParam}")]
        public IEnumerable<Post> GetPosts(int postId = 0, int userId = 0, string searchParam = "None")
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Get";
            string parameter = "";
            DynamicParameters sqlParameters = new DynamicParameters();

            if (postId != 0)
            {
                parameter += ", @PostId = @PostIdParameter";
                sqlParameters.Add("PostIdParameter", postId, DbType.Int32);
            }
            if (userId != 0)
            {
                parameter += ", @UserId = @UserIdParameter";
                sqlParameters.Add("UserIdParameter", userId, DbType.Int32);

            }
            if (searchParam.ToLower() != "none")
            {
                parameter += ", @SearchValue = @SearchValueParameter";
                sqlParameters.Add("SearchValueParameter", userId, DbType.String);
            }

            if(parameter.Length > 0)
            {
                sql += parameter.Substring(1);
            }
            IEnumerable<Post> posts = _dapper.LoadDataWithParameters<Post>(sql, sqlParameters);
            return posts;
        }

        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Get @UserId = @UserParameter";
            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("UserParameter", this.User.FindFirst("userId")?.Value, DbType.Int32);
            return _dapper.LoadDataWithParameters<Post>(sql ,sqlParameters);
        }

        [HttpPut("UpsertPost")]
        public IActionResult UpsertPost(Post postToUpsert)
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Upsert
                @UserId = @UserIdParameter,
                @PostTitle = @PostTitleParameter,
                @PostContent = @PostContentParameter";

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("UserIdParameter", postToUpsert.UserId, DbType.Int32);
            sqlParameters.Add("PostTitleParameter", postToUpsert.PostTitle, DbType.String);
            sqlParameters.Add("PostContentParameter", postToUpsert.PostContent, DbType.String);

            if (postToUpsert.PostId > 0) {
                sql +=  ", @PostId = @PostIdParameter";
                sqlParameters.Add("PostIdParameter", postToUpsert.PostId, DbType.Int32);
            }
            if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
            {
                return Ok();
            }

            throw new Exception("Failed to upsert post!");
            
        }


        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @"EXEC TutorialAppSchema.spPost_Delete @PostId = @PostIdParameter,
                @UserId = @UserIdParameter";

            DynamicParameters sqlParameters = new DynamicParameters();

            sqlParameters.Add("UserIdParameter", this.User.FindFirst("UserId")?.Value, DbType.Int32);
            sqlParameters.Add("PostIdParameter", postId, DbType.Int32);
            
            if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
            {
                return Ok();
            }

            throw new Exception("Failed to delete post!");
        }
    }
}