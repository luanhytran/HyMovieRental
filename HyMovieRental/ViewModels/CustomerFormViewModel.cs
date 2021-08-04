using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HyMovieRental.Models;

namespace HyMovieRental.ViewModels
{
    public class CustomerFormViewModel
    {
        public IEnumerable<MembershipType> MembershipTypes { get; set; }
        public Customer Customer { get; set; }
    }
}