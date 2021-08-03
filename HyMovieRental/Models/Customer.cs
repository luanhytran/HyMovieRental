using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HyMovieRental.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [DisplayName("Date of Birth")]
        public DateTime? Birthdate { get; set; }

        public bool IsSubscribedToNewsLetter { get; set; }

        // navigation property
        public MembershipType MembershipType { get; set; }

        // foreign key
        [DisplayName("Membership Type")]
        public byte MembershipTypeId { get; set; }
    }
}