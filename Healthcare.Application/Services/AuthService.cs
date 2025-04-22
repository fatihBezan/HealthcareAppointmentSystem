using Healthcare.Application.DTOs;
using Healthcare.Application.Exceptions;
using Healthcare.Application.Interfaces;
using Healthcare.Core.Security;
using Healthcare.Domain.Entities;
using Healthcare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Healthcare.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher _passwordHasher;
        private readonly JwtGenerator _jwtGenerator;

        public AuthService(AppDbContext context, PasswordHasher passwordHasher, JwtGenerator jwtGenerator)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Username already exists"
                };
            }

            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email already exists"
                };
            }

            // Create password hash
            _passwordHasher.CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            // Create user
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            // Get the default User role
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            
            if (userRole == null)
            {
                // Create User role if it doesn't exist
                userRole = new Role { Name = "User", Description = "Regular User" };
                _context.Roles.Add(userRole);
                await _context.SaveChangesAsync();
            }

            // Add user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Link user to role
            var userRoleLink = new UserRole
            {
                UserId = user.Id,
                RoleId = userRole.Id
            };
            
            _context.UserRoles.Add(userRoleLink);

            // Create patient record linked to user
            var patient = new Patient
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                BirthDate = registerDto.BirthDate,
                UserId = user.Id
            };
            
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            // Generate JWT token
            var roles = new List<string> { userRole.Name };
            var token = _jwtGenerator.GenerateToken(user.Id, user.Username, roles);

            // Create response
            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = roles
            };

            return new AuthResponseDto
            {
                Success = true,
                Token = token,
                Message = "Registration successful",
                User = userDto
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginUserDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Username or password is incorrect"
                };
            }

            // Verify password
            if (!_passwordHasher.VerifyPasswordHash(loginDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Username or password is incorrect"
                };
            }

            // Get user roles
            var userRoles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == user.Id)
                .ToListAsync();

            var roles = userRoles.Select(ur => ur.Role.Name).ToList();

            // Generate JWT token
            var token = _jwtGenerator.GenerateToken(user.Id, user.Username, roles);

            // Create response
            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = roles
            };

            return new AuthResponseDto
            {
                Success = true,
                Token = token,
                Message = "Login successful",
                User = userDto
            };
        }

        public async Task<UserDto> GetCurrentUserAsync(int userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new AppException("User not found");
            }

            // Get user roles
            var userRoles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == user.Id)
                .ToListAsync();

            var roles = userRoles.Select(ur => ur.Role.Name).ToList();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = roles
            };
        }

        public async Task<bool> IsInRoleAsync(int userId, string roleName)
        {
            var userRoles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId && ur.Role.Name == roleName)
                .FirstOrDefaultAsync();

            return userRoles != null;
        }
    }
} 