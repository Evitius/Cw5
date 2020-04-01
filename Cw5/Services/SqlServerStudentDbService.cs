using Cw5.DTOs.Requests;
using Cw5.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.Services
{
    public class SqlServerStudentDbService : IStudentDbService
    {

        public void EnrollStudent(EnrollStudentRequest request)
        {
          
            // if (request.FirstName == null || request.LastName == null)
           // {
           //     return BadRequest("Zadanie jest niepoprawne");
           // }

            var st = new Student();
            //st.FirstName = request.FirstName;
            //....
            //... 


            using (var con = new SqlConnection(""))
            using (var com = new SqlCommand())
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
                        //return BadRequest("Studia nie istnieją");
                        //....
                    }

                    int idstudies = (int)dr["IdStudies"];

                    //X. Dodanie studenta
                    com.CommandText = "INSERT INTO Student(IndexNumber, FirstName) VALUES (@Index, @Fname)";
                    com.Parameters.AddWithValue("index", request.IndexNumber);
                    //...
                    com.ExecuteNonQuery();
                    tran.Commit();
                }
                catch (SqlException exc)
                {
                    tran.Rollback();
                }


            }
        }

        public void PromoteStudents(int semestr, string studies)
        {
            throw new NotImplementedException();
        }
    }
}
