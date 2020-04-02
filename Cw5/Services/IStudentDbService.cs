﻿using Cw5.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.Services
{
    public interface IStudentDbService
    {
        public IActionResult EnrollStudent(EnrollStudentRequest request);
        public IActionResult PromoteStudents(PromotionRequest promotionRequest);
    }
}