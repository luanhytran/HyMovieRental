using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HyMovieRental.Models;
using HyMovieRental.ViewModels;


namespace HyMovieRental.Controllers
{
    public class CustomersController : Controller
    {
        // We need this context to access the database
        private ApplicationDbContext _context;

        public CustomersController()
        {
            _context = new ApplicationDbContext();
        }

        // ApplicationDbContext is disposable object so we need to correctly dispose this object 
        // We can use dependency injection but that beyond this course
        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }

        public ActionResult New()
        {
            var membershipTypes = _context.MembershipTypes.ToList();
            var viewModel = new CustomerFormViewModel()
            {
                MembershipTypes = membershipTypes,

                // Initial default value 0 for customer Id so validation summary don't show Id filed is required
                // If we don't do this the hidden filed Id in the form will be empty string so it will have validation error
                Customer = new Customer()
            };
            return View("CustomerForm",viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = new CustomerFormViewModel()
                {
                    MembershipTypes = _context.MembershipTypes.ToList(),
                    Customer = customer
                };

                return View("CustomerForm", viewModel);
            }
                // If Id is 0 we are creating a customer
            if (customer.Id == 0)
            {
                // When add this to context it's not written in the db
                // it just in the memory
                // Our db context have a change tracking mechanism
                // Any time you add a object to it or modify or remove any existing objects
                // it will mark them as added, modified or deleted
                _context.Customers.Add(customer);
            }
            else // If customer have Id we are editing a customer
            {
                // Use Single method because we edit a customer so it must have that customer in the db
                // so it will not be return a default, if it has then throw exception
                var customerInDb = _context.Customers.Single(x => x.Id == customer.Id);

                // Avoid use TryUpdateModel method for update customer because it's not loosely coupled
                // We can use AutoMapper with Dto model to update customer: Mapper.Map(customer, customerInDb)
                customerInDb.Name = customer.Name;
                customerInDb.Birthdate = customer.Birthdate;
                customerInDb.MembershipTypeId = customer.MembershipTypeId;
                customerInDb.IsSubscribedToNewsLetter = customer.IsSubscribedToNewsLetter;
            }

            // At this time db context go through all modified objects
            // and base on the kind of modification,
            // it generate sql statement at runtime and then will run them on db
            _context.SaveChanges();

            return RedirectToAction("Index", "Customers");
        }

        // GET: Customers
        public ActionResult Index()
        {
            // By default EF only load customer objects, not their related objects
            // We want to include MembershipType object so we need to include it
            // This technique is eager loading
            var customers = _context.Customers.Include(c => c.MembershipType).ToList();
            return View(customers);
        }

        public ActionResult Detail(int id)
        {
            var customer = _context.Customers.Include(c => c.MembershipType).SingleOrDefault(x => x.Id == id);

            if (customer == null)
                return HttpNotFound();

            return View(customer);
        }

        public ActionResult Edit(int id)
        {
            var customer = _context.Customers.SingleOrDefault(x => x.Id == id);

            if (customer == null)
                return HttpNotFound();

            var viewModel = new CustomerFormViewModel()
            {
                Customer = customer,
                MembershipTypes = _context.MembershipTypes.ToList()
            };

            return View("CustomerForm", viewModel);
        }
    }
}