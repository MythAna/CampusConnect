using API.Controllers;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API;

/// <summary>
/// Handles user likes functionality (like/unlike and fetching liked users).
/// </summary>
[Authorize]
public class LikesController : BaseApiController
{
    private readonly IUnitOfWork _uow;

    public LikesController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    /// <summary>
    /// Like another user.
    /// </summary>
    /// <param name="username">Target username to like</param>
    [HttpPost("{username}")]
    public async Task<ActionResult> AddLike(string username)
    {
        var sourceUserId = User.GetUserId();
        var likedUser = await _uow.UserRepository.GetUserByUsernameAsync(username);
        var sourceUser = await _uow.LikesRepository.GetUserWithLikes(sourceUserId);

        if (likedUser == null) return NotFound();

        // Prevent self-likes
        if (sourceUser.UserName == username) return BadRequest("You cannot like yourself");

        // Check for existing like
        var userLike = await _uow.LikesRepository.GetUserLike(sourceUserId, likedUser.Id);

        if (userLike != null) return BadRequest("You already like this user");

        userLike = new UserLike
        {
            SourceUserId = sourceUserId,
            TargetUserId = likedUser.Id
        };
            
        // Create and save new like
        sourceUser.LikedUser.Add(userLike);

        if (await _uow.Complete()) return Ok();

        return BadRequest("Failed to like user");
    }

    
    /// <summary>
    /// Get paginated list of users liked by or liking the current user.
    /// </summary>
    /// <param name="likesParams">Pagination and filter parameters</param>
    [HttpGet]
    public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
    {
        likesParams.UserId = User.GetUserId();

        var users = await _uow.LikesRepository.GetUserLikes(likesParams);

        Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, 
            users.TotalCount, users.TotalPages));

        return Ok(users);
    }
}
