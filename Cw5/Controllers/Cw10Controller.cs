using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cw5.Models;
using Cw5.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cw5.Controllers
{
    
    [ApiController]
    [Route("api/EF")]
    public class Cw10Controller : ControllerBase
    {

        private SqlServerEFDbService service;

        public Cw10Controller(SqlServerEFDbService service)
        {
            this.service = service;
        }

        [HttpGet]
        [Route("get")]
        public IActionResult GetStudentList()
        {
            var result = service.GetStudentList();
            return Ok(result);
        }

        [HttpPost]
        [Route("update")]
        public IActionResult UpdateStudent(Student student)
        {
            var result = service.UpdateStudent(student);
            return Ok(result);
        }

       
        [HttpDelete]
        [Route("delete")]
        public IActionResult DeleteStudent(Student student)
        {
            var result = service.DeleteStudent(student);

            if (result == null)
            {
                return BadRequest("Nie ma takiego studenta");
            }

            return Ok("Student usunięty");

        }
    }
}