using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HyMovieRental.Models
{
    public class Genre
    {
        // Id is type Byte because genre won't scale larger the max value of byte
        // unlike customer and movie, Id is type int because it will scale a lot as the customer and movie grow
        public byte Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }
    }
}