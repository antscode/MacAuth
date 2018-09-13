using System;
using System.ComponentModel.DataAnnotations;

namespace MacAuth.DbModels
{
    public class AuthRequestParam
    {
        [Key]
        public int Id { get; set; }
        public int AuthRequestId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public AuthRequest Request { get; set; }
    }
}
