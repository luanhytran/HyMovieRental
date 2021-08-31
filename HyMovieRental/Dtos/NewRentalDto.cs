using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using HyMovieRental.Models;

namespace HyMovieRental.Dtos
{
    public class NewRentalDto
    {
        public int CustomerId { get; set; }

        public List<int> MovieIds { get; set; }
    }
}