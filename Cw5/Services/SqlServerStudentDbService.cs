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
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;
using System.Security.Cryptography;

namespace Cw5.Services
{
    public class SqlServerStudentDbService : Controller,IStudentDbService
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
                var tran = con.BeginTransaction();
                com.Connection = con;
                com.Transaction = tran;
                try
                {

                    //1. Czy studia istnieją?
                    com.CommandText = "SELECT IdStudy AS idStudies FROM Studies WHERE Name=@name"; 
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
                    com.CommandText = "SELECT IndexNumber FROM Student WHERE IndexNumber = " + request.IndexNumber; // ' '
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
                    tran.Dispose();
                    return StatusCode((int)HttpStatusCode.Created, esr);
                    
                }
                catch (SqlException exc)
                {                   
                    tran.Rollback();
                    return BadRequest(exc.Message);
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
                    com.CommandText = "SELECT IdStudy FROM Studies WHERE Name = '" + pReq.Studies +"'";                            
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
                    tran.Dispose();
                    return StatusCode((int)HttpStatusCode.Created, promoResp);
                }                    
                catch (SqlException exc)
                {
                    tran.Rollback();
                    return BadRequest(exc.Message);
                }
                
            }           
        }
        public bool CheckCredentials(LoginRequestDto logreq)
        {
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s18803;Integrated Security=True"))
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;


                command.CommandText = "SELECT Salt FROM Student WHERE IndexNumber = @login";
                command.Parameters.AddWithValue("login", logreq.Login);


                var dr = command.ExecuteReader();
                dr.Read();
                var salt = dr["Salt"].ToString();
                dr.Close();

                var hash = CreateHash(logreq.Haslo, salt);

                command.Connection = connection;
                command.CommandText = "SELECT count(1) FROM Student WHERE IndexNumber = @login AND Haslo = @haslo";
                command.Parameters.AddWithValue("login", logreq.Login);
                command.Parameters.AddWithValue("haslo", hash);
                dr = command.ExecuteReader();
                dr.Read();
                int count = (int)dr.GetValue(0);
                dr.Close();

                if (count > 0)
                    return true;

                return false;
            }
        }

        public string CheckRefreshToken(string refreshToken)
        {
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s18803;Integrated Security=True"))
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = "SELECT IndexNumber FROM Student WHERE RefreshToken = @refToken";
                command.Parameters.AddWithValue("refToken", refreshToken);
                var dr = command.ExecuteReader();
                dr.Read();
                string login = dr["IndexNumber"].ToString();
                dr.Close();

                return login;
            }
        }

        public void UpdateRefreshToken(Guid refreshToken, string login)
        {
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s18803;Integrated Security=True"))
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;

                command.CommandText = "UPDATE Student SET RefreshToken = @refToken WHERE IndexNumber = @login";
                command.Parameters.AddWithValue("refToken", refreshToken);
                command.Parameters.AddWithValue("login", login);

                var dr = command.ExecuteNonQuery();
            }
        }

        private string CreateHash(string haslo, string salt)
        {
            var valueBytes = KeyDerivation.Pbkdf2(
                                    password: haslo,
                                    salt: Encoding.UTF8.GetBytes(salt),
                                    prf: KeyDerivationPrf.HMACSHA512,
                                    iterationCount: 10000,
                                    numBytesRequested: 256 / 8);

            return Convert.ToBase64String(valueBytes);
        }

        private string CreateSalt()
        {
            byte[] randomBytes = new byte[128 / 8];
            using (var randomNumber = RandomNumberGenerator.Create())
            {
                randomNumber.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }

    }
}
