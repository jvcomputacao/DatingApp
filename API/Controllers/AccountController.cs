

using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly ITokenService _tokenService;

        private readonly DataContext _context;
        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("register")] // POST: api/acount/register
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto){
            
   
            if(await UserExists(registerDto.Username)){ return BadRequest("username is taken");};

            using var hmac = new HMACSHA512();

            var user = new AppUser{
                Username = registerDto.Username.ToLower(),
                PassowrdHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.password)),
                PassowrdSalt = hmac.Key
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new UserDto{
                Username = user.Username,
                Token = _tokenService.CreatToken(user)
            };
        }

        [HttpPost("login")] // POST: api/acount/login
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto){

            var user = await _context.Users.SingleOrDefaultAsync(x => x.Username == loginDto.Username );
            if (user == null) return Unauthorized("invalid username");

            using var hmac = new HMACSHA512(user.PassowrdSalt);
            var ComputeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.password));
            for (int i = 0; i < ComputeHash.Length; i++){
                if(ComputeHash[i] != user.PassowrdHash[i]) return Unauthorized("invalid password");
            }
            return new UserDto{
                Username = user.Username,
                Token = _tokenService.CreatToken(user)
            };
        }


        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.Username == username.ToLower());
        }
    }
}