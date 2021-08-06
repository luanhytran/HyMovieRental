using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using HyMovieRental.Dtos;
using HyMovieRental.Models;
using Microsoft.Owin.Security;

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
        public CustomerDto GetCustomers(int id)
        {
            var customer = _context.Customers.SingleOrDefault(c => c.Id == id);

            if (customer == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return Mapper.Map<Customer,CustomerDto>(customer);
        }

        // POST /api/customers
        // By convention when we create new resource, then we return that resource to the client
        // because that the same resource but with Id generated from the server
        [HttpPost]
        public CustomerDto CreateCustomer(CustomerDto customerDto)
        {
            // ASP.NET API framework will will auto initialize the customer info in the request body
            // to this customer object in the parameter

            if (!ModelState.IsValid)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var customer = Mapper.Map<CustomerDto, Customer>(customerDto);

            _context.Customers.Add(customer);
            _context.SaveChanges();

            customerDto.Id = customer.Id;

            // At this point the Id property of the customer will be set 
            // base on the id generated from the db, now we return this object that have Id property
            return customerDto;
        }

        // PUT api/customers/1
        // We can either return object or void
        [HttpPut]
        public void UpdateCustomer(int id, CustomerDto customerDto)
        {
            if (!ModelState.IsValid)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var customerInDb = _context.Customers.SingleOrDefault(c => c.Id == id);

            if (customerInDb == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            //customerInDb.Name = customer.Name;
            //customerInDb.Birthdate = customer.Birthdate;
            //customerInDb.MembershipTypeId = customer.MembershipTypeId;
            //customerInDb.IsSubscribedToNewsLetter = customer.IsSubscribedToNewsLetter;

            // AutoMapper solve the above manual mapping
            // This code is shortcut for mapping the customerDto to customer type and assign it to customerInDb
            // Original way: customerInDb = Mapper.Map<CustomerDto,Customer>(customerDto);
            Mapper.Map(customerDto, customerInDb);

            _context.SaveChanges();
        }

        // DELETE api/customers/1
        [HttpDelete]
        public void DeleteCustomer(int id)
        {
            var customerInDb = _context.Customers.SingleOrDefault(c => c.Id == id);

            if (customerInDb == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            _context.Customers.Remove(customerInDb);
            _context.SaveChanges();
        }

    }
}
