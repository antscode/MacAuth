using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MacAuth.Models
{
    public class LoginRequest
    {
        [Required]
        [DisplayName("Code")]
        public string user_code { get; set; }
    }
}
