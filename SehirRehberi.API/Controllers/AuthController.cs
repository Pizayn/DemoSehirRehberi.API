using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SehirRehberi.API.Data;
using SehirRehberi.API.Dtos;
using SehirRehberi.API.Models;

namespace SehirRehberi.API.Controllers
{
    [Produces("application/json")]
    [Route("api/Auth")]
    public class AuthController : Controller
    {
        private IAuthRepository _authRepository;
        private IConfiguration _configuration;//microsoft dan aldık
        //appsettingsjson'a yazdığımız keyi konfigurasyondan okumak için kullanırız

        public AuthController(IAuthRepository authRepository, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]UserForRegisterDto userForRegisterDto)
        {
            // asnyc ile işaretlenmiş bir metodda asenkron çalışacak komutlar await ile işaretlenir.
            //async ile işaretlenmiş metodun geri dönüş tipi; void, Task veyahut Task<T> geri dönüş tiplerinde olmalıdır.
            //await; sadece async ile işaretlenmiş metodlarda kullanılabilir.
            //async ile işaretlenmiş bir metod birden fazla await kullanabilir.
            if (await _authRepository.UserExists(userForRegisterDto.UserName))
            {
                ModelState.AddModelError("UserName","Username already exists");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userToCreate = new User
            {
                UserName = userForRegisterDto.UserName
            };

            var createdUser = await _authRepository.Register(userToCreate, userForRegisterDto.Password);
            return StatusCode(201);//create edildi geri dönüş değeri
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] UserForLoginDto userForLoginDto)
        {
            var user = await _authRepository.Login(userForLoginDto.UserName, userForLoginDto.Password);

            if (user==null)
            {
                return Unauthorized();
            }
            // token işlemini yapar
            var tokenHandler = new JwtSecurityTokenHandler();
            //yazdığımız keye ulaştık
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("AppSettings:Token").Value);
            //bu tokenın neyi tutacağını belirtiriz
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                //totende tutacağımız temel bilgileri tutarız
                Subject = new ClaimsIdentity(new Claim[]
                {
                    //claim based authentication
                    //tüm backende kullanabiliriz
                    //user id sini tuttuk
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    //username tuttuk
                    new Claim(ClaimTypes.Name, user.UserName)
                }),
                //bu tokenın ne kadar geçerli olduğunu belirttik
                Expires = DateTime.Now.AddDays(1),
                //hangi key i ve hangi algoritmayı kullandığımızı belirttik
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key)
                    , SecurityAlgorithms.HmacSha512Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(tokenString);
        }


    }
}