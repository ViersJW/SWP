﻿using SWP.Domain.Models.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWP.UI.Components.AdminBlazorComponents.ViewModels
{
    public class LogRecordViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public DateTime Created { get; set; }
    }
}
