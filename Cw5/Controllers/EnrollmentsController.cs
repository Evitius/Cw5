using System;
using System.Data.SqlClient;
using Cw5.DTOs.Requests;
using Cw5.DTOs.Responses;
using Cw5.Models;
using Cw5.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cw5.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {


        private IStudentDbService _service;

        public EnrollmentsController(IStudentDbService service)
        {
            _service = service;

        }

        [HttpPost]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {

            var response = _service.EnrollStudent(request);
            return Ok(response);
     

        }
    
        [HttpPost("promotions")]
        public IActionResult PromoteStudents(PromotionRequest promotionRequest)
        {
            
            var response = _service.PromoteStudents(promotionRequest);
            return Ok(response);
        }
    }





}