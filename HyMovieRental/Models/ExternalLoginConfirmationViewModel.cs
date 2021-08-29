using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HyMovieRental.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [DataType((DataType.PhoneNumber))]
        [Required]
        [Display(Name = "Phone number")]
        [RegularExpression(@"(84|0[3|5|7|8|9])+([0-9]{8})\b", ErrorMessage = "Not a valid phone number")]
        public string PhoneNumber { get; set; }
    }
}