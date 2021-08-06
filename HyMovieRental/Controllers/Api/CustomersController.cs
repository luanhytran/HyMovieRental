using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using AutoMapper;
using HyMovieRental.Dtos;
using HyMovieRental.Models;

namespace HyMovieRental.Controllers.Api
{
    public class CustomersController : ApiController
    {
        private ApplicationDbContext _context;

        public CustomersController()
        {
            _context = new ApplicationDbContext();
        }
    
        // GET /api/customers
        // Because we return a list of object, this action will response to below url by convention
        public IEnumerable<CustomerDto> GetCustomers()
        {
            // Pass in .Select() a delegate and does the mapping
            // Map each Customer in the list to customerDto
            return _context.Customers.ToList().Select(Mapper.Map<Customer,CustomerDto>);
        }
    
        // GET /api/customers/1
        public IHttpActionResult GetCustomers(int id)
        {
            var customer = _context.Customers.SingleOrDefault(c => c.Id == id);
    
            if (customer == null)
                return NotFound();
    
            // return status code 200 with customer object
            return Ok(Mapper.Map<Customer,CustomerDto>(customer));
        }
    
        // POST /api/customers
        // By convention when we create new resource, then we return that resource to the client
        // because that the same resource but with Id generated from the server
        [HttpPost]
        public IHttpActionResult CreateCustomer(CustomerDto customerDto)
        {
            // ASP.NET API framework will will auto initialize the customer info in the request body
            // to this customer object in the parameter
    
            if (!ModelState.IsValid)
          
=======
                // this method implement IHttpActionResult 
                return BadRequest();
>>>>>>> Stashed changes

            var customer = Mapper.Map<CustomerDto, Customer>(customerDto);
    
            _context.Customers.Add(customer);
            _context.SaveChanges();
    
            // At this point the Id property of the customer will be set 
            // base on the id generated from the db, now we the customerDto object that have Id property
            customerDto.Id = customer.Id;
    
            // As part of RESTful convention, we return the URI of the newly created resource to the client
            // For example if our customer Id is 10 then the URI look like this api/customers/10
            return Created(new Uri(Request.RequestUri + "/" + customer.Id), customerDto);
        }
    
        // PUT api/customers/1
        // We can either return object or void
        [HttpPut]
        public IHttpActionResult UpdateCustomer(int id, CustomerDto customerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest();
    
            var customerInDb = _context.Customers.SingleOrDefault(c => c.Id == id);
    
            if (customerInDb == null)
                return NotFound();
    
            //customerInDb.Name = customer.Name;
            //customerInDb.Birthdate = customer.Birthdate;
            //customerInDb.MembershipTypeId = customer.MembershipTypeId;
            //customerInDb.IsSubscribedToNewsLetter = customer.IsSubscribedToNewsLetter;
    
            // AutoMapper solve the above manual mapping
            // This code is shortcut for mapping the customerDto to customer type and assign it to customerInDb
            // Original way: customerInDb = Mapper.Map<CustomerDto,Customer>(customerDto);
            Mapper.Map(customerDto, customerInDb);
    
            _context.SaveChanges();
    
            return Ok();
        }
    
        // DELETE api/customers/1
        [HttpDelete]
        public IHttpActionResult DeleteCustomer(int id)
        {
            var customerInDb = _context.Customers.SingleOrDefault(c => c.Id == id);
    
            if (customerInDb == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);
    
            _context.Customers.Remove(customerInDb);
            _context.SaveChanges();
    
            return Ok();
        }
    
    }
}
