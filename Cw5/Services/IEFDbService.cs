using Cw5.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.Services
{
    public interface IEFDbService
    {
        public List<Student> GetStudentList();

        public Student UpdateStudent(Student student);

        public Student DeleteStudent(Student student);

       
    }
}
