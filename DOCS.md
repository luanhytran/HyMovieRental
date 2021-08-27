# Server-side

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



