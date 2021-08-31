using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using HyMovieRental.Dtos;
using HyMovieRental.Models;

namespace HyMovieRental.Controllers.Api
{
    public class NewRentalsController : ApiController
    {
        private ApplicationDbContext _context;

        public NewRentalsController()
        {
            _context = new ApplicationDbContext();
        }

        [HttpPost]
        public IHttpActionResult CreateNewRental(NewRentalDto newRental)
        {
            /* Defensive vs Optimistic
             We have 2 wey to handle edge cases: defensive and optimistic
             defensive will have a lot of if else to handle edge cases, use for public API
             optimistic is simpler, use for Internal use 
             In this action, we use optimistic way and because it internal we don't have to return bad request message for edge case 
            */

            /* Single vs SingleOrDefault
             Use Single because we assume the client is sending the right customer id
             because the staff member will select the customer from the pick list or something
             if a malicious user want to send us invalid customer id, this line will throw exception
             if we build a public API that can be use by various application then we use SingleOrDefult and Defensive code 
            */
            var customer = _context.Customers.Single(
                c => c.Id == newRental.CustomerId);

            var movies = _context.Movies.Where(
                m => newRental.MovieIds.Contains(m.Id) );

            // for the other edge case, this optimistic implementation does protect us
            foreach (var movie in movies)
            {
                // prevent: malicious user to mess up our app and the number available will end up being negative
                if (movie.NumberAvailable == 0)
                    return BadRequest("Movie is not available.");

                movie.NumberAvailable--;
                var rental = new Rental()
                {
                    Customer = customer,
                    Movie = movie,
                    DateRented = DateTime.Now
                };

                _context.Rentals.Add(rental);
            }

            _context.SaveChanges();

            // We not return Created() because we creating multiple record
            return Ok();
        }

        //[HttpPost]
        //public IHttpActionResult ReturnMovie()
        //{

        //}
    }
}
