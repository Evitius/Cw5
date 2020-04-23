using System;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cw5.DTOs.Requests;
using Cw5.DTOs.Responses;
using Cw5.Models;
using Cw5.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Cw5.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {

        public IConfiguration Configuration { get; set; }
        public EnrollmentsController(IConfiguration configuration )
        {
            Configuration = configuration;
        }

        private IStudentDbService _service;

        public EnrollmentsController(IStudentDbService service)
        {
            _service = service;

        }

        [HttpPost]
        [Authorize(Roles = "employee")]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {

            var response = _service.EnrollStudent(request);
            return Ok(response);


        }

        [HttpPost("promotions")]
        [Authorize(Roles = "employee")]
        public IActionResult PromoteStudents(PromotionRequest promotionRequest)
        {

            var response = _service.PromoteStudents(promotionRequest);
            return Ok(response);
        }


        [HttpPost]
        public IActionResult Login(LoginRequestDto request) 
        {
            var claims = new[]
           {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "jan123"),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim(ClaimTypes.Role, "student")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken
                (
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
                );



            return Ok(new
            {
                token=new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken=Guid.NewGuid()
            }
                );
        }
    }
    //poprawka
}
