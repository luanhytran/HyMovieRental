using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HyMovieRental.Models
{
    public class MembershipType
    {
        public byte Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public short SignUpFee { get; set; }

        public byte DurationInMonths { get; set; }

        public byte DiscountRate { get; set; }

        // use this to represent membership type instead magic number for maintainable and clearer meaning
        // example: compare the membership type for 18+ birthdate
        public static readonly byte Unknown = 0;
        public static readonly byte PayAsYouGo = 1;
    }
}