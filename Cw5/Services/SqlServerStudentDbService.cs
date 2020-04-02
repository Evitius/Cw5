using Cw5.DTOs.Requests;
using Cw5.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Cw5.DTOs.Responses;

namespace Cw5.Services
{
    public class SqlServerStudentDbService : Controller, IStudentDbService
    {
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {

            EnrollStudentResponse esr = new EnrollStudentResponse() { };
            
            // if (request.FirstName == null || request.LastName == null)
            // {
            //     return BadRequest("Zadanie jest niepoprawne");
            // }
            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s18803;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                con.Open();
                com.Connection = con;
                var tran = con.BeginTransaction();
                com.Transaction = tran;
                try
                {

                    //1. Czy studia istnieją?
                    com.CommandText = "SELECT IdStudy AS idStudies FROM Studies WHERE Name='@name'";
                    com.Parameters.AddWithValue("name", request.Studies);                   
                    var dr = com.ExecuteReader();
                    if (!dr.Read())
                    {
                        dr.Close();
                        tran.Rollback();
                        return NotFound("Nie ma takich studiów");

                    }

                    int idStudies = (int)dr["idStudies"];
                    dr.Close();

                    //2. Sprawdzenie czy nie występuje konflikt indeksów                  
                    com.CommandText = "SELECT IndexNumber FROM Student WHERE IndexNumber = '" + request.IndexNumber + "'";
                    dr = com.ExecuteReader();
                    if (dr.Read())
                    {
                        dr.Close();
                        tran.Rollback();
                        return BadRequest("Student z takim indeksem już istnieje");
                    }
                    dr.Close();

                    //3. Nadanie IdEnrollment
                    int idEnrollment;
                    com.CommandText = "SELECT IdEnrollment FROM Enrollment JOIN Studies ON " +
                        "Enrollment.IdStudy = Studies.IdStudy WHERE Semester = 1 and IdStudy = " + idStudies;
                    dr = com.ExecuteReader();
                    if (dr.Read())
                    {
                        dr.Close();
                        com.CommandText = "SELECT MAX(IdEnrollment)+1 AS idEnroll from Enrollment";
                        dr = com.ExecuteReader();
                        idEnrollment = (int)dr["idEnroll"];
              
                    }
                    else
                    {
                        idEnrollment = 1;
                        dr.Close();                      
                    }

                    //4. Wstawienie Enrollment                 
                    com.CommandText = "INSERT INTO Enrollment(IdEnrollment,Semester,IdStudy,StartDate)" +
                        "  VALUES(" + idEnrollment + ", 1, " + idStudies + ",GetDate())";
                    com.ExecuteNonQuery();

                 

                    //5. Wstawienie studenta
                    com.CommandText = "INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) " +
                        "VALUES (" + request.IndexNumber + ", " + request.FirstName + ", " + request.LastName + ", " +
                        request.BirthDate + ", " + idEnrollment + ") ";   
                    com.ExecuteNonQuery();
                    esr.IdEnrollment = idEnrollment;
                    esr.IdStudy = idStudies;
                    esr.Semester = 1;
                    esr.StartDate = DateTime.Now;
                    tran.Commit();
                    return StatusCode((int)HttpStatusCode.Created, esr);
                }
                catch (SqlException exc)
                {                   
                    tran.Rollback();
                    throw exc;
                }
            }     
        }

        public IActionResult PromoteStudents(PromotionRequest pReq)
        {
            PromotionResponse promoResp = new PromotionResponse() { };

            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s18803;Integrated Security=True"))
            using (var com = new SqlCommand())
            {         
                con.Open();
                var tran= con.BeginTransaction();
                com.Connection = con;
                
                try
                {
                    com.Transaction = tran;
                    com.CommandText = "SELECT IdStudy FROM Studies WHERE Name = " + pReq.Studies +"";                            
                    var dr = com.ExecuteReader();
                        
                    if (!dr.Read())
                    {                           
                        dr.Close();                  
                        tran.Rollback();
                        return BadRequest("Studia nie istnieją");                       
                    }
                        
                    int idStudies = (int)dr["IdStudy"];                        
                    dr.Close();                        
                                           
                    com.CommandText = "SELECT * FROM Enrollment WHERE IdStudy = @idStudies AND Semester = @semester";   
                    com.Parameters.AddWithValue("idStudies", idStudies);                  
                    com.Parameters.AddWithValue("semester", pReq.Semester);
                        
                    dr = com.ExecuteReader();
                        
                    if (!dr.Read())                     
                    {
                        dr.Close();
                        tran.Rollback();
                        return NotFound("Nie znaleziono");                       
                    }

                    int semester = pReq.Semester + 1;
                    dr.Close();
                    com.Parameters.Clear();
                    com.CommandType = CommandType.StoredProcedure;     
                    com.CommandText = "PromoteStudents";                                
                    com.Parameters.AddWithValue("Studies", idStudies);                   
                    com.Parameters.AddWithValue("Semester", semester);      
                    com.ExecuteNonQuery();
                                              
                    com.CommandType = CommandType.Text;                       
                    com.CommandText = "SELECT IdEnrollment, StartDate FROM Enrollment WHERE IdStudy = @idStudies AND Semester = @semester";    
                    com.Parameters.AddWithValue("idStudies", idStudies);                 
                    com.Parameters.AddWithValue("semester", semester);
                        
                    dr = com.ExecuteReader();
                        
                    if (!dr.Read())                       
                    {                            
                        dr.Close();                          
                        tran.Rollback();                           
                        return BadRequest("Błąd");
                    }       

                    promoResp.IdEnrollment = (int)dr["IdEnrollment"];
                    promoResp.StartDate = (DateTime)dr["StartDate"];
                    promoResp.Semester = semester;
                    promoResp.IdStudy = idStudies;
                    dr.Close();
                    tran.Commit();
                    return StatusCode((int)HttpStatusCode.Created, promoResp);
                }                    
                catch (SqlException ex)
                {
                    tran.Rollback();
                    throw ex;                   
                }
                
            }
            
        }
        
    }

      
   
}
