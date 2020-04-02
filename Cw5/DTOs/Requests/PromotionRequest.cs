using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.DTOs.Requests
{
    public class PromotionRequest
    {
        [Required(ErrorMessage = "Musisz podać kierunek studiów")]
        public string Studies { get; set; }
        [Required(ErrorMessage = "Musisz podać semester")]
        public int Semester { get; set; }
    }
}
