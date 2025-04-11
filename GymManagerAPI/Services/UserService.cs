using AutoMapper;
using GymManagerAPI.Data.Common;
using GymManagerAPI.Data.Context;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Interfaces;
using GymManagerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagerAPI.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly JwtService jwtService;
        private readonly IMapper mapper;

        public UserService(ApplicationDbContext applicationDbContext, JwtService jwtService, IMapper mapper)
        {
            this.applicationDbContext = applicationDbContext;
            this.jwtService = jwtService;
            this.mapper = mapper;
        }

        public async Task<OperationResult<UserDTO>> AssignRolesToUserAsync(int userId, int[] roleIds)
        {
            //verify if user exists
            var user = await applicationDbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

            if(user == null)
            {
                return OperationResult<UserDTO>.Fail(404, "Invalid user id");
            }

            //verify if there are duplicated values in roleIds
            if(ThereAreDuplicatedElements(roleIds))
            {
                return OperationResult<UserDTO>.Fail(400, "Invalid role ids");
            }

            //verify if role exists
            if(await VerifyValidRolesAsync(roleIds))
            {
                return OperationResult<UserDTO>.Fail(400, "Invalid role ids");
            }

            //verify if user already has that role
            var userHasRole = await applicationDbContext.UserRoles.AsNoTracking().Where(ur => ur.UserId == userId &&  roleIds.Contains(ur.RoleId)).AnyAsync();

            if(userHasRole)
            {
                return OperationResult<UserDTO>.Fail(400, "Invalid role for the user");
            }

            var userRoles = new List<UserRole>();

            foreach (var item in roleIds)
            {
                userRoles.Add(new UserRole()
                {
                    UserId = userId,
                    RoleId = item
                });
            }

            await applicationDbContext.UserRoles.AddRangeAsync(userRoles);
            await applicationDbContext.SaveChangesAsync();

            return OperationResult<UserDTO>.Ok();
        }

        public async Task<OperationResult<UserDTO>> CreateUserAsync(UserCreateDTO userCreateDTO)
        {
            //Check if the username already exists
            if (await ValidateUserNameAsync(userCreateDTO.UserName))
            {
                return OperationResult<UserDTO>.Fail(400, "Invalid username");
            }

            // Create password hash and salt
            PasswordHasher.CreatePasswordHash(userCreateDTO.Password, out byte[] passwordHash, out byte[] passwordSalt);
 
            // Create user entity
            var user = new User()
            {
                FullName = userCreateDTO.FullName,
                UserName = userCreateDTO.UserName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            // Add user to the database
            await applicationDbContext.Users.AddAsync(user);
            await applicationDbContext.SaveChangesAsync();

            //asign user role to the new user
            var userRole = new UserRole()
            {
                UserId = user.Id,
                RoleId = 1
            };

            await applicationDbContext.UserRoles.AddAsync(userRole);
            await applicationDbContext.SaveChangesAsync();

            //mapping to response
            var userDTO = mapper.Map<UserDTO>(user);

            return OperationResult<UserDTO>.Ok(statusCode: 201, data: userDTO);
        }

        public async Task<OperationResult<UserDTO>> SoftDeleteUserAsync(int id)
        {
            var user = await applicationDbContext.Users.FindAsync(id);

            if (user == null)
            {
                return OperationResult<UserDTO>.Fail(404, "Invalid user id");
            }

            if(user.IsActive == false)
            {
                return OperationResult<UserDTO>.Fail(400, "Invalid user");
            }

            //setting IsActive as false
            user.IsActive = false;

            //revoking user refresh tokens
            await applicationDbContext.RefreshTokens
                .Where(x => x.UserId == id)
                .ExecuteUpdateAsync(x => x.SetProperty(prop => prop.IsRevoked, true));

            applicationDbContext.Users.Update(user);
            await applicationDbContext.SaveChangesAsync();

            return OperationResult<UserDTO>.Ok(statusCode: 204);
        }

        public async Task<OperationResult<UserDTO>> DeleteUserRolesAsync(int userId, int[] roleIds)
        {
            var userExists = await applicationDbContext.Users.AnyAsync(u => u.Id == userId);

            if (!userExists)
            {
                return OperationResult<UserDTO>.Fail(404, "Invalid user id");
            }

            //verify if there are duplicated elements
            if (ThereAreDuplicatedElements(roleIds))
            {
                return OperationResult<UserDTO>.Fail(400, "Invalid user ids");
            }

            //verify if there are valids
            if (await VerifyValidRolesAsync(roleIds))
            {
                return OperationResult<UserDTO>.Fail(400, "Invalid user ids");
            }

            //verify if the user has the roles who gonna be eliminated
            var countUserRoles = await applicationDbContext.UserRoles.Where(ur => ur.UserId == userId && roleIds.Contains(ur.RoleId)).CountAsync();

            if (countUserRoles != roleIds.Length)
            {
                return OperationResult<UserDTO>.Fail(400, "Invalid user ids");
            }

            await applicationDbContext.UserRoles.Where(ur => ur.UserId == userId && roleIds.Contains(ur.RoleId)).ExecuteDeleteAsync();
            await applicationDbContext.SaveChangesAsync();

            return OperationResult<UserDTO>.Ok();
        }

        public async Task<OperationResult<IEnumerable<Role>>> GetRolesAsync()
        {
            var roles = await applicationDbContext.Roles.AsNoTracking().ToListAsync();

            return OperationResult<IEnumerable<Role>>.Ok(data: roles);
        }

        public async Task<OperationResult<UserDTO>> GetUserByIdAsync(int id)
        {
            var user = await applicationDbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return OperationResult<UserDTO>.Fail(404, "Invalid user id");
            }

            var userDTO = mapper.Map<UserDTO>(user);

            return OperationResult<UserDTO>.Ok(data: userDTO);
        }

        public async Task<OperationResult<IEnumerable<Role>>> GetUserRolesAsync(int userId)
        {
            var result = await GetUserByIdAsync(userId);

            if (!result.IsSuccess)
            {
                return OperationResult<IEnumerable<Role>>.Fail(404, "Invalid user id");
            }

            var roles = await applicationDbContext.UserRoles
                .AsNoTracking()
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role)
                .ToListAsync();

            return OperationResult<IEnumerable<Role>>.Ok(data: roles);
        }

        public async Task<OperationResult<IEnumerable<UserDTO>>> GetUsersAsync()
        {
            var users = await applicationDbContext.Users.AsNoTracking().ToListAsync();

            var usersDTO = mapper.Map<IEnumerable<UserDTO>>(users);

            return OperationResult<IEnumerable<UserDTO>>.Ok(data: usersDTO);
        }

        public async Task<OperationResult<UserDTO>> UpdateUserAsync(int userId, UserUpdateDTO userUpdateDTO)
        {
            var user = await applicationDbContext.Users.FindAsync(userId);

            if (user == null)
            {
                return OperationResult<UserDTO>.Fail(400, "Invalid user id");
            }

            // verify if properties have been changed
            if (!string.IsNullOrEmpty(userUpdateDTO.FullName) && !string.Equals(user.FullName, userUpdateDTO.FullName, StringComparison.Ordinal))
            {
                //update fullname prop
                user.FullName = userUpdateDTO.FullName;
            }

            if (!string.IsNullOrEmpty(userUpdateDTO.UserName) && !string.Equals(user.UserName, userUpdateDTO.UserName, StringComparison.Ordinal))
            {
                //verify if the username doesn't exists
                if (await ValidateUserNameAsync(userUpdateDTO.UserName))
                {
                    return OperationResult<UserDTO>.Fail(400, "Username is already in use.");
                }

                //update username prop
                user.UserName = userUpdateDTO.UserName;

            }

            if (!string.IsNullOrEmpty(userUpdateDTO.Password) && !PasswordHasher.VerifyPasswordHash(userUpdateDTO.Password, user.PasswordHash, user.PasswordSalt))
            {
                //create new password and salt
                PasswordHasher.CreatePasswordHash(userUpdateDTO.Password, out byte[] passwordHash, out byte[] passwordSalt);

                //and then update
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            applicationDbContext.Update(user);
            await applicationDbContext.SaveChangesAsync();

            return OperationResult<UserDTO>.Ok();
        }

        public async Task<OperationResult<Models.User>> ValidateUserAsync(UserLoginDTO userLoginDTO)
        {
            // Verify the user by username
            var user = await applicationDbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserName == userLoginDTO.UserName);

            if (user == null)
            {
                return OperationResult<User>.Fail(401, "Invalid username or password.");
            }

            // Verify the password
            if (!PasswordHasher.VerifyPasswordHash(userLoginDTO.Password, user.PasswordHash, user.PasswordSalt))
            {
                return OperationResult<User>.Fail(401, "Invalid username or password.");
            }


            return OperationResult<User>.Ok(data: user);
        }

        //helpers
        public async Task<bool> ValidateUserNameAsync(string name)
        {
            return await applicationDbContext.Users.AnyAsync(u => u.UserName == name);
        }

        public bool ThereAreDuplicatedElements(int[] list)
        {
            return list.Length != list.Distinct().Count();
        }

        public async Task<bool> VerifyValidRolesAsync(int[] list)
        {
            var existingRoles = await applicationDbContext.Roles.AsNoTracking().CountAsync(r => list.Contains(r.Id));

            return existingRoles != list.Length;
        }
    }
}