using GymManagerAPI.Data.Common;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Models;

namespace GymManagerAPI.Interfaces
{
    public interface IUserService
    {
        // User CRUD Operations
        Task<OperationResult<IEnumerable<UserDTO>>> GetUsersAsync();
        Task<OperationResult<UserDTO>> GetUserByIdAsync(int id);
        Task<OperationResult<UserDTO>> CreateUserAsync(UserCreateDTO userCreateDTO);
        Task<OperationResult<UserDTO>> UpdateUserAsync(int userId, UserUpdateDTO userUpdateDTO);
        Task<OperationResult<UserDTO>> SoftDeleteUserAsync(int id);
        Task<OperationResult<UserDTO>> DeleteUserRolesAsync(int userId, int[] roleIds);
        Task<OperationResult<Models.User>> ValidateUserAsync(UserLoginDTO userLoginDTO);

        // Role Management
        Task<OperationResult<IEnumerable<Role>>> GetRolesAsync();
        Task<OperationResult<UserDTO>> AssignRolesToUserAsync(int userId, int[] roleId);
        Task<OperationResult<IEnumerable<Role>>> GetUserRolesAsync(int userId);
    }
}
