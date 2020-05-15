using System;
using System.ComponentModel.DataAnnotations;

namespace Cw5.Models
{
    public class Student
    {
     
        public string IndexNumber { get; set; }
    
        public string FirstName { get; set; }
  
        public string LastName { get; set; }
    
        public string BirthDate { get; set; }
     
        public int IdEnrollment { get; set; }

        public virtual Enrollment IdEnrollmentNavigation { get; set; }
       
        //prob + tabtab

    }
}
