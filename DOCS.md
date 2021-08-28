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

## Building a Feature End-to-End Systematically

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



