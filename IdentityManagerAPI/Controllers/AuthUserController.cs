using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Auth;
using Models;
using System.Net;
using DataAcess.Repos.IRepos;

namespace IdentityManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthUserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public AuthUserController(IUserRepository userRepository)
        {
            this._userRepository = userRepository;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            var entity = await _userRepository.Login(loginRequestDTO);

            return Ok(entity);

        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO registerRequestDTO)
        {
            var entity = await _userRepository.Register(registerRequestDTO);
            return Ok(entity);
        }


    }
}
