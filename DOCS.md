# Server-side

## Authentication and Authorization

We use this option for Authentication.![image-20210827085637160](https://raw.githubusercontent.com/luanhytran/img/master/image-20210827085637160.png)

### What is ASP.NET Identity?

ASP.NET Identity used to be call ASP.NET Membership. It have been re-design over the years to solve issues and the outcome of this redesign is ASP.NET Identity.

![image-20210827090812265](https://raw.githubusercontent.com/luanhytran/img/master/image-20210827090812265.png)

In your project, you see 3 assemblies , these represent ASP.NET Identity framework

![image-20210827090547586](https://raw.githubusercontent.com/luanhytran/img/master/image-20210827090547586.png)

ASP.NET Identity has a number of Domain class like `IdentityUser` , `Role` and provide some simple API like `UserManager` , `RoleManager` , `SignInManager`  to work with these Domain classes. These classes internally talk to another group of classes like `UserStore` and `RoleStore` which represent the Persistence store for ASP.NET Identity.

![image-20210827105810690](https://raw.githubusercontent.com/luanhytran/img/master/image-20210827105810690.png)

ASP.NET Identity provide an implementation of this Persistence store using Entity Framework and Relational SQL Database. But you can plug in your own implementation of persistence store like NoSQL data store.

Back to the first time we create our first migration we got all these tables like `AspNetRoles` , `AspNetUserRoles` , `AspNetUsers` ...  These table are generated base on the Domain model of ASP.NET Identity framework. 

![image-20210827110227909](https://raw.githubusercontent.com/luanhytran/img/master/image-20210827110227909.png)

In `IdentityModel.cs` , we have `ApplicationUser` class which derive from `IdentityUser` , also have `ApplicationDbContext` derive from `IdentityDbContext` .

![image-20210827111111466](https://raw.githubusercontent.com/luanhytran/img/master/image-20210827111111466.png)

Both the classes orange underlined are part of ASP.NET Identity framework and that's why the first time we create the migration, we got these tables like `AspNetRoles` , `AspNetUserRoles` , `AspNetUsers`... as part of our migration.

`AccountController.cs` expose some action like Register, Login, Logout... let's take a look at Register acion.

![image-20210827111724022](https://raw.githubusercontent.com/luanhytran/img/master/image-20210827111724022.png)

`UserManager` is a part of API that we use to work with users, it have method for creating a user, get a user, removing a user... 

We can set the register action to have account confirmation by uncomment the code below.

![image-20210827111856154](https://raw.githubusercontent.com/luanhytran/img/master/image-20210827111856154.png)

### Restrict Access with Authorization

We have this attribute, it's like a filter to apply on action and it will be call by the MVC framework before and after that action or it's result are executed.

![image-20210827112955593](https://raw.githubusercontent.com/luanhytran/img/master/image-20210827112955593.png)

Before the action is executed, this attribute will check  the current user is logged in or not, if not it will redirect the user to the login page. 

**Example:**

We apply this to the `Index` action of the customer controller, then when we go to the customers page, if you haven't login it will redirect you to the login page, and if you logged in it will redirect you back to the `ReturnUrl` with the orange underlined, `%2F` is the URL Encoded value of the Forward Slash `/`.

![image-20210827150802173](https://raw.githubusercontent.com/luanhytran/img/master/image-20210827150802173.png)

We can apply this attribute here so that all the action in this controller will have to authorize in order to access.

![image-20210827151202110](https://raw.githubusercontent.com/luanhytran/img/master/image-20210827151202110.png)

**Apply filter globally** 

Because most of our movie management action need to be authorize in order to use, so we will apply the authorize attribute globally.

Go to `FilterConfig.cs` in `App_Start` folder, in here we already have one global filter call `HandleErrorAttribute` .

```c#
public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
```

This filter redirect the user to an error page when an action throw an exception. Now we going to add the authorize attribute globally here.

```c#
public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add((new AuthorizeAttribute()));
        }
```

**Allow Anonymous**

After apply the authorize attribute globally, it will be very strict, we can't even go to the home page without need to log in. So we will use `[AllowAnonymous]` for the Home Controller because we want this page don't need to authorize but still can go to it.

![image-20210827152710859](https://raw.githubusercontent.com/luanhytran/img/master/image-20210827152710859.png)

### Seeding Users and Roles

We going to restrict movie management operation to Store Manager, first we create this role in our application and more importantly when we deploy our application, we should have at least one user assigned to this role. This way the Store Manager can enter movie and delegate other task to the Staff.

**Step 1:** Register a guest user or staff user

![image-20210827162414429](https://raw.githubusercontent.com/luanhytran/img/master/image-20210827162414429.png)

**Step 2:** Create Store Manager role and add one user to this role

Open Account Controller and go to Register method, we going to modify the logic of this action and assign any new user to the Store Manager role to have at least one user that have this role. After that we will delete this logic and set it back to normal. The `//Temp code` section implement this logic.

```c#
// POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //Temp code
                    var roleStore = new RoleStore<IdentityRole>(new ApplicationDbContext());
                    var roleManager = new RoleManager<IdentityRole>(roleStore);

                    // Create new role, we don't name this role StoreManager, the best practice is to name the role with the actual permission
                    // because later on as our application grow, we will not remember what privilege all these roles have
                    await roleManager.CreateAsync(new IdentityRole("CanManageMovies")); 
                    await UserManager.AddToRoleAsync(user.Id, "CanManageMovies");

                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    
                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
```

Now register a new user with admin name, now we have a user with Store Manager role that Can Manage Movies. After register successfully we removed the `//Temp code` section.

**Step 3:** Seed users

We create new migration call `SeedUsers` and write SQL query for creating seed data in that migration, after done code like below, remember to delete the corresponding data in the database then we `update-database`. These SQL queries can be generate by follow this https://dzone.com/articles/generate-database-scripts-with-data-in-sql-server

```c#
namespace HyMovieRental.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SeedUsers : DbMigration
    {
        public override void Up()
        {
            Sql(@"
INSERT [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName]) VALUES (N'78a76a08-753f-4fa3-b87c-fa5b463d90e4', N'admin@hymovierental.com', 0, N'ACkQnuq9a6LWmir63R/Uwn18WsyvINSGiT8ezvYRwT5onjOQlrSQVFseNMGBPlNHsw==', N'eb59b4f5-7d22-4f9a-8e4a-f86d61e50008', NULL, 0, 0, NULL, 1, 0, N'admin@hymovierental.com')
INSERT [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName]) VALUES (N'c6fb057e-39ae-486e-988e-a7b3c26e2078', N'guest@hymovierental.com', 0, N'AFeZlcdEileYS3yuRWDIXOtPFLoRk9LRmBg5MFNjvvbN2FVpGljGvz/en5OUU+8HdA==', N'446af46f-830f-429b-9369-c106213adf1b', NULL, 0, 0, NULL, 1, 0, N'guest@hymovierental.com')

INSERT [dbo].[AspNetRoles] ([Id], [Name]) VALUES (N'392da8a2-f135-45bb-9e42-ec3ac37b50c4', N'CanManageMovies')

INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'78a76a08-753f-4fa3-b87c-fa5b463d90e4', N'392da8a2-f135-45bb-9e42-ec3ac37b50c4')
");
        }
        
        public override void Down()
        {
        }
    }
}
```

The beauty of this approach is that if you run this migration on another database, let say our testing or production database we will have the exact same setup so this is the proper way to seed our database with users and role.

ASP.NET website have a tutorial the show us how to seed users and role too but that is a poor way to do this. Here is a example:

![image-20210827172020394](https://raw.githubusercontent.com/luanhytran/img/master/image-20210827172020394.png)

This tutorial tell us to use the Seed method of the configuration class of code-first migration to add users and role to the database. Basically, when we run `update-database` this seed method will get executed. We shouldn't use this because you are not going to executed `update-database` on your production database. If you want to do this you have to change the connection string in `web.cofig ` and then run `update-database` . But this is very risky, because if you forgot to change the connection string back to your development database, you gonna screw your production database. 

### Restrict Access with Roles

It's a good practice to create view for user that have less privilege. In Movies view, we create `ReadOnlyList` view for staff and for persistence we rename `Index` to `List` . **We will restrict access in our API controller too**.

**Step 1:**

In Movies controller we use `User.IsInRole` to check if a user have a specific role

![image-20210827202837687](https://raw.githubusercontent.com/luanhytran/img/master/image-20210827202837687.png)

Then we will return the view corresponding to that role.

**Step 2:**

We still have bug, even though render the view for Staff, we can still create new Movies by go to the `/Movies/New` URL. So we specify which role can have access to the create, update, delete ... movie action.

![image-20210827211318095](https://raw.githubusercontent.com/luanhytran/img/master/image-20210827211318095.png)

### Adding more Profile Data when register

**Step 1:** Go to `IdentityModel.cs` and add the model you want to add here

![image-20210828113522958](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828113522958.png)

**Step 2:** Run migration to update that model in to the database

![image-20210828113556273](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828113556273.png)

**Step 3:** Go to Register View Model inside Account View Model and add the model here too

![image-20210828113905579](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828113905579.png)

**Step 4:**  Go to Register view to add the input filed for that model

![image-20210828113742422](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828113742422.png)

**Step 5:** Go to Account Controller and code the logic to bind this model from the request to database

![image-20210828114024946](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828114024946.png)

**Result:** 

![image-20210828114105930](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828114105930.png)

### OAuth

Facebook and another external authentication providers like Google, Twitter... use an **authentication protocol** call OAuth (Open Authorization).  

**How it work?**

Let say a staff member want to login to our application with his Facebook account. First of all we need to register our application with Facebook to create some kind of partnership, Facebook will give us an API key and a Secret kinda like username and password. We use this to talk to Facebook under the hood.

When the staff click on Facebook to Login, we will redirect him to Facebook and we'll use our API key and Secret so Facebook know this request is coming from our application. To prevent malicious user from finding our Secret, we use HTTPS (Secure HTTP protocol) so the data exchange between these parties will be encrypted and no one can intercept this communication.

Now the staff are on Facebook, he login with his user and password, on our application we don't know and we don't care what his Facebook credential are. Once he login to Facebook, it tell him that our application want to access some basic information about his account. Of course we can ask for more like the list of friend, photo... 

![image-20210828115141569](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828115141569.png)

After that Facebook will redirect the user back to our application with a authorization token, this token tell our application that Facebook successfully authenticated this user. 

![image-20210828115158035](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828115158035.png)

Now on our application, we get this token and send it back with our API Key and Secret. 

![image-20210828115329894](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828115329894.png)

We do this because a hacker may send random authorization token to our application so we need to verify that it really came from Facebook, that's why we send it back and tell Facebook and say "Hey, did you really send me this authorization token", Facebook said "Yes, I did" and then it will give us an access token. 

With this access token we can access some part of the user profile, the part we have permission to access .

![image-20210828170418769](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828170418769.png)

**Mechanic**

In order to use social login there are 2 step we need to make

![image-20210828170615466](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828170615466.png)

- We enable SSL so the communication between us and Facebook will be a secure chanel.
- We need to register our application with Facebook to get our application ID and a Secret.

### Social Logins

**Enable SSL**

**Step 1:** Enable SSL

select project then press F4, here we set SSL Enabled to True.

![image-20210828172809925](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828172809925.png)

**Step 2:** Change our local URL to SSL URL

Copy the SSL URL in the above image

Then go to properties of project, paste that URL in here

![image-20210828173007745](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828173007745.png)

Now go to this URL 

![image-20210828173552768](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828173552768.png)

The reason we have the red icon is because we don't have a proper certificate, we are using a dummy certificate. If we deploy our application to a live web server, we need to get a official certificate from your web hosting company.

**Step 3:**

Even though we are at HTTPS URL `https://localhost:44389/` but we still can access to HTTP URL version `http://localhost:61097/`. So we need to prevent this.

Go to `FilterConfig.cs` in `App_Start` folder add this.

```c#
filters.Add(new RequireHttpsAttribute());
```

With this our application endpoint will no longer be available on HTTP channel. 

![image-20210828184945795](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828184945795.png)

**Register our app with Facebook**

Step 1: Do as this video

https://www.youtube.com/watch?v=lUXmAixYiU8

Go to `Startup.Auth.cs` in `App_Start` folder, you see here we have this boilerplate for using Facebook authentication. 

![image-20210828193504142](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828193504142.png)

We uncomment this and paste our API Key and Secret here.

**Result:**

![image-20210828195431878](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828195431878.png)

This Facebook login button is auto done by MVC framework when you uncomment the Facebook Authentication in the previous step. If you enable Google Authentication you will see another button here. 

Step 2: 

![image-20210828210637873](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828210637873.png)

But we still have bug, after the above step we will insert our email. Then press Register we will have error.

![image-20210828210718588](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828210718588.png)

![image-20210828210745088](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828210745088.png)

This is because we add a additional model call DriverLicense when register in [Adding  more Profile Data when register](###Adding-more-Profile-Data-when-register) .

First we add that model in here

![image-20210828211401806](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828211401806.png)

Then we go to this view and create input field for this model

![image-20210828211315266](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828211315266.png)

Last, we code the logic to add this model to database when login using external OAuth login

![image-20210828211504417](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828211504417.png)

**Result:**

- `AspNetUsers `table

![image-20210828211554522](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828211554522.png)

- `AspNetUserLogins` table

![image-20210828211607065](https://raw.githubusercontent.com/luanhytran/img/master/image-20210828211607065.png)

### Additional Reading  

**Thinktecture** 

If you want to give your admin users the ability to manage users/roles, you’re not going  to use code-first migrations to do that. You need to provide them a user interface.  

Thinktecture does that for you:

 http://www.hanselman.com/blog/ThinktectureIdentityManagerAsAReplacementForTheASPNETWebSiteAdministrationTool.aspx 

**Using Organisational Accounts with ADFS** 

In large organizations, you may want to create single sign-on between your internal apps  and cloud apps using Active Directory Federation Services. 

 http://www.cloudidentity.com/blog/2014/02/12/use-the-on-premises-organizational-authentication-option-adfs-with-asp-net-in-visual-studio-2013/

## Performance Optimization

**Rules of thumb**

- Do not sacrifice the maintainability of your code to premature optimization. 

- Be realistic and think like an “engineer”.  

- Be pragmatic and ensure your efforts have observable results and give value.  

And remember: **premature optimization is the root of all evils.**

**How to optimize?**

Performance optimization is a very broad area and there's no magic fix for it but we have usually optimize in 3 key area below.

**Three-tier Architecture**

![image-20210829082218250](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829082218250.png)

Most web application follow this architecture. Tier is where your code run.

- Data
  - Where our database and queries code.
- Application (Middle Tier)
  - Which is where we have our web server hosting our application.
- Client
  - Which is client computer.

Most of the performance bottlenecks are in the Data tier. So optimize at this tier will have the most gain compare to other tier.

![image-20210829082612440](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829082612440.png)

**Optimization Rules**

- Do not sacrifice the maintainability of your code to premature optimization.
- Be realistic and think like an "engineer".
- Be pragmatic and ensure your efforts have observable results and give value.

### Data Tier

Performance problem in this tier are often of database schema and queries.

**Schema issues**

Make sure all of your database has these when you work with legacy database (Database First):

- Every table must have a primary key. 
- Tables should have relationships. 
- Put indexes on columns where you filter records on. But remember: too many  indexes can have an adverse impact on the performance.  
- Avoid Entity-Attribute-Value (EAV) pattern.

Your first point in optimization is fix these issues in your database schema.

In our application, because we use EF Code First, our migrations auto add PK and Indexes in our tables so we don't have to worry about it in most part.

![image-20210829122719194](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829122719194.png)

Another schema related issues is to avoid this pattern when design database.

![image-20210829122948079](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829122948079.png)

Problems with this EAV pattern is:

- No O/RMs
- Long, gigantic queries
- Extremely slow performance

**Queries issues**

How you write queries can have significant impact on this performance, in our app all queries are generated by EF. These queries are fine but in certain situations, EF generate queries behind the scene is very complex.

So it good to keep an eye on EF queries, if it too complex, it better to create a Store Procedure and write an optimize queries that have the same result and it absolutely nothing wrong about this. Every tool have it's strength and weaknesses.

Some method to Optimizing Queries when it slow:

- Keep an eye on EF queries using Glimpse. If a query is slow, use a stored  procedure. 
- Use Execution Plan in SQL Server to find performance bottlenecks in your  queries.
  - Which show how SQL Server execute your queries so you can see which part of the query have biggest cost and you can optimize it.

If after all your optimizations, you still have slow queries, consider creating a  denormalised “read” database for your queries. But remember, this comes with the cost  of maintaining two databases in sync. A simpler approach is to use caching.
#### Glimpse

This is a tool to get real time diagnostic inside our app.

Install Glimpse

```
install-package glimpse.mvc5
install-package glimpse.ef6
```

To use glimpse, run your app and go to this URL: `your_url/glimpse.axd`

```po
https://localhost:44389/glimpse.axd
```

This dashboard is only accessible locally for security reason, if you want to use glimpse to a app that deploy for production, you need to do additional step and you can read about it at glimpse documentation.

![image-20210829124837794](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829124837794.png)

Now turn glimpse on by click `Turn Glimpse On` , basically glimpse put a cookie on  your machine and this cookie is sent back and forth with each request.

![image-20210829125343249](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829125343249.png)

On the server, Glimpse have a module that intercept every request. If it find your cookie sent to it, it will render diagnostic information for you.

Let test it, now click on customers page, you will see we have three tabs: HTTP, HOST, AJAX.

![image-20210829125404776](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829125404776.png)

If you hover in each of these tabs you will see more details about it.

![image-20210829125604431](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829125604431.png)

Click on Glimpse icon, you can see even more information.![image-20210829125945644](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829125945644.png)

![image-20210829130104065](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829130104065.png)

Now in SQL tab, you see we have one connection database and one queries and you can see the actual queries that EF executed on our database. Now we see whether eager loading or lazy loading have less connections and queries.

![image-20210829142711145](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829142711145.png)

**Lazy Loading**

Instead of loading a bunch of object together, object are loaded only if our code touches one or more of the navigation properties of an object.

This is bad because in web app we should know ahead of time what data we are going to return to the client. With lazy loading, we have multiple roundtrip to the database and this will have significant impact on the performance of our app.

![image-20210829143308441](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829143308441.png)

To demo lazy loading, we add the virtual keyword to that navigation property.

![image-20210829142958676](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829142958676.png)

Then we pass the customer list to the index view, as you can see we are not using the `.Include()` method which is for eager loading. 

![image-20210829143451958](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829143451958.png)

Now in the index view we render the customer name and customer membership type ( this membership should be Include in eager loading method, but it doesn't in lazy loading).

Back to the browser and refresh, we see that the Connections and Queries tab in Glimpse jump from 1 to 3.

![image-20210829143708450](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829143708450.png)

Here's our first queries which is basically select multiple column from the Customers table.

![image-20210829143809409](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829143809409.png)

Then we have 2 more queries to get the membership type of each customer

![image-20210829143945448](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829143945448.png)

![image-20210829143958573](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829143958573.png)

In the eager loading method, we didn't have these 2 queries. So what happen? 

When execute this line, EF query the database to get the list of customers but their membership type is not loaded because it's not eager loading. 

![image-20210829144100455](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829144100455.png)

But in the view, when we iterating over each customer, when we touch the Membership Type property. EF is going to query the database again to get the membership type for that customer, so it will open a new connection, execute the query  then close the connection. The same flow is repeat for each customer in the list.

![image-20210829144205673](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829144205673.png)

So if you have N Customers in the database, we have 1 query to get the customers and N queries to get the membership type for each customer, this is the worst case scenario.

![image-20210829144408569](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829144408569.png)

**N + 1 issue**

This is what we call **N + 1 issue (because of lazy loading)** . This is a question sometime come in job interview. You should avoid lazy loading in web app at all times. 

If you find an EF query slow, then you can optimize by creating a Store Procedure and call it via `DbContext`  . 

### Application Tier

Assume that our database schema and queries are in the right shape and form. Now we go to the middle tier (Application Tier). There are number of technique that we can optimize this tier.

![image-20210829145355982](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829145355982.png)

#### Output Cache

- On pages where you have costly queries on data that doesn’t change frequently,  use **OutputCache** to cache the rendered HTML.

The main purpose of using Output Caching is to dramatically improve the performance of an ASP.NET MVC Application. It enables us to cache the content returned by any controller method so that the same content does not need to be generated each time the same controller method is invoked. Output Caching has huge advantages, such as it reduces server round trips, reduces database server round trips, reduces network traffic etc.

Implement: [Output Caching in MVC (c-sharpcorner.com)](https://www.c-sharpcorner.com/UploadFile/abhikumarvatsa/output-caching-in-mvc/)

#### Data Cache

- You can also store the results of the query in cache (using **MemoryCache**), but  use this approach only in actions that are used for displaying data, not modifying  it.

Some time we may want to cache a piece of data, not HTML . For example you want to we want to cache a list of Genres we code like this. So the first time someone hit this endpoint, we'll get the genre from the db and then store it in the cache and all the subsequent request will get data from the cache.

![image-20210829193421732](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829193421732.png)

Only use this technique if you really need to and only after you have done Performance Profiling ( Glimpse is the tool that have Performance Profiling).

Don't use this approach blindly to improve the performance of your app. Unfortunately, many tutorials tell you that you should use caching for frequently access data that don't change often. This may sound like a good idea but this is a poor advice because 2 reasons:

- Blindly storing data in the cache we will increase memory consumption of our app.
- It will lead to unnecessary complex in both architecture level and code level especially when you working with EF.

Only use Data Cache to display data, not modifying data.

**Use Data Caching with Expensive Queries**  

In the demo, I stored the list of genres in the cache. Getting the list of genres would  issue a **“SELECT * FROM Genres”** query to the database, and given that this is a  simple and fast query, caching the result would just waste server’s resources.  

Use data caching for complex queries over large tables that take several seconds to  execute. This way, you can argue that using additional memory on the server can be  better (but not necessarily) than querying your database several times. You need to  profile **before and after** your optimization to ensure your assumptions are correct and  are not based on some theory you read in a book or tutorial.

**MemoryCache.Add()**

If you want to have more control over the objects you put in their cache, it’s better to use  the Add method of MemoryCache class.

```c#
MemoryCache.Default.Add(
				new	CacheItem(“Key”,	value),	
				cacheItemPolicy);
```

As you see, the second argument to this method is a **cacheItemPolicy** object. With this  object you can set the expiration date/time (both absolute and sliding) and you can also  register callbacks to be called when the item is removed from the cache.

You can read more about this class on MSDN:

[MemoryCache.Add(CacheItem, CacheItemPolicy) Method (System.Runtime.Caching) | Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.caching.memorycache.add?redirectedfrom=MSDN&view=dotnet-plat-ext-5.0#overloads)

#### Async

In web application when we receive a request, runtime allocate a thread to handle that request. When it come to I/O-bound operation like accessing a db or file or network bound operation like calling a remote service, there always have delay. 

![image-20210829191157801](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829191157801.png)

For example, in case accessing a db this thread has to wait until the db execute the query and return the result. 

Since we have limited amount of memory and thread available in a web server, if we have a lot of concurrent user, at some point you may run out of thread because many of this thread are waiting for I/O-bound or Network-bound operation to complete. In this case we either need to add more hardware memory or more machine or we can **use Async and Await**.

![image-20210829191411836](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829191411836.png)

When we use Async, our thread are no longer waiting for the db to return the result. it will be release so it can serve other requests.  So this does not improve the performance of our application because it does not reduce the average response time. We still have to wait for the db to return the result. But during that wait period, our thread can be better utilize to serve more concurrent users.

![image-20210829191809795](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829191809795.png)

This means Better scalability, not performance. But even when it come to scalability, Async and Await do not always improve the scalability of your app.

Look at this diagram, imagine this is a pipe, we pour water from the top to the bottom. Imagine on the top we have a bunch of HTTP requests coming to a web server

![image-20210829192114117](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829192114117.png)

When using Async and Await it like we opening the top of this pipe a little bit so we can pour more water or more requests. But look at the bottom, it still the same size. So if you using a single instance of SQL Server, that going to be our bottleneck. No matter how many more request we can serve in IIS, eventually all this request will still have to wait for the database server to response.

![image-20210829192024118](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829192024118.png)

On the other side, if we use SQL Cluster or NoSQL (like Mongo, Reven which is design for scalability) or Azure, the bottle of the pipe will always expand. This is when you have true scalability.

![image-20210829192631873](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829192631873.png)

-> Async doesn't improve the performance of your application, it can only improve this scalability even that you are not using single instance of SQL Server.

#### Release Builds

Another simple optimization technique for Application Tier is using Release Builds.

In VS, we got this dropdown list, in Debug mode when you compile your application the compiler add additional  data in the assemblies which is use for debugging. But when it's come to deployment we don't need this stuff, so we should use the Release build which will slightly faster and smaller assemblies.

![image-20210829194705932](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829194705932.png)

#### Disabling Session

This  is the last optimization technique in Application Tier. 

![image-20210829202630529](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829202630529.png)

We can use session to store temporary data during the user session. What wrong with using session? The more user you have, the more memory of the web server you going to use. **This kills scalability of your app**.

These day, we work on more cloud-base solution and this help the traffic to a web app increase exponentially.  

**To make our app scalable, you should make it Stateless**. Which mean a request comes in and get process and we're done. We don't maintain state, we don't use session.

**How to turn off session?**

Go to `Web.config` and add this line

![image-20210829203108809](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829203108809.png)

This is how we disable session state which improve the scalability and the performance of our app.

This is the 4 strategy to optimize performance for Application Tier, the last 2 is safe to use without needing to Performance Profiling because it simple and don't add complexity to our app. 

![image-20210829203157282](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829203157282.png)

### Client Tier

We have client and server, since the client and server machine are located in difference area, we should reduce the number of request from the client to the server and also the size of the response from.

We can apply these principle in certain area:

![image-20210829205203714](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829205203714.png)

- Put JS and CSS files in bundles.  

- Put the script bundles near the end of the  element. Modernizr is an  exception. It needs to be in the head.  

- Return small, lightweight DTOs from your APIs. 

- Render HTML markup on the client. That’s the case with single page  applications (SPA).  

- Compress images.  

- Use image sprites. This is beyond the scope of the course and you need to read  about them yourself. 

- Reduce the data you store in cookies because they’re sent back and forth with  every request.  

- Use content delivery networks (CDN). Again, beyond the scope of the course.  Implementations vary depending on where you host your application. 

### Chrome Developer Tools Tip

As we working with bundle and observing the total number of requests and the total response size, we have unreliable result. If that happen, that mean our browser are cache some of the requests. So we may switch to release mode and notice the bundle size are actually larger than before. Disable cache will be useful as we developing and debugging our app.

![image-20210829204845894](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829204845894.png)

## Building a Feature End-to-End Systematically

This section show a systematically approach to build any features or entire app end-to-end. We will add the ability to record Rentals.

#### Understanding the Problem

The first step to build software is understand what problem we are trying to solve.

**Example:** 

We want to extend our app by Add the ability to record rentals. Before start coding we need to understand how this use case work so we need to talk to the client or who we build this application for and ask them question to better understand the use case.

The customer come to the counter and give the movie to the staff member, this staff member identify the customer by looking them up in the app and then add each movie in the list of movies that customer gonna rent .

To implement this, it better to start from Back-end then to Front-end. As a software engineer, you should focus on the big picture first then the small detail later.

In the real-world, video rental store usually don't have output. 

![image-20210829232623971](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829232623971.png)

On the Back-end we need an action for Front-end to call later on. We can put this action in 1 of this 2 places, it depend on what you want to return to the client once the form is submitted.

![image-20210829232606818](https://raw.githubusercontent.com/luanhytran/img/master/image-20210829232606818.png)

Put action in API controller have advantage, that is we can have multiple Front-end type like mobile, SPA.. able to connect to this API.

#### Domain Modelling

We use UML to modelling the domain

![image-20210831105439646](https://raw.githubusercontent.com/luanhytran/img/master/image-20210831105439646.png)

#### Building the Simplest API

When it come to implementation, it best to start with the happy path. Forget about all the edge case and validation stuff at the beginning because you can easily get distracted and get stuck in an endless loop.

#### Adding the Details

#### Edge Cases

![image-20210831180504071](https://raw.githubusercontent.com/luanhytran/img/master/image-20210831180504071.png)

#### Building the Front-end

We use AJAX to submit the form the improve UX.

**Zencoding**

Install Web Essentials Plug-in we get this feature

![image-20210831183610591](https://raw.githubusercontent.com/luanhytran/img/master/image-20210831183610591.png)

#### Adding Auto-completion

**Install auto-completion library** 

After install then add it in `BundleConfig.cs`

```powershell
install-package Twitter.Typeahead
```

![image-20210831185107046](https://raw.githubusercontent.com/luanhytran/img/master/image-20210831185107046.png)



**Add CSS**

Create a CSS file call `typeahead.css` and add CSS from this https://twitter.github.io/typeahead.js/css/examples.css, this is the CSS file of https://twitter.github.io/typeahead.js/examples/ , we click View Page Source then click in the CSS source

```css
.tt-hint {
    color: #999
}

.tt-menu {
    width: 422px;
    margin: 12px 0;
    padding: 8px 0;
    background-color: #fff;
    border: 1px solid #ccc;
    border: 1px solid rgba(0, 0, 0, 0.2);
    -webkit-border-radius: 8px;
    -moz-border-radius: 8px;
    border-radius: 8px;
    -webkit-box-shadow: 0 5px 10px rgba(0,0,0,.2);
    -moz-box-shadow: 0 5px 10px rgba(0,0,0,.2);
    box-shadow: 0 5px 10px rgba(0,0,0,.2);
}

.tt-suggestion {
    padding: 3px 20px;
    font-size: 18px;
    line-height: 24px;
}

.tt-suggestion:hover {
    cursor: pointer;
    color: #fff;
    background-color: #0097cf;
}

.tt-suggestion.tt-cursor {
    color: #fff;
    background-color: #0097cf;
}

.tt-suggestion p {
    margin: 0;
}
```

Then add this CSS file that we created to bundle

![image-20210831192207503](https://raw.githubusercontent.com/luanhytran/img/master/image-20210831192207503.png)



**Add Js code**

Now go to the view we want to use this lib then add this template code from the lib docs, and change data correspond to our response from the server 

```javascript
var bestPictures = new Bloodhound({
  datumTokenizer: Bloodhound.tokenizers.obj.whitespace('value'),
  queryTokenizer: Bloodhound.tokenizers.whitespace,
  prefetch: '../data/films/post_1960.json',
  remote: {
    url: '../data/films/queries/%QUERY.json',
    wildcard: '%QUERY'
  }
});

$('#remote .typeahead').typeahead(null, {
  name: 'best-pictures',
  display: 'value',
  source: bestPictures
});
```

This is a example in our project, use typeahead to find the customer name with the customer API.

```javascript
@section scripts
{
    <script>
        $(document).ready(function () {
            // This is view model for the rental form, later we use this to post to the server
            var vm = {};

            var customers = new Bloodhound({
                datumTokenizer: Bloodhound.tokenizers.obj.whitespace('name'),
                queryTokenizer: Bloodhound.tokenizers.whitespace,
                remote: {
                    url: '/api/customers?queries=%QUERY',
                    wildcard: '%QUERY'
                }
            });

            $('#customer').typeahead({
                    minLength: 3,
                    highlight: true
                },
                {
                    name: 'customers',
                    display: 'name',
                    source: customers
                }).on("typeahead:select",
                // e: event, customer: the selected customer
                function (e, customer) {
                // logic for the select event of typeahead when we select a customer name
                vm.customerId = customer.id;
            });
        });
    </script>
}
```

Remember to add Id to refer the input element you want to apply

![image-20210831193642086](https://raw.githubusercontent.com/luanhytran/img/master/image-20210831193642086.png)

#### Updating the DOM

Create a place holder

```html
<ul id="movies" class="list-group"></ul>
```



jQuery code for updating the DOM

```javascript
var vm = {
    movieIds: []
};

$('#movie').typeahead({
    minLength: 3,
    highlight: true
},
                      {
    name: 'movies',
    display: 'name',
    source: movies
}).on("typeahead:select",
      function (e, movie) {
    $("#movies").append("<li>" + movie.name + "</li>");

    $("#movie").typeahead("val", "");

    vm.movieIds.push(movie.id);
});
```



#### Improving the Look and Feel



#### Filtering Records

Filter to show exactly the customer or movie have the same name in the suggest of typeahead.

The `queries` will be pass in by typeahead, you can see this parameter is specify in the remote > url of the jQuery code of typeahead at the page you apply.

```c#
 // GET api/movies
        public IHttpActionResult GetMovies(string queries = null)
        {
            var moviesQuery = _context.Movies.Include(m => m.Genre).Where(m=>m.NumberAvailable > 0);

            if (!String.IsNullOrWhiteSpace(queries))
                moviesQuery = moviesQuery.Where(m => m.Name.Contains(queries));

            var movieDto = moviesQuery.ToList().Select(Mapper.Map<Movie, MovieDto>);

            return Ok(movieDto);
        }
```



```c#
// GET /api/customers
        public IHttpActionResult GetCustomers(string queries = null)
        {
            // customersQuery is IQueryable type so we can filter customer by name exactly when using typeahead 
            var customersQuery = _context.Customers.Include(c=>c.MembershipType);

            if (!String.IsNullOrWhiteSpace(queries))
                customersQuery = customersQuery.Where(c => c.Name.Contains(queries));

            // Pass in .Select() a delegate and does the mapping
            // Map each Customer in the list to customerDto
            var customerDtos = customersQuery.ToList().Select(Mapper.Map<Customer,CustomerDto>);

            return Ok(customerDtos);
        }
```



#### Submitting the Form

Add id to the form element

![image-20210901054329163](https://raw.githubusercontent.com/luanhytran/img/master/image-20210901054329163.png)



Write jQuery code to handle the submit

```c#
// e: is the submit event
            $('#newRental').submit(function (e) {
                // we use AJAX, so we need to add this line to prevent submit as a traditional HTML form
                e.preventDefault();

                $.ajax({
                    url: "/api/newRentals",
                    method: "post",
                    data: vm
                })
                .done(function() {
                    console.log('done');
                })
                .fail(function() {

                });
            });
```



#### Displaying Toast Notifications

Install jQuery Plug-in Toastr

```powershell
install-package toastr
```



Then add it in bundle

![image-20210901055503866](https://raw.githubusercontent.com/luanhytran/img/master/image-20210901055503866.png)

![image-20210901060127389](https://raw.githubusercontent.com/luanhytran/img/master/image-20210901060127389.png)



Use it 

![image-20210901055602742](https://raw.githubusercontent.com/luanhytran/img/master/image-20210901055602742.png)



#### Implementing Client-side Validation

Add this bundle

![image-20210901154027501](https://raw.githubusercontent.com/luanhytran/img/master/image-20210901154027501.png)



We use standard HTML Validation Attribute and jQuery validation Plug-in understand this.





Add the validation plug-in in the jQuery code that handle the submit form event

```javascript
// submit handler and validate for the form
            $("#newRental").validate({
                submitHandler: function () {
                    // e: is the submit event
                    // we use AJAX, so we need to add this line to prevent submit as a traditional HTML form
                    e.preventDefault();

                    $.ajax({
                            url: "/api/newRentals",
                            method: "post",
                            data: vm
                        })
                        .done(function () {
                            toastr.success("Rentals successfully recorded.");
                        })
                        .fail(function () {
                            toastr.error("Something unexpected happened.");
                        });
                }
            });
```



Add CSS to style the input border to red and the error message to red

```css
.field-validation-error,
label.error
{
    color: red;
}

.input-validation-error,
input.error
{
    border: 2px solid red;
}
```

Result:

![image-20210901160155409](https://raw.githubusercontent.com/luanhytran/img/master/image-20210901160155409.png)



Create custom validator to handle the case user have to enter a valid name that exist in database. Add this code above the `$("#newRental").validate(...)`

```javascript
// specify the name for the custom validation attribute
            $.validator.addMethod("validCustomer",function () {
                // make sure our vm have customerId property and this property have numeric value != 0
                return vm.customerId && vm.customerId !== 0;
            },
                // error message
                "Please select a valid customer.");

            $.validator.addMethod("movieSelected",
                function () {
                    // everything with a value will be true, if this array empty then false
                    return vm.movieIds.length > 0; 
                }, "Please select at least one movie.");

            // submit handler and validate for the form
            var validator = $("#newRental").validate({
                rules: {
                    customer: { validCustomer: true },
                    movie: { movieSelected: true }
                },
                submitHandler: function () {
                    // e: is the submit event
                    // we use AJAX, so we need to add this line to prevent submit as a traditional HTML form
                    $.ajax({
                        url: "/api/newRentals",
                        method: "post",
                        data: vm
                    })
                        .done(function () {
                            toastr.success("Rentals successfully recorded.");
                            $("#customer").typeahead('val', '');
                            $("#movie").typeahead('val', '');
                            $("#movies").empty();
                            vm = { movieIds: [] };
                            validator.resetForm();
                        })
                        .fail(function () {
                            toastr.error("Something unexpected happened.");
                        });

                    return false;
                }
            });
```

Use that custom validator, jQuery validation plug-in look for custom attribute that start with `data-rule` . Add name="customer" and name="movie" to the inputs to have it trigger.

![image-20210901165130962](https://raw.githubusercontent.com/luanhytran/img/master/image-20210901165130962.png)

Result:

![image-20210901165216340](https://raw.githubusercontent.com/luanhytran/img/master/image-20210901165216340.png)

## Deployment

# Client-side

We use jQuery and AJAX to consume API we build

Basic jQuery: [jQuery Tutorial for Beginners: Nothing But the Goods - Impressive Webs](https://www.impressivewebs.com/jquery-tutorial-for-beginners/)

## Calling an API Using jQuery

Delete a customer using jQuery and AJAX

![image-20210825022235664](https://raw.githubusercontent.com/luanhytran/img/master/image-20210825022235664.png)

In this view, at the bottom we code like this

```javascript
@section scripts
{
    <script>
        $(document).ready(function() {
            $("#customers .js-delete").on("click",
                function () {
                    // the scope of this keyword can change in different callback function 
                    // so we need to reference to our button
                    var button = $(this);

                    if (confirm("Are you sure you want to delete this customer?")) {
                        $.ajax({
                            url: "/api/customers/" + button.attr("data-customer-id"),
                            method: "DELETE",
                            success: function () {
                                // use button variable to reference to the delete button in another callback function
                                button.parents("tr").remove();
                            }
                         });
                    }
                });
        });
    </script>
}
```

## Add dialog box

We use Bootbox.js to add bootstrap dialog box when deleting a customer 



Open Package Manager Console and run this

```powershell
install-package bootbox -version:4.3.0
```



Go to `AppStart` folder -> `BundleConfig.cs` to add reference to `Bootbox.js`

![image-20210825022950976](https://raw.githubusercontent.com/luanhytran/img/master/image-20210825022950976.png)

We not add the min version because with this bundle, we auto get min version when we compile the application in release mode.



Then apply Bootbox.js in our scripts 

```javascript
@section scripts
{
    <script>
        $(document).ready(function() {
            $("#customers .js-delete").on("click",
                function () {
                    // the scope of this keyword can change in different callback function 
                    // so we need to reference to our button
                    var button = $(this);

                    bootbox.confirm("Are you sure you want to delete this customer?",
                        function(result) {
                            if (result) {
                                $.ajax({
                                    url: "/api/customers/" + button.attr("data-customer-id"),
                                    method: "DELETE",
                                    success: function () {
                                        // use button variable to reference to the delete button in another callback function
                                        button.parents("tr").remove();
                                    }
                                });
                            }
                        });
                });
        });
    </script>
}
```



Now rebuild our project because we modified `BundleConfig.cs` and refresh the web page to see result.

![image-20210825024200761](https://raw.githubusercontent.com/luanhytran/img/master/image-20210825024200761.png)

## Optimizing jQuery Code

In our script for show dialog box when deleting customer, each customer will have each click function because of the `.js-delete` class in each customer's delete button.

This will cause memory consume very much if we have more customer, so in the script we change the selector from `#customer .js-delete` to only `#customer` and add `.js-delete` to the second parameter of `.on()` method to filter the click event which have this class.

![image-20210825185216383](https://raw.githubusercontent.com/luanhytran/img/master/image-20210825185216383.png)

With this implementation, no matter how much customer we have on the page, we only have one click handler for all delete button. Because our handler is hook to `#customer` and the only element that have this Id is the `<table>` element. If the click event is raised from an element match `.js-delete` , this callback function will be call.

## DataTables Plug-in

We use [DataTables | Table plug-in for jQuery](https://datatables.net/) to add pagination, sorting and filtering to our customer table.

**Install package**

Open Package Manager Console and install

```powershell
install-package jquery.datatables -version:1.10.11
```

**Setup**

Go to  `_Layout.cshtml` , now we will group all third party libraries together and call them in single line:

Change from this 

![image-20210825191447579](https://raw.githubusercontent.com/luanhytran/img/master/image-20210825191447579.png)

To this

![image-20210825191509406](https://raw.githubusercontent.com/luanhytran/img/master/image-20210825191509406.png)



Now go to `BundleConfig.cs` :

Change from this

![image-20210825192010254](https://raw.githubusercontent.com/luanhytran/img/master/image-20210825192010254.png)

To this

![image-20210825192232721](https://raw.githubusercontent.com/luanhytran/img/master/image-20210825192232721.png)

We need to add `datatables.bootstrap.css` to make it look like bootstrap style.

**How to use this plug-in?**

We select the reference to our table and add `.DataTable()`

![image-20210825195952356](https://raw.githubusercontent.com/luanhytran/img/master/image-20210825195952356.png)

**How this work?**

In this implementation, we render the view on the server and return HTML markup on the client. DataTables plug-in will extract our data from DOM elements and construct it's own list of customer, so it will maintain a bunch of JSON objects on the client and wherever we search for something, sort column, it use these JSON objects to provide filtering, sorting and pagination.

**The problem**

This come with a cost, we return all these html markup from the server and then datatables plug-in have to parse these markup to extract the actual data. This will be a issue if we have thousand records.

![image-20210825195510962](https://raw.githubusercontent.com/luanhytran/img/master/image-20210825195510962.png)

Solution is it better to get raw data from the server (JSON), give it to datatables plug-in and let it generate the html markup on the client. This way , instead of return all the markup from the server, you return JSON objects. In the next section will help you do this.

![image-20210825195641829](https://raw.githubusercontent.com/luanhytran/img/master/image-20210825195641829.png)



## DataTables with Ajax Source

We use DataTable to call API to get list of JSON customers.

```javascript
$(document).ready(function () { 
var table = $('#customers').DataTable({
                ajax: {
                    url: "/api/customers",
                    dataSrc: ""
                },
                columns: [
                    {
                        // property in the response JSON
                        data: "name",
                        // data = value in name property
                        // type = type of this column
                        // customer = the object in the response array, we can name what we want
                        render: function(data, type, customer) {
                            return "<a href='/customers/edit/" + customer.id + "'>" + customer.name + "</a>";
                        }
                    },
                    {
                        // we haven't eager loading to include membership type here yet
                        data: "name"
                    },
                    {
                        data: "id",
                        // we only need id so we don't have to write other parameter
                        render: function(data) {
                            return "<button class='btn-link js-delete' data-customer-id='" + data + "'>" + "Delete" + "</button>";
                        }
                    }
                ]
            });
    });
```

**Explain code**

First we specify url, second we specify `dataSrc` .

Imagine our API response an object like this, in this case the `customers` property is the actual source of data so we set `dataSrc: "customers"` .

![image-20210826015122247](https://raw.githubusercontent.com/luanhytran/img/master/image-20210826015122247.png)

But in our case, the response we get from our API is an array of objects.

![image-20210825195641829](https://raw.githubusercontent.com/luanhytran/img/master/image-20210825195641829.png)

So this array itself is the actual source of data and DataTable doesn't need to go to another object to get it. That's why we set `dataSrc: ""`  .

![image-20210826015431207](https://raw.githubusercontent.com/luanhytran/img/master/image-20210826015431207.png)

After AJAX , we specify the column for table.

## Returning Hierarchical Data

Currently our API response for get all customer haven't include member type object yet. We will use eager loading to include it in our API.

![image-20210825195641829](https://raw.githubusercontent.com/luanhytran/img/master/image-20210825195641829.png)

This is the result, the red line is a hierarchical data

![image-20210826030337404](https://raw.githubusercontent.com/luanhytran/img/master/image-20210826030337404.png)

## DataTables Removing Records

If we delete a row it will stay there, because we just remove that customer from the DOM (the <tr> element). But DataTable keep the list of customers internally, so when we go back and search something in the table, the table is refresh and use the internally customers list and of course we didn't delete our customer from that list.

![image-20210826030748439](https://raw.githubusercontent.com/luanhytran/img/master/image-20210826030748439.png)

To fix this, instead of working directly to the DOM we should remove the corresponding customer from the internal list of our DataTable then will tell it to re-draw it self.

**Step 1:** Add table variable to reference DataTable 

![image-20210826032412206](https://raw.githubusercontent.com/luanhytran/img/master/image-20210826032412206.png)

**Step 2:** 

![image-20210826032441432](https://raw.githubusercontent.com/luanhytran/img/master/image-20210826032441432.png)

**Code:**

```javascript
@section scripts
{
    <script>
        $(document).ready(function () {
            var table = $('#customers').DataTable({
                ajax: {
                    url: "/api/customers",
                    dataSrc: ""
                },
                columns: [
                    {
                        // property in the response JSON
                        data: "name",
                        // data = value in name property
                        // type = type of this column
                        // customer = the object in the response array, we can name what we want
                        render: function(data, type, customer) {
                            return "<a href='/customers/edit/" + customer.id + "'>" + customer.name + "</a>";
                        }
                    },
                    {
                        // we haven't eager loading to include membership type here yet
                        data: "membershipType.name"
                    },
                    {
                        data: "id",
                        // we only need id so we don't have to write other parameter
                        render: function(data) {
                            return "<button class='btn-link js-delete' data-customer-id='" + data + "'>" + "Delete" + "</button>";
                        }
                    }
                ]
            });

            $("#customers").on("click", ".js-delete",
                function () {
                    // the scope of this keyword can change in different callback function 
                    // so we need to reference to our button
                    var button = $(this);

                    bootbox.confirm("Are you sure you want to delete this customer?",
                        function(result) {
                            if (result) {
                                $.ajax({
                                    url: "/api/customers/" + button.attr("data-customer-id"),
                                    method: "DELETE",
                                    success: function () {
                                        // All these method are from DataTables
                                        // .row() reference to <tr> element
                                        // .draw() redraw the table when deleted a customer
                                        // .remove() remove a customer from internal list
                                        table.row(button.parents("tr")).remove().draw();
                                    }
                                });
                            }
                        });
                });
        });
    </script>
}
```



## DataTables limit

If we have an object that have a lot of properties and the API return a thousand of that fat objects. Then pagination, sorting and searching should be implement on the server then in DataTables you should eneable server-side processing. Because if we work with thousand of large objects, pagination, sorting and searching on the client-side is not efficiently.

[(7) jQuery datatables server side processing example asp net - YouTube](https://www.youtube.com/watch?v=u4QKLehvUhs)



