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

        // sometime for optimization we don't want to load the entire MembershipType.Id
        // we may only need the FK so we add this MembershipTypeId, EF know this is a FK by convention
        [DisplayName("Membership Type")]
        public byte MembershipTypeId { get; set; }

        // navigation property
        // this will add MembershipType as FK 
        public MembershipType MembershipType { get; set; }

        
        public bool IsSubscribedToNewsLetter { get; set; }

        [DisplayName("Date of Birth")]
        [Min18YearsIfAMember]
        public DateTime? Birthdate { get; set; }
    }
}