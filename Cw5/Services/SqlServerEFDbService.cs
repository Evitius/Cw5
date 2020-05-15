using Cw5.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.Services
{
    public class SqlServerEFDbService : IEFDbService
    {
        private readonly s18803Context context;

        public SqlServerEFDbService(s18803Context context)
        {
            this.context = context;
        }

        public List<Student> GetStudentList()
        {
            return context.Student.ToList();
        }

        public Student DeleteStudent(Student student)
        {
            var s = context.Student.FirstOrDefault(s => s.IndexNumber == student.IndexNumber);

            if (s == null)
            {
                return null;
            }

            context.Attach(s);
            context.Remove(s);
            context.SaveChanges();
            return s;

        }



        public Student UpdateStudent(Student student)
        {
            try
            {
                context.Attach(student);
                context.Entry(student).State = EntityState.Modified;
                context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return null;
            }
            catch (Exception)
            {
               throw;
            }
            return student;
        }
    }
    }
}
