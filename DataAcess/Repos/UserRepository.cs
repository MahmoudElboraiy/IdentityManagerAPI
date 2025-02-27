﻿using DataAcess.Repos.IRepos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Models.DTOs.Auth;
using Models.DTOs.User;
using Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;


namespace DataAcess.Repos
{
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        private readonly ApplicationDbContext db;
        private readonly IMapper mapper;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration configuration;
        private string securityKey;

        public UserRepository(ApplicationDbContext db, IConfiguration configuration, UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager) : base(db)
        {
            this.db = db;
            this.configuration = configuration;
            this.userManager = userManager;
            this.mapper = mapper;
            this.roleManager = roleManager;
            //Just install `Microsoft.Extensions.Configuration.Binder` and the method `GetValue` will be available
            securityKey = configuration.GetValue<string>("ApiSettings:Secret") ?? throw new InvalidOperationException("ApiSettings:Secret is not configured.");
        }

        public async Task<bool> IsUniqueUserName(string username)
        {
            var matchUsername = await userManager.FindByNameAsync(username);
            return matchUsername == null;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = await userManager.FindByNameAsync(loginRequestDTO.UserName);
            if (user == null || !await userManager.CheckPasswordAsync(user, loginRequestDTO.Password))
            {
                return new LoginResponseDTO()
                {
                    Token = "",
                    User = null,
                };
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),

            };

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(securityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds);

            return new LoginResponseDTO()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                User = mapper.Map<UserDTO>(user),
            };
        }

        public async Task<UserDTO> Register(RegisterRequestDTO registerRequestDTO)
        {
            var user = new ApplicationUser
            {
                UserName = registerRequestDTO.UserName,
                Name = registerRequestDTO.Name,
                Email = registerRequestDTO.Email,
                NormalizedEmail = registerRequestDTO.Email.ToUpper()
            };

            var userDTO = new UserDTO();

            try
            {
                var result = await userManager.CreateAsync(user, registerRequestDTO.Password);
                if (result.Succeeded)
                {
                    userDTO = mapper.Map<UserDTO>(user);
                }
                else
                {
                    userDTO.ErrorMessages = result.Errors.Select(e => e.Description).ToList();
                }
            }
            catch (Exception)
            {
                userDTO.ErrorMessages = new List<string> { "An unexpected error occurred while registering the user." };
            }

            return userDTO;
        }
    }
}
