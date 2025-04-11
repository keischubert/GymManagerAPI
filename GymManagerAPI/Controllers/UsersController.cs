using GymManagerAPI.Data.Context;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Interfaces;
using GymManagerAPI.Models;
using GymManagerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly JwtService jwtService;
        private readonly IUserService userService;

        public UsersController(JwtService jwtService, IUserService userService)
        {
            this.jwtService = jwtService;
            this.userService = userService;
        }

        //POST: api/users/{userId}/roles
        [Authorize(Policy = "DeveloperPolicy")]
        [HttpPost("{userId:int}/roles")]
        public async Task<ActionResult> AssignRoleToUser([FromRoute] int userId, [FromBody] int[] roleId)
        {
            var result = await userService.AssignRolesToUserAsync(userId, roleId);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return Ok();
        }

        //DELETE: api/users/{userId}/roles
        [Authorize(Policy = "DeveloperPolicy")]
        [HttpDelete("{userId:int}")]
        public async Task<ActionResult> DeleteUser([FromRoute] int userId)
        {
            var result = await userService.SoftDeleteUserAsync(userId);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return NoContent();
        }

        //DELETE: api/users/{userId}/roles
        [Authorize(Policy = "DeveloperPolicy")]
        [HttpDelete("{userId:int}/roles")]
        public async Task<ActionResult> DeleteUserRoles([FromRoute] int userId, [FromBody] int[] roleIds)
        {
            var result = await userService.DeleteUserRolesAsync(userId, roleIds);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return NoContent();
        }

        //GET: api/users
        [Authorize(Policy = "AdminPolicy")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var result = await userService.GetUsersAsync();

            return Ok(result.Data);
        }

        //GET: api/users/{userId}
        [Authorize(Policy = "AdminPolicy")]
        [HttpGet("{userId:int}")]
        public async Task<ActionResult<UserDTO>> GetUserById([FromRoute] int userId)
        {
            var result = await userService.GetUserByIdAsync(userId);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }

        //GET: api/users/{userId}/roles
        [Authorize(Policy = "DeveloperPolicy")]
        [HttpGet("{userId:int}/roles")]
        public async Task<ActionResult<IEnumerable<Role>>> GetUserRoles(int userId)
        {
            var result = await userService.GetUserRolesAsync(userId);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }

        // POST: api/users/register
        [Authorize(Policy = "DeveloperPolicy")]
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] UserCreateDTO userCreateDTO)
        {
            var result = await userService.CreateUserAsync(userCreateDTO);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return CreatedAtAction("GetUserById", new { userId = result.Data.Id }, result.Data);
        }

        //PUT: api/users/{userId}
        [Authorize(Policy = "AdminPolicy")]
        [HttpPut("{userId:int}")]
        public async Task<ActionResult> UpdateUser([FromRoute] int userId, [FromBody] UserUpdateDTO userUpdateDTO)
        {
            var result = await userService.UpdateUserAsync(userId, userUpdateDTO);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, result.Message);
            }

            return NoContent();
        }
    }
}