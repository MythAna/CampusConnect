using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

/// <summary>
/// Handles user profiles, photos, and filtering
/// </summary>
[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;

    public UsersController(IUnitOfWork uow, IMapper mapper, IPhotoService photoService)
    {
        _photoService = photoService;
        _uow = uow;
        _mapper = mapper;
    }

    /// <summary>
    /// Gets paginated list of users with filtering options
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
    {
        // Set default gender filter to opposite of current user
        var currentUser = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        userParams.CurrentUsername = currentUser.UserName;

        if (string.IsNullOrEmpty(userParams.Gender))
            userParams.Gender = currentUser.Gender == "male" ? "female" : "male";

        var users = await _uow.UserRepository.GetMembersAsync(userParams);

        Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize,
            users.TotalCount, users.TotalPages));

        return Ok(users);
    }

    /// <summary>
    /// Gets detailed profile for a specific user
    /// </summary>
    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        return await _uow.UserRepository.GetMemberAsync(username);
    }

    /// <summary>
    /// Updates user profile information
    /// </summary>
    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        _mapper.Map(memberUpdateDto, user);

        _uow.UserRepository.Update(user);

        if (await _uow.Complete()) return NoContent();

        return BadRequest("Failed to update user");
    }

    /// <summary>
    /// Uploads a new profile photo
    /// </summary>
    /// <remarks>
    /// First uploaded photo automatically becomes main photo
    /// </remarks>
    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        var result = await _photoService.AddPhotoAsync(file);

        if (result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        if (user.Photos.Count == 0) photo.IsMain = true;

        user.Photos.Add(photo);

        if (await _uow.Complete())
            return CreatedAtAction(nameof(GetUser), new { username = user.UserName },
                _mapper.Map<PhotoDto>(photo));

        return BadRequest("Problem adding photo");
    }

    /// <summary>
    /// Sets a photo as the main profile photo
    /// </summary>
    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if (photo == null) return NotFound();

        if (photo.IsMain) return BadRequest("This is already your main photo");

        // Swap main photo status
        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
        if (currentMain != null) currentMain.IsMain = false;
        photo.IsMain = true;

        if (await _uow.Complete()) return NoContent();

        return BadRequest("Problem setting main photo");
    }

    /// <summary>
    /// Deletes a profile photo
    /// </summary>
    /// <remarks>
    /// Cannot delete main photo. Removes from cloud storage if exists.
    /// </remarks>
    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if (photo == null) return NotFound();

        if (photo.IsMain) return BadRequest("You cannot delete your main photo");

        // Remove from cloud storage if applicable
        if (photo.PublicId != null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);

        if (await _uow.Complete()) return Ok();

        return BadRequest("Problem deleting photo");
    }
}  
