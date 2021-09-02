# The Complete ASP.NET MVC 5 Course

- A video rental stores website.
- Documented by Luan Hy Tran 👨‍💻.

# MVC Architectural Pattern

## Model

- Application data and behaviour in terms of its problem domain.

## View

- The HTML Markup that we display to the user.

## Controller

- Responsible for handling an HTTP Request.
- **Example:** http://vidly.com/movies, a controller will be selected to handle this request. This controller will get all the movies from the database, put them on the view then return the view to the client or browser.
- **Router**: selects the right controller/ action to handle a request.

# Setting up the development environment

Install these Extension:

- Visual Studio Productivity Power Tools
- Web Essentials 
- ReSharper (optional)

# ASP.NET MVC Project Structure

- **App_Start:** a folder that include a few classes that are called when the application started.

  - **RouteConfig.cs:** Configuration of routing rules.

    ```c#
    namespace Vidly
    {
        public class RouteConfig
        {
            public static void RegisterRoutes(RouteCollection routes)
            {
                routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
    
                routes.MapRoute(
                    name: "Default",
                    url: "{controller}/{action}/{id}",
                    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
                );
            }
        }
    }
    ```

  

- **Content:** a folder that store CSS, image and any other client side assets.



- **Controllers:** a folder that contain controller class.



- **fonts:** (prefer move to Content folder) .



- **Model:** include domain classes will be here in this folder.



- **Scripts:** store JavaScript files.



- **Views:** store views (html).

  - **Shared:** store view that can be use across all the controllers.

  

- **favicon.ico:** icon of the application that display in the browser.



- **Global.asax**: one of the traditional files that have been in ASP.NET for a long time and it's a class that provide hooks for various events and applications life cycle.

  - When the application is started, `Application_Start()` method will be called. As you can see we are registering a few thing like the RouteConfig. So when the application started, we tell the runtime these are the route for our application.

    ```c#
    namespace Vidly
    {
        public class MvcApplication : System.Web.HttpApplication
        {
            protected void Application_Start()
            {
                AreaRegistration.RegisterAllAreas();
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                BundleConfig.RegisterBundles(BundleTable.Bundles);
            }
        }
    }
    ```

    

- **packages.config**: which is use by NuGet package manager to manage the dependency of our application.



- **Startup.cs:** which is a new approach microsoft is taking for starting the application in ASP.NET Core 1.0 which drop `Global.asax` and all the startup logic is implemented in this class.



- **Web.config:** XML that include configuration for the application. All of the element in this XML, mostly we will need to work with only 2 sections

  - `<connectionStrings>` : which is where we specify database connection strings.
  - `<appSettings>`: which is where we define configuration settings for our applications.

# ASP.NET MVC Fundamentals

## Action Results

- `ActionResult` is the base class for all Action Results in ASP.NET MVC . Depending on what an action does, it will return an instance of one of the classes that derive from `ActionResult`.
- **Example:** `View( )` which is a helper method inherit from from the base Controller class, this method allow us to quickly create a `ViewResult`.

```c#
public class MoviesController : Controller 
{
    // this ActionResult is the base class for all Action Results in ASP.NET MVC .
    public ActionResult Index()
    {
        // this way is more common for ASP.NET MVC developers than below
        return View();
        
        // alternatively we can return a view result
        //return new ViewResult();
    }
}
```

Now you maybe asking why is the return type of this method is `ActionResult` when it's actually return a `ViewResult` . 

In case of this action, we can simply set the return type to `ViewResult` and this is actually good practice especially when it comes to unit testing this action.

```c#
public class MoviesController : Controller 
{
    public ViewResult Index()
    {
        return View();
    }
}
```



This way will save our-self from an extra cast in our unit tests. But sometime it's possible that in an action we may have different execution paths and return different action results. In that case we need to set the return type of the action to `ActionResult` so it can return any of it subtype.

`ViewResult` is one of the Action Results that you will work with most of the time. Most of the time we use `View()`, `HttpNotFound()` and `RedirectToAction()`.

​																													**Action Results**

| Type                  | Helper Method      |
| --------------------- | ------------------ |
| ViewResult            | View()             |
| PartialViewResult     | PartialView()      |
| ContentResult         | Content()          |
| RedirectResult        | Redirect()         |
| RedirectToRouteResult | RedirectToAction() |
| JsonResult            | Json()             |
| FileResult            | File()             |
| HttpNotFoundResult    | HttpNotFound()     |
| EmptyResult           |                    |

**Example:**

```c#
public class MoviesController : Controller
{
    // GET: Movies/Random
    public ActionResult Random()
    {
        var movie = new Movie() {Name = "Shrek!"};

        //return View(movie); // return a movie object to the view
        
        //return Content("HelloWorld"); // return a plain text
        
        //return HttpNotFound(); // return Http Not Found page
        
        //return new EmptyResult(); // return a blank empty page
        
        return RedirectToAction("Index", "Home", new {page = 1, sortBy = "name"});
        // redirect to the home page with url localhost:5000/?page=1&sortBy=name
    }
}
```



## Action Parameters

When a request come in the application, ASP.NET MVC are automatic map requests data to the parameter of action method. If a action method take a parameter, the MVC framework looks for a parameter with the same name in the request data and check if that parameter name exist then the framework automatic pass the value of that parameter to the target action.

Parameter sources can be embed in:

- In the URL: /movies/edit/1
- In the query string: /movies/edit?id=1
- In the form data: id=1

**Example:**

```c#
public class MoviesController : Controller
{
    public ActionResult Edit(int id)
    {
        return Content("id=" + id);
    }
}

```

We enter the URL `/movies/edit/1` then the page will return `id=1`.

If we change the parameter name to `movieId`, and enter this URL in the browser `/movies/edit?id=1` it will get an exception

We change to `/movies/edit?movieId=1` then everything work.

```c#
public class MoviesController : Controller
{
    public ActionResult Edit(int movieId)
    {
        return Content("id=" + movieId);
    }
}
```

ASP.NET MVC can't find a parameter name `movieId` in the URL, query string, or request data from a Form and this is why we get this exeption

We can't use this URL `/movies/edit/1` because the default parameter name is `id` not `movieId`

```c#
public class RouteConfig
{
    public static void RegisterRoutes(RouteCollection routes)
    {
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

        routes.MapRoute(
            name: "Default",
            url: "{controller}/{action}/{id}",
            defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
        );
    }
}
```

So this is how ASP.NET MVC maps request data to parameter of our action

- **Optional parameter**

  ```c#
  public ActionResult Index(int? pageIndex, string sortBy){    if (!pageIndex.HasValue)        pageIndex = 1;    if (string.IsNullOrWhiteSpace(sortBy))        sortBy = "Name";    return Content(string.Format($"pageIndex={pageIndex}&sortBy={sortBy}"));}
  ```

  When we navigate to `/movies` , default parameter value is `pageIndex=1&sortBy=Name` , we can change the value of these parameter value using query string way like this `/movies?pageIndex=2&sortBy=ReleasedDate` but we cannot embed these parameter it in the URL like this `/movies/2/ReleasedDate` because that will require a custom route that include 2 parameters.

  

## Convention-based Routing

There is a situation we need multiple route parameter for example `/movies/released/2015/04` where we can get the released year and month. We will need to create a custom route.

You need to add this route before the `Default` route, because the order of this route is matters. You need to define them from the most specific to the most generic, otherwise a more generic route will be apply to a URL and that's not what you want.

```c#
 public class RouteConfig {     public static void RegisterRoutes(RouteCollection routes)     {         routes.IgnoreRoute("{resource}.axd/{*pathInfo}");         // new custom route         routes.MapRoute(             "MoviesByReleaseDate",             "movies/released/{year}/{month}",             new { controller = "Movies", action="ByReleaseDate"}         );         routes.MapRoute(             name: "Default",             url: "{controller}/{action}/{id}",             defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }         );     } }
```

Action for the custom route, the name of the parameter in the action must match the name in the custom route and vice versa.

```c#
public ActionResult ByReleaseDate(int year, int month){    return Content(year + "/" + month);}
```

- **Add constraint to a custom route**

  We can use regular expression to apply the constraint, write constraint in the 4th argument of the custom route.

  ```c#
   routes.MapRoute(     "MoviesByReleaseDate",     "movies/released/{year}/{month}",     new { controller = "Movies", action="ByReleaseDate"},     new { year = @"\d{4}", month = @"\d{2}"}     // new { year = @"2015|2016", month = @"\d{2}"} // limit the released year only in 2015 or 2016  );
  ```

  This say the year must at least have 4 digit and month have 2 digit.

  

## Attribute Routing

In ASP.NET MVC 5, produced attribute routing. 

So why use attribute routing ? 

1. If we working in a larger application sooner or later, the custom route is gonna growth a lot and over time it's become a mess. 
2. You have to move back and forth between action and custom route.
3. If you rename the action specify in the custom route, then you have to change it too in the custom route which make the code really fragile.

Attribute routing solved these problem and we can use constraint in the route too.

First you need to add `routes.MapMvcAttributeRoutes();` to use attribute routing in `RouteConfig.cs`

```c#
public class RouteConfig{    public static void RegisterRoutes(RouteCollection routes)    {        routes.IgnoreRoute("{resource}.axd/{*pathInfo}");        routes.MapMvcAttributeRoutes();        routes.MapRoute(            name: "Default",            url: "{controller}/{action}/{id}",            defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }        );    }}
```

Then use attribute routing like this on an action

```c#
[Route("movies/released/{year:regex(^\\d{4}$)}/{month:regex(\\d{2}):range(1,12)}")]public ActionResult ByReleaseDate(int year, int month){    return Content(year + "/" + month);}
```

- Constraints

  We can apply these constraints to the attribute routing

  - `min`, `max`, `minlength`, `maxlength`, `int`, `float`, `guid`

  

## Passing Data to Views

Avoid using `ViewData` and `ViewBag` because they are fragile. Plus, you have to do extra  casting, which makes your code ugly. Pass a model (or a view model) directly to a view: `return View(movie)`;

```c#
 public ActionResult Random() {     var movie = new Movie() {Name = "Shrek!"};     return View(movie); }
```

```c#
@model Vidly.Models.Movie@{    ViewBag.Title = "Random";}<h2>@Model.Name</h2>
```



## View Models

View Models is a model specifically build for a view, it include data and rules specific for that view.

```c#
namespace Vidly.ViewModels{    public class RandomMovieViewModel    {        public Movie Movie { get; set; }        public List<Customer> Customers { get; set; }    }}
```




## Razor Syntax

```c#
@if(…)	{ 	//	C#	code	or	HTML}	@foreach(…){    }	
```



Render a class (or any attributes) conditionally:

```c#
@{    var	className =	Model.Customers.Count >	5 ?	“popular” :	null;}<h2	class=“@className”>…</h2>
```



## Partial Views

Partial Views is like a small view that we can reuse on different views

The model pass to the parent view will be automatically be passed in the partial view

```c#
@model Vidly.ViewModels.RandomMovieViewModel<!DOCTYPE html><html><head>    <meta charset="utf-8" />    <meta name="viewport" content="width=device-width, initial-scale=1.0">    <title>@ViewBag.Title - My ASP.NET Application</title>    @Styles.Render("~/Content/css")    @Scripts.Render("~/bundles/modernizr")</head><body>   @Html.Partial("_NavBar")    <div class="container body-content">    @RenderBody()    <hr/>    <footer>        <p>&copy; @DateTime.Now.Year - My ASP.NET Application</p>    </footer></div>@Scripts.Render("~/bundles/jquery")@Scripts.Render("~/bundles/bootstrap")@RenderSection("scripts", required: false)</body></html>
```



Or we can pass sub model in the view model explicitly to partial view too 

```c#
@Html.Partial("_NavBar",Model.Movie)
```



# Working with Data

## Entity Framework

EF  is a tool we use to access a database, more accurately it's classified as an Object / Relational Mapper (O/RM).

It's map data and relational database into objects of our application.



## What EF solved ?

No more

- **Stored Procedures**
- **Manage Database Connections**
- **Manual Mapping:** manually mapped the tables and records domain objects.

The framework takes care all of this for you.



### `Dbcontext` & `Dbset`

- EF provide a class call `DbContext` (database in SQL) which is a gate way to our database.
- A `DbContext `have one or more `DbSet` (table in SQL), which represent a table in our database.



### How EF query Database

We use LINQ to query these `DbSet` and EF will translate our LINQ query to SQL query in runtime. 

It's open connection to Database, read the data, maps it to object and add it to `Dbset` in our `DbContext` .

As we Add / Modify / Delete in this `Dbset`. EF keep track of this changes and when we make a change, EF will automatically generate SQL statement and execute it on our database.



## Database-first vs Code-first

### Db-First

Domain Classes <= Entity Framework <= Database



### Code-First

Domain Classes => Entity Framework => Database



### Which approach is better ?

CodeFirst is better. The reason is:

- **Increased productivity**: we don't have to waste our time with table design, it much faster to write code.
- **Full versioning of database:** we can migrate to any version of database at any time.
- **Much easier to build an integration test database**



## Changing the Model

**Note**: to have identity context and all the dbcontext stuff like the course you should create a project like normal but when choosing mvc template choose individual account authentication.

When changing/ update model, you shouldn't change all the model at a time, but instead make small changes and create a migrations and run it on a database. 

- With the big bang migrations you increase the risk of getting things going wrong and that the reason who try code-first workflow failed.
- This allow to better version your code.

### Navigation Property

In an entity class, we need a Id so we can create a Id properties by specify it's name is Id or name+Id. EF will auto know that key is an Id and set it to primary key.

![image-20210801020712097](https://raw.githubusercontent.com/luanhytran/img/master/image-20210801020712097.png)

`MembershipType` is a Navigation Property

- It's allow us to navigate from one type to another type. In this case is from Customer to it's Membership type.
- Useful when you want to load an object and it's related object together for the database. For example, we can load Customer and it's Membership type together. 

Sometime for optimization, we don't want to load the entire `MembershipType` object. We may only need the foreign key so we add `MembershipTypeId`. EF recognize the foreign key convention and treat it as a foreign key.



## Code-first Migrations

Use these command in Package Manager Console

```powershell
Install-Package EntityFramework
enable-migrations
add-migration <name>
add-migration <name> -force	(to	overwrite the last migration)
update-database
```



## Seeding the Database

Create a new empty migration (create migration without any change in model) then write the SQL statement in the `Up()` method in the migration file that just created.

![image-20210801023830250](https://raw.githubusercontent.com/luanhytran/img/master/image-20210801023830250.png)

- The argument that code-first workflow doesn't give you full control over database is FALSE. With `Sql()` method like above we can run any SQL statement and modify our database anywhere we want.

### Migrations folder

We can see all the migrations we have added. Any change in the database schema or data in database is properly coded here.

![image-20210801024153714](https://raw.githubusercontent.com/luanhytran/img/master/image-20210801024153714.png)

- When it come to deploy our application, if this is the first time then we can get all the migrations the beginning of the time to the last migration. And using a command in Package Manager Console, we as EF to generate a SQL script which will include all the changes and then we run this script on the production database.
- If this is not the first deployment, meaning we already have the database then we can find the last migration run on that database and create a new SQL script from that migration to the last one. Again the process i



## Overriding Conventions

![image-20210803002039499](https://raw.githubusercontent.com/luanhytran/img/master/image-20210803002039499.png)

The type of `Name` column is `nvarchar(max)` and nullable, in C# class the `Name` property is type of string, nullable and don't have limit how many character you can store in a string. By convention, EF will use that fact to set the type of `Name` column in `cutomer` database.

We can override these default convention by using **data annotation** or **attribute** like this:

```c#
[Required]
[StringLength(255)]
public string Name { get; set; }
```

- `[Required]` : not nullable.
- `[StringLength]` : specify maximum characters.

We can also use **Fluent API** to set override these convention that teach in the **EF in Depth** course.

After add data annotations we create migration and update the database to apply



## Querying Objects

```c#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using vidly.Models;

namespace vidly.Controllers
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

        // GET: Customers
        public ActionResult Index()
        {
            // Querying object 
            var customers = _context.Customers.ToList();
            return View(customers);
        }

        public ActionResult Detail(int id)
        {
            // Querying object 
            var customer = _context.Customers.SingleOrDefault(x => x.Id == id);

            if (customer == null)
                return HttpNotFound();

            return View(customer);

        }
    }
}
```



### LINQ Extension Methods

```c#
_context.Movies.Where(m	=> m.GenreId ==	1)
_context.Movies.Single(m =>	m.Id ==	1);
_context.Movies.SingleOrDefault(m => m.Id == 1);
_context.Movies.ToList();	
```



## Eager Loading

In `Customer` class, `MembershipType` is it's related object

```c#
using System.ComponentModel.DataAnnotations;

namespace vidly.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }
        public bool IsSubscribedToNewsLetter { get; set; }

        // navigation property
        public MembershipType MembershipType { get; set; }

        // foreign key
        public byte MembershipTypeId { get; set; }
    }
}
```

By default EF only load customer objects, not their related objects so membership is null.

![image-20210803013223485](https://raw.githubusercontent.com/luanhytran/img/master/image-20210803013223485.png)

That why we have this error.

![image-20210803013242175](https://raw.githubusercontent.com/luanhytran/img/master/image-20210803013242175.png)

To solve this problem we need to load the Customers and their Membership Types together, this is what call **Eager Loading**.

```c#
public ActionResult Index()
{
    // By default EF only load customer objects, not their related objects
    // We want to include MembershipType object so we need to include it
    // This technique is Eager Loading
    var customers = _context.Customers.Include(c => c.MembershipType).ToList();
    return View(customers);
}
```

Result:

![image-20210803013623016](https://raw.githubusercontent.com/luanhytran/img/master/image-20210803013623016.png)



## Create custom shortcut 

Because we use Package Manager Console quite often so this is how we create shortcut for it, remember to click Assign.

![image-20210803014245442](https://raw.githubusercontent.com/luanhytran/img/master/image-20210803014245442.png)



# Building Forms

# The Form Markup

`@Html.BeginForm` method just render the `<form>` tag. This method return a disposal object , if we wrap this call in `using` block, at the end of the `using` block the object return from `Html.BeginForm` will be dispose and in the dispose method it will render `</form>`.

 ```c#
@model vidly.Models.Customer
@{
    ViewBag.Title = "New";
}

<h2>New</h2>

@using (Html.BeginForm("Create", "Customers"))
{
    <div class="form-group">
        @Html.LabelFor(m => m.Name)
        @*We can add html attribute like class, id ... to Html text box like this*@
        @Html.TextBoxFor(m => m.Name, new {@class = "form-control"})
    </div>
    <div class="form-group">
        @Html.LabelFor(m => m.Birthdate)
        @Html.TextBoxFor(m => m.Birthdate, new {@class="form-control"})
    </div>
    <div class="checkbox">
        <label>
            @Html.CheckBoxFor(m=>m.IsSubscribedToNewsLetter) Subscribed to Newsletter?
        </label>
    </div>
}
 ```

`Html.TextBoxFor` helper method auto support validation for us 

![image-20210803160451183](https://raw.githubusercontent.com/luanhytran/img/master/image-20210803160451183.png)

Where are the validation coming from?

![image-20210803160624811](https://raw.githubusercontent.com/luanhytran/img/master/image-20210803160624811.png)

It's come from definition of our customer class `[Required]` and `[StringLength(255)]`. When you use the helper method `Html.TextBoxFor` ASP.NET MVC auto support validation for that field. If you use raw html you have to write this all by hand.



# Form Labels

`Html.LabelFor` is prefer for working with label because even if you change your properties name the lambda expression in the helper method will auto change too, the only problem is if you change the string in `DisplayName` annotation you have to recompile. 

If you want to change the label display name just add `DisplayName`.

```c#
[DisplayName("Date of Birth")]
public DateTime? Birthdate { get; set; }
```



# Drop-down Lists

- Add `DbSet<object>` in `DbContext` in order to load that object value in the drop down list.
- Add View Model for the Membership Type and Customer object

```c#
using System.Collections.Generic;
using vidly.Models;

namespace vidly.ViewModels
{
    public class NewCustomerViewModel
    {
        // We only iterate over the membership type only, not add, remove... so we use IEnumerable.
        // In the future if we replace the _context.MembershipTypes.ToList() in controller to
        // another collection we don't have to comeback here and modify this  viewmodel as long as 
        // that collection implement IEnumerable, this way our code more loosely coupled
        public IEnumerable<MembershipType> MembershipTypes { get; set; }

        // Need customer object to use LabelFor or TextBoxFor or validation
        // for the properties of the Customer object
        public Customer Customer { get; set; }
    }
}
```



```c#
@model HyMovieRental.ViewModels.NewCustomerViewModel
@{
    ViewBag.Title = "New";
}

<h2>New</h2>

@using (Html.BeginForm("Create", "Customers"))
{
    <div class="form-group">
        @Html.LabelFor(m => m.Customer.Name)
        @*We can add html attribute like class, id ... to Html text box like this*@
        @Html.TextBoxFor(m => m.Customer.Name, new { @class = "form-control" })
    </div>
    <div class="form-group">
        @Html.LabelFor(m => m.Customer.Birthdate)
        @Html.TextBoxFor(m => m.Customer.Birthdate, new { @class = "form-control" })
    </div>
    <div class="checkbox">
        <label>
            @Html.CheckBoxFor(m => m.Customer.IsSubscribedToNewsLetter) Subscribed to News Letter?
        </label>
    </div>
    <div class="form-group">
        @Html.LabelFor(m => m.Customer.MembershipTypeId)
        @Html.DropDownListFor(m => m.Customer.MembershipTypeId, new SelectList(Model.MembershipTypes, "Id", "Name"), "Select Membership Type", new { @class = "form-control" })
    </div>
    <button type="submit" class="btn btn-primary">Save</button>
}
```



# Model Binding

Model binding + Saving data video

**Create customer**

![image-20210804022804326](https://raw.githubusercontent.com/luanhytran/img/master/image-20210804022804326.png)

MVC framework will auto map request data of the form above to this the customer object parameter, this is what we call **Model Binding**.

```c#
[HttpPost]
public ActionResult Create(Customer customer)
{
    // When add this to context it's not written in the db
    // it just in the memory
    // Our db context have a change tracking mechanism
    // Any time you add a object to it or modify or remove any existing objects
    // it will mark them as added, modified or deleted
    _context.Customers.Add(customer);

    // At this time db context go through all modified objects
    // and base on the kind of modification,
    // it generate sql statement at runtime and then will run them on db
    _context.SaveChanges();

    return RedirectToAction("Index", "Customers");
}
```

Request data when submit the save button

![image-20210804023044205](https://raw.githubusercontent.com/luanhytran/img/master/image-20210804023044205.png)



# Edit Form

Edit form + Update data video

Edit don't have view, it's mission is search the id of the customer and pass back to create customer form to render existing info of that customer.

```c#
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
```

Create customer view

```c#
@model HyMovieRental.ViewModels.CustomerFormViewModel
@{
    ViewBag.Title = "New";
}

<h2>New</h2>

@using (Html.BeginForm("Create", "Customers"))
{
    <div class="form-group">
        @Html.LabelFor(m => m.Customer.Name)
        @*We can add html attribute like class, id ... to Html text box like this*@
        @Html.TextBoxFor(m => m.Customer.Name, new { @class = "form-control" })
    </div>
    <div class="form-group">
        @Html.LabelFor(m => m.Customer.Birthdate)
        @Html.TextBoxFor(m => m.Customer.Birthdate, "{0:d MMM yyyy}", new { @class = "form-control" })
    </div>
    <div class="form-group">
        @Html.LabelFor(m => m.Customer.MembershipTypeId)
        @Html.DropDownListFor(m => m.Customer.MembershipTypeId, new SelectList(Model.MembershipTypes, "Id", "Name"), "Select Membership Type", new { @class = "form-control" })
    </div>
    <div class="checkbox">
        <label>
            @Html.CheckBoxFor(m => m.Customer.IsSubscribedToNewsLetter) Subscribed to News Letter?
        </label>
    </div>
    <button type="submit" class="btn btn-primary">Save</button>
}
```

**Edit customer**

When edit a customer information we could use the same action for both create and edit customer. 

```c#
[HttpPost]
        public ActionResult Save(Customer customer)
        {
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
```

In the customer form we must past the customer id as hidden above the save button element.

```c#
@model HyMovieRental.ViewModels.CustomerFormViewModel@{    ViewBag.Title = "New";}<h2>New</h2>@using (Html.BeginForm("Save", "Customers")){    <div class="form-group">        @Html.LabelFor(m => m.Customer.Name)        @*We can add html attribute like class, id ... to Html text box like this*@        @Html.TextBoxFor(m => m.Customer.Name, new { @class = "form-control" })    </div>    <div class="form-group">        @Html.LabelFor(m => m.Customer.Birthdate)        @Html.TextBoxFor(m => m.Customer.Birthdate, "{0:d MMM yyyy}", new { @class = "form-control" })    </div>    <div class="form-group">        @Html.LabelFor(m => m.Customer.MembershipTypeId)        @Html.DropDownListFor(m => m.Customer.MembershipTypeId, new SelectList(Model.MembershipTypes, "Id", "Name"), "Select Membership Type", new { @class = "form-control" })    </div>    <div class="checkbox">        <label>            @Html.CheckBoxFor(m => m.Customer.IsSubscribedToNewsLetter) Subscribed to News Letter?        </label>    </div>    @Html.HiddenFor(m=>m.Customer.Id)    <button type="submit" class="btn btn-primary">Save</button>}
```



# Troubleshooting Entity Validation Errors

Use Try Catch block to have the message of what your error is and you need to catch the right error, how did you know the right error, it from the first time throw the exception and you know that error and then you catch that exception to see the real error so you know and debug it.

Example:

![image-20210804162725695](https://raw.githubusercontent.com/luanhytran/img/master/image-20210804162725695.png)



![image-20210804162709624](https://raw.githubusercontent.com/luanhytran/img/master/image-20210804162709624.png)

# Implement Validation

## Adding Validation

We can use fluent validation and data annotation the validate entities, in this docs we use the second method.

**Step 1:** Add validation data annotation to entities class

```c#
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
```



**Step 2:** Add `ModelState.IsValid` to change the flow of the program, add it in Action to get access to validation data.

ASP populate customer object using request data, it check to see if this object is valid base on the data annotation applied to the properties of the`Customer` class.

```c#
[HttpPost]
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

            if (customer.Id == 0)
            {
                _context.Customers.Add(customer);
            }
            else 
            {
                var customerInDb = _context.Customers.Single(x => x.Id == customer.Id);

                customerInDb.Name = customer.Name;
                customerInDb.Birthdate = customer.Birthdate;
                customerInDb.MembershipTypeId = customer.MembershipTypeId;
                customerInDb.IsSubscribedToNewsLetter = customer.IsSubscribedToNewsLetter;
            }

            _context.SaveChanges();

            return RedirectToAction("Index", "Customers");
        }
```



**Step 3:** Add validation message to form

Add `@Html.ValidationMessageFor()` method next to the field that need validation message

```c#
@using (Html.BeginForm("Save", "Customers"))
{
    <div class="form-group">
        @Html.LabelFor(m => m.Customer.Name)
        @*We can add html attribute like class, id ... to Html text box like this*@
        @Html.TextBoxFor(m => m.Customer.Name, new { @class = "form-control" })
        @Html.ValidationMessageFor(m=>m.Customer.Name)
    </div>
    <div class="form-group">
        @Html.LabelFor(m => m.Customer.Birthdate)
        @Html.TextBoxFor(m => m.Customer.Birthdate, "{0:d MMM yyyy}", new { @class = "form-control" })
    </div>
    <div class="form-group">
        @Html.LabelFor(m => m.Customer.MembershipTypeId)
        @Html.DropDownListFor(m => m.Customer.MembershipTypeId, new SelectList(Model.MembershipTypes, "Id", "Name"), "Select Membership Type", new { @class = "form-control" })
        @Html.ValidationMessageFor(m=>m.Customer.MembershipTypeId)
    </div>
    <div class="checkbox">
        <label>
            @Html.CheckBoxFor(m => m.Customer.IsSubscribedToNewsLetter) Subscribed to News Letter?
        </label>
    </div>
    @Html.HiddenFor(m=>m.Customer.Id)
    <button type="submit" class="btn btn-primary">Save</button>
}
```



**Note:**

Although we don't have the `[Required]` at `MembershipTypeId` and `MembershipType` but it still get validation message. Because `MembershipTypeId` is `byte` and not `byte?` so it implicitly required.

```c#
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
```

When we don't choose membership type, it's value is empty string, so when MVC framework post the form it can't get the value of the membership type in the form data. It doesn't know how to transfer empty `string` to `byte` that's why it mark this field is invalid.

![image-20210805012919710](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805012919710.png)



## Styling Validation Errors

We will style validation message and border to red if validation is errors

**Step 1:** Go to `Bundle.config ` change `site.css` to `Site.css` and add these CSS 

**Step 2:** Go to `Site.css` and add these CSS 

```css
.field-validation-error {
    color: red;
}

.input-validation-error {
    border: 2px solid red;
}
```

**Result**:

![image-20210805013204690](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805013204690.png)



## Data Annotations

We have few data annotations for implementing validation:

- `[Range(1,10)]` : Specify minimum or maximum value can access
- `[Compare("OtherProperty")]` : Compare 2 properties like password and confirm password
- `[Phone]` , `[EmailAddress]` , `[Url]` , `[RegularExpression("...")]` ...

### Override validation message

Override validation message for `[Required]`

```c#
[Required(ErrorMessage = "Please enter customer's name")]
[StringLength(255)]
public string Name { get; set; }
```



## Custom Validation

Let say we want a custom validation that want a member age is minimum 18 years old.

**Step 1:** 

- Create a class and derive class `ValidationAttribute` 
- Override `IsValid(object, ValidationContext())` method

```c#
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
            if(customer.MembershipTypeId == 0 || customer.MembershipTypeId ==1)
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
```



**Step 2:** Apply a data annotation with the class name to the entities property to apply the custom validation

```c#
[DisplayName("Date of Birth")]
[Min18YearsIfAMember]
public DateTime? Birthdate { get; set; }
```



**Step 3:** Add validation message to form

Add `@Html.ValidationMessageFor()` below the text box for birthdate.

```c#
<div class="form-group">
        @Html.LabelFor(m => m.Customer.Birthdate)
        @Html.TextBoxFor(m => m.Customer.Birthdate, "{0:d MMM yyyy}", new { @class = "form-control" })
        @Html.ValidationMessageFor(m=>m.Customer.Birthdate)
    </div>
```

 **Result:**

![image-20210805015857886](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805015857886.png)



## Refactoring Magic Number

Magic number is just like normal number: 1,2,3... We should avoid magic number because it will hurt the maintainable of your application

**Example**: 

We have a membership type and it's value is `byte`,  in [Custom Validation](#custom-validation) we have a sequence of code that use magic number, and we need to refactor it for maintainable and cleared meaning.

From this:

```c#
if(customer.MembershipTypeId == 0 || customer.MembershipTypeId ==1)
                return ValidationResult.Success;
```

To this:

```c#
// If customer membership type is not select or pay as you go then birthdate can be < 18
if(customer.MembershipTypeId == MembershipType.Unknown || customer.MembershipTypeId == MembershipType.PayAsYouGo)
    return ValidationResult.Success;
```

**Step 1:**

We just need to Add static filed or use enum then use them instead of magic number.

```c#
public class MembershipType
{
    public byte Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    public short SignUpFee { get; set; }

    public byte DurationInMonths { get; set; }

    public byte DiscountRate { get; set; }

    // use this to represent membership type instead magic number for maintainable and clearer meaning
    // example: compare the membership type for 18+ birthdate
    public static readonly byte Unknown = 0;
    public static readonly byte PayAsYouGo = 1;
}
```



## Validation Summary

This mean show all validation message on the top of the form

**Step 1:** Add this line in the form

![image-20210805021834547](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805021834547.png)

**Result:**

![image-20210805021741895](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805021741895.png)



**Step 2:** Fix the validation summary message `The Id filed is required`, that field is a hidden field and we don't want to show that.

![image-20210805110553793](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805110553793.png)

 

**Step 3 (optional):**

Custom another message for validation summary like this and we can add CSS for red font color.

![image-20210805110803283](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805110803283.png)

Code:

```c#
@using (Html.BeginForm("Save", "Customers"))
{
    @Html.ValidationSummary(true, "Please fix the following errors.")
    <div class="form-group">
        @Html.LabelFor(m => m.Customer.Name)
        @*We can add html attribute like class, id ... to Html text box like this*@
        @Html.TextBoxFor(m => m.Customer.Name, new { @class = "form-control" })
        @Html.ValidationMessageFor(m=>m.Customer.Name)
    </div>
    <div class="form-group">
        @Html.LabelFor(m => m.Customer.MembershipTypeId)
        @Html.DropDownListFor(m => m.Customer.MembershipTypeId, new SelectList(Model.MembershipTypes, "Id", "Name"), "Select Membership Type", new { @class = "form-control" })
        @Html.ValidationMessageFor(m=>m.Customer.MembershipTypeId)
    </div>
    <div class="form-group">
        @Html.LabelFor(m => m.Customer.Birthdate)
        @Html.TextBoxFor(m => m.Customer.Birthdate, "{0:d MMM yyyy}", new { @class = "form-control" })
        @Html.ValidationMessageFor(m=>m.Customer.Birthdate)
    </div>
    <div class="checkbox">
        <label>
            @Html.CheckBoxFor(m => m.Customer.IsSubscribedToNewsLetter) Subscribed to News Letter?
        </label>
    </div>
    @Html.HiddenFor(m=>m.Customer.Id)
    <button type="submit" class="btn btn-primary">Save</button>
}
```



## Client-side Validation

We can use `jqueryval` for client-side validation in our project.

Building validation at client-side have these benefits:

- **Immediate feedback :** we don't have to press submit to see new update validation error.
- **No waste of server-side resources** **:** we won't waste our server resources every time the user make a mistake filling out the form. It's better to do some basic validation in the client to make sure data is correctly formatted so when we run the server-side validation the data is in the right format, and we add the extra server-side validation mostly for security to prevent a malicious user to bypass the client-side validation.

**Step 1:** In `_Layout.cshtml` it's allow us to add a script section to our view, in the individual view that we want to add scripts reference we just have to call out this script section.

![image-20210805112020561](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805112020561.png)



**Step 2:**

Reference to `jqueryval` in the script section in order to have client-side validation.

![image-20210805112231460](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805112231460.png)

**Note**: By default ASP don't enable client-side validation so we have to do it manual. We can go to `Bundle.config` to see the path of the jqueryval.



**Result:**

![client side validation using jqueryval](https://raw.githubusercontent.com/luanhytran/img/master/client%20side%20validation%20using%20jqueryval.gif)

If the client side validation is invalid then it will not send a request to the server ( a roundtrip to the server ).

![image-20210805112815602](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805112815602.png)



**How this actually happen**

ASP.NET use **data annotations** both for server-side and client-side validation.

The text box have attribute for validation, this is added by razor when it read the properties data annotation in the view. `Jqueyval` understand these attribute and it will make the client-side validation based on this information.

![image-20210805113019725](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805113019725.png)

Client-side validation ( in this case we using jqueryval ) is only work with standard data annotation in .NET.

The custom validation we created in [Custom Validation](##custom-validation) does not have client-side support, we have to write additional jquery to support custom validation. But when we change validation logic we will need to change both in the custom validation and the related jquery.

**Note:** Mosh prefer use client-side validation for standard .NET data annotation and validation custom business rule purely on the server.



## Anti-forgery Token

This kind of attack is call **CSRF - Cross-site Request Forgery**. When user leave the site without signing out, this user will have an active session in the server so there are still authenticated for a few minutes. Then the hacker will get the user cookie and send request to the server through their website on behalf of the victim.

[(13) Hack cùng Code Dạo - Kì 5: CSRF - Cross-Site Request Forgery - YouTube](https://www.youtube.com/watch?v=sVO984z809M)

![image-20210805154506259](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805154506259.png)

The hacker can trick a user to visit a malicious page of them, on this page the he can put a image and write a little bit of JavaScript code. So when the page is loaded, it will POST a HTTP request to the user website.

![image-20210805155752276](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805155752276.png)

Because this user have an active session on our website, this request will be successfully executed. We call this kind of attack is `CSRF`. This attack can literally execute any action on behalf of the victim.

**Example:** the hacker can create a new customer, update customer, create false rental record...



**Solution**

**Step 1:** Add this line to the form, this method will create a token just like secret code and then put it as an hidden field to this form and also as a cookie on the user's computer.

![image-20210805160514450](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805160514450.png)

**Result**:

Inspect the form page we will see a hidden filed call `_RequestVerificationToken`. The value of this filed will also store at user computer as a cookie in an encrypted format.

![image-20210805160655328](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805160655328.png)

Go to `Resources` to see the cookie by the exact same name and the value is an encrypted format.

![image-20210805160833689](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805160833689.png)

This give user a token and when they POST the form, the server gonna get 2 values that are the hidden filed and the encrypted cookie and compare them if they match then that is a legit request otherwise it an attack. Because if the hacker redirect the user to the malicious page they don't have access to the hidden field to our form, because this hidden filed only exist when the user actually visit the form. Even if the hacker steal the cookie they still don't have access to the hidden filed.



**Step 2:** Add token validation on the server by add the `[ValidateAntiForgeryToken]` to the POST action of the form.

![image-20210805161343778](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805161343778.png)

**Result:**

Now let test it, we simulate a situation where an attacker doesn't have access to this hidden field value by changing it.

![image-20210805161447601](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805161447601.png)

Now press save, we will immediately get exception

![image-20210805161636184](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805161636184.png)



## Code review

We will fix the initial value for the new movie form

![image-20210805165818124](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805165818124.png)



The cause of this problem is we initialize a new Movie to prevent the Id is empty string ( which will cause validation error that the Id is empty string ) here:

```c#
public ActionResult New()
{
    var viewModel = new MovieFormViewModel()
    {
        Genres = _context.Genres.ToList(),
        Movie = new Movie()
    };

    return View("MovieForm",viewModel);
}
```

When we initialize a new Movie, all the required field will set the default value show on the text box because it's not null so it cannot be empty ( which we want to get rid of these default value because it not pretty ).



**Solution**
Add the not null properties to the `MovieFormViewModel` and make it nullable so it can be null and don't have default value.

**Step 1:** 

```c#
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using HyMovieRental.Models;

namespace HyMovieRental.ViewModels
{
    public class MovieFormViewModel
    {
        public IEnumerable<Genre> Genres { get; set; }

        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [DisplayName("Genre")]
        public byte? GenreId { get; set; }

        // The [Required] is for validation
        // The nullable is for not showing the default value on the text box
        [Required]
        [DisplayName("Release Date")]
        public DateTime? ReleaseDate { get; set; }

        [Required]
        [Range(1, 20)]
        [DisplayName("Number in Stock")]
        public byte? NumberInStock { get; set; }

        public string Title
        {
            get
            {
                return Id != 0 ? "Edit Movie" : "New Movie";
            }
        }

        public MovieFormViewModel()
        {
            Id = 0;
        }

        public MovieFormViewModel(Movie movie)
        {
            Id = movie.Id;
            Name = movie.Name;
            ReleaseDate = movie.ReleaseDate;
            GenreId = movie.GenreId;
            NumberInStock = movie.NumberInStock;
        }
    }
}
```



# Building RESTful Services with ASP.NET Web API

## What is a Web API

Quick review ASP.NET MVC architecture

![image-20210805183100587](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805183100587.png)

When a request arrive at out application, MVC framework handle that request to an action in controller. This action most of the time return a view which is parse by razor view engine and then eventually a HTML markup is return to the client. In this approach HTML markup is generated on the server and then return to the client.

There is a alternative way to generate html markup, we can generate it on the client. Instead of our action returning markup it can return **raw data**.

**Benefits of generating markup on the client** 

- Less server resources (improve scalability)
- Less bandwidth (improve performance)
- Support for a broad range of clients : like mobile and tablet apps.

These app simply call the endpoint, get the data and generate the view locally at their own. We call this endpoint **Data Services** or **Web APIs** because it's just return data not markup.

![image-20210805183454002](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805183454002.png)

API are not only for building mobile and tablet apps, other website can consume our Web APIs and build new functionality for example we can use API of YouTube, Facebook, Twitter.. we can merge their data with the data in our application and provide new experience to the user.

These services are not only for getting data, it can have services for modifying data like adding a customer...

![image-20210805183622778](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805183622778.png)



Framework we use to build these services is call **ASP.NET Web API**, follow the same architecture principle with ASP.NET MVC like: routing, controller ,action, action result... In ASP.NET Core, Microsoft has merge ASP.NET Web API and ASP.NET MVC into one framework.

**Example:**

Instead of generating HTML markup for the list of customer, we can expose a web API to return data ( list of customer ) and then use `Jquery plug-in : DataTable` to generate this table on the client. 

![image-20210805184324694](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805184324694.png)



## RESTful Convention

This is standard convention of RESTful (Representational State Transfer).

![image-20210805184517753](https://raw.githubusercontent.com/luanhytran/img/master/image-20210805184517753.png)



## Building an API

**Step 1:** Create a folder name `Api` in `Controller` folder and add an API controller in there

![image-20210806002502477](https://raw.githubusercontent.com/luanhytran/img/master/image-20210806002502477.png)



**Step 2:** 

Install these 2 NuGet package 

- `Microsoft.AspNet.WebApi.WebHost`
- `Microsoft.AspNet.WebApi.Client`

Then do the following instruction

```
The Global.asax.cs file in the project may require additional changes to enable ASP.NET Web API.
 
1. Add the following namespace references:
 
    using System.Web.Http;
    using System.Web.Routing;
 
2. If the code does not already define an Application_Start method, add the following method:
 
    protected void Application_Start()
    {
    }
 
3. Add the following lines to the beginning of the Application_Start method:
 
    GlobalConfiguration.Configure(WebApiConfig.Register);
```



**Now you can write your action in API controller.**

**Example:**

```c#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
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
        public IEnumerable<Customer> GetCustomers()
        {
            return _context.Customers.ToList();
        }

        // GET /api/customers/1
        public Customer GetCustomers(int id)
        {
            var customer = _context.Customers.SingleOrDefault(c => c.Id == id);

            if (customer == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return customer;
        }

        // POST /api/customers
        // By convention when we create new resource, then we return that resource to the client
        // because that the same resource but with Id generated from the server
        [HttpPost]
        public Customer CreateCustomer(Customer customer)
        {
            // ASP.NET API framework will will auto initialize the customer info in the request body
            // to this customer object in the parameter

            if (!ModelState.IsValid)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            _context.Customers.Add(customer);
            _context.SaveChanges();

            // At this point the Id property of the customer will be set 
            // base on the id generated from the db, now we return this object that have Id property
            return customer;
        }

        // PUT api/customers/1
        // We can either return object or void
        [HttpPut]
        public void UpdateCustomer(int id, Customer customer)
        {
            if (!ModelState.IsValid)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var customerInDb = _context.Customers.SingleOrDefault(c => c.Id == id);

            if (customerInDb == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            // We also can use auto mapper for this mapping operation
            customerInDb.Name = customer.Name;
            customerInDb.Birthdate = customer.Birthdate;
            customerInDb.MembershipTypeId = customer.MembershipTypeId;
            customerInDb.IsSubscribedToNewsLetter = customer.IsSubscribedToNewsLetter;

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

```



## Testing API

If we enter the address `https://localhost:44389/api/customers` . You can see the list of customer return as xml.

![image-20210806010218892](https://raw.githubusercontent.com/luanhytran/img/master/image-20210806010218892.png)

ASP.NET Web API has what we call a **Media Formatter** , when we return a list of customer it will be format into what the format the client ask: XML or JSON.

![image-20210806010304019](https://raw.githubusercontent.com/luanhytran/img/master/image-20210806010304019.png)

We can inspect and see the default return format is XML

![image-20210806011330210](https://raw.githubusercontent.com/luanhytran/img/master/image-20210806011330210.png)



**Test API** 

**Step 1:** Install Tabbed Postman - REST Client chrome extension to the browser.

**Step 2:** Enter the API address and press Send. 

![image-20210806011813969](https://raw.githubusercontent.com/luanhytran/img/master/image-20210806011813969.png)

**Note:** most of the time we will use JSON because it's native to our JavaScript code. XML is mostly use in larger organization like government which are always behind modern technology. JSON format is more lightweight because don't have noisy opening, closing tag.



**Step 3:** to PUT or POST a object we need to set the data type that sent request to the server. 4 and 5 is the step we set the content type from plain text to JSON because our application doesn't support plain text for request data.

![image-20210806012828604](https://raw.githubusercontent.com/luanhytran/img/master/image-20210806012828604.png)

 

## DTO (Data Transfer Objects)

Our API should never receive or return domain objects because it will cause some issue

![image-20210806105315462](https://raw.githubusercontent.com/luanhytran/img/master/image-20210806105315462.png)



**Issue 1** 

In [Building an API](##building-an-api) and [Testing API](##testing-api) we have build a API but there a couple issue with this design. Our API receive and return an objects (Customer) which is a part of domain model of our application, the domain model may change frequently in the future to add new features. These changes can break existing client that are depending on the objects (Customer). For example, if you remove or rename a property of that objects, this can impact on the client that depending on that property. 

So basic we need to make a contract of this API as stable as possible, doesn't mean that this contract never could change but because it's a public contract, it should be changes in a slower pace than the domain objects.



**DTO (Data Transfer Objects)**

To solve this issue we need a different model which is call **Data Transfer Objects**.

![image-20210806104946817](https://raw.githubusercontent.com/luanhytran/img/master/image-20210806104946817.png)

DTO is plain data structure and is use to transfer data from the client to the server or vice versa. By creating DTO, it reduce our change of our API breaking as we refactor our domain model. We should remember the change in DTO can be costly so when we need to change them we should plan a prober strategy. 



**Issue 2**

When we using domain object we are opening a security hole in our application, the hacker can easily pass additional data in the JSON and it will be map into our domain object. 

What if a property should never be updated, hacker can easily bypass this. But if you use **DTO**  you can simply exclude  the property that can't be updated.



**Create DTO**

**Step 1:** Create folder name `DTOs` in the root folder

![image-20210806105812700](https://raw.githubusercontent.com/luanhytran/img/master/image-20210806105812700.png)

**Step 2:** 

Exclude all the property that can't be change like the related domain model/ navigation property like this

![image-20210806105940623](https://raw.githubusercontent.com/luanhytran/img/master/image-20210806105940623.png)

This create a dependency of our `CustomerDto` to the `MembershipType` domain model, when we change this domain model it can impact our Dto. We either use primitive types like int, string, byte... or Custom DTO instead for example create a `MembershipTypeDto`.

**Step 3:** delete `DisplayName` or `Display` attribute because we only use it to display name and nothing to work with DTO.

![image-20210806110409897](https://raw.githubusercontent.com/luanhytran/img/master/image-20210806110409897.png)

**Step 4:** 

- In the API controller, anywhere return a domain model, we need to map it to DTO first.
- And also in the method we modify domain model like create, update... we need to **map** DTO property back to domain model objects. 

![image-20210806110546993](https://raw.githubusercontent.com/luanhytran/img/master/image-20210806110546993.png)

**Note:** We use [Auto Mapper](##auto-mapper) to implement the map operation.



## `AutoMapper`

**Step 1:** 

Open Package Manager Console and install `AutoMapper`

```c#
install-package automapper -version:4.1
```

**Step 2:** 

Go to `App_Start` folder and add a new class call `MappingProfile` , this class will be configuring our mapping profile.

```c#
using AutoMapper;
using HyMovieRental.Dtos;
using HyMovieRental.Models;

namespace HyMovieRental.App_Start
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // The generic take 2 param, 1 is source type and other is target type
            Mapper.CreateMap<Customer, CustomerDto>();

            // .ForMember(...) purpose is to prevent from updating the existing customer Id to another Id
            // it's mission is simply ignore the Id property when you send PUT request to update the customer
            Mapper.CreateMap<CustomerDto, Customer>().ForMember(c => c.Id, opt => opt.Ignore());
        }
    }
}
```

When we call these method `AutoMapper` use reflection to scan these type, they find properties and map base on their name. So this is why we call `AutoMapper` is convention-based mapper tool because it use property as a convention to map object.

**Step 3:**

Open `Global.asax.cs` and add `AutoMapper` profile `Mapper.Initialize(c=>c.AddProfile<MappingProfile>());`

```c#
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Http;
using AutoMapper;
using HyMovieRental.App_Start;

namespace HyMovieRental
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Add AutoMapper profile
            Mapper.Initialize(c=>c.AddProfile<MappingProfile>());
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}

```



**AutoMapper in action**

```c#
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
```



**Note:** `AutoMapper` have a few feature that you might find useful in certain situation

- If property name don't match we can override the default convention
- Exclude some property from mapping 
- Create custom mapping class

To learn more about these feature please read more about `AutoMapper` documents.



## Using Camel Notation for JSON 

In ASP.NET Web API default JSON object is return in pascal case which is a bit ugly when JavaScript consume it when JavaScript use camel case.

![image-20210806154117260](https://raw.githubusercontent.com/luanhytran/img/master/image-20210806154117260.png)

If we consume object with pascal case in JavaScript our code is a little bit ugly. So we need configure web API to return JSON object using camel case.

**Config JSON object return using camel case**

Go to `App_Start` folder and open `WebApiConfig.cs` and add code like below 

```c#
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HyMovieRental
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var settings = config.Formatters.JsonFormatter.SerializerSettings;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.Formatting = Formatting.Indented;

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
```

**Result:**

![image-20210806154931039](https://raw.githubusercontent.com/luanhytran/img/master/image-20210806154931039.png)



## IHttpActionResult

In RESTful convention, when we create a resource, the status code should be `201` or `Created` . In [Building an API](##building-an-api) the API we build, when create resource it return status code is `200` which is not match the RESTful convention. 

We use `IHttpActionResult` to refactor our API to return the RESTful convention status code. 



**Refactor `HttpPost` to meet RESTful convention**

 From this 

```c#
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

    // At this point the Id property of the customer will be set 
    // base on the id generated from the db, now we the customerDto object that have Id property
    customerDto.Id = customer.Id;

    return customerDto;
}
```

To this

```c#
[HttpPost]public IHttpActionResult CreateCustomer(CustomerDto customerDto){    if (!ModelState.IsValid)        // this method implement IHttpActionResult         return BadRequest();    var customer = Mapper.Map<CustomerDto, Customer>(customerDto);    _context.Customers.Add(customer);    _context.SaveChanges();    customerDto.Id = customer.Id;    // As part of RESTful convention, we return the URI of the newly created resource to the client    // For example if our customer Id is 10 then the URI look like this api/customers/10    return Created(new Uri(Request.RequestUri + "/" + customer.Id), customerDto);}
```

**Result**:

*Before*

![image-20210806161401476](https://raw.githubusercontent.com/luanhytran/img/master/image-20210806161401476.png)

*After*

![image-20210806161602875](https://raw.githubusercontent.com/luanhytran/img/master/image-20210806161602875.png)

And when you look at Header, that's the URI of the newly created customer.

![image-20210806161738457](https://raw.githubusercontent.com/luanhytran/img/master/image-20210806161738457.png)



**Refactor GET method**

```c#
// GET /api/customers/1public IHttpActionResult GetCustomers(int id){    var customer = _context.Customers.SingleOrDefault(c => c.Id == id);    if (customer == null)        return NotFound();    // return status code 200 with customer object    return Ok(Mapper.Map<Customer,CustomerDto>(customer));}
```



**Problem:** In `CustomerDto` we need to comment out this attribute otherwise we gonna have an exception.

![image-20210806161121757](https://raw.githubusercontent.com/luanhytran/img/master/image-20210806161121757.png)

*Explanation*

![image-20210806160949558](https://raw.githubusercontent.com/luanhytran/img/master/image-20210806160949558.png)

Because we casting the `.ObjectInstance` to `Customer`, but if `Min18YearsIfMember` applied to 1 of the member of `CustomerDto` we gonna get exception. Currently we have different endpoint:

- API endpoint

- Normal MVC endpoint

If we want to go with the API approach we should drop all MVC action and modify our form to POST to API endpoint and cast the `.ObjectInstance` to `CustomerDto`. 







