﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MacAuth.DbModels
{
    public class AuthRequest
    {
        [Key]
        public int Id { get; set; }
        public string DeviceCode { get; set; }
        public string UserCode { get; set; }
        public string Provider { get; set; }
        public string ClientId { get; set; }
        public string Status { get; set; }
        public string Code { get; set; }
        public string Error { get; set; }
        public DateTime Expires { get; set; }
        public List<AuthRequestParam> Params { get; set; }
    }
}
