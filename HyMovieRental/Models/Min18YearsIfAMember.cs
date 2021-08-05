using System;
using System.ComponentModel.DataAnnotations;

namespace HyMovieRental.Models
{
    public class Min18YearsIfAMember : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // We implement our own validation logic in here
            
            // This give us access to the containing class (Customer class)
            var customer = (Customer) validationContext.ObjectInstance;

            // If customer membership type is not select or pay as you go then birthdate can be < 18
            if(customer.MembershipTypeId == MembershipType.Unknown || customer.MembershipTypeId == MembershipType.PayAsYouGo)
                return ValidationResult.Success;

            if (customer.Birthdate == null)
                return new ValidationResult("Birthdate is required.");

            var age = DateTime.Now.Year - customer.Birthdate.Value.Year;

            return age >= 18
                ? ValidationResult.Success
                : new ValidationResult("Customer should be at least 18 years old to go on a membership.");
        }
    }
}