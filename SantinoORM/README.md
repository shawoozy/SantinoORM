
# Santino ORM - An expansion on Dapper for .NET
![img](https://i.imgur.com/kWhIY0e.png)

![img](https://circleci.com/gh/GamebasicsBV/SantinoORM/tree/master.svg?&style=shield&circle-token=b076c59e625a7123d75de5dd69652f877ddafc1b)  ![img](https://img.shields.io/teamcity/codebetter/bt428.svg)



Dapper is a performance purpose micro-ORM, but lacks out of the box functions like mapping of object relations.

This ORM is built upon Dapper to provide user friendly interfaces and methods to make you write cleaner code with a built in query builder.


## Features

### Query Builder

The query builder is made with alot of options for better, cleaner and easier to read queries.

For now only MySql/MariaDB is supported.

See example below. For full list of options see bottom of this page.
```csharp  
var user = new User().Where("user_id", 360).First<User>();  
var cities = new City().Where("population", ">" 50000).OrWhere("crime", "low").Get<City>();  
```  

### Relation mapper

While Dapper is very fast, it cannot map multiple database relation models.

Normally, to achieve this you have to write a manual mapper for each relation, but this not needed when using this ORM.

See below for example.


```csharp  
//                          table      column1     column2 
var user = new User().Join("address", "adddres_id", "id")
           .Where("user_id", 360)
           .First<User, Address>();  
// the user object will have the address object mapped in it.  
  
var cities = new City()  
            .LeftJoin("countries", "country_id", "id") 
            .RightJoin("food", "main_food_id", "id")
 //          column       value           value       value   
            .WhereIn("food.name", "FishAndChips", "Croquete", "Tacos")             
            .Get<City, Country, Food>();  
// this will return a list of city objects which contain their country and food objects  
```  

### Lazy loading

Lazy loading is doing a seperate query for each relation.

To 'lazy' load a relation after the model has previously been fetched from the database, use the lazyload method like below.
```csharp  
// single object 
      var user = new User()
                     .Where("user_id", 360)
                     .First<User>(); 
                     
     user.LazyLoad<Address>("adddres_id", "id");  
  
// list of objects  
var cities = new City()
            .Where("population", ">" 50000)
            .First<User>(); 
            
cities.LazyLoad<Country>("country_id", "id");  
```  

It is also possible to load the relations 'lazy' while building a query to fetch.
```csharp 
var user = new User()  
          .With<Address>("adddres_id", "id") 
          .Where("user_id", 360) 
          .First<User, Address>(); // the user object will have the address object mapped in it.  
// this will run two queries.  
```  

### Performance

While manual mapping is always going to be faster, SantinoORM has great performance.

![performance](https://i.imgur.com/hDQR2Kv.png)


## How to use

### Database connection

Adjust your project settings or environment variables to contain a database connection string.

```json  
{"DatabaseConnectionString": "Server=localhost;Database=somedatabase;Uid=root;Pwd=;"}  
```  

### Dependency injection

Add SantinoORM to your services in your startup class
```csharp  
 public static void ConfigureServices(IServiceCollection services) 
 { 
        var builder = new ConfigurationBuilder() 
                     .SetBasePath(Directory.GetCurrentDirectory()) 
                     .AddJsonFile("appsettings.json");   
       
       Configuration = builder.Build();  
       
      services.AddSantinoORM(Configuration); 
   //...  
} 
```  

### Models

Create a class for each of your database table if you want to use them.

See below for class example.
```csharp  
 public class User : BaseModel // required, must extend the BaseModel class 
 { 
 public new static string Table { get; set; } = "users";
       
 [PrimaryKey] // required, add this attribute to your primary key property(ies)  
 public int UserId { get; set; } 
     
 public string Firstname { get; set; }  
 
 public string Lastname { get; set; }  

 // one-to-one relation  
 public Address Address { get; set; } // to have the mapping of relation work, add the relation to the class. note: the property name must be the same as the relation class name     
  
 // one-to-many relation  
 public IList<Device> Device { get; set; }     
  
 public override object GetPrimaryKey() // required, override this method and return the primary key property(ies)  
  { 
     return UserId;
  } 
 }
 ```  

### Repository pattern

Alot of applications use the repository pattern and do not want any database related code anywhere else, obviously.

To make it easier, extend the BaseRepository with your repository classes to make use of the built in CRUD functions.
```csharp  
 // the user repository class 
 public class UserRepository : BaseRepository<User>, IUserRepository 
 {       
   protected override User Model() // this method is required to override  
   {
       return new User(); 
   } 
  
 }  
    
// the user service class  
 public class UserService : IUserService 
 {        
 
   public UserService(IUserRepository userRepository)       
   {  
        var user = userRepository.Find(360); // fetches an user object with id 360 from database            
        user.Lastname = "Doe";  
        userRepository.Update(user); // updates previously fetched user object in the database             
        userRepository.Delete(user); // deletes the user object from the database  
        var users = userRepository.GetCollectionFromSingleWhere("role", "developer"); // fetches users with role developer from database                            
   }  
 }  
```  

## Query builder

### Where statements

* Where
```csharp  
// statement operator is not '='  
new User().Where("lastname", "like", "%Doe%");  
// statement operator is '='  
new User().Where("lastname", "Doe");  
```  
* OrWhere
```csharp  
new User().OrWhere("firstname", "John");  
```  
* WhereIn
```csharp  
new User().WhereIn("user_id", 1, 2, 3, 4, 5);  
```  
* OrWhereIn
```csharp  
new User().OrWhereIn("user_id", 6, 7, 8, 9, 10);  
```  
* WhereNotIn
```csharp  
new User().WhereNotIn("user_id", 1, 2, 3, 4, 5);  
```  
* OrWhereNotIn
```csharp  
new User().OrWhereNotIn("user_id", 6, 7, 8, 9, 10);  
```  
* WhereNull
```csharp  
new User().WhereNull("address");  
```  
* OrWhereNull
```csharp  
new User().OrWhereNull("address");  
```  
* WhereNotNull
```csharp  
new User().WhereNotNull("address");  
```  
* OrWhereNotNull
```csharp  
new User().OrWhereNotNull("address");  
```  
* WhereRaw
```csharp  
var userId = 23;  
var role = "Admin"  
new User().WhereRaw("UserId = @UserId or Role <> @Role", userid, role);  
  
// putting raw values in the statement is also possible  
new User().WhereRaw("UserId = 23 or Role <> 'Admin'");  
```  
* Nested where statements
```csharp  
new User().Where("firstname", "John")  
          .Where("lastname", "<>", "Doe") 
          .OrWhere("firstname", "Chong") 
          .Where("Lastname, "Wayne");  
// this will built the statement like:  
// where (`user`.`firstname` = 'John' and `user`.`Lastname` <> 'Doe')  
// or (`user`.`firstname` = 'Chong' and `user`.`Lastname` = 'Wayne')  
// the values are actually not in the query but have a key which is bound to the variable  
```  
### Select statements

* Select
```csharp  
// select which columns to be in the query result  
new User().Select("user_id", "firstname", "lastname");  
```  
* SelectRaw
```csharp  
// add a raw select statement, like query functions  
new User().SelectRaw("AVG(`field`) as average");  
```  
* Distinct
```csharp  
// add select distinct to the query  
new User().Distinct();  
```  

### Join statements

* Join
```csharp  
// a 'inner join' statement, the first parameter is the table, the second the column of the original table and the third parameter is the column of the joined table to join on  
new User().Join("address", "adddres_id", "id");  
  
// when there are more columns to join on, add as a raw statement as fourth parameter  
new City().Join("countries", "country_id", "id", "and cities.column = countries.column");  
  
// when the operator of the joined columns is not '='  
new City().Join("countries", "country_id", "id", "<>");  
  
```  
* LeftJoin
```csharp  
//  a 'left join' statement  
new User().LeftJoin("address", "adddres_id", "id");  
```  
* RightJoin
```csharp  
//  a 'right join' statement  
new User().RightJoin("address", "adddres_id", "id");  
```  

### Having statements
* Having
```csharp  
// statement operator is not '='  
new User().Where("firstname", "John").Having("lastname", "like", "%Doe%");  
// statement operator is '='  
new User().Where("firstname", "John").Having("lastname", "Doe");  
```  
* OrHaving
```csharp  
new User().Where("firstname", "John").Having("lastname", "Doe").OrHaving("lastname", "Wayne");  
```  
* HavingRaw
```csharp  
new User().Where("firstname", "John").HavingRaw("Lastname = @Lastname", "Doe")  
```  
* OrHavingRaw
```csharp  
new User()
    .Where("firstname", "John")
    .HavingRaw("Lastname = @Lastname", "Doe")
    .OrHavingRaw("Lastname = @Lastname", "Wayne")  
```  

### Other statements

* Limit
```csharp  
new User().Limit(50); // returns first 50 rows  
```  

* Limit with offset
```csharp  
new User().Limit(25, 25); // returns the 26th to 50th rows  
```  
* OrderBy
```csharp  
new User().OrderBy("lastname"); // order by desc  
```  
* OrderByDesc
```csharp  
new User().OrderByDesc("lastname"); // order by desc  
```  
* OrderByAsc
```csharp  
new User().OrderByAsc("lastname"); // order by asc  
```  
* OrderByRandom
```csharp  
new User().OrderByRandom("lastname"); // order by rand()  
```  
* GroupBy
```csharp  
new User().GroupBy("lastname");  
```  

### Return the results

* First<>
```csharp  
// returns one user object found within the result of the query  
new User().Where("firstname", "John").Where("lastname", "Doe").First<User>();  
```  
* First<First, Second, Third, ...>
```csharp  
// returns one user object and within the relation object(s)  
// keep in mind, this does not work propery if the relation is an one-to-many relation, please use 'Get<>'  
new User().Join("address", "adddres_id", "id").Where("user_id", 360).First<User, Address>();  
```  
* Get<>
```csharp  
// returns a list of user objects found within the results of the query  
new User().Where("firstname", "John").Where("lastname", "Doe").Get<User>();  
```  
* Get<First, Second, Third, ...>
```csharp  
// returns a list of user objects and within the relation object(s)  
new User()
   .Join("address", "adddres_id", "id")  
   .Join("...").Where("user_id", 360)
   .Get<User, Address, ...>();  
   
// when you only want to use the join for a constraint  
// the query results will not containt the joined table data  
new User()
   .Join("address", "adddres_id", "id")  
   .Join("...")
   .Where("address.street", "Route66")
   .Get<User>();

```  
* GetAsObject
```csharp  
// Returns a 'DapperRow' object which can be used as dictionary  
// Returns a list of 'DapperRow' objects if there are multiple rows in the query result  
new User().Where("firstname", "John").Where("lastname", "Doe").GetAsObject();  
```  
* BuildQuery
```csharp  
// returns the built query as a string  
new User().Where("user_id", 360).BuildQuery():  
```  

* GetBindingsParameters
```csharp  
// returns the dynamic binding dictionary object 
new User().Where("user_id", 360).GetBindingsParameters():  
```  

### Save

* Save
```csharp  
var user = new User{ Firstname = "John", Lastname = "Doe" };  
user.Save();
```  

* BulkSave
```csharp  
var doe = new User{ Firstname = "John", Lastname = "Doe" };  
var wayne = new User{ Firstname = "John", Lastname = "Wayne" };  
var users = new List<User> {doe, wayne};  
users.BulkSave();
```  

### Update
* Update
```csharp  
var user = new User().Where("firstname", "John").First<User>();  
user.Firstname = "Chong";  
user.Update();

```  

* BulkUpdate
```csharp  
var doe = new User().Where("lastname", "Doe").First<User>();  
doe.Firstname = "Chong";  
  
var wayne = new User().Where("lastname", "Wayne").First<User>();  
wayne.Firstname = "John";  
  
var users = new List<User> {doe, wayne};  
users.BulkUpdate(); 
```  

### Delete
* Update
```csharp  
var user = new User().Where("firstname", "John").First<User>();  
user.Delete(); // deletes one record 
```  
* Update (with query)
```csharp  
var user = new User().Where("firstname", "John").Delete(); // deletes all records where firstname is john  
```  


## To do's

* Make it possible to have different property name for relations
* ~~Lazy loading~~
* ~~Only update "dirty" fields~~
* Other database's integration

## Built With

* [.NET CORE 2](https://docs.microsoft.com/en-us/dotnet/core//) - Built as a .net core library
* [Dapper](https://github.com/StackExchange/Dapper/) - Dappper
## Authors

* **Sharokh Aria** - *Initial work* -