using System;
using System.Data.SqlClient;
using Cw5.DTOs.Requests;
using Cw5.DTOs.Responses;
using Cw5.Models;
using Microsoft.AspNetCore.Mvc;

namespace Cw5.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        [HttpPost]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {

            if (request.FirstName == null || request.LastName == null)
            {
                return BadRequest("Zadanie jest niepoprawne");
            }
         
            var st = new Student();
            st.FirstName = request.FirstName;
            //....
            //...

            
            using(var con = new SqlConnection(""))
            using(var com= new SqlCommand())
            {
               
                    com.Connection = con;
                    con.Open();
                    var tran = con.BeginTransaction();
                try
                {
                    //1. Czy studia istnieją?
                    com.CommandText = "select IdStudies from studies WHERE name=@name";
                    com.Parameters.AddWithValue("name", request.Studies);

                    var dr = com.ExecuteReader();
                    if (!dr.Read())
                    {
                        tran.Rollback();
                        return BadRequest("Studia nie istnieją");
                        //....
                    }

                    int idstudies = (int)dr["IdStudies"];

                    //X. Dodanie studenta
                    com.CommandText = "INSERT INTO Student(IndexNumber, FirstName) VALUES (@Index, @Fname)";
                    com.Parameters.AddWithValue("index", request.IndexNumber);
                    //...
                    com.ExecuteNonQuery();
                    tran.Commit();
                }catch(SqlException exc)
                {
                    tran.Rollback();
                }


            }

            var response = new EnrollStudentResponse();
            response.LastName = st.LastName;


            return Ok(response);

        }
    }
}
