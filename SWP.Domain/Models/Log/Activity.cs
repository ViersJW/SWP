﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SWP.Domain.Models.Log
{
    public class Activity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string UserName { get; set; }
        [MaxLength(100)]
        public string Message { get; set; }
        [MaxLength(50)]
        public string Action { get; set; }
        [Required]
        public DateTime TimeStamp { get; set; }
    }
}
